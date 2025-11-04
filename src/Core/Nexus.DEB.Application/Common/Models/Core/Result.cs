namespace Nexus.DEB.Application.Common.Models
{
    /// <summary>
    /// Represents the result of an operation.
    /// Base class for both simple results and results with data.
    /// </summary>
    public class Result
    {
        public bool IsSuccess { get; protected set; }
        public List<ValidationError> Errors { get; protected set; } = new();

        protected Result() { }

        protected Result(bool isSuccess, List<ValidationError> errors)
        {
            IsSuccess = isSuccess;
            Errors = errors;
        }

        public static Result Success()
        {
            return new Result
            {
                IsSuccess = true,
                Errors = new List<ValidationError>()
            };
        }

        public static Result Failure(params ValidationError[] errors)
        {
            return new Result
            {
                IsSuccess = false,
                Errors = errors.ToList()
            };
        }

        public static Result Failure(IEnumerable<ValidationError> errors)
        {
            return new Result
            {
                IsSuccess = false,
                Errors = errors.ToList()
            };
        }

        /// <summary>
        /// Convenience method for creating a failure with a simple message
        /// </summary>
        public static Result Failure(string message, string? code = null, string? field = null)
        {
            return Failure(new ValidationError
            {
                Message = message,
                Code = code,
                Field = field
            });
        }
    }

    /// <summary>
    /// Represents the result of an operation with a return value.
    /// </summary>
    public class Result<T> : Result
    {
        public T? Data { get; private set; }

        private Result() : base() { }

        private Result(bool isSuccess, T? data, List<ValidationError> errors)
            : base(isSuccess, errors)
        {
            Data = data;
        }

        public static Result<T> Success(T data)
        {
            return new Result<T>
            {
                IsSuccess = true,
                Data = data,
                Errors = new List<ValidationError>()
            };
        }

        public new static Result<T> Failure(params ValidationError[] errors)
        {
            return new Result<T>
            {
                IsSuccess = false,
                Data = default,
                Errors = errors.ToList()
            };
        }

        public new static Result<T> Failure(IEnumerable<ValidationError> errors)
        {
            return new Result<T>
            {
                IsSuccess = false,
                Data = default,
                Errors = errors.ToList()
            };
        }

        /// <summary>
        /// Convenience method for creating a failure with a simple message
        /// </summary>
        public new static Result<T> Failure(string message, string? code = null, string? field = null)
        {
            return Failure(new ValidationError
            {
                Message = message,
                Code = code,
                Field = field
            });
        }
    }
}
