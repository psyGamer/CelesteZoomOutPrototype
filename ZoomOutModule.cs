using System;
using System.Reflection;
using Monocle;

namespace Celeste.Mod.ZoomOut;

public class ZoomOutModule : EverestModule
{
    public static ZoomOutModule Instance { get; private set; }

    public const string LoggerTag = "ZoomOut";

    private static readonly MethodInfo m_UpdateMatrices = typeof(Camera).GetMethod("UpdateMatrices", BindingFlags.Instance | BindingFlags.NonPublic);

    private static float _gameScale = 1.0f;
    public static float GameScale
    {
        get => _gameScale;
        set {
            _gameScale = value;

            if (Celeste.Scene is Level level)
            {
                // Recreate the gameplay buffers to actually have more pixels
                GameplayBuffers.Create();

                level.Camera.Viewport.Width = GameWidth;
                level.Camera.Viewport.Height = GameHeight;
                m_UpdateMatrices.Invoke(level.Camera, null);

                // Recreate all backdrops since most of them don't handle the size changing at runtime very well
                var mapData = level.Session.MapData;
                level.Background.Backdrops = mapData.CreateBackdrops(mapData.Background);
                foreach (Backdrop backdrop in level.Background.Backdrops)
                {
                    backdrop.Renderer = level.Background;
                }
                level.Foreground.Backdrops = mapData.CreateBackdrops(mapData.Foreground);
                foreach (Backdrop backdrop2 in level.Foreground.Backdrops)
                {
                    backdrop2.Renderer = level.Foreground;
                }
            }
        }
    }

    public static int GameWidth  => (int)(Celeste.GameWidth * _gameScale);
    public static int GameHeight => (int)(Celeste.GameHeight * _gameScale);

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

    [Command("zoom", "")]
    private static void CmdZoom(float zoom)
    {
        GameScale = zoom;
    }
}