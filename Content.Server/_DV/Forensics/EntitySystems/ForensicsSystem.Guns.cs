using Content.Shared._DV.Forensics.Components;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Server.Forensics;

public sealed partial class ForensicsSystem
{
    private void InitializeGunForensics()
    {
        SubscribeLocalEvent<GunComponent, AmmoShotEvent>(OnAmmoShot);
    }

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
    }
}
