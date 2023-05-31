using System;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Monocle;

namespace Celeste.Mod.ZoomOut;

public static class GameSizeUnInliner
{
    private static IDetour Player_get_CameraTarget_hook;
    private static IDetour Parallax_orig_Render_hook;

    private static IDetour FrostHelper_CustomSpinner_InView_hook;
    private static IDetour FrostHelper_HDlesteCompat_get_Scale_hook;

    public static void Load()
    {
        /// Core-Mechanic Hooks ///
        IL.Celeste.Audio.Position += Audio_Position;
        
        IL.Celeste.BloomRenderer.Apply += BloomRenderer_Apply;

        IL.Celeste.GameplayBuffers.Create += GameplayBuffers_Create;
        IL.Celeste.GameplayRenderer.ctor += GameplayRenderer_ctor;

        IL.Celeste.Level.Render += Level_Render;
        IL.Celeste.Level.ResetZoom += Level_ResetZoom;
        IL.Celeste.Level.EnforceBounds += Level_EnforceBounds;
        IL.Celeste.Level.IsInCamera += Level_IsInCamera;

        IL.Celeste.LightingRenderer.BeforeRender += LightingRenderer_BeforeRender;

        IL.Celeste.LightningRenderer.OnRenderBloom += LightningRenderer_OnRenderBloom;

        Player_get_CameraTarget_hook = new ILHook(
            typeof(Player).GetProperty("CameraTarget").GetGetMethod(),
            Player_get_CameraTarget
        );

        /// Other Entity Hooks ///
        IL.Celeste.CrystalStaticSpinner.InView += CrystalStaticSpinner_InView;
        IL.Celeste.DustEdges.BeforeRender += DustEdges_BeforeRender;
        IL.Celeste.Lightning.InView += Lightning_InView;

        /// Backdrop Hooks ///
        IL.Celeste.BlackholeBG.ctor += BlackholeBG_ctor;
        IL.Celeste.BlackholeBG.Update += BlackholeBG_Update;
        IL.Celeste.BlackholeBG.BeforeRender += BlackholeBG_BeforeRender;

        IL.Celeste.CoreStarsFG.Reset += CoreStarsFG_Reset;
        IL.Celeste.CoreStarsFG.Render += CoreStarsFG_Render;

        IL.Celeste.DreamStars.ctor += DreamStars_ctor;
        IL.Celeste.DreamStars.Render += DreamStars_Render;

        IL.Celeste.FinalBossStarfield.ctor += FinalBossStarfield_ctor;
        IL.Celeste.FinalBossStarfield.Render += FinalBossStarfield_Render;

        IL.Celeste.Godrays.Update += Godrays_Update;
        IL.Celeste.Godrays.Ray.Reset += Godrays_Ray_Reset;

        IL.Celeste.HeatWave.Reset += HeatWave_Reset;
        IL.Celeste.HeatWave.Render += HeatWave_Render;

        IL.Celeste.MirrorFG.Reset += MirrorFG_Reset;
        IL.Celeste.MirrorFG.Render += MirrorFG_Render;

        IL.Celeste.NorthernLights.ctor += NorthernLights_ctor;
        IL.Celeste.NorthernLights.BeforeRender += NorthernLights_BeforeRender;

        // Both orig and patched use the same layout
        IL.Celeste.Parallax.Render += Parallax_patched_Render;
        Parallax_orig_Render_hook = new ILHook(
            typeof(Parallax).GetMethod("orig_Render", BindingFlags.Instance | BindingFlags.Public),
            Parallax_orig_Render
        );

        IL.Celeste.Petals.Reset += Petals_Reset;
        IL.Celeste.Petals.Render += Petals_Render;

        IL.Celeste.Planets.ctor += Planets_ctor;
        IL.Celeste.Planets.Render += Planets_Render;

        IL.Celeste.RainFG.Update += RainFG_Update;
        IL.Celeste.RainFG.Render += RainFG_Render;
        IL.Celeste.RainFG.Particle.Init += RainFG_Particle_Init;

        IL.Celeste.ReflectionFG.Reset += ReflectionFG_Reset;
        IL.Celeste.ReflectionFG.Render += ReflectionFG_Render;

        IL.Celeste.Snow.Update += Snow_Update;
        IL.Celeste.Snow.Render += Snow_Render;
        IL.Celeste.Snow.Particle.Init += Snow_Particle_Init;

        IL.Celeste.StardustFG.Reset += StardustFG_Reset;
        IL.Celeste.StardustFG.Render += StardustFG_Render;

        IL.Celeste.Starfield.ctor += Starfield_ctor;
        IL.Celeste.Starfield.Render += Starfield_Render;

        IL.Celeste.StarsBG.ctor += StarsBG_ctor;
        IL.Celeste.StarsBG.Render += StarsBG_Render;
    }

    public static void Load_FrostHelper()
    {
        FrostHelper_CustomSpinner_InView_hook = new ILHook(
            typeof(FrostHelper.CustomSpinner).GetMethod("InView", BindingFlags.Instance | BindingFlags.NonPublic),
            FrostHelper_CustomSpinner_InView
        );
        FrostHelper_HDlesteCompat_get_Scale_hook = new ILHook(
            typeof(FrostHelper.ModIntegration.HDlesteCompat).GetProperty("Scale").GetGetMethod(),
            FrostHelper_HDlesteCompat_get_Scale
        );
    }

    public static void Unload()
    {
        /// Core-Mechanic Hooks ///
        IL.Celeste.Audio.Position -= Audio_Position;
        
        IL.Celeste.BloomRenderer.Apply -= BloomRenderer_Apply;

        IL.Celeste.GameplayBuffers.Create -= GameplayBuffers_Create;
        IL.Celeste.GameplayRenderer.ctor -= GameplayRenderer_ctor;

        IL.Celeste.Level.Render -= Level_Render;
        IL.Celeste.Level.ResetZoom -= Level_ResetZoom;
        IL.Celeste.Level.EnforceBounds -= Level_EnforceBounds;
        IL.Celeste.Level.IsInCamera -= Level_IsInCamera;

        IL.Celeste.LightingRenderer.BeforeRender -= LightingRenderer_BeforeRender;

        IL.Celeste.LightningRenderer.OnRenderBloom -= LightningRenderer_OnRenderBloom;

        Player_get_CameraTarget_hook.Dispose();

        /// Other Entity Hooks ///
        IL.Celeste.CrystalStaticSpinner.InView -= CrystalStaticSpinner_InView;
        IL.Celeste.DustEdges.BeforeRender -= DustEdges_BeforeRender;
        IL.Celeste.Lightning.InView -= Lightning_InView;

        /// Backdrop Hooks ///
        IL.Celeste.BlackholeBG.ctor -= BlackholeBG_ctor;
        IL.Celeste.BlackholeBG.Update -= BlackholeBG_Update;
        IL.Celeste.BlackholeBG.BeforeRender -= BlackholeBG_BeforeRender;

        IL.Celeste.CoreStarsFG.Reset -= CoreStarsFG_Reset;
        IL.Celeste.CoreStarsFG.Render -= CoreStarsFG_Render;

        IL.Celeste.DreamStars.ctor -= DreamStars_ctor;
        IL.Celeste.DreamStars.Render -= DreamStars_Render;

        IL.Celeste.FinalBossStarfield.ctor -= FinalBossStarfield_ctor;
        IL.Celeste.FinalBossStarfield.Render -= FinalBossStarfield_Render;

        IL.Celeste.Godrays.Update -= Godrays_Update;
        IL.Celeste.Godrays.Ray.Reset -= Godrays_Ray_Reset;

        IL.Celeste.HeatWave.Reset -= HeatWave_Reset;
        IL.Celeste.HeatWave.Render -= HeatWave_Render;

        IL.Celeste.MirrorFG.Reset -= MirrorFG_Reset;
        IL.Celeste.MirrorFG.Render -= MirrorFG_Render;

        IL.Celeste.NorthernLights.ctor -= NorthernLights_ctor;
        IL.Celeste.NorthernLights.BeforeRender -= NorthernLights_BeforeRender;

        // Both orig and patched use the same layout
        IL.Celeste.Parallax.Render -= Parallax_patched_Render;
        Parallax_orig_Render_hook.Dispose();

        IL.Celeste.Petals.Reset -= Petals_Reset;
        IL.Celeste.Petals.Render -= Petals_Render;

        IL.Celeste.Planets.ctor -= Planets_ctor;
        IL.Celeste.Planets.Render -= Planets_Render;

        IL.Celeste.RainFG.Update -= RainFG_Update;
        IL.Celeste.RainFG.Render -= RainFG_Render;
        IL.Celeste.RainFG.Particle.Init -= RainFG_Particle_Init;

        IL.Celeste.ReflectionFG.Reset -= ReflectionFG_Reset;
        IL.Celeste.ReflectionFG.Render -= ReflectionFG_Render;

        IL.Celeste.Snow.Update -= Snow_Update;
        IL.Celeste.Snow.Render -= Snow_Render;
        IL.Celeste.Snow.Particle.Init -= Snow_Particle_Init;

        IL.Celeste.StardustFG.Reset -= StardustFG_Reset;
        IL.Celeste.StardustFG.Render -= StardustFG_Render;

        IL.Celeste.Starfield.ctor -= Starfield_ctor;
        IL.Celeste.Starfield.Render -= Starfield_Render;

        IL.Celeste.StarsBG.ctor -= StarsBG_ctor;
        IL.Celeste.StarsBG.Render -= StarsBG_Render;
    }

    public static void Unload_FrostHelper()
    {
        FrostHelper_CustomSpinner_InView_hook.Dispose();
        FrostHelper_HDlesteCompat_get_Scale_hook.Dispose();
    }

    private static void PrintInstructions(ILContext ctx)
    {
        foreach (var instr in ctx.Instrs)
        {
            try { Console.WriteLine($"{instr.Offset}:{instr.ToString()}"); }
            catch (Exception) { Console.WriteLine($"{instr.Offset}:{instr.OpCode}"); }
        }
    }

#region Find & Replace Functions
    // NOTE: We don't remove instructions to not break mods relying on them

    private static readonly MethodInfo m_GameWidth  = typeof(ZoomOutModule).GetProperty(nameof(ZoomOutModule.GameWidth)).GetGetMethod();
    private static readonly MethodInfo m_GameHeight = typeof(ZoomOutModule).GetProperty(nameof(ZoomOutModule.GameHeight)).GetGetMethod();
    private static readonly MethodInfo m_GameScale  = typeof(ZoomOutModule).GetProperty(nameof(ZoomOutModule.GameScale)).GetGetMethod();

    private static void FindAndReplace_Int(this ILCursor cursor, MethodInfo method, int target)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Call, method);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"Failed to un-inline at {cursor.Context.Method.Name} for {target} (Int)");
        }
    }
    private static void FindAndReplace_IntAdd(this ILCursor cursor, MethodInfo method, int target, int offset)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Call, method);
            cursor.Emit(OpCodes.Ldc_I4_S, (sbyte)offset);
            cursor.Emit(OpCodes.Add);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"Failed to un-inline at {cursor.Context.Method.Name} for {target} (IntAdd)");
        }
    }
    private static void FindAndReplace_IntHalf(this ILCursor cursor, MethodInfo method, int target)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Call, method);
            cursor.Emit(OpCodes.Ldc_I4_2);
            cursor.Emit(OpCodes.Div);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"Failed to un-inline at {cursor.Context.Method.Name} for {target} (FloatHalf)");
        }
    }

    private static void FindAndReplace_Float(this ILCursor cursor, MethodInfo method, float target)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Call, method);
            cursor.Emit(OpCodes.Conv_R4);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"Failed to un-inline at {cursor.Context.Method.Name} for {target} (Float)");
        }
    }
    private static void FindAndReplace_FloatAdd(this ILCursor cursor, MethodInfo method, float target, int offset)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Call, method);
            cursor.Emit(OpCodes.Ldc_I4_S, (sbyte)offset);
            cursor.Emit(OpCodes.Add);
            cursor.Emit(OpCodes.Conv_R4);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"Failed to un-inline at {cursor.Context.Method.Name} for {target} (FloatAdd)");
        }
    }
    private static void FindAndReplace_FloatHalf(this ILCursor cursor, MethodInfo method, float target)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Call, method);
            cursor.Emit(OpCodes.Ldc_I4_2);
            cursor.Emit(OpCodes.Div);
            cursor.Emit(OpCodes.Conv_R4);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"Failed to un-inline at {cursor.Context.Method.Name} for {target} (FloatHalf)");
        }
    }
    private static void FindAndReplace_FloatMul(this ILCursor cursor, MethodInfo method, float target, float multiplier)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Call, method);
            cursor.Emit(OpCodes.Conv_R4);
            cursor.Emit(OpCodes.Ldc_R4, multiplier);
            cursor.Emit(OpCodes.Mul);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"Failed to un-inline at {cursor.Context.Method.Name} for {target} (FloatMul)");
        }
    }

    private static void FindAndReplace_GameWidth_Int(this ILCursor cursor) => cursor.FindAndReplace_Int(m_GameWidth, 320);
    private static void FindAndReplace_GameHeight_Int(this ILCursor cursor) => cursor.FindAndReplace_Int(m_GameHeight, 180);

    private static void FindAndReplace_GameWidth_IntAdd(this ILCursor cursor, int target, int offset) => cursor.FindAndReplace_IntAdd(m_GameWidth, target, offset);
    private static void FindAndReplace_GameHeight_IntAdd(this ILCursor cursor, int target, int offset) => cursor.FindAndReplace_IntAdd(m_GameHeight, target, offset);

    private static void FindAndReplace_GameWidth_IntHalf(this ILCursor cursor) => cursor.FindAndReplace_IntHalf(m_GameWidth, 160);
    private static void FindAndReplace_GameHeight_IntHalf(this ILCursor cursor) => cursor.FindAndReplace_IntHalf(m_GameHeight, 90);


    private static void FindAndReplace_GameWidth_Float(this ILCursor cursor) => cursor.FindAndReplace_Float(m_GameWidth, 320.0f);
    private static void FindAndReplace_GameHeight_Float(this ILCursor cursor) => cursor.FindAndReplace_Float(m_GameHeight, 180.0f);

    private static void FindAndReplace_GameWidth_FloatAdd(this ILCursor cursor, float target, int offset) => cursor.FindAndReplace_FloatAdd(m_GameWidth, target, offset);
    private static void FindAndReplace_GameHeight_FloatAdd(this ILCursor cursor, float target, int offset) => cursor.FindAndReplace_FloatAdd(m_GameHeight, target, offset);

    private static void FindAndReplace_GameWidth_FloatHalf(this ILCursor cursor) => cursor.FindAndReplace_FloatHalf(m_GameWidth, 160.0f);
    private static void FindAndReplace_GameHeight_FloatHalf(this ILCursor cursor) => cursor.FindAndReplace_FloatHalf(m_GameHeight, 90.0f);

    private static void FindAndReplace_GameWidth_FloatMul(this ILCursor cursor, float target, float multiplier) => cursor.FindAndReplace_FloatMul(m_GameWidth, target, multiplier);
    private static void FindAndReplace_GameHeight_FloatMul(this ILCursor cursor, float target, float multiplier) => cursor.FindAndReplace_FloatMul(m_GameHeight, target, multiplier);

#endregion

#region Core-Mechanic Hooks

    private static void Audio_Position(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void BloomRenderer_Apply(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        if (cursor.TryGotoNext(instr => instr.MatchCallvirt<SpriteBatch>("Begin")) &&
            cursor.TryGotoNext(instr => instr.MatchCallvirt<SpriteBatch>("Begin")) &&
            cursor.TryGotoNext(instr => instr.MatchCallvirt<SpriteBatch>("Begin")))
        {
            cursor.FindAndReplace_GameWidth_FloatAdd(340.0f, 20);
            cursor.FindAndReplace_GameHeight_FloatAdd(200.0f, 20);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"Failed to un-inline at {ctx.Method.Name}");
        }
    }

    private static void GameplayBuffers_Create(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);

        // Gameplay
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();

        // Level
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();

        // ResortDust
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();

        // Light
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();

        // Displacement
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();

        // MirrorSources
        cursor.FindAndReplace_GameWidth_IntAdd(384, 64);
        cursor.FindAndReplace_GameHeight_IntAdd(244, 64);

        // MirrorMasks
        cursor.FindAndReplace_GameWidth_IntAdd(384, 64);
        cursor.FindAndReplace_GameHeight_IntAdd(244, 64);

        // TempA
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();

        // TempB
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();
    }

    private static void GameplayRenderer_ctor(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();
    }

    private static void Level_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        if (cursor.TryGotoNext(instr => instr.MatchCall<Matrix>("CreateScale")))
        {
            // Zoom out depending on GameScale
            cursor.Emit(OpCodes.Call, m_GameScale);
            cursor.Emit(OpCodes.Div);

            cursor.FindAndReplace_GameWidth_Float();
            cursor.FindAndReplace_GameHeight_Float();

            cursor.FindAndReplace_GameWidth_Float();
            cursor.FindAndReplace_GameWidth_Float();

            cursor.FindAndReplace_GameWidth_FloatHalf();
            cursor.FindAndReplace_GameWidth_FloatHalf();
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"Failed to un-inline at {ctx.Method.Name}");
        }
    }

    private static void Level_ResetZoom(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void Level_EnforceBounds(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();
    }

    private static void Level_IsInCamera(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();
    }

    private static void LightingRenderer_BeforeRender(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void LightningRenderer_OnRenderBloom(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private delegate void FixPlayerCameraDelegate(Player self, ref Vector2 at, ref Vector2 target);
    private static void fixPlayerCamera(Player self, ref Vector2 at, ref Vector2 target)
    {
        // For some reason the Clamp from Calc.cs behaves differently than from MathHelper?
        at.X = Calc.Clamp(target.X, self.level.Bounds.Left, self.level.Bounds.Right - ZoomOutModule.GameWidth);
        at.Y = Calc.Clamp(target.Y, self.level.Bounds.Top, self.level.Bounds.Bottom - ZoomOutModule.GameHeight);

        // Center the camera if the bounds are too small
        if (self.level.Bounds.Width < ZoomOutModule.GameWidth)
            at.X -= (self.level.Bounds.Width - ZoomOutModule.GameWidth) / 2.0f;
        if (self.level.Bounds.Height < ZoomOutModule.GameHeight)
            at.Y -= (self.level.Bounds.Height - ZoomOutModule.GameHeight) / 2.0f;
    }

    private static void Player_get_CameraTarget(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        var Calc_Clamp = typeof(Calc).GetMethod("Clamp", BindingFlags.Static | BindingFlags.Public, new Type[]{ typeof(float), typeof(float), typeof(float) });

        cursor.FindAndReplace_GameWidth_FloatHalf();
        cursor.FindAndReplace_GameHeight_FloatHalf();

        VariableDefinition atVectorDef = ctx.Body.Variables[0];
        VariableDefinition targetVectorDef = ctx.Body.Variables[1];

        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Player>("EnforceLevelBounds"),
                                               instr => instr.OpCode == OpCodes.Brfalse))
        {
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldloca_S, atVectorDef);
            cursor.Emit(OpCodes.Ldloca_S, targetVectorDef);
            cursor.EmitDelegate<FixPlayerCameraDelegate>(fixPlayerCamera);

            ILLabel label = null;
            int index = cursor.Index;
            cursor.TryGotoNext(instr => instr.MatchBr(out label));

            if (label == null)
            {
                Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"Failed to find branch label at {cursor.Context.Method.Name}");
                return;
            }

            cursor.Index = index;
            cursor.Emit(OpCodes.Br_S, label); // Skip everything else since that's done in fixPlayerCamrea
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"Failed to insert centerPlayerCamera delegate at {cursor.Context.Method.Name}");
        }
    }

#endregion

#region Other Entity Hooks

    private static void CrystalStaticSpinner_InView(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void DustEdges_BeforeRender(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();

        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();

        // 1.0f / GameWidth
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.003125f)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Ldc_R4, 1.0f);
            cursor.Emit(OpCodes.Call, m_GameWidth);
            cursor.Emit(OpCodes.Conv_R4);
            cursor.Emit(OpCodes.Div);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"Failed to un-inline at {cursor.Context.Method.Name}");
        }

        // 1.0f / GameHeight
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(0.0055555557f)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Ldc_R4, 1.0f);
            cursor.Emit(OpCodes.Call, m_GameHeight);
            cursor.Emit(OpCodes.Conv_R4);
            cursor.Emit(OpCodes.Div);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"Failed to un-inline at {cursor.Context.Method.Name}");
        }
    }

    private static void Lightning_InView(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

#endregion

#region Backdrop Hooks

    private static void BlackholeBG_ctor(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }
    private static void BlackholeBG_Update(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }
    private static void BlackholeBG_BeforeRender(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();
    }

    private static void CoreStarsFG_Reset(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();
    }
    private static void CoreStarsFG_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void DreamStars_ctor(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }
    private static void DreamStars_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void FinalBossStarfield_ctor(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_IntAdd(384, 64);
        cursor.FindAndReplace_GameHeight_IntAdd(244, 64);
    }
    private static void FinalBossStarfield_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatAdd(330.0f, 10);

        cursor.FindAndReplace_GameWidth_FloatAdd(330.0f, 10);
        cursor.FindAndReplace_GameWidth_FloatAdd(190.0f, 10);

        cursor.FindAndReplace_GameWidth_FloatAdd(330.0f, 10);
        cursor.FindAndReplace_GameWidth_FloatAdd(190.0f, 10);

        cursor.FindAndReplace_GameWidth_FloatAdd(190.0f, 10);

        cursor.FindAndReplace_GameWidth_FloatAdd(384.0f, 64);
        cursor.FindAndReplace_GameWidth_FloatAdd(244.0f, 64);
    }

    private static void Godrays_Update(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatAdd(384.0f, 64);
        cursor.FindAndReplace_GameHeight_FloatAdd(244.0f, 64);
    }
    private static void Godrays_Ray_Reset(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatAdd(384.0f, 64);
        cursor.FindAndReplace_GameHeight_FloatAdd(244.0f, 64);
    }

    private static void HeatWave_Reset(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();
    }
    private static void HeatWave_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void MirrorFG_Reset(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();
    }
    private static void MirrorFG_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void NorthernLights_ctor(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();

        cursor.FindAndReplace_GameWidth_Float();

        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();

        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();

        cursor.FindAndReplace_GameHeight_Float();
    }
    private static void NorthernLights_BeforeRender(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();

        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void Parallax_orig_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatHalf();
        cursor.FindAndReplace_GameHeight_FloatHalf();

        cursor.FindAndReplace_GameHeight_Float();
        cursor.FindAndReplace_GameWidth_Float();
    }
    private static void Parallax_patched_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatHalf();
        cursor.FindAndReplace_GameHeight_FloatHalf();

        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void Petals_Reset(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_IntAdd(352, 32);
        cursor.FindAndReplace_GameHeight_IntAdd(212, 32);
    }
    private static void Petals_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatAdd(352, 32);
        cursor.FindAndReplace_GameHeight_FloatAdd(212, 32);
    }

    private static void Planets_ctor(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatMul(640.0f, 2.0f);
        cursor.FindAndReplace_GameHeight_FloatMul(360.0f, 2.0f);
    }
    private static void Planets_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatMul(640.0f, 2.0f);
        cursor.FindAndReplace_GameHeight_FloatMul(360.0f, 2.0f);
    }

    private static void RainFG_Update(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatHalf();
    }
    private static void RainFG_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatAdd(384.0f, 64);
        cursor.FindAndReplace_GameHeight_FloatAdd(244.0f, 64);
    }
    private static void RainFG_Particle_Init(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatAdd(384.0f, 64);
        cursor.FindAndReplace_GameHeight_FloatAdd(244.0f, 64);
    }

    private static void ReflectionFG_Reset(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();
    }
    private static void ReflectionFG_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void Snow_Update(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatHalf();
    }
    private static void Snow_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }
    private static void Snow_Particle_Init(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void StardustFG_Reset(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Int();
        cursor.FindAndReplace_GameHeight_Int();
    }
    private static void StardustFG_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    // TODO: Change star count to properly fix this
    private static void Starfield_ctor(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        //cursor.FindAndReplace_GameHeight_Float();
    }
    private static void Starfield_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        //cursor.FindAndReplace_GameWidth_FloatAdd(448.0f, 128);
        //cursor.FindAndReplace_GameHeight_FloatAdd(212.0f, 32);
    }

    private static void StarsBG_ctor(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }
    private static void StarsBG_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_Float();

        cursor.FindAndReplace_GameHeight_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void Tentacles_ctor(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameHeight_Float();
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameHeight_FloatHalf();

        cursor.FindAndReplace_GameHeight_Float();
        cursor.FindAndReplace_GameHeight_FloatHalf();

        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameWidth_FloatHalf();

        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameWidth_FloatHalf();
        cursor.FindAndReplace_GameHeight_Float();
    }
    private static void Tentacles_Update(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_Float();
        cursor.FindAndReplace_GameWidth_FloatHalf();

        cursor.FindAndReplace_GameHeight_Float();
        cursor.FindAndReplace_GameHeight_Float();
    }

    // TODO: WindSnowFG, which uses member variables instead of inlining

#endregion

#region Mod Compatability Hooks

    private static void FrostHelper_CustomSpinner_InView(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatAdd(336.0f, 16);
        cursor.FindAndReplace_GameHeight_FloatAdd(196.0f, 16);
    }

    private static void FrostHelper_HDlesteCompat_get_Scale(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.Emit(OpCodes.Ldc_I4_1);
        cursor.Emit(OpCodes.Ret);
    }

#endregion

}