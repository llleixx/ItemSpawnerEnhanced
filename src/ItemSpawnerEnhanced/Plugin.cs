using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ItemSpawnerEnhanced.Api;
using ItemSpawnerEnhanced.Core;

namespace ItemSpawnerEnhanced;

[BepInPlugin(PluginGuid, PluginName, BuildInfo.Version)]
[BepInIncompatibility(OriginalPluginGuid)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.github.lllei.ItemSpawnerEnhanced";
    public const string PluginName = "ItemSpawnerEnhanced";
    public const string OriginalPluginGuid = "com.quackandcheese.ItemSpawner";

    internal static Plugin? Instance { get; private set; }
    internal static ManualLogSource Log => Instance!.Logger;

    private Harmony? _harmony;
    private ItemSpawnerController? _controller;
    private IDisposable? _chineseProviderRegistration;

    private void Awake()
    {
        Instance = this;
        var settings = new ModConfig(Config);
        _controller = new ItemSpawnerController(settings, Logger);
        _chineseProviderRegistration = SearchAliasRegistry.Register(new ChinesePinyinAliasProvider());
        _harmony = new Harmony(PluginGuid);

        if (!PatchInstaller.Install(_harmony, Logger))
            Logger.LogError("Item spawner UI is disabled because GUIManager.Start could not be patched.");

        Logger.LogInfo($"{PluginName} {BuildInfo.Version} loaded for PEAK 2.1.a baseline.");
    }

    private void Update() => _controller?.Tick();

    internal void Attach(GUIManager guiManager) => _controller?.Attach(guiManager);

    private void OnDestroy()
    {
        _controller?.Dispose();
        _chineseProviderRegistration?.Dispose();
        _harmony?.UnpatchSelf();
        Instance = null;
    }
}

