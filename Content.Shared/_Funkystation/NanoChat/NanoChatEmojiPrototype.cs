// SPDX-FileCopyrightText: 2025 Evaisa <evagiacosa1@gmail.com>
// SPDX-FileCopyrightText: 2025 EvaisaDev <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Funkystation.NanoChat;

/// <summary>
/// Prototype for NanoChat emoji definitions.
/// </summary>
[Prototype]
public sealed partial class NanoChatEmojiPrototype : IPrototype
{
    /// <summary>
    /// The unique identifier for this emoji (e.g., "godo")
    /// </summary>
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    /// <summary>
    /// Optional category for grouping emojis (e.g., "Godo", "Plushies")
    /// </summary>
    [DataField]
    public string Category { get; private set; } = string.Empty;

    /// <summary>
    /// The sprite specifier for this emoji
    /// </summary>
    [DataField(required: true)]
    public SpriteSpecifier Sprite = default!;
}
