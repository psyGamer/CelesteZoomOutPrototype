using System;

namespace Celeste.Mod.ZoomOut;

public class ZoomOutModule : EverestModule
{
    public static ZoomOutModule Instance { get; private set; }

    public const string LoggerTag = "ZoomOut";

    public static float GameScale = 2.0f;
    public static int GameWidth = (int)(Celeste.GameWidth * GameScale);
    public static int GameHeight = (int)(Celeste.GameHeight * GameScale);

    public override Type SessionType => typeof(ZoomOutSession);
    public static ZoomOutSession Session => (ZoomOutSession) Instance._Session;

    private static readonly EverestModuleMetadata _FrostHelperModule = new EverestModuleMetadata()
    {
        Name = "FrostHelper",
        Version = new Version(1, 41, 0)
    };
    private bool _FrostHelperLoaded = false;

    public ZoomOutModule()
    {
        Instance = this;
    }

    public override void Load()
    {
        _FrostHelperLoaded = Everest.Loader.DependencyLoaded(_FrostHelperModule);

        GameSizeUnInliner.Load();
        if (_FrostHelperLoaded) GameSizeUnInliner.Load_FrostHelper();
    }

    public override void Unload()
    {
        GameSizeUnInliner.Unload();
        if (_FrostHelperLoaded) GameSizeUnInliner.Unload_FrostHelper();
    }
}