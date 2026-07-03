using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Shared.Body;
using Content.Shared.Gibbing;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Preferences;
using Content.Shared.Species;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Diona;

public sealed class DVGestaltSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly GibbingSystem _gibbing = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly HumanoidProfileSystem _profile = default!;
    [Dependency] private readonly SharedVisualBodySystem _visualBody = default!;

    private static readonly EntProtoId<DVGestaltComponent> GestaltPrototype = "DVMobDionaGestalt";
    private static readonly EntProtoId ReformedPrototype = "MobDionaReformed";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DVNymphProfileComponent, DVAssimilateNymphActionEvent>(OnNymphAssimilate);

        SubscribeLocalEvent<DVGestaltComponent, MapInitEvent>(OnGestaltInit);
        SubscribeLocalEvent<DVGestaltComponent, ComponentShutdown>(OnGestaltShutdown);
        SubscribeLocalEvent<DVGestaltComponent, DVAssimilateNymphActionEvent>(OnGestaltAssimilate);
        SubscribeLocalEvent<DVGestaltComponent, GibActionSystem.GibActionEvent>(OnGestaltGib);
        SubscribeLocalEvent<DVGestaltComponent, BeingGibbedEvent>(OnGestaltBeingGibbed);
        SubscribeLocalEvent<DVGestaltComponent, GibbedBeforeDeletionEvent>(OnGestaltGibbedBeforeDeletion);
        SubscribeLocalEvent<DVGestaltComponent, ReformSystem.ReformEvent>(OnGestaltReforming);
    }

    private void OnGestaltInit(Entity<DVGestaltComponent> ent, ref MapInitEvent args)
    {
        var map = _map.CreateMap(runMapInit: false);
        ent.Comp.NymphStorageMap = map;
        _metadata.SetEntityName(map, $"Diona Gestalt Storage {ToPrettyString(ent)}");
        Dirty(ent);
    }

    private void OnGestaltShutdown(Entity<DVGestaltComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<MapComponent>(ent.Comp.NymphStorageMap, out var map))
            _map.QueueDeleteMap(map.MapId);
    }

    private void OnGestaltAssimilate(Entity<DVGestaltComponent> ent, ref DVAssimilateNymphActionEvent args)
    {
        if (!_net.IsServer)
            return;

        if (_mind.TryGetMind(args.Target, out _, out _))
            return;

        Assimilate(ent, args.Target);
        args.Handled = true;
    }

    private void OnNymphAssimilate(Entity<DVNymphProfileComponent> ent, ref DVAssimilateNymphActionEvent args)
    {
        if (!_net.IsServer)
            return;

        if (_mind.TryGetMind(args.Target, out _, out _))
            return;

        var gestalt = SpawnAtPosition(GestaltPrototype, Transform(ent).Coordinates);
        var gestaltComp = Comp<DVGestaltComponent>(gestalt);
        Assimilate((gestalt, gestaltComp), ent.Owner);
        Assimilate((gestalt, gestaltComp), args.Target);

        if (_mind.TryGetMind(ent, out var mindId, out var mind))
            _mind.TransferTo(mindId, gestalt, true, mind: mind);

        args.Handled = true;
    }

    private void Assimilate(Entity<DVGestaltComponent> gestalt, Entity<DVGestaltMemberComponent?> nymph)
    {
        if (!Resolve(nymph, ref nymph.Comp) || !TryComp<MapComponent>(gestalt.Comp.NymphStorageMap, out var map))
            return;

        _transform.SetMapCoordinates(nymph, new MapCoordinates(Vector2.Zero, map.MapId));
        gestalt.Comp.StoredNymphs.Add(nymph);
        gestalt.Comp.NymphCount++;
        nymph.Comp.StoredInGestalt = gestalt;
        Dirty(nymph, nymph.Comp);
        Dirty(gestalt);
    }

    private void OnGestaltGib(Entity<DVGestaltComponent> ent, ref GibActionSystem.GibActionEvent args)
    {
        _popup.PopupPredicted(Loc.GetString(ent.Comp.PopupText, ("name", ent)), ent, ent);
        _gibbing.Gib(ent, user: args.Performer);
    }

    private void OnGestaltBeingGibbed(Entity<DVGestaltComponent> ent, ref BeingGibbedEvent args)
    {
        args.Giblets.UnionWith(ent.Comp.StoredNymphs);
    }

    private void OnGestaltGibbedBeforeDeletion(Entity<DVGestaltComponent> ent, ref GibbedBeforeDeletionEvent args)
    {
        foreach (var giblet in args.Giblets)
        {
            if (!TryComp<DVNymphMindMemoryComponent>(giblet, out var memory))
                continue;

            if (!Exists(memory.Mind) || !TryComp<MindComponent>(memory.Mind, out var mindComponent))
                continue;

            _mind.TransferTo(memory.Mind.Value, giblet, true, mind: mindComponent);
        }
    }

    private bool DetermineProfile(
        Entity<DVGestaltComponent> ent,
        [NotNullWhen(true)] out DVNymphProfileComponent? profile)
    {
        profile = null;
        var nymphs = ent.Comp.StoredNymphs.ToList();
        if (nymphs.Count == 0)
            return false;

        if (nymphs.Count == 1)
        {
            profile = Comp<DVNymphProfileComponent>(nymphs[0]);
            return true;
        }

        var headProfile = Comp<DVNymphProfileComponent>(nymphs[0]);
        foreach (var nymph in nymphs.Skip(1))
        {
            var nymphProfile = Comp<DVNymphProfileComponent>(nymph);

            if (nymphProfile.Name != headProfile.Name
                || nymphProfile.Species != headProfile.Species
                || nymphProfile.Gender != headProfile.Gender
                || nymphProfile.Sex != headProfile.Sex
                || nymphProfile.Age != headProfile.Age
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                || nymphProfile.Height != headProfile.Height)
            {
                return false;
            }
        }

        profile = headProfile;
        return true;
    }

    private void OnGestaltReforming(Entity<DVGestaltComponent> ent, ref ReformSystem.ReformEvent args)
    {
        if (ent.Comp.NymphCount < ent.Comp.RequiredNymphs)
            return;

        args.Handled = true;
        if (!_net.IsServer)
            return;

        var child = SpawnNextToOrDrop(ReformedPrototype, ent);
        if (_mind.TryGetMind(ent, out var mindId, out var mind))
            _mind.TransferTo(mindId, child, mind: mind);

        if (DetermineProfile(ent, out var profile))
        {
            if (profile.Name is { } name)
                _metadata.SetEntityName(child, name);

            _profile.ApplyProfileTo(child,
                new HumanoidCharacterProfile()
                    .WithSpecies(profile.Species)
                    .WithAge(profile.Age)
                    .WithSex(profile.Sex)
                    .WithHeight(profile.Height)
                    .WithGender(profile.Gender)
                    .WithHeight(profile.Height));

            if (profile.OrganProfiles is { } profiles)
                _visualBody.ApplyProfiles(child, profiles);

            if (profile.OrganMarkings is { } markings)
                _visualBody.ApplyMarkings(child, markings);
        }

        var newComp = CopyComp(ent, child, ent.Comp);
        newComp.StoredNymphs.Clear();
        newComp.StoredNymphs.UnionWith(ent.Comp.StoredNymphs);
        var newMap = Comp<MapComponent>(newComp.NymphStorageMap);
        foreach (var nymph in ent.Comp.StoredNymphs)
        {
            var gestaltMember = Comp<DVGestaltMemberComponent>(nymph);
            gestaltMember.StoredInGestalt = child;
            _transform.SetMapCoordinates(nymph, new MapCoordinates(Vector2.Zero, newMap.MapId));
        }
        ent.Comp.StoredNymphs.Clear();
        _metadata.SetEntityName(ent.Comp.NymphStorageMap, $"Diona Gestalt Storage {ToPrettyString(ent)}");

        QueueDel(ent);
    }
}
