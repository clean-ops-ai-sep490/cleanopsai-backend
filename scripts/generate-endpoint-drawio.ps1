$ErrorActionPreference = "Stop"

function Escape-Xml([string]$text) {
    if ($null -eq $text) { return "" }
    return [System.Security.SecurityElement]::Escape($text).Replace("`r`n", "&#xa;").Replace("`n", "&#xa;")
}

function New-VertexXml {
    param(
        [string]$Id,
        [string]$Value,
        [string]$Style,
        [int]$X,
        [int]$Y,
        [int]$Width,
        [int]$Height,
        [string]$Parent = "1"
    )

    $escaped = Escape-Xml $Value
    return @"
        <mxCell id="$Id" value="$escaped" style="$Style" vertex="1" parent="$Parent">
          <mxGeometry x="$X" y="$Y" width="$Width" height="$Height" as="geometry"/>
        </mxCell>
"@
}

function New-EdgeXml {
    param(
        [string]$Id,
        [string]$Source,
        [string]$Target,
        [string]$Label = "",
        [string]$Style = "endArrow=open;endFill=0;html=1;rounded=0;edgeStyle=orthogonalEdgeStyle;strokeColor=#666666;",
        [string]$Parent = "1"
    )

    $valueAttr = ""
    if ($Label) {
        $valueAttr = " value=""$(Escape-Xml $Label)"""
    }

    return @"
        <mxCell id="$Id"$valueAttr style="$Style" edge="1" parent="$Parent" source="$Source" target="$Target">
          <mxGeometry relative="1" as="geometry"/>
        </mxCell>
"@
}

function Get-MethodSignature {
    param([string[]]$Lines, [int]$StartIndex)

    $signature = @()
    for ($j = $StartIndex; $j -lt [Math]::Min($Lines.Count, $StartIndex + 20); $j++) {
        $signature += $Lines[$j].Trim()
        if ($Lines[$j] -match '\)') { break }
    }

    return ($signature -join ' ')
}

function Normalize-TypeName([string]$typeName) {
    if (-not $typeName) { return $typeName }
    return (($typeName -split '<')[0] -split '\[')[0].Trim('?')
}

function Is-PrimitiveLike([string]$typeName) {
    if (-not $typeName) { return $true }
    return $typeName -match '^(Guid|int|long|double|decimal|float|bool|string|DateTime|DateOnly|TimeOnly|CancellationToken|IFormFile|byte\[\]|object)$'
}

function Get-ClassProperties {
    param([string]$FilePath)

    if (-not (Test-Path $FilePath)) { return @() }
    $lines = Get-Content $FilePath
    $props = @()
    foreach ($line in $lines) {
        $trim = $line.Trim()
        if ($trim -match '^public\s+([A-Za-z0-9_<>,\.\[\]\?]+)\s+([A-Za-z0-9_]+)\s*\{\s*get;') {
            $props += "+$($matches[2]) : $($matches[1])"
        }
    }
    return @($props | Select-Object -Unique)
}

function Get-ClassMethods {
    param([string]$FilePath)

    if (-not (Test-Path $FilePath)) { return @() }
    $lines = Get-Content $FilePath
    $methods = @()
    foreach ($line in $lines) {
        $trim = $line.Trim()
        if ($trim -match '^public\s+(?:async\s+)?(?:[A-Za-z0-9_<>,\.\[\]\?]+)\s+([A-Za-z0-9_]+)\s*\(') {
            $methods += "+$($matches[1])()"
        }
    }
    return @($methods | Select-Object -Unique)
}

function Get-TypeIndex {
    $index = @{}
    $files = Get-ChildItem "src\Modules" -Recurse -Filter "*.cs" | Where-Object { $_.FullName -notmatch "Tests" }

    foreach ($file in $files) {
        $content = Get-Content $file.FullName
        foreach ($line in $content) {
            $trim = $line.Trim()
            if ($trim -match '^(public\s+)?(class|record|interface)\s+([A-Za-z0-9_]+)') {
                $typeName = $matches[3]
                if (-not $index.ContainsKey($typeName)) {
                    $index[$typeName] = $file.FullName
                }
            }
        }
    }

    return $index
}

function Get-ImplementationIndex {
    $map = @{}
    $files = Get-ChildItem "src\Modules" -Recurse -Filter "*.cs" | Where-Object { $_.FullName -notmatch "Tests" }

    foreach ($file in $files) {
        $content = Get-Content $file.FullName
        foreach ($line in $content) {
            $trim = $line.Trim()
            if ($trim -match '^public\s+class\s+([A-Za-z0-9_]+)\s*:\s*([A-Za-z0-9_<>,\.\[\]\?\s]+)') {
                $className = $matches[1]
                $inheritance = $matches[2]
                $parts = $inheritance -split ',' | ForEach-Object { Normalize-TypeName $_.Trim() }
                foreach ($part in $parts) {
                    if (-not $map.ContainsKey($part)) { $map[$part] = @() }
                    $map[$part] += [pscustomobject]@{ Name = $className; FilePath = $file.FullName }
                }
            }
        }
    }

    return $map
}

function Get-ServiceMetadata {
    param([hashtable]$TypeIndex, [hashtable]$ImplementationIndex)

    $map = @{}
    $services = Get-ChildItem "src\Modules" -Recurse -Filter "*Service.cs" | Where-Object { $_.FullName -notmatch "Tests" }

    foreach ($file in $services) {
        $content = Get-Content $file.FullName
        $classMatch = $content | Select-String 'class\s+([A-Za-z0-9_]+)\s*:' | Select-Object -First 1
        if (-not $classMatch) { continue }

        $className = $classMatch.Matches[0].Groups[1].Value
        $deps = @()
        foreach ($line in $content) {
            $trim = $line.Trim()
            if ($trim -match '^private readonly ([A-Za-z0-9_<>,\.\[\]\?]+) (_[A-Za-z0-9_]+);') {
                $depType = $matches[1].Trim()
                $depName = $matches[2].Trim()
                $normalized = Normalize-TypeName $depType
                $kind = if ($normalized -match 'Repository$') { 'RepositoryInterface' } elseif ($normalized -match 'DbContext$|Context$') { 'DbContext' } else { 'Dependency' }
                $props = @()
                $methods = @()
                if ($TypeIndex.ContainsKey($normalized)) {
                    $props = Get-ClassProperties -FilePath $TypeIndex[$normalized]
                    $methods = Get-ClassMethods -FilePath $TypeIndex[$normalized]
                }
                $implementations = @()
                if ($ImplementationIndex.ContainsKey($normalized)) {
                    foreach ($impl in $ImplementationIndex[$normalized]) {
                        $implementations += [pscustomobject]@{
                            Name = $impl.Name
                            FilePath = $impl.FilePath
                            Properties = Get-ClassProperties -FilePath $impl.FilePath
                            Methods = Get-ClassMethods -FilePath $impl.FilePath
                        }
                    }
                }
                $deps += [pscustomobject]@{
                    Type = $depType
                    Name = $depName
                    NormalizedType = $normalized
                    Kind = $kind
                    Properties = $props
                    Methods = $methods
                    Implementations = @($implementations)
                }
            }
        }

        $map[$className] = [pscustomobject]@{
            ClassName = $className
            Dependencies = @($deps)
            FilePath = $file.FullName
            Properties = Get-ClassProperties -FilePath $file.FullName
            Methods = Get-ClassMethods -FilePath $file.FullName
        }
    }

    return $map
}

function Get-ControllerMetadata {
    param([hashtable]$TypeIndex, [hashtable]$ImplementationIndex)

    $controllers = @()
    $files = Get-ChildItem "src\Api\CleanOpsAi.Api" -Recurse -Filter "*Controller.cs"

    foreach ($file in $files) {
        $lines = Get-Content $file.FullName
        $controllerName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
        $shortController = $controllerName -replace 'Controller$', ''

        $route = ""
        foreach ($line in $lines) {
            $trim = $line.Trim()
            if ($trim -match '^\[Route\("([^"]+)"\)\]') {
                $route = $matches[1]
                break
            }
        }
        if (-not $route) { continue }
        $route = $route.Replace('[controller]', $shortController)

        $services = @()
        foreach ($line in $lines) {
            $trim = $line.Trim()
            if ($trim -match '^private readonly ([A-Za-z0-9_<>,\.\[\]\?]+) (_[A-Za-z0-9_]+);') {
                $serviceType = $matches[1].Trim()
                $normalized = Normalize-TypeName $serviceType
                $serviceImpls = @()
                $implGuess = Normalize-TypeName ($serviceType -replace '^I(?=[A-Z].*Service$)', '')
                if ($ImplementationIndex.ContainsKey($normalized)) {
                    $serviceImpls = @($ImplementationIndex[$normalized])
                } elseif ($ImplementationIndex.ContainsKey($implGuess)) {
                    $serviceImpls = @($ImplementationIndex[$implGuess])
                }
                $services += [pscustomobject]@{
                    Type = $serviceType
                    Field = $matches[2].Trim()
                    InterfaceType = $normalized
                    Implementations = @($serviceImpls)
                }
            }
        }
        $services = @($services | Group-Object Type | ForEach-Object { $_.Group[0] })

        $pendingVerb = $null
        $pendingSubRoute = ""
        $endpoints = @()

        for ($i = 0; $i -lt $lines.Count; $i++) {
            $trim = $lines[$i].Trim()
            if ($trim -match '^//') { continue }

            if ($trim -match '^\[(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)(\("([^"]*)"\))?') {
                $pendingVerb = $matches[1] -replace '^Http', ''
                $pendingSubRoute = $matches[3]
                continue
            }

            if ($pendingVerb -and $trim -match '^public async Task<[^>]+>\s+([A-Za-z0-9_]+)\s*\(') {
                $action = $matches[1]
                $signature = Get-MethodSignature -Lines $lines -StartIndex $i
                $paramMatches = [regex]::Matches($signature, '(?:\[[^\]]+\]\s*)?([A-Za-z0-9_<>,\.\[\]\?]+)\s+([A-Za-z0-9_]+)(?:\s*=\s*[^,\)]*)?')
                $parameters = @()
                $dtoTypes = @()

                foreach ($m in $paramMatches) {
                    $typeName = $m.Groups[1].Value
                    $paramName = $m.Groups[2].Value
                    if ($typeName -in @('public','async','Task<IActionResult>','Task<ActionResult>')) { continue }
                    if ($paramName -eq $action) { continue }
                    if (-not $typeName) { continue }

                    $parameters += [pscustomobject]@{ Name = $paramName; Type = $typeName }

                    $normalized = Normalize-TypeName $typeName
                    if (-not (Is-PrimitiveLike $normalized) -and $TypeIndex.ContainsKey($normalized)) {
                        $dtoTypes += [pscustomobject]@{
                            Name = $normalized
                            Properties = Get-ClassProperties -FilePath $TypeIndex[$normalized]
                            Methods = Get-ClassMethods -FilePath $TypeIndex[$normalized]
                            FilePath = $TypeIndex[$normalized]
                        }
                    }
                }

                $fullPath = $route
                if ($pendingSubRoute) {
                    if ($fullPath.EndsWith('/')) { $fullPath = "$fullPath$pendingSubRoute" }
                    else { $fullPath = "$fullPath/$pendingSubRoute" }
                }

                $endpoints += [pscustomobject]@{
                    Verb = $pendingVerb
                    Action = $action
                    Path = $fullPath
                    Parameters = @($parameters)
                    DtoTypes = @($dtoTypes | Group-Object Name | ForEach-Object { $_.Group[0] })
                }

                $pendingVerb = $null
                $pendingSubRoute = ""
            }
        }

        if ($endpoints.Count -gt 0) {
            $controllers += [pscustomobject]@{
                Controller = $controllerName
                Route = $route
                Services = @($services)
                Endpoints = @($endpoints)
            }
        }
    }

    return $controllers
}

function Build-ClassText {
    param(
        [string]$Name,
        [string[]]$Attributes,
        [string[]]$Operations,
        [string]$Stereotype = ""
    )

    $lines = @()
    if ($Stereotype) { $lines += "<<$Stereotype>> $Name" } else { $lines += $Name }
    if ($Attributes -and $Attributes.Count -gt 0) { $lines += '-'; $lines += $Attributes }
    if ($Operations -and $Operations.Count -gt 0) { $lines += '-'; $lines += $Operations }
    return ($lines -join "`n")
}

function Build-EndpointDiagram {
    param(
        [pscustomobject]$Controller,
        [pscustomobject]$Endpoint,
        [hashtable]$ServiceMetadata,
        [string]$DiagramId,
        [string]$DiagramName
    )

    $cells = New-Object System.Collections.Generic.List[string]
    $cells.Add('        <mxCell id="0"/>')
    $cells.Add('        <mxCell id="1" parent="0"/>')

    $headerStyle = 'rounded=1;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;fontStyle=1;fontSize=16;align=center;'
    $endpointStyle = 'shape=umlClass;whiteSpace=wrap;html=1;fillColor=#fff2cc;strokeColor=#d6b656;align=left;spacingLeft=8;'
    $requestStyle = 'shape=umlClass;whiteSpace=wrap;html=1;fillColor=#f5f5f5;strokeColor=#666666;align=left;spacingLeft=8;'
    $controllerStyle = 'shape=umlClass;whiteSpace=wrap;html=1;fillColor=#d5e8d4;strokeColor=#82b366;align=left;spacingLeft=8;'
    $serviceInterfaceStyle = 'shape=umlClass;whiteSpace=wrap;html=1;fillColor=#dae8fc;strokeColor=#6c8ebf;align=left;spacingLeft=8;'
    $serviceImplStyle = 'shape=umlClass;whiteSpace=wrap;html=1;fillColor=#cfe2f3;strokeColor=#3c78d8;align=left;spacingLeft=8;'
    $repoInterfaceStyle = 'shape=umlClass;whiteSpace=wrap;html=1;fillColor=#f8cecc;strokeColor=#b85450;align=left;spacingLeft=8;'
    $repoImplStyle = 'shape=umlClass;whiteSpace=wrap;html=1;fillColor=#f4cccc;strokeColor=#cc0000;align=left;spacingLeft=8;'
    $dtoStyle = 'shape=umlClass;whiteSpace=wrap;html=1;fillColor=#e1d5e7;strokeColor=#9673a6;align=left;spacingLeft=8;'

    $cells.Add((New-VertexXml -Id "${DiagramId}-title" -Value "$($Endpoint.Verb) $($Endpoint.Path)" -Style $headerStyle -X 380 -Y 20 -Width 740 -Height 40))

    $endpointText = Build-ClassText -Name "$($Endpoint.Action)Endpoint" -Stereotype 'Endpoint' -Attributes @(
        "+route : $($Endpoint.Path)",
        "+verb : $($Endpoint.Verb)"
    ) -Operations @("+Handle()")

    $requestAttrs = @(
        "+controller : $($Controller.Controller)",
        "+action : $($Endpoint.Action)"
    ) + ($Endpoint.Parameters | ForEach-Object { "+$($_.Name) : $($_.Type)" })
    $requestText = Build-ClassText -Name "$($Endpoint.Action)Request" -Stereotype 'Request' -Attributes $requestAttrs -Operations @()

    $controllerText = Build-ClassText -Name $Controller.Controller -Stereotype 'Controller' -Attributes @(
        "+route : $($Controller.Route)"
    ) -Operations @("+$($Endpoint.Action)() : IActionResult")

    $cells.Add((New-VertexXml -Id "${DiagramId}-endpoint" -Value $endpointText -Style $endpointStyle -X 610 -Y 90 -Width 280 -Height 110))
    $cells.Add((New-VertexXml -Id "${DiagramId}-request" -Value $requestText -Style $requestStyle -X 940 -Y 230 -Width 340 -Height ([Math]::Max(120, 90 + ($Endpoint.Parameters.Count * 20)))))
    $cells.Add((New-VertexXml -Id "${DiagramId}-controller" -Value $controllerText -Style $controllerStyle -X 470 -Y 250 -Width 300 -Height 120))

    $cells.Add((New-EdgeXml -Id "${DiagramId}-e1" -Source "${DiagramId}-endpoint" -Target "${DiagramId}-request" -Label 'receives'))
    $cells.Add((New-EdgeXml -Id "${DiagramId}-e2" -Source "${DiagramId}-endpoint" -Target "${DiagramId}-controller" -Label 'routes to'))

    $dtoX = 1320
    $dtoY = 230
    $edgeIndex = 3
    $pageBottom = 700

    foreach ($dto in $Endpoint.DtoTypes) {
        $dtoAttrs = if ($dto.Properties.Count -gt 0) { $dto.Properties } else { @('+properties : not resolved') }
        $dtoOps = if ($dto.Methods.Count -gt 0) { $dto.Methods } else { @() }
        $dtoText = Build-ClassText -Name $dto.Name -Stereotype 'DTO' -Attributes $dtoAttrs -Operations $dtoOps
        $dtoHeight = [Math]::Max(110, 70 + (($dtoAttrs.Count + $dtoOps.Count) * 18))
        $dtoNodeId = "${DiagramId}-dto-$edgeIndex"
        $cells.Add((New-VertexXml -Id $dtoNodeId -Value $dtoText -Style $dtoStyle -X $dtoX -Y $dtoY -Width 320 -Height $dtoHeight))
        $cells.Add((New-EdgeXml -Id "${DiagramId}-e$edgeIndex" -Source "${DiagramId}-request" -Target $dtoNodeId -Label 'uses dto'))
        $edgeIndex++
        $dtoY += $dtoHeight + 30
        $pageBottom = [Math]::Max($pageBottom, $dtoY + 40)
    }

    $laneY = 470
    foreach ($service in $Controller.Services) {
        $interfaceText = Build-ClassText -Name $service.InterfaceType -Stereotype 'ServiceInterface' -Attributes @("+$($service.Field) : $($service.Type)") -Operations @()
        $serviceInterfaceNodeId = "${DiagramId}-service-if-$edgeIndex"
        $cells.Add((New-VertexXml -Id $serviceInterfaceNodeId -Value $interfaceText -Style $serviceInterfaceStyle -X 60 -Y $laneY -Width 250 -Height 110))
        $cells.Add((New-EdgeXml -Id "${DiagramId}-e$edgeIndex" -Source "${DiagramId}-controller" -Target $serviceInterfaceNodeId -Label 'depends on'))
        $edgeIndex++

        $serviceImpl = $null
        if ($service.Implementations.Count -gt 0) { $serviceImpl = $service.Implementations[0] }

        $serviceImplNodeId = "${DiagramId}-service-impl-$edgeIndex"
        if ($serviceImpl) {
            $serviceImplName = $serviceImpl.Name
            $serviceMeta = $null
            if ($ServiceMetadata.ContainsKey($serviceImplName)) { $serviceMeta = $ServiceMetadata[$serviceImplName] }
            $serviceImplAttrs = if ($serviceMeta -and $serviceMeta.Properties.Count -gt 0) { $serviceMeta.Properties } else { @("+implementation : $serviceImplName") }
            $serviceImplOps = if ($serviceMeta -and $serviceMeta.Methods.Count -gt 0) { $serviceMeta.Methods } else { @('+ExecuteBusinessLogic()') }
            $serviceImplText = Build-ClassText -Name $serviceImplName -Stereotype 'ServiceImpl' -Attributes $serviceImplAttrs -Operations $serviceImplOps
            $serviceImplHeight = [Math]::Max(120, 80 + (($serviceImplAttrs.Count + $serviceImplOps.Count) * 18))
            $cells.Add((New-VertexXml -Id $serviceImplNodeId -Value $serviceImplText -Style $serviceImplStyle -X 360 -Y $laneY -Width 300 -Height $serviceImplHeight))
            $cells.Add((New-EdgeXml -Id "${DiagramId}-e$edgeIndex" -Source $serviceInterfaceNodeId -Target $serviceImplNodeId -Label 'implemented by'))
            $edgeIndex++

            $depY = $laneY
            if ($serviceMeta -and $serviceMeta.Dependencies.Count -gt 0) {
                foreach ($dep in $serviceMeta.Dependencies) {
                    $depAttrs = if ($dep.Properties.Count -gt 0) { $dep.Properties } else { @("+$($dep.Name) : $($dep.Type)") }
                    $depOps = if ($dep.Methods.Count -gt 0) { $dep.Methods } else { @() }
                    $depText = Build-ClassText -Name $dep.NormalizedType -Stereotype $dep.Kind -Attributes $depAttrs -Operations $depOps
                    $depHeight = [Math]::Max(120, 80 + (($depAttrs.Count + $depOps.Count) * 18))
                    $depNodeId = "${DiagramId}-dep-if-$edgeIndex"
                    $depX = 730
                    $cells.Add((New-VertexXml -Id $depNodeId -Value $depText -Style $repoInterfaceStyle -X $depX -Y $depY -Width 320 -Height $depHeight))
                    $cells.Add((New-EdgeXml -Id "${DiagramId}-e$edgeIndex" -Source $serviceImplNodeId -Target $depNodeId -Label 'depends on'))
                    $edgeIndex++

                    $implY = $depY
                    if ($dep.Implementations.Count -gt 0) {
                        foreach ($repoImpl in $dep.Implementations) {
                            $repoImplAttrs = if ($repoImpl.Properties.Count -gt 0) { $repoImpl.Properties } else { @("+implementation : $($repoImpl.Name)") }
                            $repoImplOps = if ($repoImpl.Methods.Count -gt 0) { $repoImpl.Methods } else { @() }
                            $repoImplText = Build-ClassText -Name $repoImpl.Name -Stereotype 'RepositoryImpl' -Attributes $repoImplAttrs -Operations $repoImplOps
                            $repoImplHeight = [Math]::Max(120, 80 + (($repoImplAttrs.Count + $repoImplOps.Count) * 18))
                            $repoImplNodeId = "${DiagramId}-dep-impl-$edgeIndex"
                            $cells.Add((New-VertexXml -Id $repoImplNodeId -Value $repoImplText -Style $repoImplStyle -X 1110 -Y $implY -Width 340 -Height $repoImplHeight))
                            $cells.Add((New-EdgeXml -Id "${DiagramId}-e$edgeIndex" -Source $depNodeId -Target $repoImplNodeId -Label 'implemented by'))
                            $edgeIndex++
                            $implY += $repoImplHeight + 20
                            $pageBottom = [Math]::Max($pageBottom, $implY + 60)
                        }
                    }

                    $depY += [Math]::Max($depHeight, ($implY - $depY)) + 30
                    $pageBottom = [Math]::Max($pageBottom, $depY + 60)
                }
            }

            $laneY = [Math]::Max($laneY + $serviceImplHeight + 80, $depY + 40)
        } else {
            $unknownText = Build-ClassText -Name 'UnknownServiceImplementation' -Stereotype 'ServiceImpl' -Attributes @('+resolution : unavailable') -Operations @()
            $cells.Add((New-VertexXml -Id $serviceImplNodeId -Value $unknownText -Style $serviceImplStyle -X 360 -Y $laneY -Width 300 -Height 120))
            $cells.Add((New-EdgeXml -Id "${DiagramId}-e$edgeIndex" -Source $serviceInterfaceNodeId -Target $serviceImplNodeId -Label 'implemented by'))
            $edgeIndex++
            $laneY += 220
        }
    }

    $content = ($cells -join "`n")

    return @"
  <diagram id="$DiagramId" name="$DiagramName" compressed="false">
    <mxGraphModel dx="1900" dy="1100" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="2000" pageHeight="$pageBottom" math="0" shadow="0">
      <root>
$content
      </root>
    </mxGraphModel>
  </diagram>
"@
}

$typeIndex = Get-TypeIndex
$implementationIndex = Get-ImplementationIndex
$serviceMetadata = Get-ServiceMetadata -TypeIndex $typeIndex -ImplementationIndex $implementationIndex
$controllers = Get-ControllerMetadata -TypeIndex $typeIndex -ImplementationIndex $implementationIndex
$diagrams = @()
$pageIndex = 1

foreach ($controller in $controllers) {
    foreach ($endpoint in $controller.Endpoints) {
        $safeName = "$($endpoint.Verb)-$($controller.Controller)-$($endpoint.Action)-$pageIndex"
        $diagramId = ($safeName -replace '[^A-Za-z0-9\-_]', '-')
        $diagramName = "$($endpoint.Verb) $($controller.Controller).$($endpoint.Action)"
        $diagrams += Build-EndpointDiagram -Controller $controller -Endpoint $endpoint -ServiceMetadata $serviceMetadata -DiagramId $diagramId -DiagramName $diagramName
        $pageIndex++
    }
}

$xml = @"
<mxfile host="app.diagrams.net" modified="2026-04-20T20:50:00.000Z" agent="Cursor" version="24.7.17" type="device">
$($diagrams -join "`n")
</mxfile>
"@

[System.IO.File]::WriteAllText((Join-Path (Get-Location) 'docs\cleanopsai-api-class-diagram.drawio'), $xml, (New-Object System.Text.UTF8Encoding($false)))
Write-Output "Generated docs\cleanopsai-api-class-diagram.drawio"