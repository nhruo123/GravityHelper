// Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Celeste.Mod.GravityHelper.Components;
using Celeste.Mod.GravityHelper.Extensions;
using Monocle;
using MonoMod.Utils;

namespace Celeste.Mod.GravityHelper.Hooks
{
    public static class GliderHooks
    {
        public static void Load()
        {
            Logger.Log(nameof(GravityHelperModule), $"Loading {nameof(Glider)} hooks...");

            On.Celeste.Glider.Added += Glider_Added;
            On.Celeste.Glider.Render += Glider_Render;
            On.Celeste.Glider.Update += Glider_Update;
        }

        public static void Unload()
        {
            Logger.Log(nameof(GravityHelperModule), $"Unloading {nameof(Glider)} hooks...");

            On.Celeste.Glider.Added -= Glider_Added;
            On.Celeste.Glider.Render -= Glider_Render;
            On.Celeste.Glider.Update -= Glider_Update;
        }

        private static void Glider_Added(On.Celeste.Glider.orig_Added orig, Glider self, Scene scene)
        {
            orig(self, scene);
            if (self.Get<GravityComponent>() != null) return;
            self.Add(new GravityComponent
            {
                UpdateSpeed = args =>
                {
                    if (args.Changed)
                        self.Speed.Y *= -args.MomentumMultiplier;
                },
            });
        }

        private static void Glider_Render(On.Celeste.Glider.orig_Render orig, Glider self)
        {
            if (!self.ShouldInvert())
            {
                orig(self);
                return;
            }

            var data = new DynData<Glider>(self);
            var sprite = data.Get<Sprite>("sprite");
            sprite.Scale.Y *= -1;
            orig(self);
            sprite.Scale.Y *= -1;
        }

        private static void Glider_Update(On.Celeste.Glider.orig_Update orig, Glider self)
        {
            var value = Input.GliderMoveY.Value;
            if (GravityHelperModule.ShouldInvertPlayer)
                Input.GliderMoveY.Value = -value;
            orig(self);
            Input.GliderMoveY.Value = value;
        }
    }
}