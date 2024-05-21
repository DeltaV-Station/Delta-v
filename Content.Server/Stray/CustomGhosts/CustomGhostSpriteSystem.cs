using Content.Server.Ghost.Components;
using Content.Shared.Stray.CustomGhosts;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.Prototypes;
using Content.Shared.Ghost;
﻿using Robust.Shared.Player;

namespace Content.Server.Stray.CustomGhosts;

public sealed class CustomGhostSpriteSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GhostComponent, PlayerAttachedEvent>(OnShit);
    }

    private void OnShit(EntityUid uid, GhostComponent component, PlayerAttachedEvent args)
    {
        if(!_playerManager.TryGetSessionByEntity(uid, out var session))
            return;

        TrySetCustomSprite(uid, session.Name);
    }


    public void TrySetCustomSprite(EntityUid ghostUid, string ckey)
    {
        var prototypes = _prototypeManager.EnumeratePrototypes<CustomGhostPrototype>();

        foreach (var customGhostPrototype in prototypes)
        {
            if (string.Equals(customGhostPrototype.Ckey, ckey, StringComparison.CurrentCultureIgnoreCase))
            {
                _appearanceSystem.SetData(ghostUid, CustomGhostAppearance.Sprite, customGhostPrototype.CustomSpritePath.ToString());
                _appearanceSystem.SetData(ghostUid, CustomGhostAppearance.SizeOverride, customGhostPrototype.SizeOverride);

                if(customGhostPrototype.AlphaOverride > 0)
                {
                    _appearanceSystem.SetData(ghostUid, CustomGhostAppearance.AlphaOverride, customGhostPrototype.AlphaOverride);
                }

                if (customGhostPrototype.GhostName != string.Empty)
                {
                    MetaData(ghostUid).EntityName = customGhostPrototype.GhostName;
                }

                if (customGhostPrototype.GhostDescription != string.Empty)
                {
                    MetaData(ghostUid).EntityDescription = customGhostPrototype.GhostDescription;
                }




                return;
            }

        }
    }
}
