using ModularMonolith.Shared.Interfaces;

namespace ModularMonolith.Shared.Common
{
    public class Result<T> : IResult
    {
        public bool IsSuccess { get; init; }
        public T? Data { get; init; }
        public string Message { get; init; } = string.Empty;
        public IEnumerable<string> Errors { get; init; } = [];
        public ErrorType? ErrorType { get; init; }

        public static Result<T> Success(T data, string message = "Success")
        {
            return new Result<T>
            {
                IsSuccess = true,
                Data = data,
                Message = message
            };
        }

        public static Result<T> Failure(ErrorType errorType, string message = "Failed", IEnumerable<string>? errors = null)
        {
            return new Result<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors?.ToList() ?? [],
                ErrorType = errorType
            };
        }

        static IResult IResult.Failure(ErrorType errorType, string message, IEnumerable<string>? errors)
            => Failure(errorType, message, errors);
    }

    public class Result : Result<object>, IResult
    {
        public static Result Success(string message = "Success")
        {
            return new Result
            {
                IsSuccess = true,
                Message = message
            };
        }

        public static new Result Failure(ErrorType errorType, string message = "Failed", IEnumerable<string>? errors = null)
        {
            return new Result
            {
                IsSuccess = false,
                Message = message,
                Errors = errors?.ToList() ?? [],
                ErrorType = errorType
            };
        }

        static IResult IResult.Failure(ErrorType errorType, string message, IEnumerable<string>? errors)
            => Failure(errorType, message, errors);
    }
}
