// Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.GravityHelper.Hooks
{
    public static class SandwichLavaHooks
    {
        public static void Load()
        {
            Logger.Log(nameof(GravityHelperModule), $"Loading {nameof(SandwichLava)} hooks...");

            On.Celeste.SandwichLava.OnPlayer += SandwichLava_OnPlayer;
        }

        public static void Unload()
        {
            Logger.Log(nameof(GravityHelperModule), $"Unloading {nameof(SandwichLava)} hooks...");

            On.Celeste.SandwichLava.OnPlayer -= SandwichLava_OnPlayer;
        }

        private static void SandwichLava_OnPlayer(On.Celeste.SandwichLava.orig_OnPlayer orig, SandwichLava self, Player player)
        {
            if (!GravityHelperModule.ShouldInvertPlayer || !SaveData.Instance.Assists.Invincible)
            {
                orig(self, player);
                return;
            }

            var data = DynamicData.For(self);
            if (data.Get<float>("delay") > 0f) return;
            var topRect = data.Get<LavaRect>("topRect");
            var loopSfx = data.Get<SoundSource>("loopSfx");

            int num = player.Y < self.Y + topRect.Position.Y + topRect.Height + 32.0 ? 1 : -1;
            float from = self.Y;
            float to = self.Y - num * 48;
            player.Speed.Y = -num * 200;
            if (num > 0) player.RefillDash();
            Tween.Set(self, Tween.TweenMode.Oneshot, 0.4f, Ease.CubeOut, t => self.Y = MathHelper.Lerp(from, to, t.Eased));
            data.Set("delay", 0.5f);
            loopSfx.Param("rising", 0.0f);
            Audio.Play("event:/game/general/assist_screenbottom", player.Position);
        }
    }
}
