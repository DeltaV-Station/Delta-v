using Content.Client.Overlays;
using Content.Shared._DV.MedicalRecords;
using Content.Shared._DV.Overlays;
using Content.Shared.Access.Systems;
using Content.Shared.Overlays;
using Content.Shared.StatusIcon;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.Overlays;

public sealed class ShowTriageIconsSystem : EquipmentHudSystem<ShowTriageIconsComponent>
{
    [Dependency] private readonly SharedIdCardSystem _idCard = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private static readonly ProtoId<HealthIconPrototype> Minor = "TriageStatusMinor";
    private static readonly ProtoId<HealthIconPrototype> Delayed = "TriageStatusDelayed";
    private static readonly ProtoId<HealthIconPrototype> Immediate = "TriageStatusImmediate";
    private static readonly ProtoId<HealthIconPrototype> Expectant = "TriageStatusExpectant";

    private static readonly ProtoId<HealthIconPrototype> TriageClaimedYoursId = "TriageClaimedYours";
    private static readonly ProtoId<HealthIconPrototype> TriageClaimedOthersId = "TriageClaimedOthers";
    private static readonly ProtoId<HealthIconPrototype> TriageUnclaimedId = "TriageUnclaimed";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MedicalRecordComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(Entity<MedicalRecordComponent> ent, ref GetStatusIconsEvent ev)
    {
        if (!IsActive)
            return;

        var triageStatusIcon = ent.Comp.Record.Status switch
        {
            TriageStatus.None => null,
            TriageStatus.Minor => _prototype.Index(Minor),
            TriageStatus.Delayed => _prototype.Index(Delayed),
            TriageStatus.Immediate => _prototype.Index(Immediate),
            TriageStatus.Expectant => _prototype.Index(Expectant),
            _ => null,
        };

        if (triageStatusIcon is not { } statusPrototype)
            return;

        ev.StatusIcons.Add(statusPrototype);

        if (ent.Comp.Record.ClaimedName is not { } claimedName)
        {
            ev.StatusIcons.Add(_prototype.Index(TriageUnclaimedId));
            return;
        }

        if (_player.LocalEntity is { } local && _idCard.TryFindIdCard(local, out var idCard) && idCard.Comp.FullName == claimedName)
        {
            ev.StatusIcons.Add(_prototype.Index(TriageClaimedYoursId));
            return;
        }

        ev.StatusIcons.Add(_prototype.Index(TriageClaimedOthersId));
    }
}
