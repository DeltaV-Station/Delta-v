using Content.Shared.Damage;
using Content.Shared.FixedPoint; // Goobstation
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.Bible.Components
{
    [RegisterComponent]
    public sealed partial class BibleComponent : Component
    {
        /// <summary>
        /// Default sound when bible hits somebody.
        /// </summary>
        private static readonly ProtoId<SoundCollectionPrototype> DefaultBibleHit = new("BibleHit");

        /// <summary>
        /// Sound to play when bible hits somebody.
        /// </summary>
        [DataField]
        public SoundSpecifier BibleHitSound = new SoundCollectionSpecifier(DefaultBibleHit, AudioParams.Default.WithVolume(-4f));

        /// <summary>
        /// Damage that will be healed on a success
        /// </summary>
        [DataField("damage", required: true)]
        [ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier Damage = default!;

        /// <summary>
        /// Damage that will be dealt on a failure
        /// </summary>
        [DataField("damageOnFail", required: true)]
        [ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier DamageOnFail = default!;

        /// <summary>
        /// Damage that will be dealt when a non-chaplain attempts to heal
        /// </summary>
        [DataField("damageOnUntrainedUse", required: true)]
        [ViewVariables(VVAccess.ReadWrite)]
        public DamageSpecifier DamageOnUntrainedUse = default!;

        /// <summary>
        /// How much damage to deal to the entity being smitten - Goob
        /// </summary>
        [DataField]
        public DamageSpecifier SmiteDamage = new() { DamageDict = new Dictionary<string, FixedPoint2>() { { "Holy", 25 } } }; // Ungodly

        /// <summary>
        /// How long to stun the entity being smitten - Goob
        /// </summary>
        [DataField]
        public TimeSpan SmiteStunDuration = TimeSpan.FromSeconds(8);

        /// <summary>
        /// Chance the bible will fail to heal someone with no helmet
        /// </summary>
        [DataField("failChance")]
        [ViewVariables(VVAccess.ReadWrite)]
        public float FailChance = 0.34f;

        [DataField("sizzleSound")]
        public SoundSpecifier SizzleSoundPath = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");
        [DataField("healSound")]
        public SoundSpecifier HealSoundPath = new  SoundPathSpecifier("/Audio/Effects/holy.ogg");

        [DataField("locPrefix")]
        public string LocPrefix = "bible";
    }
}
