using Content.Shared.Cuffs;
using Content.Shared.Popups;

namespace Content.Shared.Abilities.Mime;

public sealed partial class MimePowersSystem
{
    [Dependency] private SharedCuffableSystem _cuffable = default!;

    private void OnCuffed(Entity<MimePowersComponent> mime, ref TargetHandcuffedEvent args)
    {
        if (!mime.Comp.Enabled)
            return;

        var message = Loc.GetString("mime-cuffed");
        _popupSystem.PopupClient(message, mime, mime, PopupType.MediumCaution);
    }
}
