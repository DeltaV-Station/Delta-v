using Content.Server.Radio.EntitySystems;
using Content.Shared.Access.Components;
using Content.Shared.CartridgeLoader;
using Content.Shared.PDA;
using Content.Shared.Popups;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server._Impstation.CartridgeLoader.Cartridges;

public sealed class SOSCartridgeSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly RadioSystem _radio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SOSCartridgeComponent, CartridgeActivatedEvent>(OnActivated);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SOSCartridgeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Timer > 0)
            {
                comp.Timer -= frameTime;
            }
        }
    }

    private void OnActivated(EntityUid uid, SOSCartridgeComponent component, CartridgeActivatedEvent args)
    {
        if (component.CanCall)
        {
            //Get the PDA
            if (!TryComp<PdaComponent>(args.Loader, out var pda))
                return;

            //Get the id container
            if (_container.TryGetContainer(args.Loader, SOSCartridgeComponent.PDAIdContainer, out var idContainer))
            {
                //If theres nothing in id slot, send message anonymously
                if (idContainer.ContainedEntities.Count == 0)
                {
                    _radio.SendRadioMessage(uid, component.LocalizedDefaultName + " " + component.LocalizedHelpMessage, component.HelpChannel, uid);
                }
                else
                {
                    //Otherwise, send a message with the full name of every id in there
                    foreach (var idCard in idContainer.ContainedEntities)
                    {
                        if (!TryComp<IdCardComponent>(idCard, out var idCardComp))
                            return;

                        _radio.SendRadioMessage(uid, idCardComp.FullName + " " + component.LocalizedHelpMessage, component.HelpChannel, uid);
                    }
                }

                component.Timer = SOSCartridgeComponent.TimeOut;
                // DeltaV - send feedback that you succeeded
                _popupSystem.PopupEntity(Loc.GetString("sos-message-sent-success"), uid, PopupType.Medium);
            }
        }
        // DeltaV - send feedback that you failed
        else
        {
            var seconds = Math.Round(component.Timer);
            _popupSystem.PopupEntity(Loc.GetString("sos-message-sent-cooldown", ("count", seconds)), uid, PopupType.MediumCaution);
        }
    }
}
