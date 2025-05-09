using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared._Shitmed.Body.Events;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Robust.Shared.Network;
using System.Numerics;

namespace Content.Shared._Shitmed.BodyEffects.Subsystems;

public sealed class GenerateChildPartSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GenerateChildPartComponent, MechanismAddedEvent>(OnAdded);
    }

    private void OnAdded(Entity<GenerateChildPartComponent> ent, ref MechanismAddedEvent args)
    {
        if (ent.Comp.Active || !TryComp<BodyPartComponent>(ent, out var partComp))
            return;

        // I pinky swear to also move this to the server side properly next update :)
        if (_net.IsClient)
            return;

        var childPart = Spawn(ent.Comp.Id, new EntityCoordinates(args.Body, Vector2.Zero));
        if (!TryComp(childPart, out BodyPartComponent? childPartComp))
            return;

        var slotName = _bodySystem.GetSlotFromBodyPart(childPartComp);
        _bodySystem.TryCreatePartSlot(ent.Owner, slotName, childPartComp.PartType, out var _);
        _bodySystem.AttachPart(ent.Owner, slotName, childPart, partComp, childPartComp);
        ent.Comp.ChildPart = childPart;
        ent.Comp.Active = true;
        Dirty(ent);
    }

    // Still unusued, gotta figure out what I want to do with this function outside of fuckery with mantis blades.
    private void DeletePart(EntityUid uid, GenerateChildPartComponent component)
    {
        if (!TryComp(uid, out BodyPartComponent? partComp))
            return;

        _bodySystem.DropSlotContents((uid, partComp));
        var ev = new BodyPartDroppedEvent((uid, partComp));
        RaiseLocalEvent(uid, ref ev);
        QueueDel(uid);
    }
}

