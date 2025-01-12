using System.Text;
using Content.Server._DV.Speech.Components;
using Content.Server.Speech;
using Content.Shared.Drunk;
using Content.Shared.StatusEffect;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Speech.EntitySystems;

public sealed class DrunkardAccentSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DrunkardAccentComponent, AccentGetEvent>(OnAccent);
    }

    // A modified copy of SlurredSystem's Accentuate.
    public string Accentuate(string message, float scale)
    {
        var sb = new StringBuilder();

        foreach (var character in message)
        {
            if (_random.Prob(scale / 3))
            {
                var lower = char.ToLowerInvariant(character);
                var newString = lower switch
                {
                    'o' => "u",
                    's' => "ch",
                    'a' => "ah",
                    'u' => "oo",
                    'c' => "k",
                    _ => $"{character}",
                };

                sb.Append(newString);
            }

            if (!_random.Prob(scale * 3 / 20))
            {
                sb.Append(character);
                continue;
            }

            var next = _random.Next(1, 3) switch
            {
                1 => "'",
                2 => $"{character}{character}",
                _ => $"{character}{character}{character}",
            };

            sb.Append(next);
        }

        return sb.ToString();
    }

    private void OnAccent(Entity<DrunkardAccentComponent> ent, ref AccentGetEvent args)
    {
        // Drunk status effect calculations, ripped directly from SlurredSystem B)
        if (!_statusEffects.TryGetTime(ent.Owner, SharedDrunkSystem.DrunkKey, out var time))
        {
            args.Message = Accentuate(args.Message, 0.25f);
        }
        else
        {
            var curTime = _timing.CurTime;
            var timeLeft = (float)(time.Value.Item2 - curTime).TotalSeconds;
            var drunkScale = Math.Clamp((timeLeft - 80) / 1100, 0f, 1f);

            args.Message = Accentuate(args.Message, Math.Max(0.25f, drunkScale));
        }
    }
}
