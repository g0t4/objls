namespace PInvoke.Ntdll
{
	public class Result<T>
	{
		public string FailureReason;
		public bool Success;
		public T Value;

		public static Result<T> Failed(string failureReason = "")
		{
			return new Result<T>
			{
				Success = false,
				FailureReason = failureReason
			};
		}

		public static Result<T> Succeeded(T value)
		{
			return new Result<T>
			{
				Success = true,
				Value = value
			};
		}
	}
}