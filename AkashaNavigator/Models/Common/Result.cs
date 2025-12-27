using System;

namespace AkashaNavigator.Models.Common
{
    /// <summary>
    /// 通用的操作结果类型
    /// </summary>
    /// <typeparam name="T">成功时的返回值类型</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 是否失败
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// 成功时的值
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// 失败时的错误（使用新的Error类型）
        /// </summary>
        public Error? Error { get; }

        /// <summary>
        /// 失败时的错误信息（向后兼容）
        /// </summary>
        public string? ErrorMessage => Error?.Message;

        /// <summary>
        /// 失败时的异常（向后兼容）
        /// </summary>
        public Exception? Exception => Error?.Exception;

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static Result<T> Success(T value) =>
            new Result<T>(true, value, null);

        /// <summary>
        /// 创建失败结果（Error对象）
        /// </summary>
        public static Result<T> Failure(Error error) =>
            new Result<T>(false, default, error);

        /// <summary>
        /// 创建失败结果（错误信息，向后兼容）
        /// </summary>
        public static Result<T> Failure(string error) =>
            new Result<T>(false, default, Error.Unknown("LEGACY", error));

        /// <summary>
        /// 创建失败结果（异常，向后兼容）
        /// </summary>
        public static Result<T> Failure(Exception ex) =>
            new Result<T>(false, default, Error.Unknown("EXCEPTION", ex.Message, ex));

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private Result(bool isSuccess, T? value, Error? error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        /// <summary>
        /// 隐式转换：从 T 转换为 Result<T>
        /// </summary>
        public static implicit operator Result<T>(T value) => Success(value);
    }

    /// <summary>
    /// 无返回值的操作结果
    /// </summary>
    public class Result
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; }

        /// <summary>
        /// 是否失败
        /// </summary>
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// 失败时的错误（使用新的Error类型）
        /// </summary>
        public Error? Error { get; }

        /// <summary>
        /// 失败时的错误信息（向后兼容）
        /// </summary>
        public string? ErrorMessage => Error?.Message;

        /// <summary>
        /// 失败时的异常（向后兼容）
        /// </summary>
        public Exception? Exception => Error?.Exception;

        /// <summary>
        /// 创建成功结果
        /// </summary>
        public static Result Success() =>
            new Result(true, null);

        /// <summary>
        /// 创建失败结果（Error对象）
        /// </summary>
        public static Result Failure(Error error) =>
            new Result(false, error);

        /// <summary>
        /// 创建失败结果（错误信息，向后兼容）
        /// </summary>
        public static Result Failure(string error) =>
            new Result(false, Error.Unknown("LEGACY", error));

        /// <summary>
        /// 创建失败结果（异常，向后兼容）
        /// </summary>
        public static Result Failure(Exception ex) =>
            new Result(false, Error.Unknown("EXCEPTION", ex.Message, ex));

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private Result(bool isSuccess, Error? error)
        {
            IsSuccess = isSuccess;
            Error = error;
        }
    }
}
