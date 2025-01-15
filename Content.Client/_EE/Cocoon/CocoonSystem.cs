using Content.Shared.Cocoon;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Shared.Containers;
using System.Numerics;

namespace Content.Client.Cocoon
{
    public sealed class CocoonSystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CocoonComponent, EntInsertedIntoContainerMessage>(OnCocEntInserted);
        }

        private void OnCocEntInserted(EntityUid uid, CocoonComponent component, EntInsertedIntoContainerMessage args)
        {
            if (!TryComp<SpriteComponent>(uid, out var cocoonSprite))
                return;

            if (TryComp<HumanoidAppearanceComponent>(args.Entity, out var humanoidAppearance)) // If humanoid, use height and width
                cocoonSprite.Scale = new Vector2(humanoidAppearance.Width, humanoidAppearance.Height);
            else if (!TryComp<SpriteComponent>(args.Entity, out var entSprite))
                return;
            else if (entSprite.BaseRSI != null) // Set scale based on sprite scale + sprite dimensions. Ideally we would somehow get a bounding box from the sprite size not including transparent pixels, but FUCK figuring that out.
                cocoonSprite.Scale = entSprite.Scale * (entSprite.BaseRSI.Size / 32);
            else if (entSprite.Scale != cocoonSprite.Scale) // if basersi somehow not found (?) just use scale
                cocoonSprite.Scale = entSprite.Scale;
        }
    }
}
