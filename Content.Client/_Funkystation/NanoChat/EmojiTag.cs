// SPDX-FileCopyrightText: 2025 EvaisaDev <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Numerics;
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

        if (!NanoChatEmojis.EmojiPaths.TryGetValue(emojiName, out var texturePath))
        {
            return false;
        }

        if (!_resourceCache.TryGetResource<TextureResource>(texturePath, out var texture))
        {
            return false;
        }

        var emojiSize = new Vector2(32f, 32f);
        control = new TextureRect
        {
            Texture = texture.Texture,
            Stretch = TextureRect.StretchMode.KeepAspect,
            MinSize = emojiSize,
            MaxSize = emojiSize,
            Margin = new Thickness(0, -8, 0, 0),
        };

        return true;
    }
}
