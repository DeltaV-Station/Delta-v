using Content.Shared.Containers.ItemSlots;
using Content.Shared._DV.Biscuit;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;

namespace Content.Server._DV.Biscuit;

public sealed class BiscuitSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly ItemSlotsSystem _slotSystem = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BiscuitComponent, GetVerbsEvent<AlternativeVerb>>(AddCrackVerb);
    }

    private void AddCrackVerb(EntityUid uid, BiscuitComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || component.Cracked)
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                CrackBiscuit(uid, component);
            },
            Text = Loc.GetString("biscuit-verb-crack"),
            Priority = 2
        };
        args.Verbs.Add(verb);
    }

    private void CrackBiscuit(EntityUid uid, BiscuitComponent component)
    {
        component.Cracked = true;

        _appearanceSystem.SetData(uid, BiscuitStatus.Cracked, true);

        _audioSystem.PlayPvs("/Audio/_DV/Effects/crack1.ogg", uid,
            AudioParams.Default.WithVariation(0.2f).WithVolume(-4f));

        _slotSystem.SetLock(uid, "PaperSlip", false);
    }
}
