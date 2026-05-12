// DeltaV - When "Fix eating and drinking verbs showing up after a short delay and making your verb UI bounce (#38164)"
// was merged from upstream, it moved StomachComponent into shared, causing a duplicate component registered
// crash on client startup. I have no idea if me commenting this out is problematic, but i don't think so?

//namespace Content.Client._Shitmed.Body.Components;
//[RegisterComponent]
//public sealed partial class StomachComponent : Component { }
