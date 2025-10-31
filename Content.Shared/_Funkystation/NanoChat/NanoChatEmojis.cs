// SPDX-FileCopyrightText: 2025 EvaisaDev <mail@evaisa.dev>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Utility;

namespace Content.Shared._Funkystation.NanoChat;

/// <summary>
/// Shared emoji definitions for NanoChat.
/// </summary>
public static class NanoChatEmojis
{
    /// <summary>
    /// Dictionary mapping emoji names to their sprite specifiers (can be either textures or RSI states).
    /// </summary>
    public static readonly IReadOnlyDictionary<string, SpriteSpecifier> EmojiSpecifiers = new Dictionary<string, SpriteSpecifier>
    {
        { "godo", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/godo.png")) },
        { "do", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/do.png")) },
        { "donow", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/donow.png")) },
        { "gocrazy", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/donot.png")) },
        { "gosad", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/dontdo.png")) },
        { "godover", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/godover.png")) },
        { "golove", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/golove.png")) },

        { "troll", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/troll.png")) },

        { "bubble", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/bubble.png")) },
        { "buzz", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/buzztwo.png")) },
        { "chime", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/chime.png")) },
        { "chirp", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/chirp.png")) },
        { "clap", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/clap.png")) },
        { "click", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/click.png")) },
        { "cough", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/cough.png")) },
        { "cry", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/cry.png")) },
        { "deathgasp", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/deathgasp.png")) },
        { "fizz", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/fizz.png")) },
        { "hat", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/hat.png")) },
        { "hew", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/hew.png")) },
        { "honk", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/honk.png")) },
        { "laugh", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/laugh.png")) },
        { "salute", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/salute.png")) },
        { "scream", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/scream.png")) },
        { "sigh", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/sigh.png")) },
        { "snap", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/snap.png")) },
        { "squeak", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/squeak.png")) },
        { "tailslap", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/tailslap.png")) },
        { "vocal", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/vocal.png")) },
        { "weh", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/weh.png")) },
        { "whistle", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/whistle.png")) },
        { "yawn", new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Emotes/yawn.png")) },

        { "bee", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_h") },
        { "ratvar", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_ratvar") },
        { "penguin", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_penguin") },
        { "moth", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_moth") },
        { "rouny", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/rouny.png")) },
        { "lizard", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_lizard") },
        { "spacelizard", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_spacelizard") },
        { "hampter", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_hampter") },
        { "arachnid", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_arachnid") },
        { "lamp", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_lamp") },
        { "nukie", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_nuke") },
        { "atmosian", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_atmosian") },
        { "slime", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_slime") },
        { "snake", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_snake") },
        { "vox", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_vox") },
        { "diona", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/plushie_diona.png")) },
        { "human", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_human") },
        { "xeno", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "plushie_xeno") },
        { "carp", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "carpplush") },
        { "rainbowcarp", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/rainbowcarp.png")) },
        { "rainbowlizard", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/rainbowlizardplush.png")) },
        { "nar", new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Fun/toys.rsi"), "narplush") },
        { "cirno", new SpriteSpecifier.Rsi(new ResPath("/Textures/_Funkystation/Objects/Fun/toys.rsi"), "plushie_cirno") },
        { "mothroach", new SpriteSpecifier.Rsi(new ResPath("/Textures/_Funkystation/Objects/Fun/toys.rsi"), "plushie_mothroach") },
        { "louie", new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Objects/Fun/toys.rsi"), "plushie_louie") },
        { "cosmic", new SpriteSpecifier.Rsi(new ResPath("/Textures/_DV/CosmicCult/Objects/unknownplushie.rsi"), "plushie_cosmic") },

        { "floorgoblin", new SpriteSpecifier.Texture(new ResPath("/Textures/_Funkystation/Interface/NanoChat/Emotes/floorgoblin.png")) }
    };
}
