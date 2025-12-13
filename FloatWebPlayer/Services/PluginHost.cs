using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FloatWebPlayer.Helpers;
using FloatWebPlayer.Models;
using FloatWebPlayer.Plugins;

namespace FloatWebPlayer.Services
{
    /// <summary>
    /// 插件宿主服务
    /// 负责插件的加载、执行和生命周期管理
    /// </summary>
    public class PluginHost : IDisposable
    {
        #region Singleton

        private static PluginHost? _instance;
        private static readonly object _lock = new();

        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static PluginHost Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new PluginHost();
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// 插件加载完成事件
        /// </summary>
        public event EventHandler<PluginContext>? PluginLoaded;

        /// <summary>
        /// 插件卸载事件
        /// </summary>
        public event EventHandler<string>? PluginUnloaded;

        #endregion


        #region Fields

        private readonly List<PluginContext> _loadedPlugins = new();
        private readonly Dictionary<string, PluginConfig> _pluginConfigs = new();
        private string? _currentProfileId;
        private bool _disposed;

        #endregion

        #region Properties

        /// <summary>
        /// 已加载的插件列表
        /// </summary>
        public IReadOnlyList<PluginContext> LoadedPlugins => _loadedPlugins.AsReadOnly();

        /// <summary>
        /// 当前 Profile ID
        /// </summary>
        public string? CurrentProfileId => _currentProfileId;

        #endregion

        #region Constructor

        private PluginHost()
        {
            // 私有构造函数，单例模式
        }

        /// <summary>
        /// 用于测试的内部构造函数
        /// </summary>
        internal PluginHost(bool forTesting)
        {
            // 测试用构造函数
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 加载指定 Profile 的所有插件
        /// </summary>
        /// <param name="profileId">Profile ID</param>
        public void LoadPluginsForProfile(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId))
                return;

            // 如果已有插件加载，先卸载
            if (_loadedPlugins.Count > 0)
            {
                UnloadAllPlugins();
            }

            _currentProfileId = profileId;

            // 获取插件目录
            var pluginsDir = GetPluginsDirectory(profileId);
            if (!Directory.Exists(pluginsDir))
            {
                Log($"插件目录不存在: {pluginsDir}");
                return;
            }

            // 遍历插件目录
            var pluginDirs = Directory.GetDirectories(pluginsDir);
            foreach (var pluginDir in pluginDirs)
            {
                LoadPlugin(pluginDir);
            }

            Log($"已加载 {_loadedPlugins.Count} 个插件 (Profile: {profileId})");
        }

        /// <summary>
        /// 卸载所有插件
        /// </summary>
        public void UnloadAllPlugins()
        {
            foreach (var plugin in _loadedPlugins.ToList())
            {
                UnloadPlugin(plugin);
            }

            _loadedPlugins.Clear();
            _pluginConfigs.Clear();
            _currentProfileId = null;

            Log("已卸载所有插件");
        }

        /// <summary>
        /// 启用或禁用插件
        /// </summary>
        /// <param name="pluginId">插件 ID</param>
        /// <param name="enabled">是否启用</param>
        public void SetPluginEnabled(string pluginId, bool enabled)
        {
            var plugin = _loadedPlugins.FirstOrDefault(p => p.PluginId == pluginId);
            if (plugin == null)
                return;

            if (plugin.IsEnabled == enabled)
                return;

            plugin.IsEnabled = enabled;

            // 更新配置
            if (_pluginConfigs.TryGetValue(pluginId, out var config))
            {
                config.Enabled = enabled;
                SavePluginConfig(config, plugin.PluginDirectory);
            }

            Log($"插件 {pluginId} 已{(enabled ? "启用" : "禁用")}");
        }

        /// <summary>
        /// 根据 ID 获取插件
        /// </summary>
        public PluginContext? GetPlugin(string pluginId)
        {
            return _loadedPlugins.FirstOrDefault(p => p.PluginId == pluginId);
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// 加载单个插件
        /// </summary>
        private void LoadPlugin(string pluginDir)
        {
            var manifestPath = Path.Combine(pluginDir, "plugin.json");
            
            // 加载清单
            var loadResult = PluginManifest.LoadFromFile(manifestPath);
            if (!loadResult.IsSuccess)
            {
                Log($"加载插件清单失败 ({pluginDir}): {loadResult.ErrorMessage}");
                return;
            }

            var manifest = loadResult.Manifest!;

            // 检查是否已加载同 ID 插件
            if (_loadedPlugins.Any(p => p.PluginId == manifest.Id))
            {
                Log($"插件 {manifest.Id} 已加载，跳过");
                return;
            }

            // 加载配置
            var configPath = Path.Combine(pluginDir, "config.json");
            var config = PluginConfig.LoadFromFile(configPath, manifest.Id!);
            
            // 应用默认配置
            config.ApplyDefaults(manifest.DefaultConfig);
            _pluginConfigs[manifest.Id!] = config;

            // 如果插件被禁用，跳过加载
            if (!config.Enabled)
            {
                Log($"插件 {manifest.Id} 已禁用，跳过加载");
                return;
            }

            // 创建插件上下文
            var context = new PluginContext(manifest, pluginDir)
            {
                IsEnabled = config.Enabled
            };

            // 加载脚本
            if (!context.LoadScript())
            {
                Log($"加载插件脚本失败 ({manifest.Id}): {context.LastError}");
                context.Dispose();
                return;
            }

            // 调用 onLoad
            // 注意：这里暂时不传入 API，后续任务会实现 PluginApi
            if (!context.CallOnLoad())
            {
                Log($"插件 {manifest.Id} onLoad 调用失败: {context.LastError}");
                // 即使 onLoad 失败，也保留插件（异常隔离）
            }

            _loadedPlugins.Add(context);
            PluginLoaded?.Invoke(this, context);

            Log($"插件 {manifest.Name} (v{manifest.Version}) 加载成功");
        }

        /// <summary>
        /// 卸载单个插件
        /// </summary>
        private void UnloadPlugin(PluginContext plugin)
        {
            var pluginId = plugin.PluginId;

            // 调用 onUnload
            plugin.CallOnUnload();

            // 释放资源
            plugin.Dispose();

            PluginUnloaded?.Invoke(this, pluginId);
            Log($"插件 {pluginId} 已卸载");
        }

        /// <summary>
        /// 获取 Profile 的插件目录
        /// </summary>
        private string GetPluginsDirectory(string profileId)
        {
            return Path.Combine(AppPaths.ProfilesDirectory, profileId, "plugins");
        }

        /// <summary>
        /// 保存插件配置
        /// </summary>
        private void SavePluginConfig(PluginConfig config, string pluginDir)
        {
            var configPath = Path.Combine(pluginDir, "config.json");
            config.SaveToFile(configPath);
        }

        /// <summary>
        /// 记录日志
        /// </summary>
        private void Log(string message)
        {
            Debug.WriteLine($"[PluginHost] {message}");
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                UnloadAllPlugins();
            }

            _disposed = true;
        }

        /// <summary>
        /// 析构函数
        /// </summary>
        ~PluginHost()
        {
            Dispose(false);
        }

        #endregion
    }
}
