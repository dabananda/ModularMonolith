using ModularMonolith.Shared.Common;

namespace ModularMonolith.Shared.Interfaces
{
    public interface IResult
    {
        bool IsSuccess { get; }
        string Message { get; }
        IEnumerable<string> Errors { get; }
        ErrorType? ErrorType { get; }

        static abstract IResult Failure(ErrorType errorType, string message = "Failed", IEnumerable<string>? errors = null);
    }
}
