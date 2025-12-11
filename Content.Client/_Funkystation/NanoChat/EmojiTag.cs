// SPDX-FileCopyrightText: 2025 Evaisa <evagiacosa1@gmail.com>
// SPDX-FileCopyrightText: 2025 EvaisaDev <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.RichText;
using Content.Shared._Funkystation.NanoChat;
using Robust.Shared.Utility;
using Content.Client._DV.NanoChat.UI; // DeltaV - Fix emoji alignment

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

    // Begin DeltaV - Render invalid emoji as text

    private static void GetEmoji(MarkupNode node, out string text, out SpriteSpecifier? emoji)
    {
        if (!node.Value.TryGetString(out var emojiName))
        {
            // If we don't have a value, then render nothing
            emoji = null;
            text = string.Empty;
        }
        else if (NanoChatEmojis.EmojiSpecifiers.TryGetValue(emojiName, out emoji))
        {
            // If we have a valid emoji, render the emoji, but no text
            text = string.Empty;
        }
        else
        {
            // If we have an invalid emoji, render only text
            text = $":{emojiName}:";
        }
    }

    string IMarkupTagHandler.TextBefore(MarkupNode node)
    {
        GetEmoji(node, out var text, out _);
        return text;
    }

    // End DeltaV - Render invalid emoji as text

    public bool TryCreateControl(MarkupNode node, out Control control)
    {
        control = default!;

        GetEmoji(node, out _, out var maybeSpecifier);
        if (maybeSpecifier is not { } specifier)
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

        control = new EmojiControl // DeltaV - Fix emoji alignment, replace TextureRect with EmojiControl
        {
            Texture = texture,
            SetWidth = 32f,
            SetHeight = 32f,
            Name = "Emoji",
            // Evilly hard-coded Margin and Offset, don't ask me where they come from please
            Margin = new(5f, 0f, 2f, 0),
            Offset = new(5f, -5f),
        };

        return true;
    }
}
