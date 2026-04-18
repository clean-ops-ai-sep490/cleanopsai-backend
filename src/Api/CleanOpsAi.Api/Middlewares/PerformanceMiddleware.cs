using System.Diagnostics;

namespace CleanOpsAi.Api.Middlewares
{
	public class PerformanceMiddleware(ILogger<PerformanceMiddleware> logger) : IMiddleware
	{
		private readonly ILogger<PerformanceMiddleware> _logger = logger;

		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			var stopwatch = Stopwatch.StartNew();

			await next(context);

			stopwatch.Stop();

			_logger.LogInformation(
				"Request {Method} {Path} took {Elapsed} ms",
				context.Request.Method,
				context.Request.Path,
				stopwatch.ElapsedMilliseconds);
		}
	}
}
