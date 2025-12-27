using System;

namespace AkashaNavigator.Models.Common
{
    /// <summary>
    /// 文件系统操作结果（带文件路径）
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    public class FileSystemResult<T>
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
        /// 失败时的错误
        /// </summary>
        public Error? Error { get; }

        /// <summary>
        /// 文件路径
        /// </summary>
        public string? FilePath { get; }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private FileSystemResult(bool isSuccess, T? value, Error? error, string? filePath)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            FilePath = filePath;
        }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>成功结果</returns>
        public static FileSystemResult<T> Success(T value, string filePath)
        {
            return new FileSystemResult<T>(true, value, null, filePath);
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="error">错误</param>
        /// <returns>失败结果</returns>
        public static FileSystemResult<T> Failure(string filePath, Error error)
        {
            return new FileSystemResult<T>(false, default, error, filePath);
        }

        /// <summary>
        /// 隐式转换为Result<T>
        /// </summary>
        public static implicit operator Result<T>(FileSystemResult<T> fileSystemResult)
        {
            return fileSystemResult.IsSuccess
                ? Result<T>.Success(fileSystemResult.Value!)
                : Result<T>.Failure(fileSystemResult.Error!);
        }
    }

    /// <summary>
    /// 插件操作结果
    /// </summary>
    public class PluginResult
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
        /// 插件ID
        /// </summary>
        public string? PluginId { get; }

        /// <summary>
        /// 失败时的错误
        /// </summary>
        public Error? Error { get; }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private PluginResult(bool isSuccess, string? pluginId, Error? error)
        {
            IsSuccess = isSuccess;
            PluginId = pluginId;
            Error = error;
        }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <returns>成功结果</returns>
        public static PluginResult Success(string pluginId)
        {
            return new PluginResult(true, pluginId, null);
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <param name="error">错误</param>
        /// <returns>失败结果</returns>
        public static PluginResult Failure(string pluginId, Error error)
        {
            return new PluginResult(false, pluginId, error);
        }

        /// <summary>
        /// 创建失败结果（无插件ID）
        /// </summary>
        /// <param name="error">错误</param>
        /// <returns>失败结果</returns>
        public static PluginResult Failure(Error error)
        {
            return new PluginResult(false, null, error);
        }

        /// <summary>
        /// 隐式转换为Result
        /// </summary>
        public static implicit operator Result(PluginResult pluginResult)
        {
            return pluginResult.IsSuccess
                ? Result.Success()
                : Result.Failure(pluginResult.Error!);
        }
    }

    /// <summary>
    /// 插件操作结果（带返回值）
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    public class PluginResult<T>
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
        /// 插件ID
        /// </summary>
        public string? PluginId { get; }

        /// <summary>
        /// 成功时的值
        /// </summary>
        public T? Value { get; }

        /// <summary>
        /// 失败时的错误
        /// </summary>
        public Error? Error { get; }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private PluginResult(bool isSuccess, string? pluginId, T? value, Error? error)
        {
            IsSuccess = isSuccess;
            PluginId = pluginId;
            Value = value;
            Error = error;
        }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <param name="value">值</param>
        /// <returns>成功结果</returns>
        public static PluginResult<T> Success(string pluginId, T value)
        {
            return new PluginResult<T>(true, pluginId, value, null);
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="pluginId">插件ID</param>
        /// <param name="error">错误</param>
        /// <returns>失败结果</returns>
        public static PluginResult<T> Failure(string pluginId, Error error)
        {
            return new PluginResult<T>(false, pluginId, default, error);
        }

        /// <summary>
        /// 创建失败结果（无插件ID）
        /// </summary>
        /// <param name="error">错误</param>
        /// <returns>失败结果</returns>
        public static PluginResult<T> Failure(Error error)
        {
            return new PluginResult<T>(false, null, default, error);
        }

        /// <summary>
        /// 隐式转换为Result<T>
        /// </summary>
        public static implicit operator Result<T>(PluginResult<T> pluginResult)
        {
            return pluginResult.IsSuccess
                ? Result<T>.Success(pluginResult.Value!)
                : Result<T>.Failure(pluginResult.Error!);
        }
    }

    /// <summary>
    /// Profile操作结果
    /// </summary>
    public class ProfileResult
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
        /// Profile ID
        /// </summary>
        public string? ProfileId { get; }

        /// <summary>
        /// Profile名称
        /// </summary>
        public string? ProfileName { get; }

        /// <summary>
        /// 失败时的错误
        /// </summary>
        public Error? Error { get; }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private ProfileResult(bool isSuccess, string? profileId, string? profileName, Error? error)
        {
            IsSuccess = isSuccess;
            ProfileId = profileId;
            ProfileName = profileName;
            Error = error;
        }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        /// <param name="profileName">Profile名称</param>
        /// <returns>成功结果</returns>
        public static ProfileResult Success(string profileId, string profileName)
        {
            return new ProfileResult(true, profileId, profileName, null);
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="error">错误</param>
        /// <returns>失败结果</returns>
        public static ProfileResult Failure(Error error)
        {
            return new ProfileResult(false, null, null, error);
        }

        /// <summary>
        /// 隐式转换为Result<string>（返回ProfileId）
        /// </summary>
        public static implicit operator Result<string>(ProfileResult profileResult)
        {
            return profileResult.IsSuccess
                ? Result<string>.Success(profileResult.ProfileId!)
                : Result<string>.Failure(profileResult.Error!);
        }

        /// <summary>
        /// 隐式转换为Result
        /// </summary>
        public static implicit operator Result(ProfileResult profileResult)
        {
            return profileResult.IsSuccess
                ? Result.Success()
                : Result.Failure(profileResult.Error!);
        }
    }

    /// <summary>
    /// 网络操作结果
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    public class NetworkResult<T>
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
        /// 失败时的错误
        /// </summary>
        public Error? Error { get; }

        /// <summary>
        /// 请求的URL
        /// </summary>
        public string? Url { get; }

        /// <summary>
        /// HTTP状态码（如果适用）
        /// </summary>
        public int? StatusCode { get; }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private NetworkResult(bool isSuccess, T? value, Error? error, string? url, int? statusCode)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            Url = url;
            StatusCode = statusCode;
        }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="url">URL</param>
        /// <param name="statusCode">状态码</param>
        /// <returns>成功结果</returns>
        public static NetworkResult<T> Success(T value, string url, int statusCode = 200)
        {
            return new NetworkResult<T>(true, value, null, url, statusCode);
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="error">错误</param>
        /// <param name="statusCode">状态码</param>
        /// <returns>失败结果</returns>
        public static NetworkResult<T> Failure(string url, Error error, int? statusCode = null)
        {
            return new NetworkResult<T>(false, default, error, url, statusCode);
        }

        /// <summary>
        /// 隐式转换为Result<T>
        /// </summary>
        public static implicit operator Result<T>(NetworkResult<T> networkResult)
        {
            return networkResult.IsSuccess
                ? Result<T>.Success(networkResult.Value!)
                : Result<T>.Failure(networkResult.Error!);
        }
    }
}
