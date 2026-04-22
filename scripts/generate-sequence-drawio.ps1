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
        [string]$Style = "endArrow=block;endFill=1;html=1;rounded=0;edgeStyle=orthogonalEdgeStyle;strokeColor=#333333;fontColor=#000000;",
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

function New-FreeEdgeXml {
    param(
        [string]$Id,
        [int]$X1,
        [int]$Y1,
        [int]$X2,
        [int]$Y2,
        [string]$Label = "",
        [string]$Style = "endArrow=block;endFill=1;html=1;rounded=0;edgeStyle=none;strokeColor=#333333;fontColor=#000000;",
        [string]$Parent = "1"
    )

    $valueAttr = ""
    if ($Label) {
        $valueAttr = " value=""$(Escape-Xml $Label)"""
    }

    return @"
        <mxCell id="$Id"$valueAttr style="$Style" edge="1" parent="$Parent">
          <mxGeometry relative="1" as="geometry">
            <mxPoint x="$X1" y="$Y1" as="sourcePoint"/>
            <mxPoint x="$X2" y="$Y2" as="targetPoint"/>
          </mxGeometry>
        </mxCell>
"@
}

function New-ReturnEdgeXml {
    param(
        [string]$Id,
        [int]$X1,
        [int]$Y1,
        [int]$X2,
        [int]$Y2,
        [string]$Label = "",
        [string]$Parent = "1"
    )

    return New-FreeEdgeXml -Id $Id -X1 $X1 -Y1 $Y1 -X2 $X2 -Y2 $Y2 -Label $Label -Style "dashed=1;endArrow=open;endFill=0;html=1;rounded=0;edgeStyle=none;strokeColor=#666666;fontColor=#000000;" -Parent $Parent
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
    param([hashtable]$ImplementationIndex)

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
                $normalized = Normalize-TypeName $depType
                $impls = @()
                if ($ImplementationIndex.ContainsKey($normalized)) {
                    $impls = @($ImplementationIndex[$normalized])
                }
                $deps += [pscustomobject]@{
                    InterfaceType = $normalized
                    Implementations = $impls
                    Kind = if ($normalized -match 'Repository$') { 'Repository' } elseif ($normalized -match 'DbContext$|Context$') { 'DbContext' } else { 'Dependency' }
                }
            }
        }

        $map[$className] = [pscustomobject]@{
            Name = $className
            Dependencies = @($deps)
        }
    }
    return $map
}

function Get-ControllerMetadata {
    param([hashtable]$ImplementationIndex)

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
                    InterfaceType = $normalized
                    Implementations = @($serviceImpls)
                }
            }
        }
        $services = @($services | Group-Object InterfaceType | ForEach-Object { $_.Group[0] })

        $pendingVerb = $null
        $pendingSubRoute = ""
        $endpoints = @()
        for ($i = 0; $i -lt $lines.Count; $i++) {
            $trim = $lines[$i].Trim()
            if ($trim -match '^\[(HttpGet|HttpPost|HttpPut|HttpDelete|HttpPatch)(\("([^"]*)"\))?') {
                $pendingVerb = $matches[1] -replace '^Http', ''
                $pendingSubRoute = $matches[3]
                continue
            }

            if ($pendingVerb -and $trim -match '^public async Task<[^>]+>\s+([A-Za-z0-9_]+)\s*\(') {
                $action = $matches[1]
                $signature = Get-MethodSignature -Lines $lines -StartIndex $i
                $paramMatches = [regex]::Matches($signature, '(?:\[[^\]]+\]\s*)?([A-Za-z0-9_<>,\.\[\]\?]+)\s+([A-Za-z0-9_]+)(?:\s*=\s*[^,\)]*)?')
                $params = @()
                foreach ($m in $paramMatches) {
                    $typeName = $m.Groups[1].Value
                    $paramName = $m.Groups[2].Value
                    if ($typeName -in @('public','async','Task<IActionResult>','Task<ActionResult>')) { continue }
                    if ($paramName -eq $action) { continue }
                    if (-not $typeName) { continue }
                    $params += "${paramName}: ${typeName}"
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
                    Parameters = @($params)
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

function Get-DbVerb([string]$verb) {
    switch ($verb) {
        'GET' { return 'SELECT' }
        'POST' { return 'INSERT' }
        'PUT' { return 'UPDATE' }
        'PATCH' { return 'UPDATE' }
        'DELETE' { return 'DELETE' }
        default { return 'EXEC' }
    }
}

function Get-ReturnOutcome([string]$verb) {
    switch ($verb) {
        'GET' { return 'entity | null' }
        'POST' { return 'created entity' }
        'PUT' { return 'success | null' }
        'PATCH' { return 'success | null' }
        'DELETE' { return 'success | not found' }
        default { return 'result' }
    }
}

function Build-SequenceDiagram {
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

    $bgStyle = 'rounded=0;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#ffffff;'
    $actorClientStyle = 'rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#666666;fontStyle=1;fontColor=#000000;'
    $actorControllerStyle = 'rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#666666;fontStyle=1;fontColor=#000000;'
    $actorServiceStyle = 'rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#666666;fontStyle=1;fontColor=#000000;'
    $actorRepoStyle = 'rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#666666;fontStyle=1;fontColor=#000000;'
    $actorDbStyle = 'rounded=1;whiteSpace=wrap;html=1;fillColor=#ffffff;strokeColor=#666666;fontStyle=1;fontColor=#000000;'
    $lifeLineStyle = 'shape=line;strokeColor=#999999;dashed=1;'
    $activationControllerStyle = 'rounded=0;whiteSpace=wrap;html=1;fillColor=#d9d9d9;strokeColor=#666666;'
    $activationServiceStyle = 'rounded=0;whiteSpace=wrap;html=1;fillColor=#d9d9d9;strokeColor=#666666;'
    $activationRepoStyle = 'rounded=0;whiteSpace=wrap;html=1;fillColor=#d9d9d9;strokeColor=#666666;'
    $activationDbStyle = 'rounded=0;whiteSpace=wrap;html=1;fillColor=#d9d9d9;strokeColor=#666666;'
    $altBoxStyle = 'rounded=0;whiteSpace=wrap;html=1;fillColor=none;strokeColor=#666666;fontColor=#000000;dashed=0;'
    $labelStyle = 'rounded=0;whiteSpace=wrap;html=1;fillColor=none;strokeColor=none;fontColor=#000000;align=left;'

    $cells.Add((New-VertexXml -Id "${DiagramId}-bg" -Value "" -Style $bgStyle -X 0 -Y 0 -Width 1800 -Height 1400))

    $lanes = @(
        [pscustomobject]@{ Name = 'Client'; X = 90; Style = $actorClientStyle; ActivationStyle = '' },
        [pscustomobject]@{ Name = $Controller.Controller; X = 450; Style = $actorControllerStyle; ActivationStyle = $activationControllerStyle },
        [pscustomobject]@{ Name = if ($Controller.Services.Count -gt 0 -and $Controller.Services[0].Implementations.Count -gt 0) { $Controller.Services[0].Implementations[0].Name } elseif ($Controller.Services.Count -gt 0) { $Controller.Services[0].InterfaceType } else { 'Service' }; X = 850; Style = $actorServiceStyle; ActivationStyle = $activationServiceStyle },
        [pscustomobject]@{ Name = 'Repository'; X = 1260; Style = $actorRepoStyle; ActivationStyle = $activationRepoStyle },
        [pscustomobject]@{ Name = 'DB'; X = 1640; Style = $actorDbStyle; ActivationStyle = $activationDbStyle }
    )

    $repoName = 'Repository'
    $serviceMeta = $null
    if ($Controller.Services.Count -gt 0 -and $Controller.Services[0].Implementations.Count -gt 0) {
        $serviceImplName = $Controller.Services[0].Implementations[0].Name
        if ($ServiceMetadata.ContainsKey($serviceImplName)) {
            $serviceMeta = $ServiceMetadata[$serviceImplName]
            $repoDep = $serviceMeta.Dependencies | Where-Object { $_.Kind -eq 'Repository' } | Select-Object -First 1
            if ($repoDep) {
                if ($repoDep.Implementations.Count -gt 0) { $repoName = $repoDep.Implementations[0].Name }
                else { $repoName = $repoDep.InterfaceType }
                $lanes[3].Name = $repoName
            }
        }
    }

    $topY = 40
    foreach ($lane in $lanes) {
        $cells.Add((New-VertexXml -Id "${DiagramId}-actor-$($lane.X)" -Value $lane.Name -Style $lane.Style -X ($lane.X - 90) -Y $topY -Width 180 -Height 42))
        $cells.Add((New-VertexXml -Id "${DiagramId}-lifeline-$($lane.X)" -Value "" -Style $lifeLineStyle -X ($lane.X - 1) -Y 90 -Width 2 -Height 1180))
    }

    $messageY = 150
    $step = 1
    $paramText = if ($Endpoint.Parameters.Count -gt 0) { ', ' + (($Endpoint.Parameters -join ', ') -replace ': ', ':') } else { '' }
    $clientLabel = "$step. $($Endpoint.Verb) /$($Endpoint.Path) {JSON}"
    $cells.Add((New-FreeEdgeXml -Id "${DiagramId}-m$step" -X1 $lanes[0].X -Y1 $messageY -X2 $lanes[1].X -Y2 $messageY -Label $clientLabel))
    $cells.Add((New-VertexXml -Id "${DiagramId}-act-controller" -Value "" -Style $activationControllerStyle -X ($lanes[1].X - 7) -Y ($messageY - 5) -Width 14 -Height 930))

    $step++
    $serviceCallY = $messageY + 90
    $serviceCallName = if ($Endpoint.Verb -eq 'GET') { 'GetByIdAsync' } elseif ($Endpoint.Verb -eq 'POST') { 'CreateAsync' } elseif ($Endpoint.Verb -eq 'PUT') { 'UpdateAsync' } elseif ($Endpoint.Verb -eq 'PATCH') { 'PatchAsync' } elseif ($Endpoint.Verb -eq 'DELETE') { 'DeleteAsync' } else { $Endpoint.Action }
    $serviceLabel = "$step. $serviceCallName($(($Endpoint.Parameters -join ', ') -replace ': ', ':'))"
    $cells.Add((New-FreeEdgeXml -Id "${DiagramId}-m$step" -X1 $lanes[1].X -Y1 $serviceCallY -X2 $lanes[2].X -Y2 $serviceCallY -Label $serviceLabel))
    $cells.Add((New-VertexXml -Id "${DiagramId}-act-service" -Value "" -Style $activationServiceStyle -X ($lanes[2].X - 7) -Y ($serviceCallY - 5) -Width 14 -Height 710))

    $step++
    $repoLookupY = $serviceCallY + 95
    $repoLookupName = if ($Endpoint.Verb -eq 'POST') { 'CreateAsync' } elseif ($Endpoint.Verb -eq 'DELETE') { 'GetByIdAsync' } else { 'GetByIdAsync' }
    $cells.Add((New-FreeEdgeXml -Id "${DiagramId}-m$step" -X1 $lanes[2].X -Y1 $repoLookupY -X2 $lanes[3].X -Y2 $repoLookupY -Label "$step. $repoLookupName(id)"))
    $cells.Add((New-VertexXml -Id "${DiagramId}-act-repo" -Value "" -Style $activationRepoStyle -X ($lanes[3].X - 7) -Y ($repoLookupY - 5) -Width 14 -Height 520))

    $step++
    $dbQueryY = $repoLookupY + 85
    $dbVerb = Get-DbVerb $Endpoint.Verb
    $dbLabel = if ($Endpoint.Verb -eq 'GET') { "$step. $dbVerb * FROM $repoName WHERE Id=@id" } elseif ($Endpoint.Verb -eq 'POST') { "$step. $dbVerb INTO $repoName VALUES (...)" } elseif ($Endpoint.Verb -eq 'DELETE') { "$step. SELECT * FROM $repoName WHERE Id=@id" } else { "$step. SELECT * FROM $repoName WHERE Id=@id" }
    $cells.Add((New-FreeEdgeXml -Id "${DiagramId}-m$step" -X1 $lanes[3].X -Y1 $dbQueryY -X2 $lanes[4].X -Y2 $dbQueryY -Label $dbLabel))
    $cells.Add((New-VertexXml -Id "${DiagramId}-act-db" -Value "" -Style $activationDbStyle -X ($lanes[4].X - 7) -Y ($dbQueryY - 5) -Width 14 -Height 140))

    $return1Y = $dbQueryY + 85
    $cells.Add((New-ReturnEdgeXml -Id "${DiagramId}-r1" -X1 $lanes[4].X -Y1 $return1Y -X2 $lanes[3].X -Y2 $return1Y -Label (Get-ReturnOutcome $Endpoint.Verb)))
    $return2Y = $return1Y + 45
    $cells.Add((New-ReturnEdgeXml -Id "${DiagramId}-r2" -X1 $lanes[3].X -Y1 $return2Y -X2 $lanes[2].X -Y2 $return2Y -Label (Get-ReturnOutcome $Endpoint.Verb)))

    $altTopY = $return2Y + 35
    $altHeight = if ($Endpoint.Verb -eq 'GET' -or $Endpoint.Verb -eq 'POST') { 170 } else { 260 }
    $cells.Add((New-VertexXml -Id "${DiagramId}-altbox" -Value "alt" -Style $altBoxStyle -X 80 -Y $altTopY -Width 1490 -Height $altHeight))

    if ($Endpoint.Verb -eq 'PUT' -or $Endpoint.Verb -eq 'PATCH') {
        $cells.Add((New-VertexXml -Id "${DiagramId}-alt-label1" -Value "[entity exists]" -Style $labelStyle -X 110 -Y ($altTopY + 12) -Width 220 -Height 24))
        $step++
        $updateServiceY = $altTopY + 70
        $cells.Add((New-FreeEdgeXml -Id "${DiagramId}-m$step" -X1 $lanes[2].X -Y1 $updateServiceY -X2 $lanes[3].X -Y2 $updateServiceY -Label "$step. UpdateAsync(entity)"))

        $step++
        $updateDbY = $updateServiceY + 85
        $cells.Add((New-FreeEdgeXml -Id "${DiagramId}-m$step" -X1 $lanes[3].X -Y1 $updateDbY -X2 $lanes[4].X -Y2 $updateDbY -Label "$step. UPDATE $repoName SET ... WHERE Id=@id"))
        $return3Y = $updateDbY + 70
        $cells.Add((New-ReturnEdgeXml -Id "${DiagramId}-r3" -X1 $lanes[4].X -Y1 $return3Y -X2 $lanes[3].X -Y2 $return3Y -Label 'success'))
        $return4Y = $return3Y + 40
        $cells.Add((New-ReturnEdgeXml -Id "${DiagramId}-r4" -X1 $lanes[3].X -Y1 $return4Y -X2 $lanes[2].X -Y2 $return4Y -Label 'updated DTO | null'))

        $cells.Add((New-VertexXml -Id "${DiagramId}-alt-label2" -Value "[else -- entity not found]" -Style $labelStyle -X 110 -Y ($altTopY + 150) -Width 300 -Height 24))
        $cells.Add((New-VertexXml -Id "${DiagramId}-alt-note2" -Value "(no DB update -> return not found)" -Style $labelStyle -X 620 -Y ($altTopY + 150) -Width 340 -Height 24))
    } elseif ($Endpoint.Verb -eq 'DELETE') {
        $cells.Add((New-VertexXml -Id "${DiagramId}-alt-label1" -Value "[entity exists]" -Style $labelStyle -X 110 -Y ($altTopY + 12) -Width 220 -Height 24))
        $step++
        $deleteServiceY = $altTopY + 70
        $cells.Add((New-FreeEdgeXml -Id "${DiagramId}-m$step" -X1 $lanes[2].X -Y1 $deleteServiceY -X2 $lanes[3].X -Y2 $deleteServiceY -Label "$step. DeleteAsync(id)"))
        $step++
        $deleteDbY = $deleteServiceY + 85
        $cells.Add((New-FreeEdgeXml -Id "${DiagramId}-m$step" -X1 $lanes[3].X -Y1 $deleteDbY -X2 $lanes[4].X -Y2 $deleteDbY -Label "$step. DELETE FROM $repoName WHERE Id=@id"))
        $return3Y = $deleteDbY + 70
        $cells.Add((New-ReturnEdgeXml -Id "${DiagramId}-r3" -X1 $lanes[4].X -Y1 $return3Y -X2 $lanes[3].X -Y2 $return3Y -Label 'success'))
        $return4Y = $return3Y + 40
        $cells.Add((New-ReturnEdgeXml -Id "${DiagramId}-r4" -X1 $lanes[3].X -Y1 $return4Y -X2 $lanes[2].X -Y2 $return4Y -Label 'deleted | 0'))
        $cells.Add((New-VertexXml -Id "${DiagramId}-alt-label2" -Value "[else -- entity not found]" -Style $labelStyle -X 110 -Y ($altTopY + 150) -Width 300 -Height 24))
    } elseif ($Endpoint.Verb -eq 'POST') {
        $cells.Add((New-VertexXml -Id "${DiagramId}-alt-label1" -Value "[valid request]" -Style $labelStyle -X 110 -Y ($altTopY + 12) -Width 220 -Height 24))
        $step++
        $insertY = $altTopY + 80
        $cells.Add((New-FreeEdgeXml -Id "${DiagramId}-m$step" -X1 $lanes[2].X -Y1 $insertY -X2 $lanes[3].X -Y2 $insertY -Label "$step. CreateAsync(entity)"))
        $step++
        $insertDbY = $insertY + 85
        $cells.Add((New-FreeEdgeXml -Id "${DiagramId}-m$step" -X1 $lanes[3].X -Y1 $insertDbY -X2 $lanes[4].X -Y2 $insertDbY -Label "$step. INSERT INTO $repoName VALUES (...)"))
        $return3Y = $insertDbY + 70
        $cells.Add((New-ReturnEdgeXml -Id "${DiagramId}-r3" -X1 $lanes[4].X -Y1 $return3Y -X2 $lanes[3].X -Y2 $return3Y -Label 'created'))
        $return4Y = $return3Y + 40
        $cells.Add((New-ReturnEdgeXml -Id "${DiagramId}-r4" -X1 $lanes[3].X -Y1 $return4Y -X2 $lanes[2].X -Y2 $return4Y -Label 'created DTO'))
    } else {
        $cells.Add((New-VertexXml -Id "${DiagramId}-alt-label1" -Value "[entity found]" -Style $labelStyle -X 110 -Y ($altTopY + 12) -Width 220 -Height 24))
        $cells.Add((New-VertexXml -Id "${DiagramId}-alt-label2" -Value "[else -- not found]" -Style $labelStyle -X 110 -Y ($altTopY + 100) -Width 240 -Height 24))
    }

    $finalY = $altTopY + $altHeight + 40
    $finalStatus = if ($Endpoint.Verb -eq 'POST') { '200 OK / 400 Bad Request' } elseif ($Endpoint.Verb -eq 'GET') { '200 OK / 404 Not Found' } else { '200 OK / 404 Not Found' }
    $cells.Add((New-ReturnEdgeXml -Id "${DiagramId}-r-final" -X1 $lanes[1].X -Y1 $finalY -X2 $lanes[0].X -Y2 $finalY -Label "$step. $finalStatus"))

    $content = ($cells -join "`n")
    $pageHeight = $finalY + 120

    return @"
  <diagram id="$DiagramId" name="$DiagramName" compressed="false">
    <mxGraphModel dx="1800" dy="1000" grid="1" gridSize="10" guides="1" tooltips="1" connect="1" arrows="1" fold="1" page="1" pageScale="1" pageWidth="1900" pageHeight="$pageHeight" math="0" shadow="0">
      <root>
$content
      </root>
    </mxGraphModel>
  </diagram>
"@
}

$implementationIndex = Get-ImplementationIndex
$serviceMetadata = Get-ServiceMetadata -ImplementationIndex $implementationIndex
$controllers = Get-ControllerMetadata -ImplementationIndex $implementationIndex
$diagrams = @()
$pageIndex = 1

foreach ($controller in $controllers) {
    foreach ($endpoint in $controller.Endpoints) {
        $safeName = "SEQ-$($endpoint.Verb)-$($controller.Controller)-$($endpoint.Action)-$pageIndex"
        $diagramId = ($safeName -replace '[^A-Za-z0-9\-_]', '-')
        $diagramName = "$($endpoint.Verb) $($controller.Controller).$($endpoint.Action)"
        $diagrams += Build-SequenceDiagram -Controller $controller -Endpoint $endpoint -ServiceMetadata $serviceMetadata -DiagramId $diagramId -DiagramName $diagramName
        $pageIndex++
    }
}

$xml = @"
<mxfile host="app.diagrams.net" modified="2026-04-20T21:20:00.000Z" agent="Cursor" version="24.7.17" type="device">
$($diagrams -join "`n")
</mxfile>
"@

[System.IO.File]::WriteAllText((Join-Path (Get-Location) 'docs\cleanopsai-api-sequence-diagram.drawio'), $xml, (New-Object System.Text.UTF8Encoding($false)))
Write-Output "Generated docs\cleanopsai-api-sequence-diagram.drawio"