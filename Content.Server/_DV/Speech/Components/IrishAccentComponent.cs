using Content.Server._DV.Speech.EntitySystems;

namespace Content.Server._DV.Speech.Components;

// Takes the ES and assigns the system and component to each other
[RegisterComponent]
[Access(typeof(IrishAccentSystem))]
public sealed partial class IrishAccentComponent : Component
{ }
