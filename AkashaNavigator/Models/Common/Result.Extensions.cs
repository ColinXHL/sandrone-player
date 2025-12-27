using System;

namespace AkashaNavigator.Models.Common
{
    /// <summary>
    /// Result类型的扩展方法，提供函数式编程支持
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// 转换成功的值（类似Select）
        /// </summary>
        /// <typeparam name="T">原始类型</typeparam>
        /// <typeparam name="TNew">新类型</typeparam>
        /// <param name="result">结果</param>
        /// <param name="mapper">转换函数</param>
        /// <returns>转换后的结果</returns>
        public static Result<TNew> Map<T, TNew>(this Result<T> result, Func<T, TNew> mapper)
        {
            if (result.IsSuccess)
            {
                try
                {
                    return Result<TNew>.Success(mapper(result.Value!));
                }
                catch (Exception ex)
                {
                    return Result<TNew>.Failure(Error.Unknown("MAP_FAILED", "Map transformation failed", ex));
                }
            }
            return Result<TNew>.Failure(result.Error!);
        }

        /// <summary>
        /// 链式调用（类似SelectMany）
        /// </summary>
        /// <typeparam name="T">原始类型</typeparam>
        /// <typeparam name="TNew">新类型</typeparam>
        /// <param name="result">结果</param>
        /// <param name="binder">绑定函数，返回新的Result</param>
        /// <returns>绑定后的结果</returns>
        public static Result<TNew> Bind<T, TNew>(this Result<T> result, Func<T, Result<TNew>> binder)
        {
            if (result.IsSuccess)
            {
                try
                {
                    return binder(result.Value!);
                }
                catch (Exception ex)
                {
                    return Result<TNew>.Failure(Error.Unknown("BIND_FAILED", "Bind operation failed", ex));
                }
            }
            return Result<TNew>.Failure(result.Error!);
        }

        /// <summary>
        /// 提供失败时的回退值
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="result">结果</param>
        /// <param name="fallback">回退函数</param>
        /// <returns>原始结果或回退结果</returns>
        public static Result<T> OrElse<T>(this Result<T> result, Func<Result<T>> fallback)
        {
            return result.IsSuccess ? result : fallback();
        }

        /// <summary>
        /// 获取值或默认值
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="result">结果</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>值或默认值</returns>
        public static T GetValueOrDefault<T>(this Result<T> result, T defaultValue)
        {
            return result.IsSuccess ? result.Value! : defaultValue;
        }

        /// <summary>
        /// 获取值或计算默认值
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="result">结果</param>
        /// <param name="defaultValueProvider">默认值提供者</param>
        /// <returns>值或计算出的默认值</returns>
        public static T GetValueOrDefault<T>(this Result<T> result, Func<T> defaultValueProvider)
        {
            return result.IsSuccess ? result.Value! : defaultValueProvider();
        }

        /// <summary>
        /// 模式匹配：根据成功或失败执行不同操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="result">结果</param>
        /// <param name="onSuccess">成功时的处理</param>
        /// <param name="onFailure">失败时的处理</param>
        /// <returns>处理结果</returns>
        public static TResult Match<T, TResult>(
            this Result<T> result,
            Func<T, TResult> onSuccess,
            Func<Error, TResult> onFailure)
        {
            return result.IsSuccess
                ? onSuccess(result.Value!)
                : onFailure(result.Error!);
        }

        /// <summary>
        /// 模式匹配（无返回值）：根据成功或失败执行不同操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="result">结果</param>
        /// <param name="onSuccess">成功时的处理</param>
        /// <param name="onFailure">失败时的处理</param>
        public static void Match<T>(
            this Result<T> result,
            Action<T> onSuccess,
            Action<Error> onFailure)
        {
            if (result.IsSuccess)
            {
                onSuccess(result.Value!);
            }
            else
            {
                onFailure(result.Error!);
            }
        }

        /// <summary>
        /// 无返回值Result的模式匹配
        /// </summary>
        /// <typeparam name="TResult">结果类型</typeparam>
        /// <param name="result">结果</param>
        /// <param name="onSuccess">成功时的处理</param>
        /// <param name="onFailure">失败时的处理</param>
        /// <returns>处理结果</returns>
        public static TResult Match<TResult>(
            this Result result,
            Func<TResult> onSuccess,
            Func<Error, TResult> onFailure)
        {
            return result.IsSuccess
                ? onSuccess()
                : onFailure(result.Error!);
        }

        /// <summary>
        /// 无返回值Result的模式匹配（无返回值）
        /// </summary>
        /// <param name="result">结果</param>
        /// <param name="onSuccess">成功时的处理</param>
        /// <param name="onFailure">失败时的处理</param>
        public static void Match(
            this Result result,
            Action onSuccess,
            Action<Error> onFailure)
        {
            if (result.IsSuccess)
            {
                onSuccess();
            }
            else
            {
                onFailure(result.Error!);
            }
        }

        /// <summary>
        /// 成功时执行操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="result">结果</param>
        /// <param name="action">要执行的操作</param>
        /// <returns>原始结果</returns>
        public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
        {
            if (result.IsSuccess)
            {
                action(result.Value!);
            }
            return result;
        }

        /// <summary>
        /// 失败时执行操作
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="result">结果</param>
        /// <param name="action">要执行的操作</param>
        /// <returns>原始结果</returns>
        public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action)
        {
            if (result.IsFailure)
            {
                action(result.Error!);
            }
            return result;
        }

        /// <summary>
        /// 成功时执行操作（无返回值Result）
        /// </summary>
        /// <param name="result">结果</param>
        /// <param name="action">要执行的操作</param>
        /// <returns>原始结果</returns>
        public static Result OnSuccess(this Result result, Action action)
        {
            if (result.IsSuccess)
            {
                action();
            }
            return result;
        }

        /// <summary>
        /// 失败时执行操作（无返回值Result）
        /// </summary>
        /// <param name="result">结果</param>
        /// <param name="action">要执行的操作</param>
        /// <returns>原始结果</returns>
        public static Result OnFailure(this Result result, Action<Error> action)
        {
            if (result.IsFailure)
            {
                action(result.Error!);
            }
            return result;
        }

        /// <summary>
        /// 将Result<T>转换为Result（丢弃值）
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="result">结果</param>
        /// <returns>无返回值的结果</returns>
        public static Result ToResult<T>(this Result<T> result)
        {
            return result.IsSuccess
                ? Result.Success()
                : Result.Failure(result.Error!);
        }

        /// <summary>
        /// 将Result转换为Result<T>（提供默认值）
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="result">结果</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>带返回值的结果</returns>
        public static Result<T> WithValue<T>(this Result result, T defaultValue)
        {
            return result.IsSuccess
                ? Result<T>.Success(defaultValue)
                : Result<T>.Failure(result.Error!);
        }
    }
}
