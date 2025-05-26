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

    private static ProtoId<HealthIconPrototype> Dnr = "TriageStatusDnr";
    private static ProtoId<HealthIconPrototype> Low = "TriageStatusLow";
    private static ProtoId<HealthIconPrototype> High = "TriageStatusHigh";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MedicalRecordComponent, GetStatusIconsEvent>(OnGetStatusIconsEvent);
    }

    private void OnGetStatusIconsEvent(Entity<MedicalRecordComponent> ent, ref GetStatusIconsEvent ev)
    {
        if (!IsActive)
            return;

        var triageStatusIcon = ent.Comp.Record.Status switch {
            TriageStatus.Normal => null,
            TriageStatus.Dnr => _prototype.Index(Dnr),
            TriageStatus.Low => _prototype.Index(Low),
            TriageStatus.High => _prototype.Index(High),
        };

        if (triageStatusIcon is {} statusPrototype)
        {
            ev.StatusIcons.Add(statusPrototype);
        }

        if (ent.Comp.Record.ClaimedName is {} claimedName)
        {
            if (_player.LocalEntity is {} local && _idCard.TryFindIdCard(local, out var idCard) && idCard.Comp.FullName == claimedName)
            {
                ev.StatusIcons.Add(_prototype.Index<HealthIconPrototype>("TriageClaimedYours"));
            }
            else
            {
                ev.StatusIcons.Add(_prototype.Index<HealthIconPrototype>("TriageClaimedOthers"));
            }
        }
    }
}
