using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using Celeste;

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

    public ZoomOutModule()
    {
        Instance = this;
    }

    public override void Load()
    {
        GameSizeUnInliner.Load();
    }

    public override void Unload()
    {
        GameSizeUnInliner.Unload();
    }
}