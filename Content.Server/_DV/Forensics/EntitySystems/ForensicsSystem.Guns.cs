using System.Linq;
using Content.Server.Body.Systems;
using Content.Server.Hands.Systems;
using Content.Server.Interaction;
using Content.Shared._DV.Forensics.Components;
using Content.Shared._DV.Forensics.Events;
using Content.Shared._Shitmed.Medical.Surgery.Tools;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.Forensics.Components;
using Content.Shared.Throwing;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Forensics;

/// <summary>
/// Partial implementation to handle the specifics for gun based forensics.
/// </summary>
public sealed partial class ForensicsSystem
{
    private readonly EntProtoId _retrievedProjectile = "RetrievedBullet";

    [Dependency] private readonly HandsSystem _handsSystem = default!;
    [Dependency] private readonly InteractionSystem _interactionSystem = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;

    private void InitializeGunForensics()
    {
        SubscribeLocalEvent<RiflingForensicsComponent, MapInitEvent>(OnRiflingInit, after: [typeof(BloodstreamSystem)]);
        SubscribeLocalEvent<LodgedProjectileStorageComponent, LodgeProjectileAttemptEvent>(OnBulletLodgeAttempt);
        SubscribeLocalEvent<HemostatComponent, GetVerbsEvent<UtilityVerb>>(AddProjectileRetriveVerb);
        SubscribeLocalEvent<LodgedProjectileStorageComponent, RetrieveProjectilesDoAfterEvent>(
            OnRetrieveBulletDoAfter);
        SubscribeLocalEvent<GunComponent, AmmoShotEvent>(OnAmmoShot);
        SubscribeLocalEvent<SpentProjectileExaminationComponent, ExaminedEvent>(OnSpentBulletExamined);
    }

    /// <summary>
    /// Raised at map init for the component so it can generate a unique identifier
    /// </summary>
    /// <param name="ent">Entity that has just spawned with the Rifling component</param>
    /// <param name="args">Event args</param>
    private void OnRiflingInit(Entity<RiflingForensicsComponent> ent, ref MapInitEvent args)
    {
        // Just the same system as fingerprints
        ent.Comp.Identifier ??= GenerateFingerprint();
    }

    /// <summary>
    /// Raised when an entity with the SpentProjectile component is examined.
    /// Allows for the addition of information about the make/caliber of the bullet.
    /// </summary>
    /// <param name="ent">Entity being examined</param>
    /// <param name="args">Event args</param>
    private void OnSpentBulletExamined(Entity<SpentProjectileExaminationComponent> ent, ref ExaminedEvent args)
    {
        if (string.IsNullOrEmpty(ent.Comp.Name))
        {
            args.PushMarkup(Loc.GetString("forensics-bullet-unknown-make"));
        }
        else
        {
            args.PushMarkup(Loc.GetString("forensics-bullet-known-make", ("make", ent.Comp.Name)));
        }
    }

    /// <summary>
    /// Raised when a projectile has interacted with a target that allows has the possibility to store it.
    /// The projectile shot MUST have the <see cref="LodgeableProjectileComponent"/> in order for it to become lodged.
    /// </summary>
    /// <param name="ent">Entity which was struck by the projectile</param>
    /// <param name="args">Event args</param>
    private void OnBulletLodgeAttempt(Entity<LodgedProjectileStorageComponent> ent,
        ref LodgeProjectileAttemptEvent args)
    {
        if (!args.Component.DeleteOnCollide || !args.Component.ProjectileSpent)
            return; // This projectile can/will still travel so it can be ignored

        if (!TryComp<LodgeableProjectileComponent>(args.ProjUid, out var lodgeableComp))
            return; // Not something we can lodge into the entity

        if (_random.NextDouble() > lodgeableComp.LodgeChance + ent.Comp.LodgeChanceModifier)
            return; // Failed to lodge into the entity based on random chance

        var projectile = new LodgedProjectile();
        if (TryComp<RiflingForensicsComponent>(args.ProjUid, out var rifling))
            projectile.Rifling = rifling.Identifier;

        if (EntityManager.TryGetComponent<MetaDataComponent>(args.ProjUid, out var metadata))
            projectile.Name = metadata.EntityName;

        if (ent.Comp.Projectiles.Contains(projectile))
            return; // Already have a copy of this projectile type in there, no dupes allowed

        ent.Comp.Projectiles.Add(projectile);
    }

    /// <summary>
    /// Begins the doafter event once the user has selected the proper verb from the interaction menu.
    /// </summary>
    /// <param name="target">Entity which will have the projectiles removed from</param>
    /// <param name="user">Entity attempting to remove the projectiles</param>
    private void TryRetrieveProjectiles(Entity<LodgedProjectileStorageComponent> target, EntityUid user)
    {
        if (target.Comp.Projectiles.Count == 0)
            return; // No projectiles to retrieve

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            user,
            target.Comp.RetrievalTime,
            new RetrieveProjectilesDoAfterEvent(),
            eventTarget: target.Owner
        )
        {
            BreakOnHandChange = true,
            BreakOnMove = true,
            BreakOnDamage = true,
        };

        _doAfterSystem.TryStartDoAfter(doAfterArgs);
    }

    /// <summary>
    /// Actually performs the retrieval of the projectiles lodged in the entity.
    /// Spawns a new spent bullet and applies the DNA/Rifling forensics to it if possible.
    /// Also raises a contact interaction so the users' forensics is applied to the spent bullet.
    /// May repeat the interaction if there are more projectiles to retrieve.
    /// </summary>
    /// <param name="ent">Entity which will have the projeciles removed from</param>
    /// <param name="args">Event args</param>
    private void OnRetrieveBulletDoAfter(Entity<LodgedProjectileStorageComponent> ent,
        ref RetrieveProjectilesDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || ent.Comp.Deleted)
            return;

        if (ent.Comp.Projectiles.Count == 0)
            return; // No projectiles to retrieve

        var lastProjectile = ent.Comp.Projectiles.Last();
        ent.Comp.Projectiles.Remove(lastProjectile); // Ensure it's removed early

        // Spawn the entity at the feet of the user, before attempting to pick it up
        var mapCoordinates = _transformSystem.GetMapCoordinates(args.User);
        var retrieved = Spawn(_retrievedProjectile, mapCoordinates);

        var recipientComp = EnsureComp<ForensicsComponent>(retrieved);
        if (TryComp<DnaComponent>(ent.Owner, out var dnaComponent) &&
            !string.IsNullOrEmpty(dnaComponent.DNA))
        {
            recipientComp.DNAs.Add(dnaComponent.DNA);
            recipientComp.CanDnaBeCleaned = true;
        }

        if (!string.IsNullOrEmpty(lastProjectile.Rifling))
        {
            recipientComp.Rifling = lastProjectile.Rifling;
        }

        if (!string.IsNullOrEmpty(lastProjectile.Name))
        {
            var examinationComp = EnsureComp<SpentProjectileExaminationComponent>(retrieved);
            examinationComp.Name = lastProjectile.Name;
        }

        if (!_handsSystem.TryPickupAnyHand(args.User, retrieved))
        {
            // It's fallen to the ground, raise an event so it plays a nice sound
            var landEv = new LandEvent(args.User, true);
            RaiseLocalEvent(retrieved, ref landEv);
        }
        _interactionSystem.DoContactInteraction(args.User, retrieved); // Ensure fingerprints/fibers propagate now

        args.Repeat = ent.Comp.Projectiles.Count != 0; // Repeatable only if we have more things to pull out
    }

    /// <summary>
    /// Adds the 'Retrieve Projectiles' verb when the user is holding hemostat tool and the target has
    /// projectiles to retrieve.
    /// </summary>
    /// <param name="ent">Hemostat entity being held by some user</param>
    /// <param name="ev">Event args</param>
    private void AddProjectileRetriveVerb(Entity<HemostatComponent> ent, ref GetVerbsEvent<UtilityVerb> ev)
    {
        if (!ev.CanInteract ||
            !ev.CanAccess ||
            !TryComp<LodgedProjectileStorageComponent>(ev.Target, out var bodyStorage))
            return;

        if (bodyStorage.Projectiles.Count == 0)
            return; // No bullets to retrieve

        var user = ev.User;
        var target = ev.Target;

        UtilityVerb verb = new()
        {
            Act = () =>
            {
                TryRetrieveProjectiles((target, bodyStorage), user);
            },
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Objects/Specific/Medical/Surgery/scalpel.rsi/"), "scalpel"),
            Text = Loc.GetString("forensics-verb-retrieve-projectiles"),
            Priority = -1,
        };

        ev.Verbs.Add(verb);
    }

    /// <summary>
    /// Localises a given residue
    /// </summary>
    /// <param name="comp">Residue to localise</param>
    /// <returns>Localised description of the residue</returns>
    private string DescribeResidue(ResidueComponent comp)
    {
        if (string.IsNullOrEmpty(comp.ResidueColor))
        {
            return Loc.GetString("forensic-residue", ("adjective", comp.ResidueAdjective));
        }
        else
        {
            return Loc.GetString("forensic-residue-colored",
                ("color", comp.ResidueColor),
                ("adjective", comp.ResidueAdjective));
        }
    }

    /// <summary>
    /// Applies firearm residue to both the weapon and the user.
    /// Applies rifling forensics to any projectiles that the weapon has fired.
    /// </summary>
    /// <param name="ent">Weapon that is being fired</param>
    /// <param name="args">Event args</param>
    private void OnAmmoShot(Entity<GunComponent> ent, ref AmmoShotEvent args)
    {
        // Apply Firearm residue to the weapon AND the entity firing the weapon,
        // if one exists.
        if (TryComp<ResidueComponent>(ent.Owner, out var residue))
        {
            var weaponForensics = EnsureComp<ForensicsComponent>(ent.Owner);
            var description = DescribeResidue(residue);
            weaponForensics.Residues.Add(description);
            if (ent.Comp.Holder.HasValue)
            {
                var userForensics = EnsureComp<ForensicsComponent>(ent.Comp.Holder.Value);
                userForensics.Residues.Add(description);
            }
        }

        // Apply Firearm rifling to all projectiles filed from this weapon
        if (TryComp<RiflingForensicsComponent>(ent.Owner, out var rifling))
        {
            foreach (var projectile in args.FiredProjectiles)
            {
                var projectileRifling = EnsureComp<RiflingForensicsComponent>(projectile);
                projectileRifling.Identifier = rifling.Identifier;
            }
        }
    }
}
