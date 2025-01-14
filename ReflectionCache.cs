// Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.GravityHelper
{
    // ReSharper disable InconsistentNaming
    internal static class ReflectionCache
    {
        private static List<Type> _loadableTypes;
        public static IEnumerable<Type> LoadableTypes => _loadableTypes ??= typeof(ReflectionCache).Assembly.GetTypesSafe().ToList();

        public static Type GetTypeByName(string name) =>
            AppDomain.CurrentDomain.GetAssemblies().Reverse().Select(a => a.GetType(name)).FirstOrDefault(t => t != null);

        public static Type GetModdedTypeByName(string module, string name)
        {
            var mod = Everest.Modules.FirstOrDefault(m => m.Metadata.Name == module);
            return mod?.GetType().Assembly.GetType(name);
        }

        #region Player

        public static readonly FieldInfo Player_DashCooldownTimer = typeof(Player).GetField("dashCooldownTimer", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Player_DuckHitbox = typeof(Player).GetField("duckHitbox", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Player_DuckHurtbox = typeof(Player).GetField("duckHurtbox", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Player_DuckingLightOffset = typeof(Player).GetField("duckingLightOffset", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Player_LastClimbMove = typeof (Player).GetField("lastClimbMove", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Player_NormalHitbox = typeof(Player).GetField("normalHitbox", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Player_NormalHurtbox = typeof(Player).GetField("normalHurtbox", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Player_NormalLightOffset = typeof(Player).GetField("normalLightOffset", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Player_StarFlyBloom = typeof(Player).GetField("starFlyBloom", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Player_StarFlyHitbox = typeof(Player).GetField("starFlyHitbox", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Player_StarFlyHurtbox = typeof(Player).GetField("starFlyHurtbox", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Player_VarJumpSpeed = typeof(Player).GetField("varJumpSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Player_VarJumpTimer = typeof(Player).GetField("varJumpTimer", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Player_CanUnDuck = typeof(Player).GetMethod("get_CanUnDuck", BindingFlags.Instance | BindingFlags.Public);
        public static readonly MethodInfo Player_CameraTarget = typeof(Player).GetMethod("get_CameraTarget", BindingFlags.Instance | BindingFlags.Public);
        public static readonly MethodInfo Player_DashCoroutine = typeof(Player).GetMethod("DashCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Player_DashCorrectCheck = typeof(Player).GetMethod("DashCorrectCheck", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Player_IntroJumpCoroutine = typeof(Player).GetMethod("IntroJumpCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Player_OrigUpdateSprite = typeof(Player).GetMethod("orig_UpdateSprite", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Player_OrigWallJump = typeof(Player).GetMethod("orig_WallJump", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Player_OrigUpdate = typeof(Player).GetMethod(nameof(Player.orig_Update));
        public static readonly MethodInfo Player_PickupCoroutine = typeof(Player).GetMethod("PickupCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly PropertyInfo Player_OnSafeGround = typeof(Player).GetProperty(nameof(Player.OnSafeGround));

        #endregion

        #region Misc

        public static readonly FieldInfo Actor_MovementCounter = typeof (Actor).GetField("movementCounter", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Spring_PlayerCanUse = typeof(Spring).GetField("playerCanUse", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly FieldInfo Sprite_Animations = typeof(Sprite).GetField("animations", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo FinalBoss_MoveSequence = typeof(FinalBoss).GetMethod("MoveSequence", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo FlyFeather_CollectRoutine = typeof(FlyFeather).GetMethod("CollectRoutine", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Booster_BoostRoutine = typeof(Booster).GetMethod("BoostRoutine", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo BadelineBoost_BoostRoutine = typeof(BadelineBoost).GetMethod("BoostRoutine", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Glider_OnCollideH = typeof(Glider).GetMethod("OnCollideH", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Glider_OnCollideV = typeof(Glider).GetMethod("OnCollideV", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Level_NextLevel = typeof(Level).GetMethod("NextLevel", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Level_OrigTransitionRoutine = typeof(Level).GetMethod("orig_TransitionRoutine", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo MoveBlock_AddImage = typeof(MoveBlock).GetMethod("AddImage", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo PlayerDeadBody_DeathRoutine = typeof(PlayerDeadBody).GetMethod("DeathRoutine", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Puffer_GotoHitSpeed = typeof(Puffer).GetMethod("GotoHitSpeed", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Puffer_Alert = typeof(Puffer).GetMethod("Alert", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Seeker_RegenerateCoroutine = typeof(Seeker).GetMethod("RegenerateCoroutine", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo SolidTiles_SurfaceSoundIndexAt = typeof(SolidTiles).GetMethod("SurfaceSoundIndexAt", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo Spring_BounceAnimate = typeof(Spring).GetMethod("BounceAnimate", BindingFlags.Instance | BindingFlags.NonPublic);
        public static readonly MethodInfo VirtualJoystick_SetValue = typeof(VirtualJoystick).GetProperty("Value")?.GetSetMethod(true);
        public static readonly object[] VirtualJoystick_SetValue_Params = { Vector2.Zero };

        #endregion

        #region Optional Dependencies

        public static Type MaddieHelpingHandUpsideDownJumpThruType { get; private set; }
        public static Type MaddieHelpingHandGroupedTriggerSpikesType { get; private set; }
        public static Type FancyFallingBlockType { get; private set; }
        public static MethodInfo FancyFallingBlock_SurfaceSoundIndexAt { get; private set; }
        public static Type MaddyCrownModuleType { get; private set; }
        public static Type FrostHelperCustomSpringType { get; private set; }
        public static Type OutbackHelperPortalType { get; private set; }
        public static Type CatelineModuleType { get; private set; }
        public static FieldInfo CatelineModuleInstanceFieldInfo { get; private set; }
        public static FieldInfo CatelineModuleTailNodesFieldInfo { get; private set; }
        public static Type CommunalHelperConnectedSolidType { get; private set; }
        public static Type CommunalHelperTimedTriggerSpikesType { get; private set; }
        public static Type ExtendedVariantsDashTrailAllTheTimeType { get; private set; }
        public static Type ExtendedVariantsJumpIndicatorType { get; private set; }
        public static Type ExtendedVariantsDashCountIndicatorType { get; private set; }
        public static Type ExtendedVariantsJumpCountType { get; private set; }
        public static MethodInfo ExtendedVariantsJumpCountGetJumpBufferMethodInfo { get; private set; }
        public static MethodInfo ExtendedVariantsJumpCountSetJumpCountMethodInfo { get; private set; }
        public static Type StaminaMeterSmallStaminaMeterDisplayType { get; private set; }
        public static Type BunnelineModuleType { get; private set; }
        public static Type CelesteNetGhostType { get; private set; }
        public static Type CelesteNetGhostNameTagType { get; private set; }
        public static Type CelesteNetGhostEmoteType { get; private set; }
        public static Type BounceHelperModuleType { get; private set; }

        public static void LoadThirdPartyTypes()
        {
            FancyFallingBlockType = GetModdedTypeByName("FancyTileEntities", "Celeste.Mod.FancyTileEntities.FancyFallingBlock");
            FancyFallingBlock_SurfaceSoundIndexAt = FancyFallingBlockType?.GetMethod("SurfaceSoundIndexAt", BindingFlags.Instance | BindingFlags.NonPublic);
            MaddyCrownModuleType = GetModdedTypeByName("MaddyCrown", "Celeste.Mod.MaddyCrown.MaddyCrownModule");
            MaddieHelpingHandUpsideDownJumpThruType = GetModdedTypeByName("MaxHelpingHand", "Celeste.Mod.MaxHelpingHand.Entities.UpsideDownJumpThru");
            MaddieHelpingHandGroupedTriggerSpikesType = GetModdedTypeByName("MaxHelpingHand", "Celeste.Mod.MaxHelpingHand.Entities.GroupedTriggerSpikes");
            FrostHelperCustomSpringType = GetModdedTypeByName("FrostHelper", "FrostHelper.CustomSpring");
            OutbackHelperPortalType = GetModdedTypeByName("OutbackHelper", "Celeste.Mod.OutbackHelper.Portal");
            CatelineModuleType = GetModdedTypeByName("Cateline", "Celeste.Mod.Cateline.CatelineModule");
            CatelineModuleInstanceFieldInfo = CatelineModuleType?.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
            CatelineModuleTailNodesFieldInfo = CatelineModuleType?.GetField("tailNodes", BindingFlags.Instance | BindingFlags.NonPublic);
            CommunalHelperConnectedSolidType = GetModdedTypeByName("CommunalHelper", "Celeste.Mod.CommunalHelper.ConnectedSolid");
            CommunalHelperTimedTriggerSpikesType = GetModdedTypeByName("CommunalHelper", "Celeste.Mod.CommunalHelper.Entities.TimedTriggerSpikes");
            ExtendedVariantsDashTrailAllTheTimeType = GetModdedTypeByName("ExtendedVariantMode", "ExtendedVariants.Variants.DashTrailAllTheTime");
            ExtendedVariantsJumpIndicatorType = GetModdedTypeByName("ExtendedVariantMode", "ExtendedVariants.Entities.JumpIndicator");
            ExtendedVariantsJumpIndicatorType = GetModdedTypeByName("ExtendedVariantMode", "ExtendedVariants.Entities.DashCountIndicator");
            ExtendedVariantsJumpCountType = GetModdedTypeByName("ExtendedVariantMode", "ExtendedVariants.Variants.JumpCount");
            ExtendedVariantsJumpCountGetJumpBufferMethodInfo = ExtendedVariantsJumpCountType?.GetMethod("GetJumpBuffer", BindingFlags.Public | BindingFlags.Static);
            ExtendedVariantsJumpCountSetJumpCountMethodInfo = ExtendedVariantsJumpCountType?.GetMethod("SetJumpCount", BindingFlags.Public | BindingFlags.Static);
            StaminaMeterSmallStaminaMeterDisplayType = GetModdedTypeByName("StaminaMeter", "Celeste.Mod.StaminaMeter.SmallStaminaMeterDisplay");
            BunnelineModuleType = GetModdedTypeByName("Bunneline", "Celeste.Mod.Bunneline.BunnelineModule");
            CelesteNetGhostType = GetModdedTypeByName("CelesteNet.Client", "Celeste.Mod.CelesteNet.Client.Entities.Ghost");
            CelesteNetGhostNameTagType = GetModdedTypeByName("CelesteNet.Client", "Celeste.Mod.CelesteNet.Client.Entities.GhostNameTag");
            CelesteNetGhostEmoteType = GetModdedTypeByName("CelesteNet.Client", "Celeste.Mod.CelesteNet.Client.Entities.GhostEmote");
            BounceHelperModuleType = GetModdedTypeByName("BounceHelper", "Celeste.Mod.BounceHelper.BounceHelperModule");
        }

        #endregion

        #region Reflection Extensions

        public static void SetLastClimbMove(this Player player, int value) => Player_LastClimbMove.SetValue(player, value);
        public static Hitbox GetNormalHitbox(this Player player) => (Hitbox) Player_NormalHitbox.GetValue(player);
        public static Hitbox GetNormalHurtbox(this Player player) => (Hitbox) Player_NormalHurtbox.GetValue(player);
        public static Hitbox GetDuckHitbox(this Player player) => (Hitbox) Player_DuckHitbox.GetValue(player);
        public static Hitbox GetDuckHurtbox(this Player player) => (Hitbox) Player_DuckHurtbox.GetValue(player);
        public static BloomPoint GetStarFlyBloom(this Player player) => (BloomPoint) Player_StarFlyBloom.GetValue(player);
        public static Hitbox GetStarFlyHitbox(this Player player) => (Hitbox) Player_StarFlyHitbox.GetValue(player);
        public static Hitbox GetStarFlyHurtbox(this Player player) => (Hitbox) Player_StarFlyHurtbox.GetValue(player);
        public static Dictionary<string, Sprite.Animation> GetAnimations(this Sprite sprite) => (Dictionary<string, Sprite.Animation>) Sprite_Animations.GetValue(sprite);
        public static void SetNormalLightOffset(this Player player, Vector2 value) => Player_NormalLightOffset.SetValue(player, value);
        public static void SetDuckingLightOffset(this Player player, Vector2 value) => Player_DuckingLightOffset.SetValue(player, value);
        public static void SetVarJumpTimer(this Player player, float value) => Player_VarJumpTimer.SetValue(player, value);
        public static void SetVarJumpSpeed(this Player player, float value) => Player_VarJumpSpeed.SetValue(player, value);
        public static void SetDashCooldownTimer(this Player player, float value) => Player_DashCooldownTimer.SetValue(player, value);
        public static void SetOnSafeGround(this Player player, bool value) => Player_OnSafeGround.SetValue(player, value);
        public static bool GetPlayerCanUse(this Spring spring) => (bool) Spring_PlayerCanUse.GetValue(spring);
        public static void CallNextLevel(this Level level, Vector2 at, Vector2 dir) => Level_NextLevel.Invoke(level, new object[]{at, dir});
        public static int CallSurfaceSoundIndexAt(this SolidTiles solidTiles, Vector2 readPosition) => (int) SolidTiles_SurfaceSoundIndexAt.Invoke(solidTiles, new object[] {readPosition});
        public static bool CallDashCorrectCheck(this Player player, Vector2 add) => (bool)Player_DashCorrectCheck.Invoke(player, new object[] {add});
        public static void CallBounceAnimate(this Spring spring) => Spring_BounceAnimate.Invoke(spring, new object[0]);
        public static void CallAddImage(this MoveBlock moveBlock, MTexture tex, Vector2 position, float rotation, Vector2 scale, List<Image> addTo) =>
            MoveBlock_AddImage.Invoke(moveBlock, new object[]
            {
                tex, position, rotation, scale, addTo,
            });

        public static int CallFancyFallingBlockSurfaceSoundIndexAt(this FallingBlock fallingBlock, Vector2 readPosition)
        {
            if (FancyFallingBlock_SurfaceSoundIndexAt == null) return -1;
            return (int) FancyFallingBlock_SurfaceSoundIndexAt.Invoke(fallingBlock, new object[] {readPosition});
        }

        public static void SetValue(this VirtualJoystick virtualJoystick, Vector2 value)
        {
            VirtualJoystick_SetValue_Params[0] = value;
            VirtualJoystick_SetValue.Invoke(virtualJoystick, VirtualJoystick_SetValue_Params);
        }

        #endregion
    }
}
