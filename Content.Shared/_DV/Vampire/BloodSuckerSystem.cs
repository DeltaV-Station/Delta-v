using Content.Shared.Administration.Logs;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.HealthExaminable;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Vampire
{
    public sealed class BloodSuckerSystem : EntitySystem
    {
        [Dependency] private readonly SharedSolutionContainerSystem _solutionSystem = default!;
        [Dependency] private readonly SharedPopupSystem _popups = default!;
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<BloodSuckedComponent, HealthBeingExaminedEvent>(OnHealthExamined);
            SubscribeLocalEvent<BloodSuckedComponent, DamageChangedEvent>(OnDamageChanged);
        }


        private void OnHealthExamined(EntityUid uid, BloodSuckedComponent component, HealthBeingExaminedEvent args)
        {
            args.Message.PushNewline();
            args.Message.AddMarkup(Loc.GetString("bloodsucked-health-examine", ("target", uid)));
        }

        private void OnDamageChanged(EntityUid uid, BloodSuckedComponent component, DamageChangedEvent args)
        {
            if (args.DamageIncreased)
                return;

            if (_prototypeManager.TryIndex<DamageGroupPrototype>("Brute", out var brute) && args.Damageable.Damage.TryGetDamageInGroup(brute, out var bruteTotal)
                && _prototypeManager.TryIndex<DamageGroupPrototype>("Airloss", out var airloss) && args.Damageable.Damage.TryGetDamageInGroup(airloss, out var airlossTotal))
                if (bruteTotal == 0 && airlossTotal == 0)
                    RemComp<BloodSuckedComponent>(uid);
        }


        private bool TryValidateSolution(EntityUid bloodsucker)
        {
            if (_solutionSystem.PercentFull(bloodsucker) >= 1)
            {
                _popups.PopupEntity(Loc.GetString("drink-component-try-use-drink-had-enough"), bloodsucker, bloodsucker, Shared.Popups.PopupType.MediumCaution);
                return false;
            }
            return true;
        }

        public void PlayBloodSuckEffects(EntityUid bloodsucker, EntityUid victim)
        {
            _adminLogger.Add(Shared.Database.LogType.MeleeHit, Shared.Database.LogImpact.Medium, $"{ToPrettyString(bloodsucker):player} sucked blood from {ToPrettyString(victim):target}");
            _audio.PlayPvs("/Audio/Items/drink.ogg", bloodsucker);
            _popups.PopupEntity(Loc.GetString("bloodsucker-blood-sucked-victim", ("sucker", bloodsucker)), victim, victim, Shared.Popups.PopupType.LargeCaution);
            _popups.PopupEntity(Loc.GetString("bloodsucker-blood-sucked", ("target", victim)), bloodsucker, bloodsucker, Shared.Popups.PopupType.Medium);
            EnsureComp<BloodSuckedComponent>(victim);
        }
    }
}
