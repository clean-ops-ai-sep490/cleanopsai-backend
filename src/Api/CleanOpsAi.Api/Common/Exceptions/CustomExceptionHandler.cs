using CleanOpsAi.BuildingBlocks.Application.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CleanOpsAi.Api.Common.Exceptions;

public class CustomExceptionHandler : IExceptionHandler
{
	private readonly ILogger<CustomExceptionHandler> _logger;

	public CustomExceptionHandler(ILogger<CustomExceptionHandler> logger)
	{
		_logger = logger;
	}

	public async ValueTask<bool> TryHandleAsync(
		HttpContext context,
		Exception exception,
		CancellationToken cancellationToken)
	{
		_logger.LogError(
			exception,
			"Exception occurred: {Method} {Path} at {Time}",
			context.Request.Method,
			context.Request.Path,
			DateTime.UtcNow);

		var problemDetails = CreateProblemDetails(context, exception);

		context.Response.ContentType = "application/problem+json";
		context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

		await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

		return true;
	}

	private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
	{
		var problemDetails = new ProblemDetails
		{
			Instance = context.Request.Path,
			Title = exception.GetType().Name,
		};

		switch (exception)
		{
			case ValidationException fluentEx:
				problemDetails.Status = StatusCodes.Status400BadRequest;
				problemDetails.Detail = "One or more validation errors occurred.";
				problemDetails.Extensions["errors"] = fluentEx.Errors
					.GroupBy(e => e.PropertyName)
					.ToDictionary(
						g => g.Key,
						g => g.Select(e => e.ErrorMessage).ToArray()
					);
				break;

			case BadRequestException:
				problemDetails.Status = StatusCodes.Status400BadRequest;
				problemDetails.Detail = exception.Message;
				break;

			case NotFoundException:
				problemDetails.Status = StatusCodes.Status404NotFound;
				problemDetails.Detail = exception.Message;
				break;

			case ForbiddenException:
				problemDetails.Status = StatusCodes.Status403Forbidden;
				problemDetails.Detail = exception.Message;
				break;

			case InternalServerException:
			default:
				problemDetails.Status = StatusCodes.Status500InternalServerError;
				problemDetails.Detail = "An unexpected error occurred."; // Ẩn message thật
				break;
		}

		problemDetails.Extensions["traceId"] = context.TraceIdentifier;

		return problemDetails;
	}
}