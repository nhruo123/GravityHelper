using System;
using System.Reflection;
using Celeste;
using GravityHelper.Triggers;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

namespace GravityHelper
{
    public static class PlayerHooks
    {
        private static IDetour hook_Player_orig_Update;
        private static IDetour hook_Player_orig_UpdateSprite;
        private static IDetour hook_Player_DashCoroutine;

        public static void Load()
        {
            IL.Celeste.Player.Bounce += Player_Bounce;
            IL.Celeste.Player.ClimbCheck += Player_ClimbCheck;
            IL.Celeste.Player.ClimbHopBlockedCheck += Player_ClimbHopBlockedCheck;
            IL.Celeste.Player.ClimbUpdate += Player_ClimbUpdate;
            IL.Celeste.Player._IsOverWater += Player_IsOverWater;
            IL.Celeste.Player.Jump += Player_Jump;
            IL.Celeste.Player.NormalUpdate += Player_NormalUpdate;
            IL.Celeste.Player.OnCollideH += Player_OnCollideH;
            IL.Celeste.Player.OnCollideV += Player_OnCollideV;
            IL.Celeste.Player.SideBounce += Player_SideBounce;
            IL.Celeste.Player.StarFlyUpdate += Player_StarFlyUpdate;
            IL.Celeste.Player.SuperBounce += Player_SuperBounce;
            IL.Celeste.Player.SwimCheck += Player_SwimCheck;
            IL.Celeste.Player.SwimJumpCheck += Player_SwimJumpCheck;
            IL.Celeste.Player.SwimRiseCheck += Player_SwimRiseCheck;
            IL.Celeste.Player.SwimUnderwaterCheck += Player_SwimUnderwaterCheck;

            On.Celeste.Player.ctor += Player_ctor;
            On.Celeste.Player.Added += Player_Added;
            On.Celeste.Player.BeforeUpTransition += Player_BeforeUpTransition;
            On.Celeste.Player.BeforeDownTransition += Player_BeforeDownTransition;
            On.Celeste.Player.DreamDashCheck += Player_DreamDashCheck;
            On.Celeste.Player.DreamDashUpdate += Player_DreamDashUpdate;
            On.Celeste.Player.JumpThruBoostBlockedCheck += Player_JumpThruBoostBlockedCheck;
            On.Celeste.Player.ReflectBounce += Player_ReflectBounce;
            On.Celeste.Player.Render += Player_Render;
            On.Celeste.Player.SlipCheck += Player_SlipCheck;
            On.Celeste.Player.TransitionTo += Player_TransitionTo;
            On.Celeste.Player.Update += Player_Update;

            hook_Player_orig_Update = new ILHook(ReflectionCache.PlayerOrigUpdateMethodInfo, Player_orig_Update);
            hook_Player_orig_UpdateSprite = new ILHook(ReflectionCache.UpdateSpriteMethodInfo, Player_orig_UpdateSprite);
            hook_Player_DashCoroutine = new ILHook(ReflectionCache.PlayerDashCoroutineMethodInfo.GetStateMachineTarget(), Player_DashCoroutine);
        }

        public static void Unload()
        {
            IL.Celeste.Player.Bounce -= Player_Bounce;
            IL.Celeste.Player.ClimbCheck -= Player_ClimbCheck;
            IL.Celeste.Player.ClimbHopBlockedCheck -= Player_ClimbHopBlockedCheck;
            IL.Celeste.Player.ClimbUpdate -= Player_ClimbUpdate;
            IL.Celeste.Player._IsOverWater -= Player_IsOverWater;
            IL.Celeste.Player.Jump -= Player_Jump;
            IL.Celeste.Player.NormalUpdate -= Player_NormalUpdate;
            IL.Celeste.Player.OnCollideH -= Player_OnCollideH;
            IL.Celeste.Player.OnCollideV -= Player_OnCollideV;
            IL.Celeste.Player.SideBounce -= Player_SideBounce;
            IL.Celeste.Player.StarFlyUpdate -= Player_StarFlyUpdate;
            IL.Celeste.Player.SuperBounce -= Player_SuperBounce;
            IL.Celeste.Player.SwimCheck -= Player_SwimCheck;
            IL.Celeste.Player.SwimJumpCheck -= Player_SwimJumpCheck;
            IL.Celeste.Player.SwimRiseCheck -= Player_SwimRiseCheck;
            IL.Celeste.Player.SwimUnderwaterCheck -= Player_SwimUnderwaterCheck;

            On.Celeste.Player.ctor -= Player_ctor;
            On.Celeste.Player.Added -= Player_Added;
            On.Celeste.Player.BeforeUpTransition -= Player_BeforeUpTransition;
            On.Celeste.Player.BeforeDownTransition -= Player_BeforeDownTransition;
            On.Celeste.Player.DreamDashCheck -= Player_DreamDashCheck;
            On.Celeste.Player.DreamDashUpdate -= Player_DreamDashUpdate;
            On.Celeste.Player.JumpThruBoostBlockedCheck -= Player_JumpThruBoostBlockedCheck;
            On.Celeste.Player.ReflectBounce -= Player_ReflectBounce;
            On.Celeste.Player.Render -= Player_Render;
            On.Celeste.Player.SlipCheck -= Player_SlipCheck;
            On.Celeste.Player.TransitionTo -= Player_TransitionTo;
            On.Celeste.Player.Update -= Player_Update;

            hook_Player_orig_Update?.Dispose();
            hook_Player_orig_Update = null;

            hook_Player_orig_UpdateSprite?.Dispose();
            hook_Player_orig_UpdateSprite = null;

            hook_Player_DashCoroutine?.Dispose();
            hook_Player_DashCoroutine = null;
        }

        private static bool Player_JumpThruBoostBlockedCheck(On.Celeste.Player.orig_JumpThruBoostBlockedCheck orig, Player self) =>
            GravityHelperModule.ShouldInvert || orig(self);

        #region IL Hooks

        private static void Player_Bounce(ILContext il)
        {
            var cursor = new ILCursor(il);

            // this.MoveVExact((int) ((double) fromY - (double) this.Bottom));
            cursor.ReplaceBottomWithDelegate();
        }

        private static void Player_ClimbCheck(ILContext il)
        {
            var cursor = new ILCursor(il);

            // replace Y
            cursor.ReplaceAdditionWithDelegate();

            // skip X
            cursor.GotoNextAddition(MoveType.After);

            // replace Y
            cursor.ReplaceAdditionWithDelegate();
        }

        private static void Player_ClimbHopBlockedCheck(ILContext il) => new ILCursor(il).ReplaceSubtractionWithDelegate();

        private static void Player_ClimbUpdate(ILContext il)
        {
            var cursor = new ILCursor(il);

            // if (this.CollideCheck<Solid>(this.Position - Vector2.UnitY) || this.ClimbHopBlockedCheck() && this.SlipCheck(-1f))
            cursor.GotoNext(MoveType.After, instr => Extensions.UnitYPredicate(instr) && Extensions.SubtractionPredicate(instr.Next));
            cursor.ReplaceSubtractionWithDelegate();

            // if (Input.MoveY.Value != 1 && (double) this.Speed.Y > 0.0 && !this.CollideCheck<Solid>(this.Position + new Vector2((float) this.Facing, 1f)))
            cursor.ReplaceAdditionWithDelegate();
        }

        private static void Player_DashCoroutine(ILContext il)
        {
            var cursor = new ILCursor(il);

            // if (player.onGround && (double) player.DashDir.X != 0.0 && ((double) player.DashDir.Y > 0.0 && (double) player.Speed.Y > 0.0) && (!player.Inventory.DreamDash || !player.CollideCheck<DreamBlock>(player.Position + Vector2.UnitY)))
            cursor.ReplaceAdditionWithDelegate();

            // SlashFx.Burst(player.Center, player.DashDir.Angle());
            cursor.GotoNext(instr => instr.MatchCall<SlashFx>(nameof(SlashFx.Burst)));
            cursor.Index--;
            cursor.EmitInvertVectorDelegate();
        }

        private static void Player_IsOverWater(ILContext il)
        {
            var cursor = new ILCursor(il);
            cursor.GotoNext(MoveType.After, instr => instr.MatchLdloc(0));
            cursor.EmitDelegate<Func<Rectangle, Rectangle>>(r =>
            {
                if (GravityHelperModule.ShouldInvert) r.Y -= 2;
                return r;
            });
        }

        private static void Player_Jump(ILContext il)
        {
            var cursor = new ILCursor(il);

            // Platform platformByPriority = SurfaceIndex.GetPlatformByPriority(this.CollideAll<Platform>(this.Position + Vector2.UnitY, this.temp));
            cursor.ReplaceAdditionWithDelegate();

            // Dust.Burst(this.BottomCenter, -1.5707964f, 4, this.DustParticleFromSurfaceIndex(index));
            cursor.ReplaceBottomCenterWithDelegate();
        }

        private static void Player_NormalUpdate(ILContext il)
        {
            var cursor = new ILCursor(il);

            // if (!this.CollideCheck<Solid>(this.Position + Vector2.UnitY * (float) -index) && this.ClimbCheck((int) this.Facing, -index))
            cursor.GotoNextUnitY(MoveType.After);
            cursor.ReplaceAdditionWithDelegate();

            // if ((water = this.CollideFirst<Water>(this.Position + Vector2.UnitY * 2f)) != null)
            cursor.GotoNextUnitY(MoveType.After);
            cursor.ReplaceAdditionWithDelegate();
        }

        private static void Player_OnCollideH(ILContext il)
        {
            var cursor = new ILCursor(il);

            // (SKIP) if (this.onGround && this.DuckFreeAt(this.Position + Vector2.UnitX * (float) Math.Sign(this.Speed.X)))
            cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>(nameof(Player.DuckFreeAt)));

            // if (!this.CollideCheck<Solid>(this.Position + new Vector2((float) Math.Sign(this.Speed.X), (float) (index1 * index2))))
            cursor.ReplaceAdditionWithDelegate();
        }

        private static void Player_OnCollideV(ILContext il)
        {
            var cursor = new ILCursor(il);

            /*
             * if (this.DashAttacking && (double) data.Direction.Y == (double) Math.Sign(this.DashDir.Y))
             */

            // ensure the check uses the real dash direction
            cursor.ReplaceSignWithDelegate();

            /*
             * if (!this.CollideCheck<Solid>(this.Position + new Vector2((float) -index, -1f)))
             * {
             *   this.Position = this.Position + new Vector2((float) -index, -1f);
             *   return;
             * }
             */

            // ensure ceiling correction works
            cursor.GotoNext(instr => instr.MatchCall<Entity>(nameof(Entity.CollideCheck)));
            cursor.Goto(cursor.Index - 2);
            cursor.ReplaceAdditionWithDelegate(4);

            /*
             * if ((double) this.Speed.Y < 0.0)
             * {
             *   int num = 4;
             *   if (this.DashAttacking && (double) Math.Abs(this.Speed.X) < 0.009999999776482582)
             */

            // insert code to stop at jumpthrus
            cursor.GotoNext(instr => instr.MatchCallvirt<Player>("DreamDashCheck"));
            cursor.GotoPrev(instr => instr.MatchLdarg(0));
            var dreamDashCheck = cursor.Next;
            cursor.GotoPrev(instr => instr.MatchCallvirt<Player>("get_DashAttacking"));
            cursor.GotoPrev(instr => instr.MatchLdcI4(4));
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Func<Player, bool>>(p =>
            {
                if (!GravityHelperModule.ShouldInvert) return true;
                var jumpthru = p.CollideFirstOutside<JumpThru>(p.Position + Vector2.UnitY);
                var shouldStop = jumpthru != null && p.Bottom <= jumpthru.Top;
                if (!shouldStop)
                {
                    p.SetVarJumpTimer(0);
                    FieldInfo field = typeof (Player).GetField("lastClimbMove", BindingFlags.Instance | BindingFlags.NonPublic);
                    field.SetValue(p, 0);
                }
                return !shouldStop;
            });
            cursor.Emit(OpCodes.Brfalse_S, dreamDashCheck);
        }

        private static void Player_orig_Update(ILContext il)
        {
            var cursor = new ILCursor(il);

            /*
             * else if ((double) this.Speed.Y >= 0.0)
             * {
             *   Platform platform = (Platform) this.CollideFirst<Solid>(this.Position + Vector2.UnitY) ?? (Platform) this.CollideFirstOutside<JumpThru>(this.Position + Vector2.UnitY);
             *   if (platform != null)
             */

            // ensure we check ground collisions the right direction
            cursor.ReplaceAdditionWithDelegate(2);

            // prevent Madeline from attempting to stand on the underside of jumpthrus
            cursor.GotoNext(instr => instr.MatchLdloc(1));
            var platformNotEqualNull = cursor.Next;
            cursor.GotoPrev(instr => instr.MatchLdarg(0) && instr.Next.MatchLdarg(0));
            cursor.EmitDelegate<Func<bool>>(() => GravityHelperModule.ShouldInvert);
            cursor.Emit(OpCodes.Brtrue_S, platformNotEqualNull);
            cursor.Goto(platformNotEqualNull);

            /*
             * else if (this.onGround && (this.CollideCheck<Solid, NegaBlock>(this.Position + Vector2.UnitY) || this.CollideCheckOutside<JumpThru>(this.Position + Vector2.UnitY)) && (!this.CollideCheck<Spikes>(this.Position) || SaveData.Instance.Assists.Invincible))
             */

            // ensure we check ground collisions the right direction for refilling dash
            cursor.ReplaceAdditionWithDelegate();

            // TODO: ignore jumpthrus if inverted... is crashing?
            cursor.GotoNextAddition(MoveType.After);
            // cursor.Index++;
            // cursor.EmitDelegate<Func<bool, bool>>(b => b && !GravityHelperModule.ShouldInvert);

            // (SKIP) else if (!this.CollideCheck<Solid>(this.Position + Vector2.UnitX * (float) Math.Sign(this.wallSpeedRetained)))
            cursor.GotoNextAddition(MoveType.After);

            // (SKIP) else if (!this.CollideCheck<Solid>(this.Position + Vector2.UnitX * (float) this.hopWaitX))
            cursor.GotoNextAddition(MoveType.After);

            /*
             * if (!this.onGround && this.DashAttacking && (double) this.DashDir.Y == 0.0 && (this.CollideCheck<Solid>(this.Position + Vector2.UnitY * 3f) || this.CollideCheckOutside<JumpThru>(this.Position + Vector2.UnitY * 3f)))
             */

            // fix inverted ground correction for dashing (may need to ignore jumpthrus later)
            cursor.ReplaceAdditionWithDelegate(2);

            /*
             * if (water != null && (double) this.Center.Y < (double) water.Center.Y)
             */

            // invert Center.Y check (fixes Madeline slamming into the ground when climbing down into water)
            cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("SwimCheck"));
            cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("SwimCheck"));
            cursor.GotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("SwimCheck"));
            cursor.GotoNext(MoveType.After, instr => instr.MatchLdfld<Vector2>(nameof(Vector2.Y)));
            cursor.EmitInvertFloatDelegate();
            cursor.GotoNext(MoveType.After, instr => instr.MatchLdfld<Vector2>(nameof(Vector2.Y)));
            cursor.EmitInvertFloatDelegate();
        }

        private static void Player_orig_UpdateSprite(ILContext il)
        {
            var cursor = new ILCursor(il);

            // fix dangling animation
            cursor.ReplaceAdditionWithDelegate();

            // skip push check
            cursor.GotoNextAddition(MoveType.After);

            // fix edge animation
            cursor.ReplaceAdditionWithDelegate(3);

            // fix edgeBack animation
            cursor.ReplaceAdditionWithDelegate(3);
        }

        private static void Player_SideBounce(ILContext il)
        {
            var cursor = new ILCursor(il);

            // this.MoveV(Calc.Clamp(fromY - this.Bottom, -4f, 4f));
            cursor.ReplaceBottomWithDelegate();
        }

        private static void Player_StarFlyUpdate(ILContext il)
        {
            var cursor = new ILCursor(il);

            // this.level.Particles.Emit(FlyFeather.P_Flying, 1, this.Center, Vector2.One * 2f, (-this.Speed).Angle());
            cursor.GotoNext(instr => instr.MatchCallvirt<ParticleSystem>(nameof(ParticleSystem.Emit)));
            cursor.Index--;
            cursor.EmitInvertVectorDelegate();
        }

        private static void Player_SuperBounce(ILContext il)
        {
            var cursor = new ILCursor(il);

            // this.MoveV(fromY - this.Bottom);
            cursor.ReplaceBottomWithDelegate();
        }

        private static void Player_SwimCheck(ILContext il)
        {
            var cursor = new ILCursor(il);
            cursor.ReplaceAdditionWithDelegate();
        }

        private static void Player_SwimJumpCheck(ILContext il)
        {
            var cursor = new ILCursor(il);
            cursor.ReplaceAdditionWithDelegate();
        }

        private static void Player_SwimRiseCheck(ILContext il)
        {
            var cursor = new ILCursor(il);
            cursor.ReplaceAdditionWithDelegate();
        }

        private static void Player_SwimUnderwaterCheck(ILContext il)
        {
            var cursor = new ILCursor(il);
            cursor.ReplaceAdditionWithDelegate();
        }

        #endregion

        #region On Hooks

        private static void Player_Added(On.Celeste.Player.orig_Added orig, Player self, Scene scene)
        {
            orig(self, scene);

            SpawnGravityTrigger trigger = self.CollideFirstOrDefault<SpawnGravityTrigger>();
            GravityHelperModule.Session.Gravity = trigger?.GravityType ?? GravityHelperModule.Session.PreviousGravity;
        }

        private static void Player_BeforeDownTransition(On.Celeste.Player.orig_BeforeDownTransition orig, Player self)
        {
            if (!GravityHelperModule.ShouldInvert)
            {
                orig(self);
                return;
            }

            // FIXME: copied from Player.BeforeUpTransition - we never call orig!
            self.Speed.X = 0.0f;
            if (self.StateMachine.State != Player.StRedDash && self.StateMachine.State != Player.StReflectionFall && self.StateMachine.State != Player.StStarFly)
            {
                self.SetVarJumpSpeed(self.Speed.Y = -105f);
                self.StateMachine.State = self.StateMachine.State != Player.StSummitLaunch ? Player.StNormal : Player.StIntroJump;
                self.AutoJump = true;
                self.AutoJumpTimer = 0.0f;
                self.SetVarJumpTimer(0.2f);
            }
            self.SetDashCooldownTimer(0.2f);
        }

        private static void Player_BeforeUpTransition(On.Celeste.Player.orig_BeforeUpTransition orig, Player self)
        {
            if (!GravityHelperModule.ShouldInvert)
            {
                orig(self);
                return;
            }

            // FIXME: copied from Player.BeforeDownTransition - we never call orig!
            if (self.StateMachine.State != Player.StRedDash && self.StateMachine.State != Player.StReflectionFall && self.StateMachine.State != Player.StStarFly)
            {
                self.StateMachine.State = Player.StNormal;
                self.Speed.Y = Math.Max(0.0f, self.Speed.Y);
                self.AutoJump = false;
                self.SetVarJumpTimer(0.0f);
            }
            foreach (Entity entity in self.Scene.Tracker.GetEntities<Celeste.Platform>())
            {
                if (!(entity is SolidTiles) && self.CollideCheckOutside(entity, self.Position - Vector2.UnitY * self.Height))
                    entity.Collidable = false;
            }
        }

        private static void Player_ctor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position,
            PlayerSpriteMode spriteMode)
        {
            orig(self, position, spriteMode);

            self.Add(new TransitionListener
            {
                OnOutBegin = () => GravityHelperModule.Session.PreviousGravity = GravityHelperModule.Session.Gravity,
            }, new GravityListener());
        }

        private static bool Player_DreamDashCheck(On.Celeste.Player.orig_DreamDashCheck orig, Player self, Vector2 dir)
        {
            if (!GravityHelperModule.ShouldInvert)
                return orig(self, dir);

            self.Speed.Y *= -1;
            self.DashDir.Y *= -1;

            var rv = orig(self, new Vector2(dir.X, -dir.Y));

            self.Speed.Y *= -1;
            self.DashDir.Y *= -1;

            return rv;
        }

        private static int Player_DreamDashUpdate(On.Celeste.Player.orig_DreamDashUpdate orig, Player self)
        {
            if (!GravityHelperModule.ShouldInvert)
                return orig(self);

            self.Speed.Y *= -1;
            self.DashDir.Y *= -1;

            var rv = orig(self);

            self.Speed.Y *= -1;
            self.DashDir.Y *= -1;

            return rv;
        }

        private static void Player_ReflectBounce(On.Celeste.Player.orig_ReflectBounce orig, Player self, Vector2 direction) =>
            orig(self, GravityHelperModule.ShouldInvert ? new Vector2(direction.X, -direction.Y) : direction);

        private static void Player_Render(On.Celeste.Player.orig_Render orig, Player self)
        {
            var scaleY = self.Sprite.Scale.Y;

            if (GravityHelperModule.ShouldInvert)
                self.Sprite.Scale.Y = -scaleY;

            orig(self);

            if (GravityHelperModule.ShouldInvert)
                self.Sprite.Scale.Y = scaleY;
        }

        private static bool Player_SlipCheck(On.Celeste.Player.orig_SlipCheck orig, Player self, float addY)
        {
            if (!GravityHelperModule.ShouldInvert)
                return orig(self, addY);

            Vector2 point = self.Facing != Facings.Right ? self.BottomLeft - Vector2.UnitX - Vector2.UnitY * (4f + addY) : self.BottomRight - Vector2.UnitY * (4f + addY);
            return !self.Scene.CollideCheck<Solid>(point) && !self.Scene.CollideCheck<Solid>(point - Vector2.UnitY * (addY - 4f));
        }

        private static bool Player_TransitionTo(On.Celeste.Player.orig_TransitionTo orig, Player self, Vector2 target,
            Vector2 direction)
        {
            GravityHelperModule.Transitioning = true;
            bool val = orig(self, target, direction);
            GravityHelperModule.Transitioning = false;
            return val;
        }

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            if (!GravityHelperModule.ShouldInvert)
            {
                orig(self);
                return;
            }

            var aimY = Input.Aim.Value.Y;
            var moveY = Input.MoveY.Value;

            Input.Aim.SetValue(new Vector2(Input.Aim.Value.X, -aimY));
            Input.MoveY.Value = -moveY;

            orig(self);

            Input.MoveY.Value = moveY;
            Input.Aim.SetValue(new Vector2(Input.Aim.Value.X, aimY));
        }

        #endregion
    }
}