namespace Content.Server.Bible.Components
{
    [RegisterComponent]
    public sealed partial class BibleUserComponent : Component
    {
        /// <summary>
        /// DeltaV - Wheter or not the BibleUser can heal. This is to prevent multiple bibles from
        /// being spammed from a single bible user.
        /// </summary>
        [DataField]
        public bool CanHeal = true;

        /// <summary>
        /// DeltaV - The default cooldown of the bible user's ability to heal with a bible.
        /// 
        /// Should match UseDelay of Bible in Resources/Prototypes/Entities/Objects/Specific/Chapel/bibles.yml
        /// </summary>
        [DataField]
        public TimeSpan Cooldown = TimeSpan.FromSeconds(10);

        /// <summary>
        /// DeltaV - The time that <see cref="CanHeal"/> will be set to true again.  
        /// </summary>
        [DataField]
        public TimeSpan? NextUse;
    }
}
