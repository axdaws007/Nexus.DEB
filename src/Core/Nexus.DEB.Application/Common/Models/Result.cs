namespace Nexus.DEB.Application.Common.Models
{
    public class Result<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public List<ValidationError> Errors { get; private set; } = new();

        private Result() { }

        public static Result<T> Success(T data)
        {
            return new Result<T>
            {
                IsSuccess = true,
                Data = data,
                Errors = new List<ValidationError>()
            };
        }

        public static Result<T> Failure(params ValidationError[] errors)
        {
            return new Result<T>
            {
                IsSuccess = false,
                Data = default,
                Errors = errors.ToList()
            };
        }
    }
}
