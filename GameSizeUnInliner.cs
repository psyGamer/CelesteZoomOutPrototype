using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using Celeste;

namespace Celeste.Mod.ZoomOut;

public static class GameSizeUnInliner
{
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
    }

    private static void PrintInstructions(ILContext ctx)
    {
        foreach (var instr in ctx.Instrs)
        {
            try { Console.WriteLine($"{instr.Offset}:{instr.ToString()}"); }
            catch (Exception) {}
        }
    }

#region Find & Replace Functions
    // NOTE: We don't remove instructions to not break mods relying on them

    private static void FindAndReplace_Int(this ILCursor cursor, string fieldName, int target)
    {
        if (cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcI4(target)))
        {
            cursor.Emit(OpCodes.Pop);
            cursor.Emit<ZoomOutModule>(OpCodes.Ldsfld, nameof(ZoomOutModule.GameWidth));
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
            cursor.Emit<ZoomOutModule>(OpCodes.Ldsfld, nameof(ZoomOutModule.GameWidth));
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
        if (cursor.TryGotoNext(instr => instr.MatchCallvirt<Camera>("get_Position")))
        {
            cursor.FindAndReplace_GameWidth_Float();
            cursor.FindAndReplace_GameHeight_Float();
        }
        else
        {
            Logger.Log(LogLevel.Error, ZoomOutModule.LoggerTag, $"FAILED TO UN-INLINE INSIDE {ctx.Method.Name}");
        }
    }

    private static void BloomRenderer_Apply(ILContext ctx)
    {
        var cursor = new ILCursor(ctx);
        if (cursor.TryGotoNext(instr => instr.MatchCallvirt<SpriteBatch>(nameof(SpriteBatch.Begin))) &&
            cursor.TryGotoNext(instr => instr.MatchCallvirt<SpriteBatch>(nameof(SpriteBatch.Begin))) &&
            cursor.TryGotoNext(instr => instr.MatchCallvirt<SpriteBatch>(nameof(SpriteBatch.Begin))))
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
        if (cursor.TryGotoNext(instr => instr.MatchCall<Matrix>(nameof(Matrix.CreateScale))))
        {
            // Zoom out
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

#endregion
}