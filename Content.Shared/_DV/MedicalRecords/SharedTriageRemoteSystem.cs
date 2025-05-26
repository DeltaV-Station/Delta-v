using Content.Shared.Interaction.Events;
using Content.Shared.Popups;

namespace Content.Shared._DV.MedicalRecords;

public abstract class SharedTriageRemoteSystem : EntitySystem
{
    [Dependency] protected readonly SharedPopupSystem _popup = default!;

    // todo localize this entire thing!
    public override void Initialize()
    {
        SubscribeLocalEvent<TriageRemoteComponent, UseInHandEvent>(OnInHandActivation);
    }

    private void OnInHandActivation(Entity<TriageRemoteComponent> ent, ref UseInHandEvent args)
    {
        string switchMessageId;
        switch (ent.Comp.Mode)
        {
            case OperatingMode.GiveDnr:
                ent.Comp.Mode = OperatingMode.GiveLow;
                switchMessageId = "todo-locstring-low";
                break;
            case OperatingMode.GiveLow:
                ent.Comp.Mode = OperatingMode.GiveHigh;
                switchMessageId = "todo-locstring-high";
                break;
            case OperatingMode.GiveHigh:
                ent.Comp.Mode = OperatingMode.GiveDnr;
                switchMessageId = "todo-locstring-dnr";
                break;
            default:
                throw new InvalidOperationException(
                    $"Tried to switch to operating mode {ent.Comp.Mode} when it's not a valid operating mode.");
        }

        Dirty(ent);
        _popup.PopupClient(Loc.GetString(switchMessageId), ent, args.User);
    }
}
