using System;
using System.Collections.Generic;

namespace AkashaNavigator.Models.Common
{
    /// <summary>
    /// 错误类别枚举
    /// </summary>
    public enum ErrorCategory
    {
        /// <summary>
        /// 未知错误
        /// </summary>
        Unknown,

        /// <summary>
        /// 验证错误（输入参数不合法）
        /// </summary>
        Validation,

        /// <summary>
        /// 文件系统错误（IO操作失败）
        /// </summary>
        FileSystem,

        /// <summary>
        /// 网络错误（HTTP请求失败）
        /// </summary>
        Network,

        /// <summary>
        /// 权限错误（访问被拒绝）
        /// </summary>
        Permission,

        /// <summary>
        /// 插件错误（插件加载、执行失败）
        /// </summary>
        Plugin,

        /// <summary>
        /// 配置错误（配置文件无效）
        /// </summary>
        Configuration,

        /// <summary>
        /// 业务逻辑错误（违反业务规则）
        /// </summary>
        BusinessLogic
    }

    /// <summary>
    /// 统一的错误类型，包含错误分类、代码、消息和可选的异常
    /// </summary>
    public class Error
    {
        /// <summary>
        /// 错误类别
        /// </summary>
        public ErrorCategory Category { get; }

        /// <summary>
        /// 错误代码（用于程序识别）
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// 错误消息（开发者使用）
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 用户友好的错误消息（可选，显示给用户）
        /// </summary>
        public string? UserMessage { get; }

        /// <summary>
        /// 关联的异常（可选）
        /// </summary>
        public Exception? Exception { get; }

        /// <summary>
        /// 额外的元数据（可选）
        /// </summary>
        public Dictionary<string, object> Metadata { get; }

        /// <summary>
        /// 私有构造函数
        /// </summary>
        private Error(
            ErrorCategory category,
            string code,
            string message,
            string? userMessage = null,
            Exception? exception = null,
            Dictionary<string, object>? metadata = null)
        {
            Category = category;
            Code = code;
            Message = message;
            UserMessage = userMessage;
            Exception = exception;
            Metadata = metadata ?? new Dictionary<string, object>();
        }

        /// <summary>
        /// 创建文件系统错误
        /// </summary>
        public static Error FileSystem(string code, string message, Exception? ex = null, string? filePath = null)
        {
            var error = new Error(ErrorCategory.FileSystem, code, message, exception: ex);
            if (filePath != null)
            {
                error.Metadata["FilePath"] = filePath;
            }
            return error;
        }

        /// <summary>
        /// 创建网络错误
        /// </summary>
        public static Error Network(string code, string message, Exception? ex = null, string? url = null)
        {
            var error = new Error(ErrorCategory.Network, code, message, exception: ex);
            if (url != null)
            {
                error.Metadata["Url"] = url;
            }
            return error;
        }

        /// <summary>
        /// 创建验证错误
        /// </summary>
        public static Error Validation(string code, string message, string? userMessage = null)
        {
            return new Error(ErrorCategory.Validation, code, message, userMessage);
        }

        /// <summary>
        /// 创建权限错误
        /// </summary>
        public static Error Permission(string code, string message, string? userMessage = null, Exception? ex = null)
        {
            return new Error(ErrorCategory.Permission, code, message, userMessage, ex);
        }

        /// <summary>
        /// 创建插件错误
        /// </summary>
        public static Error Plugin(string code, string message, Exception? ex = null, string? pluginId = null)
        {
            var error = new Error(ErrorCategory.Plugin, code, message, exception: ex);
            if (pluginId != null)
            {
                error.Metadata["PluginId"] = pluginId;
            }
            return error;
        }

        /// <summary>
        /// 创建配置错误
        /// </summary>
        public static Error Configuration(string code, string message, Exception? ex = null)
        {
            return new Error(ErrorCategory.Configuration, code, message, exception: ex);
        }

        /// <summary>
        /// 创建业务逻辑错误
        /// </summary>
        public static Error BusinessLogic(string code, string message, string? userMessage = null)
        {
            return new Error(ErrorCategory.BusinessLogic, code, message, userMessage);
        }

        /// <summary>
        /// 创建未知错误
        /// </summary>
        public static Error Unknown(string code, string message, Exception? ex = null)
        {
            return new Error(ErrorCategory.Unknown, code, message, exception: ex);
        }

        /// <summary>
        /// 创建序列化错误（FileSystem的特殊类型）
        /// </summary>
        public static Error Serialization(string code, string message, Exception? ex = null, string? filePath = null)
        {
            var error = new Error(ErrorCategory.FileSystem, code, message, exception: ex);
            if (filePath != null)
            {
                error.Metadata["FilePath"] = filePath;
            }
            return error;
        }

        public override string ToString()
        {
            return $"[{Category}] {Code}: {Message}";
        }
    }
}
