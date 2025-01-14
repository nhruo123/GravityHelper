// Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Celeste.Mod.GravityHelper.Entities.Controllers;
using Celeste.Mod.GravityHelper.Extensions;
using Monocle;

namespace Celeste.Mod.GravityHelper.Components
{
    [Tracked]
    public class GravityHoldable : Component
    {
        private float _invertTime = 2f;
        public float InvertTime
        {
            get => _invertTime;
            set => _invertTime = _invertTimeRemaining = value;
        }

        private float _invertTimeRemaining;

        public GravityHoldable() : base(true, false)
        {
        }

        public void ResetInvertTime() => _invertTimeRemaining = InvertTime;

        public void SetGravityHeld()
        {
            if (Entity?.Get<GravityComponent>() is not { } gravityComponent) return;

            ResetInvertTime();

            var targetGravity = GravityHelperModule.PlayerComponent?.CurrentGravity ?? GravityType.Normal;
            if (gravityComponent.CurrentGravity != targetGravity)
                gravityComponent.SetGravity(targetGravity);
        }

        public override void Added(Entity entity)
        {
            base.Added(entity);

            // the entity may already be part of the scene before the component is added
            if (entity.Scene != null)
                updateInvertTime(entity.Scene);

            entity.Add(new GravityListener(entity)
            {
                GravityChanged = (_, _) => ResetInvertTime(),
            });
        }

        public override void EntityAdded(Scene scene)
        {
            base.EntityAdded(scene);
            updateInvertTime(scene);
        }

        private void updateInvertTime(Scene scene)
        {
            var controller = scene.GetActiveController<BehaviorGravityController>();
            InvertTime = controller?.HoldableResetTime ?? 2f;
        }

        public override void Update()
        {
            base.Update();

            var holdable = Entity.Get<Holdable>();
            var gravityComponent = Entity.Get<GravityComponent>();
            if (holdable == null || gravityComponent == null) return;

            if (holdable.IsHeld)
                SetGravityHeld();
            else if (InvertTime > 0 && _invertTimeRemaining > 0 && gravityComponent.CurrentGravity == GravityType.Inverted)
            {
                _invertTimeRemaining -= Engine.DeltaTime;
                if (_invertTimeRemaining <= 0)
                    gravityComponent.SetGravity(GravityType.Normal);
            }
        }
    }
}
