using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Robust.Shared.Network;

namespace Content.Shared.Uprising;

public sealed class ConversionAcceptActionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedRoleSystem _role = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindContainerComponent, ConversionAcceptActionEvent>(OnAcceptConversion);
    }

    private void OnAcceptConversion(Entity<MindContainerComponent> ent, ref ConversionAcceptActionEvent args)
    {
        if (_mind.GetMind(ent) is not { } mind)
            return;

        args.Handled = true;
        if (_net.IsServer)
        {
            _role.MindAddRole(mind, args.MindRole);
            _role.MindRemoveRole(mind, args.RemoveMindRole);
        }
    }
}
