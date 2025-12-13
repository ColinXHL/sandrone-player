using System;
using System.IO;
using System.Text.Json;
using FloatWebPlayer.Models;
using FloatWebPlayer.Plugins;
using FloatWebPlayer.Services;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace FloatWebPlayer.Tests
{
    /// <summary>
    /// PluginHost 属性测试
    /// </summary>
    public class PluginHostTests : IDisposable
    {
        private readonly string _tempProfilesDir;
        private readonly string _testProfileId = "test-profile";

        public PluginHostTests()
        {
            _tempProfilesDir = Path.Combine(Path.GetTempPath(), $"plugin_host_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempProfilesDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempProfilesDir))
            {
                try
                {
                    Directory.Delete(_tempProfilesDir, true);
                }
                catch
                {
                    // 忽略清理错误
                }
            }
        }

        /// <summary>
        /// 创建测试用的插件目录和文件
        /// </summary>
        private string CreateTestPlugin(string pluginId, string jsCode, bool enabled = true)
        {
            var pluginDir = Path.Combine(_tempProfilesDir, _testProfileId, "plugins", pluginId);
            Directory.CreateDirectory(pluginDir);

            // 创建 plugin.json
            var manifest = new
            {
                id = pluginId,
                name = $"Test Plugin {pluginId}",
                version = "1.0.0",
                main = "main.js"
            };
            File.WriteAllText(
                Path.Combine(pluginDir, "plugin.json"),
                JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true })
            );

            // 创建 main.js
            File.WriteAllText(Path.Combine(pluginDir, "main.js"), jsCode);

            // 创建 config.json（如果需要禁用）
            if (!enabled)
            {
                var config = new { pluginId, enabled = false, settings = new { } };
                File.WriteAllText(
                    Path.Combine(pluginDir, "config.json"),
                    JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true })
                );
            }

            return pluginDir;
        }


        #region Property 3: 生命周期回调

        /// <summary>
        /// **Feature: game-plugin-system, Property 3: 生命周期回调**
        /// *对于任意*有效插件，加载时 onLoad 函数应被调用，卸载时 onUnload 函数应被调用
        /// **Validates: Requirements 5.1, 5.2**
        /// </summary>
        [Property(MaxTest = 50)]
        public Property LifecycleCallbacks_ShouldBeCalled(PositiveInt pluginCount)
        {
            // 限制插件数量在合理范围内
            var count = Math.Min(pluginCount.Get % 5 + 1, 5);

            // 为每次测试创建唯一的 profile ID
            var uniqueProfileId = $"profile-{Guid.NewGuid():N}";

            // 创建多个插件
            for (int i = 0; i < count; i++)
            {
                var jsCode = @"
function onLoad() {
    // onLoad called
}

function onUnload() {
    // onUnload called
}
";
                CreateTestPluginForProfile(uniqueProfileId, $"plugin-{i}", jsCode);
            }

            // 创建 PluginHost 实例（使用测试构造函数）
            var host = new TestablePluginHost(_tempProfilesDir);

            // 加载插件
            host.LoadPluginsForProfile(uniqueProfileId);

            // 验证所有插件都已加载
            var allLoaded = host.LoadedPlugins.Count == count;

            // 验证 onLoad 被调用（通过检查 IsLoaded 状态）
            var allOnLoadCalled = host.LoadedPlugins.All(p => p.IsLoaded);

            // 卸载所有插件
            host.UnloadAllPlugins();

            // 验证所有插件都已卸载
            var allUnloaded = host.LoadedPlugins.Count == 0;

            host.Dispose();

            return (allLoaded && allOnLoadCalled && allUnloaded)
                .Label($"Count: {count}, Loaded: {allLoaded} ({host.LoadedPlugins.Count}), OnLoad called: {allOnLoadCalled}, Unloaded: {allUnloaded}");
        }

        /// <summary>
        /// **Feature: game-plugin-system, Property 3: 生命周期回调（单插件）**
        /// *对于任意*有效插件，生命周期顺序应为：LoadScript -> onLoad -> onUnload -> Dispose
        /// **Validates: Requirements 5.1, 5.2**
        /// </summary>
        [Property(MaxTest = 100)]
        public Property LifecycleOrder_ShouldBeCorrect(PositiveInt pluginIndex)
        {
            // 使用预定义的有效插件 ID 列表
            var validPluginIds = new[] { "test-plugin", "my-plugin", "sample", "demo", "example" };
            var pluginId = validPluginIds[pluginIndex.Get % validPluginIds.Length];

            var lifecycleLog = new System.Collections.Generic.List<string>();

            var jsCode = @"
function onLoad() {
    // onLoad called
}

function onUnload() {
    // onUnload called
}
";
            // 为每次测试创建唯一的 profile ID
            var uniqueProfileId = $"profile-{Guid.NewGuid():N}";
            var pluginDir = CreateTestPluginForProfile(uniqueProfileId, pluginId, jsCode);

            // 加载清单
            var manifestPath = Path.Combine(pluginDir, "plugin.json");
            var loadResult = PluginManifest.LoadFromFile(manifestPath);
            
            if (!loadResult.IsSuccess)
                return false.ToProperty().Label("Failed to load manifest");

            var context = new PluginContext(loadResult.Manifest!, pluginDir);

            // 1. LoadScript
            var scriptLoaded = context.LoadScript();
            if (scriptLoaded) lifecycleLog.Add("LoadScript");

            // 2. onLoad
            var onLoadCalled = context.CallOnLoad();
            if (onLoadCalled && context.IsLoaded) lifecycleLog.Add("onLoad");

            // 3. onUnload
            var onUnloadCalled = context.CallOnUnload();
            if (onUnloadCalled && !context.IsLoaded) lifecycleLog.Add("onUnload");

            // 4. Dispose
            context.Dispose();
            lifecycleLog.Add("Dispose");

            var correctOrder = lifecycleLog.Count == 4 &&
                               lifecycleLog[0] == "LoadScript" &&
                               lifecycleLog[1] == "onLoad" &&
                               lifecycleLog[2] == "onUnload" &&
                               lifecycleLog[3] == "Dispose";

            return correctOrder
                .Label($"Lifecycle: {string.Join(" -> ", lifecycleLog)}");
        }

        /// <summary>
        /// 创建测试用的插件目录和文件（指定 profile）
        /// </summary>
        private string CreateTestPluginForProfile(string profileId, string pluginId, string jsCode, bool enabled = true)
        {
            var pluginDir = Path.Combine(_tempProfilesDir, profileId, "plugins", pluginId);
            Directory.CreateDirectory(pluginDir);

            // 创建 plugin.json
            var manifest = new
            {
                id = pluginId,
                name = $"Test Plugin {pluginId}",
                version = "1.0.0",
                main = "main.js"
            };
            File.WriteAllText(
                Path.Combine(pluginDir, "plugin.json"),
                JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true })
            );

            // 创建 main.js
            File.WriteAllText(Path.Combine(pluginDir, "main.js"), jsCode);

            // 创建 config.json（如果需要禁用）
            if (!enabled)
            {
                var config = new { pluginId, enabled = false, settings = new { } };
                File.WriteAllText(
                    Path.Combine(pluginDir, "config.json"),
                    JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true })
                );
            }

            return pluginDir;
        }

        #endregion


        #region Unit Tests

        /// <summary>
        /// 加载不存在的 Profile 应该不崩溃
        /// </summary>
        [Fact]
        public void LoadNonExistentProfile_ShouldNotCrash()
        {
            var host = new TestablePluginHost(_tempProfilesDir);

            // 不应该抛出异常
            host.LoadPluginsForProfile("non-existent-profile");

            Assert.Empty(host.LoadedPlugins);

            host.Dispose();
        }

        /// <summary>
        /// 禁用的插件不应该被加载
        /// </summary>
        [Fact]
        public void DisabledPlugin_ShouldNotBeLoaded()
        {
            var jsCode = "function onLoad() {} function onUnload() {}";
            CreateTestPlugin("disabled-plugin", jsCode, enabled: false);
            CreateTestPlugin("enabled-plugin", jsCode, enabled: true);

            var host = new TestablePluginHost(_tempProfilesDir);
            host.LoadPluginsForProfile(_testProfileId);

            // 只有启用的插件应该被加载
            Assert.Single(host.LoadedPlugins);
            Assert.Equal("enabled-plugin", host.LoadedPlugins[0].PluginId);

            host.Dispose();
        }

        /// <summary>
        /// SetPluginEnabled 应该正确更新状态
        /// </summary>
        [Fact]
        public void SetPluginEnabled_ShouldUpdateState()
        {
            var jsCode = "function onLoad() {} function onUnload() {}";
            CreateTestPlugin("test-plugin", jsCode);

            var host = new TestablePluginHost(_tempProfilesDir);
            host.LoadPluginsForProfile(_testProfileId);

            var plugin = host.GetPlugin("test-plugin");
            Assert.NotNull(plugin);
            Assert.True(plugin.IsEnabled);

            host.SetPluginEnabled("test-plugin", false);
            Assert.False(plugin.IsEnabled);

            host.SetPluginEnabled("test-plugin", true);
            Assert.True(plugin.IsEnabled);

            host.Dispose();
        }

        /// <summary>
        /// 切换 Profile 应该卸载旧插件并加载新插件
        /// </summary>
        [Fact]
        public void SwitchProfile_ShouldReloadPlugins()
        {
            // 创建两个 Profile 的插件
            var jsCode = "function onLoad() {} function onUnload() {}";
            
            // Profile 1
            var profile1Dir = Path.Combine(_tempProfilesDir, "profile1", "plugins", "plugin1");
            Directory.CreateDirectory(profile1Dir);
            File.WriteAllText(Path.Combine(profile1Dir, "plugin.json"), 
                JsonSerializer.Serialize(new { id = "plugin1", name = "Plugin 1", version = "1.0.0", main = "main.js" }));
            File.WriteAllText(Path.Combine(profile1Dir, "main.js"), jsCode);

            // Profile 2
            var profile2Dir = Path.Combine(_tempProfilesDir, "profile2", "plugins", "plugin2");
            Directory.CreateDirectory(profile2Dir);
            File.WriteAllText(Path.Combine(profile2Dir, "plugin.json"), 
                JsonSerializer.Serialize(new { id = "plugin2", name = "Plugin 2", version = "1.0.0", main = "main.js" }));
            File.WriteAllText(Path.Combine(profile2Dir, "main.js"), jsCode);

            var host = new TestablePluginHost(_tempProfilesDir);

            // 加载 Profile 1
            host.LoadPluginsForProfile("profile1");
            Assert.Single(host.LoadedPlugins);
            Assert.Equal("plugin1", host.LoadedPlugins[0].PluginId);

            // 切换到 Profile 2
            host.LoadPluginsForProfile("profile2");
            Assert.Single(host.LoadedPlugins);
            Assert.Equal("plugin2", host.LoadedPlugins[0].PluginId);

            host.Dispose();
        }

        #endregion
    }

    /// <summary>
    /// 可测试的 PluginHost（允许自定义 Profiles 目录）
    /// </summary>
    internal class TestablePluginHost : IDisposable
    {
        private readonly List<PluginContext> _loadedPlugins = new();
        private readonly Dictionary<string, PluginConfig> _pluginConfigs = new();
        private readonly string _profilesDirectory;
        private string? _currentProfileId;
        private bool _disposed;

        public IReadOnlyList<PluginContext> LoadedPlugins => _loadedPlugins.AsReadOnly();
        public string? CurrentProfileId => _currentProfileId;

        public TestablePluginHost(string profilesDirectory)
        {
            _profilesDirectory = profilesDirectory;
        }

        public void LoadPluginsForProfile(string profileId)
        {
            if (string.IsNullOrWhiteSpace(profileId))
                return;

            if (_loadedPlugins.Count > 0)
            {
                UnloadAllPlugins();
            }

            _currentProfileId = profileId;

            var pluginsDir = Path.Combine(_profilesDirectory, profileId, "plugins");
            if (!Directory.Exists(pluginsDir))
                return;

            var pluginDirs = Directory.GetDirectories(pluginsDir);
            foreach (var pluginDir in pluginDirs)
            {
                LoadPlugin(pluginDir);
            }
        }

        public void UnloadAllPlugins()
        {
            foreach (var plugin in _loadedPlugins.ToList())
            {
                plugin.CallOnUnload();
                plugin.Dispose();
            }

            _loadedPlugins.Clear();
            _pluginConfigs.Clear();
            _currentProfileId = null;
        }

        public void SetPluginEnabled(string pluginId, bool enabled)
        {
            var plugin = _loadedPlugins.FirstOrDefault(p => p.PluginId == pluginId);
            if (plugin == null)
                return;

            plugin.IsEnabled = enabled;

            if (_pluginConfigs.TryGetValue(pluginId, out var config))
            {
                config.Enabled = enabled;
            }
        }

        public PluginContext? GetPlugin(string pluginId)
        {
            return _loadedPlugins.FirstOrDefault(p => p.PluginId == pluginId);
        }

        private void LoadPlugin(string pluginDir)
        {
            var manifestPath = Path.Combine(pluginDir, "plugin.json");
            var loadResult = PluginManifest.LoadFromFile(manifestPath);
            if (!loadResult.IsSuccess)
                return;

            var manifest = loadResult.Manifest!;

            if (_loadedPlugins.Any(p => p.PluginId == manifest.Id))
                return;

            var configPath = Path.Combine(pluginDir, "config.json");
            var config = PluginConfig.LoadFromFile(configPath, manifest.Id!);
            config.ApplyDefaults(manifest.DefaultConfig);
            _pluginConfigs[manifest.Id!] = config;

            if (!config.Enabled)
                return;

            var context = new PluginContext(manifest, pluginDir)
            {
                IsEnabled = config.Enabled
            };

            if (!context.LoadScript())
            {
                context.Dispose();
                return;
            }

            context.CallOnLoad();
            _loadedPlugins.Add(context);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            UnloadAllPlugins();
            _disposed = true;
        }
    }
}
