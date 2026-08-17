using System;
using BepInEx;
using ItemSpawnerEnhanced.Api;

namespace ItemSpawnerEnhanced.ChineseSearch;

[BepInPlugin(PluginGuid, PluginName, BuildInfo.Version)]
[BepInDependency(
    ItemSpawnerEnhanced.Plugin.PluginGuid,
    BepInDependency.DependencyFlags.HardDependency)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.github.lllei.ItemSpawnerEnhanced.ChineseSearch";
    public const string PluginName = "ItemSpawnerEnhanced Chinese Search";

    private IDisposable? _providerRegistration;

    private void Awake()
    {
        try
        {
            ChinesePinyinAliasProvider.WarmUp();
        }
        catch (Exception exception)
        {
            Logger.LogWarning($"TinyPinyin warm-up failed; aliases will initialize on first use: {exception}");
        }
        _providerRegistration = SearchAliasRegistry.Register(new ChinesePinyinAliasProvider());
        Logger.LogInfo($"{PluginName} {BuildInfo.Version} loaded.");
    }

    private void OnDestroy()
    {
        _providerRegistration?.Dispose();
    }
}
