using MonoMod.Cil;

// ReSharper disable InconsistentNaming

namespace Celeste.Mod.GravityHelper.Hooks
{
    public static class PlayerHairHooks
    {
        public static void Load()
        {
            Logger.Log(nameof(GravityHelperModule), $"Loading {nameof(PlayerHair)} hooks...");

            IL.Celeste.PlayerHair.AfterUpdate += PlayerHair_AfterUpdate;
            IL.Celeste.PlayerHair.Render += PlayerHair_Render;
        }

        public static void Unload()
        {
            Logger.Log(nameof(GravityHelperModule), $"Unloading {nameof(PlayerHair)} hooks...");

            IL.Celeste.PlayerHair.AfterUpdate -= PlayerHair_AfterUpdate;
            IL.Celeste.PlayerHair.Render -= PlayerHair_Render;
        }

        private static void PlayerHair_AfterUpdate(ILContext il) => HookUtils.SafeHook(() =>
        {
            var cursor = new ILCursor(il);

            void invertAdditions()
            {
                cursor.GotoNextAddition();
                cursor.EmitInvertVectorDelegate();
                cursor.Index += 2;
                cursor.GotoNextAddition();
                cursor.EmitInvertVectorDelegate();
            }

            // this.Nodes[0] = this.Sprite.RenderPosition + new Vector2(0.0f, -9f * this.Sprite.Scale.Y) + this.Sprite.HairOffset * new Vector2((float) this.Facing, 1f);
            invertAdditions();

            // Vector2 target = this.Nodes[0] + new Vector2((float) ((double) -(int) this.Facing * (double) this.StepInFacingPerSegment * 2.0), (float) Math.Sin((double) this.wave) * this.StepYSinePerSegment) + this.StepPerSegment;
            cursor.GotoNext(instr => instr.MatchLdfld<PlayerHair>(nameof(PlayerHair.StepYSinePerSegment)));
            invertAdditions();

            // target = this.Nodes[index] + new Vector2((float) -(int) this.Facing * this.StepInFacingPerSegment, (float) Math.Sin((double) this.wave + (double) index * 0.800000011920929) * this.StepYSinePerSegment) + this.StepPerSegment;
            cursor.GotoNext(instr => instr.MatchLdfld<PlayerHair>(nameof(PlayerHair.StepYSinePerSegment)));
            invertAdditions();
        });

        private static void PlayerHair_Render(ILContext il) => HookUtils.SafeHook(() =>
        {
            var cursor = new ILCursor(il);

            // Vector2 hairScale = this.GetHairScale(index);
            cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<PlayerHair>("GetHairScale"));
            cursor.EmitInvertVectorDelegate();

            // this.GetHairTexture(index).Draw(this.Nodes[index], origin, this.GetHairColor(index), this.GetHairScale(index));
            cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<PlayerHair>("GetHairScale"));
            cursor.EmitInvertVectorDelegate();
        });
    }
}