using Content.Server.DeltaV.Speech.EntitySystems;

namespace Content.Server.DeltaV.Speech.Components;

// Takes the ES and assigns the system and component to each other
[RegisterComponent]
[Access(typeof(IrishAccentSystem))]
public sealed partial class IrishAccentComponent : Component
{ }
