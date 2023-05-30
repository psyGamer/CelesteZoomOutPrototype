using System;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Celeste;

namespace Celeste.Mod.ZoomOut;

public static class GameSizeUnInliner
{
    private static IDetour Player_get_CameraTarget_hook;

    public static void Load()
    {
        IL.Celeste.Audio.Position += Audio_Position;
        IL.Celeste.BloomRenderer.Apply += BloomRenderer_Apply;
        IL.Celeste.GameplayBuffers.Create += GameplayBuffers_Create;
        IL.Celeste.GameplayRenderer.ctor += GameplayRenderer_ctor;
        IL.Celeste.Level.Render += Level_Render;
        IL.Celeste.Level.ResetZoom += Level_ResetZoom;
        IL.Celeste.Level.EnforceBounds += Level_EnforceBounds;
        IL.Celeste.Level.IsInCamera += Level_IsInCamera;
        IL.Celeste.Parallax.Render += Parallax_Render;
        IL.Celeste.Starfield.ctor += Starfield_ctor;
        IL.Celeste.Starfield.Render += Starfield_Render;
        IL.Celeste.StarsBG.ctor += StarsBG_ctor;
        IL.Celeste.StarsBG.Render += StarsBG_Render;

        Player_get_CameraTarget_hook = new ILHook(
            typeof(Player).GetProperty("CameraTarget").GetGetMethod(),
            Player_get_CameraTarget
        );
    }

    public static void Unload()
    {
        IL.Celeste.Audio.Position -= Audio_Position;
        IL.Celeste.BloomRenderer.Apply -= BloomRenderer_Apply;
        IL.Celeste.GameplayBuffers.Create -= GameplayBuffers_Create;
        IL.Celeste.GameplayRenderer.ctor -= GameplayRenderer_ctor;
        IL.Celeste.Level.Render -= Level_Render;
        IL.Celeste.Level.ResetZoom -= Level_ResetZoom;
        IL.Celeste.Level.EnforceBounds -= Level_EnforceBounds;
        IL.Celeste.Level.IsInCamera -= Level_IsInCamera;
        IL.Celeste.Parallax.Render -= Parallax_Render;
        IL.Celeste.Starfield.ctor -= Starfield_ctor;
        IL.Celeste.Starfield.Render -= Starfield_Render;
        IL.Celeste.StarsBG.ctor -= StarsBG_ctor;
        IL.Celeste.StarsBG.Render -= StarsBG_Render;

        Player_get_CameraTarget_hook.Dispose();
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

    private static void FindAndReplace_Int(this ILCursor cursor, string fieldName, int target)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit<ZoomOutModule>(OpCodes.Ldsfld, fieldName);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"FAILED TO UN-INLINE INSIDE {cursor.Context.Method.Name} for {target} (Int)");
        }
    }
    private static void FindAndReplace_IntAdd(this ILCursor cursor, string fieldName, int target, int offset)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit<ZoomOutModule>(OpCodes.Ldsfld, fieldName);
            cursor.Emit(OpCodes.Ldc_I4_S, (sbyte)offset);
            cursor.Emit(OpCodes.Add);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"FAILED TO UN-INLINE INSIDE {cursor.Context.Method.Name} for {target} (IntAdd)");
        }
    }
    private static void FindAndReplace_IntHalf(this ILCursor cursor, string fieldName, int target)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit<ZoomOutModule>(OpCodes.Ldsfld, fieldName);
            cursor.Emit(OpCodes.Ldc_I4_2);
            cursor.Emit(OpCodes.Div);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"FAILED TO UN-INLINE INSIDE {cursor.Context.Method.Name} for {target} (FloatHalf)");
        }
    }

    private static void FindAndReplace_Float(this ILCursor cursor, string fieldName, float target)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit<ZoomOutModule>(OpCodes.Ldsfld, fieldName);
            cursor.Emit(OpCodes.Conv_R4);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"FAILED TO UN-INLINE INSIDE {cursor.Context.Method.Name} for {target} (Float)");
        }
    }
    private static void FindAndReplace_FloatAdd(this ILCursor cursor, string fieldName, float target, int offset)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit<ZoomOutModule>(OpCodes.Ldsfld, fieldName);
            cursor.Emit(OpCodes.Ldc_I4_S, (sbyte)offset);
            cursor.Emit(OpCodes.Add);
            cursor.Emit(OpCodes.Conv_R4);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"FAILED TO UN-INLINE INSIDE {cursor.Context.Method.Name} for {target} (FloatAdd)");
        }
    }
    private static void FindAndReplace_FloatHalf(this ILCursor cursor, string fieldName, float target)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit<ZoomOutModule>(OpCodes.Ldsfld, fieldName);
            cursor.Emit(OpCodes.Ldc_I4_2);
            cursor.Emit(OpCodes.Div);
            cursor.Emit(OpCodes.Conv_R4);
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"FAILED TO UN-INLINE INSIDE {cursor.Context.Method.Name} for {target} (FloatHalf)");
        }
    }

    private static void FindAndReplace_GameWidth_Int(this ILCursor cursor)
        => cursor.FindAndReplace_Int(nameof(ZoomOutModule.GameWidth), 320);
    private static void FindAndReplace_GameHeight_Int(this ILCursor cursor)
        => cursor.FindAndReplace_Int(nameof(ZoomOutModule.GameHeight), 180);

    private static void FindAndReplace_GameWidth_IntAdd(this ILCursor cursor, int target, int offset)
        => cursor.FindAndReplace_IntAdd(nameof(ZoomOutModule.GameWidth), target, offset);
    private static void FindAndReplace_GameHeight_IntAdd(this ILCursor cursor, int target, int offset)
        => cursor.FindAndReplace_IntAdd(nameof(ZoomOutModule.GameHeight), target, offset);

    private static void FindAndReplace_GameWidth_IntHalf(this ILCursor cursor)
        => cursor.FindAndReplace_IntHalf(nameof(ZoomOutModule.GameWidth), 160);
    private static void FindAndReplace_GameHeight_IntHalf(this ILCursor cursor)
        => cursor.FindAndReplace_IntHalf(nameof(ZoomOutModule.GameHeight), 90);


    private static void FindAndReplace_GameWidth_Float(this ILCursor cursor)
        => cursor.FindAndReplace_Float(nameof(ZoomOutModule.GameWidth), 320.0f);
    private static void FindAndReplace_GameHeight_Float(this ILCursor cursor)
        => cursor.FindAndReplace_Float(nameof(ZoomOutModule.GameHeight), 180.0f);

    private static void FindAndReplace_GameWidth_FloatAdd(this ILCursor cursor, float target, int offset)
        => cursor.FindAndReplace_FloatAdd(nameof(ZoomOutModule.GameWidth), target, offset);
    private static void FindAndReplace_GameHeight_FloatAdd(this ILCursor cursor, float target, int offset)
        => cursor.FindAndReplace_FloatAdd(nameof(ZoomOutModule.GameHeight), target, offset);

    private static void FindAndReplace_GameWidth_FloatHalf(this ILCursor cursor)
        => cursor.FindAndReplace_FloatHalf(nameof(ZoomOutModule.GameWidth), 160.0f);
    private static void FindAndReplace_GameHeight_FloatHalf(this ILCursor cursor)
        => cursor.FindAndReplace_FloatHalf(nameof(ZoomOutModule.GameHeight), 90.0f);

#endregion

#region Hooks

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
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"FAILED TO UN-INLINE INSIDE {ctx.Method.Name}");
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
            cursor.Emit<ZoomOutModule>(OpCodes.Ldsfld, nameof(ZoomOutModule.GameScale));
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
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"FAILED TO UN-INLINE INSIDE {ctx.Method.Name}");
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

    private static void Parallax_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatHalf();
        cursor.FindAndReplace_GameHeight_FloatHalf();

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

    private static void Starfield_ctor(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameHeight_Float();
    }

    private static void Starfield_Render(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        cursor.FindAndReplace_GameWidth_FloatAdd(448.0f, 128);
        cursor.FindAndReplace_GameHeight_FloatAdd(212.0f, 32);
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

#endregion
}