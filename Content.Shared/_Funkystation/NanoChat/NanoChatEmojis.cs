// SPDX-FileCopyrightText: 2025 Evaisa <evagiacosa1@gmail.com>
// SPDX-FileCopyrightText: 2025 EvaisaDev <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Funkystation.NanoChat;

/// <summary>
/// Shared emoji definitions for NanoChat.
/// This class provides access to emoji prototypes.
/// </summary>
public static class NanoChatEmojis
{
    private static Dictionary<string, SpriteSpecifier>? _cachedEmojis;

    /// <summary>
    /// Dictionary mapping emoji names to their sprite specifiers.
    /// </summary>
    public static IReadOnlyDictionary<string, SpriteSpecifier> EmojiSpecifiers
    {
        get
        {
            if (_cachedEmojis == null)
            {
                var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
                _cachedEmojis = new Dictionary<string, SpriteSpecifier>();

                var sortedEmojis = prototypeManager.EnumeratePrototypes<NanoChatEmojiPrototype>()
                    .OrderBy(proto => proto.Category)
                    .ThenBy(proto => proto.ID);

                foreach (var proto in sortedEmojis)
                {
                    _cachedEmojis[proto.ID] = proto.Sprite;
                }
            }

            return _cachedEmojis;
        }
    }

    /// <summary>
    /// Clears the emoji cache. Call this if prototypes are reloaded.
    /// </summary>
    public static void ClearCache()
    {
        _cachedEmojis = null;
    }
}
