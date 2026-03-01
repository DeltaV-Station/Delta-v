using Content.Server.Bible.Components;
using JetBrains.Annotations;
using Robust.Shared.Timing;

namespace Content.Server._DV.BibleUser;

/// <summary>
/// A system that tracks if the BibleUser has used a bible in the last X seconds (see <see cref="BibleUserComponent.Cooldown"/>).
/// This system is used to reset that cooldown.
/// <br />
/// <br />
/// See changes in Content.Server/Bible/BibleSystem.cs around the BibleUser component.
/// This system can easily be moved to shared once the upstream bible system is moved to shared.
/// </summary>
public sealed class BibleUserSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BibleUserComponent, ComponentInit>(OnComponentInit);
    }

    /// <summary>
    /// Puts the usage of the Bible on cooldown.
    /// </summary>
    /// <param name="entity">The entity that may have the BibleUserComponent to put on cooldown.</param>
    /// <param name="cooldown">If specified, the amount of time that the bible will be on cooldown.
    /// If null, <see cref="BibleUserComponent.Cooldown"/> will be used.</param>
    [PublicAPI]
    public void StartBibleCooldown(Entity<BibleUserComponent?> entity, TimeSpan? cooldown = null)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        // If they can't heal yet, don't reset the cooldown
        if (!entity.Comp.CanHeal)
            return;

        if (!cooldown.HasValue)
            cooldown = entity.Comp.Cooldown;

        entity.Comp.CanHeal = false;
        entity.Comp.NextUse = _timing.CurTime + cooldown.Value;
    }

    private void OnComponentInit(Entity<BibleUserComponent> entity, ref ComponentInit args)
    {
        entity.Comp.NextUse = _timing.CurTime;
    }

    /// <summary>
    /// Updates if enough time has passed and the chaplain can use their bible again.
    /// </summary>
    /// <param name="frameTime"></param>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BibleUserComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // If they can heal, we don't need to check if the cooldown time.
            if (comp.CanHeal)
                return;

            if (_timing.CurTime > comp.NextUse)
            {
                comp.CanHeal = true;
            }
        }
    }
}
