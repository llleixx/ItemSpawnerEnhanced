using BepInEx;
using HarmonyLib;

namespace ItemSpawnerEnhanced;

[BepInPlugin(PluginGuid, PluginName, BuildInfo.Version)]
public sealed class Plugin : BaseUnityPlugin
{
    public const string PluginGuid = "com.github.lllei.ItemSpawnerEnhanced";
    public const string PluginName = "ItemSpawnerEnhanced";

    internal static Plugin? Instance { get; private set; }

    private Harmony? _harmony;
    private ItemSpawnerController? _controller;

    private void Awake()
    {
        Instance = this;
        var settings = new ModConfig(Config);
        _controller = new ItemSpawnerController(settings, Logger);
        _harmony = new Harmony(PluginGuid);

        if (!PatchInstaller.Install(_harmony, Logger))
            Logger.LogError("Item spawner UI is disabled because GUIManager.Start could not be patched.");

        Logger.LogInfo($"{PluginName} {BuildInfo.Version} loaded for PEAK 2.1.a baseline.");
    }

    private void Update() => _controller?.Tick();

    internal void Attach() => _controller?.Attach();

    private void OnDestroy()
    {
        _controller?.Dispose();
        _harmony?.UnpatchSelf();
        Instance = null;
    }
}
