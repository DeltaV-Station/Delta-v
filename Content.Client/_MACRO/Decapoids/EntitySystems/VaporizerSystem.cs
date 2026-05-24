using Content.Shared._MACRO.Decapoids.EntitySystems;

namespace Content.Client._MACRO.Decapoids.EntitySystems;

/// <summary>
/// Client-side shell so <see cref="SharedVaporizerSystem.Initialize"/> runs and predicted
/// events (e.g. examine) match the server.
/// </summary>
public sealed class VaporizerSystem : SharedVaporizerSystem;
