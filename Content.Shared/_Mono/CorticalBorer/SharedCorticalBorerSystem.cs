// SPDX-FileCopyrightText: 2025 Coenx-flex
// SPDX-FileCopyrightText: 2025 Cojoke
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Examine;
using Content.Shared.MedicalScanner;
using Content.Shared.Popups;
using Content.Shared.StatusEffect;
using Content.Shared.Coordinates;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage.Components;
using Content.Shared.IdentityManagement;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;



namespace Content.Shared._Mono.CorticalBorer;

public partial class SharedCorticalBorerSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ISerializationManager _serManager = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] protected readonly SharedPopupSystem _popup = default!;
    [Dependency] protected readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] protected readonly SharedActionsSystem _actions = default!;
    [Dependency] protected readonly SharedContainerSystem _container = default!;

    public bool CanUseAbility(Entity<CorticalBorerComponent> ent, EntityUid target)
    {
        if (_statusEffects.HasStatusEffect(target, "CorticalBorerProtection")) // hardcoded the status effect because...
        {
            _popup.PopupEntity(Loc.GetString("cortical-borer-sugar-block"), ent.Owner, ent.Owner, PopupType.Medium);
            return false;
        }

        return true;
    }

    public void InfestTarget(Entity<CorticalBorerComponent> ent, EntityUid target)
    {
        var (uid, comp) = ent;

        // Make sure the infected person is infected right
        var infestedComp = EnsureComp<CorticalBorerInfestedComponent>(target);

        // Make sure they get into the target
        if (!_container.Insert(uid, infestedComp.InfestationContainer))
        {
            RemCompDeferred<CorticalBorerInfestedComponent>(target); // oh no it didn't work somehow so remove the comp you just added...
            return;
        }

        // Set up the Borer
        infestedComp.Borer = ent;
        comp.Host = target;

        if (comp.AddOnInfest is not null)
        {
            foreach (var (key, compReg) in comp.AddOnInfest)
            {
                var compType = compReg.Component.GetType();
                if (HasComp(ent, compType))
                    continue;

                var newComp = (Component) _serManager.CreateCopy(compReg.Component, notNullableOverride: true);
                EntityManager.AddComponent(ent, newComp, true);
            }
        }

        if (comp.RemoveOnInfest is not null)
        {
            foreach (var (key, compReg) in comp.RemoveOnInfest)
                RemCompDeferred(ent, compReg.Component.GetType());
        }

        _damage.SetAllDamage(ent.Owner, 0);
    }

    public bool TryEjectBorer(Entity<CorticalBorerComponent> ent)
    {
        var (uid, comp) = ent;

        if (ent.Comp.Host is not { } host)
            return false;

        // Make sure they get out of the host
        if (!_container.TryRemoveFromContainer(uid))
            return false;

        // close all the UIs that relate to host
        if (TryComp<UserInterfaceComponent>(ent, out var uic))
        {
            _ui.CloseUi((ent.Owner, uic), HealthAnalyzerUiKey.Key);
            _ui.CloseUi((ent.Owner, uic), CorticalBorerDispenserUiKey.Key);
        }

        RemCompDeferred<CorticalBorerInfestedComponent>(ent.Comp.Host.Value);
        ent.Comp.Host = null;

        if (comp.RemoveOnInfest is not null)
        {
            foreach (var (key, compReg) in comp.RemoveOnInfest)
            {
                var compType = compReg.Component.GetType();
                if (HasComp(ent, compType))
                    continue;

                var newComp = (Component) _serManager.CreateCopy(compReg.Component, notNullableOverride: true);
                EntityManager.AddComponent(ent, newComp, true);
            }
        }

        if (comp.AddOnInfest is not null)
        {
            foreach (var (key, compReg) in comp.AddOnInfest)
                RemCompDeferred(ent, compReg.Component.GetType());
        }

        return true;
    }

    public void LayEgg(Entity<CorticalBorerComponent> ent)
    {
        if (ent.Comp.Host is not { } host)
            return;

        if (ent.Comp.EggProto is not {} egg)
            return;

        var coordinates = _transform.ToMapCoordinates(host.ToCoordinates());
        var spawnedEgg = Spawn(egg, coordinates);
    }
}

public sealed class InfestHostAttempt : CancellableEntityEventArgs
{
    /// <summary>
    ///     The equipment that is blocking the entrance
    /// </summary>
    public EntityUid? Blocker = null;
}

[Serializable, NetSerializable]
public enum CorticalBorerDispenserUiKey
{
    Key
}


[Serializable, NetSerializable]
public sealed class CorticalBorerDispenserSetInjectAmountMessage : BoundUserInterfaceMessage
{
    public readonly int CorticalBorerDispenserDispenseAmount;

    public CorticalBorerDispenserSetInjectAmountMessage(int amount)
    {
        CorticalBorerDispenserDispenseAmount = amount;
    }

    public CorticalBorerDispenserSetInjectAmountMessage(String s)
    {
        switch (s)
        {
            case "1":
                CorticalBorerDispenserDispenseAmount = 1;
                break;
            case "5":
                CorticalBorerDispenserDispenseAmount = 5;
                break;
            case "10":
                CorticalBorerDispenserDispenseAmount = 10;
                break;
            case "15":
                CorticalBorerDispenserDispenseAmount = 15;
                break;
            case "20":
                CorticalBorerDispenserDispenseAmount = 20;
                break;
            case "25":
                CorticalBorerDispenserDispenseAmount = 25;
                break;
            case "30":
                CorticalBorerDispenserDispenseAmount = 30;
                break;
            case "50":
                CorticalBorerDispenserDispenseAmount = 50;
                break;
            case "100":
                CorticalBorerDispenserDispenseAmount = 100;
                break;
            default:
                throw new Exception($"Cannot convert the string `{s}` into a valid DispenseAmount");
        }
    }
}

[Serializable, NetSerializable]
public sealed class CorticalBorerDispenserInjectMessage : BoundUserInterfaceMessage
{
    public readonly string ChemProtoId;

    public CorticalBorerDispenserInjectMessage(string proto)
    {
        ChemProtoId = proto;
    }
}

[Serializable, NetSerializable]
public sealed class CorticalBorerDispenserBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly List<CorticalBorerDispenserItem> DisList;

    public readonly int SelectedDispenseAmount;
    public CorticalBorerDispenserBoundUserInterfaceState(List<CorticalBorerDispenserItem> disList, int dispenseAmount)
    {
        DisList = disList;
        SelectedDispenseAmount = dispenseAmount;
    }
}

[Serializable, NetSerializable]
public sealed class CorticalBorerDispenserItem(string reagentName, string reagentId, int cost, int amount, int chems, Color reagentColor)
{
    public string ReagentName = reagentName;
    public string ReagentId = reagentId;
    public int Cost = cost;
    public int Amount = amount;
    public int Chems = chems;
    public Color ReagentColor = reagentColor;
}
