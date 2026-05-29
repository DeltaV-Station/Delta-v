using Content.Shared._DV.CoughingFit.Components;
using Content.Shared.StatusEffectNew;
using Content.Shared.Popups;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Chat;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._DV.CoughingFit.EntitySystems;

public sealed class SharedCoughingFitSystem
{
    /// <summary>
    /// This handles coughing fits, causing the player to cough uncontrollably every so often.
    /// </summary>
    public sealed class CoughingFitSystem : EntitySystem
    {
        [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly AutoEmoteSystem _autoEmote = default!;
        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<CoughingFitComponent, MapInitEvent>(OnMapInit);
        }

        private void OnMapInit(Entity<CoughingFitComponent> ent, ref MapInitEvent args)
        {
            ent.Comp.NextFitTime =
                _timing.CurTime + _random.Next(ent.Comp.MinTimeBetweenFits, ent.Comp.MaxTimeBetweenFits);
            DirtyField(ent, ent.Comp, nameof(ent.Comp.NextFitTime));
        }
        private void OnEmote(Entity<CoughingFitComponent> ent, ref EmoteEvent args)
        {
            if (args.Handled)
                return;

            if (!ent.Comp.RandomEmote)
                return;

            args.Handled = _chat.TryPlayEmoteSound(ent.Owner, EmoteSounds, args.Emote);

            if (_robustRandom.Prob(ent.Comp.GiggleRandomChance))
            {
                _audio.PlayPvs(ent.Comp.SpawnSound, ent.Owner);

    }
}
