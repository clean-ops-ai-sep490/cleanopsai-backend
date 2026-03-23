namespace CleanOpsAi.BuildingBlocks.Application.Common
{
	public class Result
	{
		internal Result(bool succeeded, IEnumerable<string> errors)
		{
			Succeeded = succeeded;
			Errors = errors.ToArray();
		}

		public bool Succeeded { get; init; }

		public string[] Errors { get; init; }

		public static Result Success()
		{
			return new Result(true, Array.Empty<string>());
		}

		public static Result Failure(IEnumerable<string> errors)
		{
			return new Result(false, errors);
		}

		public static Result Failure(string error)
		{
			return new Result(false, new[] { error });
		}
	}

	public class Result<T> : Result
	{
		private Result(bool succeeded, T? value, IEnumerable<string> errors)
			: base(succeeded, errors)
		{
			Value = value;
		}

		public T? Value { get; }

		public static Result<T> Success(T value)
			=> new Result<T>(true, value, Array.Empty<string>());

		public static new Result<T> Failure(string error)
			=> new Result<T>(false, default, new[] { error });

		public static new Result<T> Failure(IEnumerable<string> errors)
			=> new Result<T>(false, default, errors);
	}
}
