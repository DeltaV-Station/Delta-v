// SPDX-FileCopyrightText: 2025 Evaisa <evagiacosa1@gmail.com>
// SPDX-FileCopyrightText: 2025 EvaisaDev <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Numerics;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.RichText;
using Content.Shared._Funkystation.NanoChat;
using Robust.Shared.Utility;

namespace Content.Client._Funkystation.NanoChat;

/// <summary>
/// Markup tag for rendering emoji images inline in RichTextLabel.
/// </summary>
public sealed class EmojiTag : IMarkupTagHandler
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    private SpriteSystem? _spriteSystem;

    public EmojiTag()
    {
        IoCManager.InjectDependencies(this);
    }

    public string Name => "emoji";

    public bool TryCreateControl(MarkupNode node, out Control control)
    {
        control = default!;

        string? emojiName;

        if (!node.Value.TryGetString(out emojiName))
        {
            if (!node.Attributes.TryGetValue("name", out var nameParam) || !nameParam.TryGetString(out emojiName))
            {
                return false;
            }
        }

        if (!NanoChatEmojis.EmojiSpecifiers.TryGetValue(emojiName, out var specifier))
        {
            return false;
        }

        _spriteSystem ??= _entitySystemManager.GetEntitySystem<SpriteSystem>();

        Texture texture;
        switch (specifier)
        {
            case SpriteSpecifier.Texture texSpecifier:
                if (!_resourceCache.TryGetResource<TextureResource>(texSpecifier.TexturePath, out var textureResource))
                {
                    return false;
                }
                texture = textureResource.Texture;
                break;

            case SpriteSpecifier.Rsi rsiSpecifier:
                var state = _spriteSystem.GetState(rsiSpecifier);
                texture = state.Frame0;
                break;

            default:
                return false;
        }

        var emojiSize = new Vector2(32f, 32f);
        control = new TextureRect
        {
            Texture = texture,
            Stretch = TextureRect.StretchMode.KeepAspect,
            MinSize = emojiSize,
            MaxSize = emojiSize,
            Margin = new Thickness(0, -8, 0, 0),
        };

        return true;
    }
}
