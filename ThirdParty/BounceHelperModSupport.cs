// Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Reflection;
using Celeste.Mod.GravityHelper.Extensions;
using Celeste.Mod.GravityHelper.Hooks;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;

namespace Celeste.Mod.GravityHelper.ThirdParty
{
    [ThirdPartyMod("BounceHelper")]
    public class BounceHelperModSupport : ThirdPartyModSupport
    {
        // ReSharper disable InconsistentNaming
        private IDetour hook_BounceHelperModule_modDashUpdate;
        private IDetour hook_BounceHelperModule_modNormalUpdate;
        private IDetour hook_BounceHelperModule_bounce;
        private IDetour hook_BounceHelperModule_bounce_il;
        // ReSharper restore InconsistentNaming

        protected override void Load()
        {
            var bhmt = ReflectionCache.BounceHelperModuleType;

            var modDashUpdateMethod = bhmt?.GetMethod("modDashUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
            if (modDashUpdateMethod != null)
                hook_BounceHelperModule_modDashUpdate = new ILHook(modDashUpdateMethod, BounceHelperModule_modDashUpdate);

            var modNormalUpdateMethod = bhmt?.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(m =>
                m.Name == "modNormalUpdate" && m.ReturnType == typeof(int));
            if (modNormalUpdateMethod != null)
                hook_BounceHelperModule_modNormalUpdate = new ILHook(modNormalUpdateMethod, BounceHelperModule_modNormalUpdate);

            var bounceMethod = bhmt?.GetMethod("bounce", BindingFlags.Instance | BindingFlags.NonPublic);
            if (bounceMethod != null)
            {
                var target = GetType().GetMethod(nameof(BounceHelperModule_bounce), BindingFlags.Static | BindingFlags.NonPublic);
                hook_BounceHelperModule_bounce = new Hook(bounceMethod, target);
                hook_BounceHelperModule_bounce_il = new ILHook(bounceMethod, BounceHelperModule_bounce_il);
            }
        }

        protected override void Unload()
        {
            hook_BounceHelperModule_modDashUpdate?.Dispose();
            hook_BounceHelperModule_modDashUpdate = null;
            hook_BounceHelperModule_modNormalUpdate?.Dispose();
            hook_BounceHelperModule_modNormalUpdate = null;
            hook_BounceHelperModule_bounce?.Dispose();
            hook_BounceHelperModule_bounce = null;
            hook_BounceHelperModule_bounce_il?.Dispose();
            hook_BounceHelperModule_bounce_il = null;
        }

        private static void BounceHelperModule_modDashUpdate(ILContext il) => HookUtils.SafeHook(() =>
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(ILCursorExtensions.SubtractionPredicate))
                throw new HookException("Couldn't find sub UnitY");
            cursor.EmitInvertVectorDelegate();
        });

        private static void BounceHelperModule_modNormalUpdate(ILContext il) => HookUtils.SafeHook(() =>
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(ILCursorExtensions.SubtractionPredicate))
                throw new HookException("Couldn't find sub UnitY");
            cursor.EmitInvertVectorDelegate();
        });

        private static void BounceHelperModule_bounce(Action<object, Player, Vector2, int, Vector2, bool, int> orig,
            object module, Player player, Vector2 bounceSpeed, int bounceStrength, Vector2 surfaceDir, bool dreamRipple, int wallCheckDistance)
        {
            if (GravityHelperModule.ShouldInvertPlayer)
                surfaceDir = new Vector2(surfaceDir.X, -surfaceDir.Y);
            orig(module, player, bounceSpeed, bounceStrength, surfaceDir, dreamRipple, wallCheckDistance);
        }

        private static void BounceHelperModule_bounce_il(ILContext il) => HookUtils.SafeHook(() =>
        {
            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdcR4(12f)))
                throw new HookException("Couldn't find 12f");
            cursor.Index++;
            cursor.EmitInvertVectorDelegate();

            if (!cursor.TryGotoNext(instr => instr.MatchCall<SlashFx>(nameof(SlashFx.Burst))))
                throw new HookException("Couldn't find SlashFx.Burst");
            cursor.EmitInvertFloatDelegate();

            if (!cursor.TryGotoNext(MoveType.After, instr => instr.MatchLdfld<Vector2>(nameof(Vector2.Y))))
                throw new HookException("Couldn't find player.Sprite.Scale.Y");
            cursor.EmitInvertFloatDelegate();
        });
    }
}
