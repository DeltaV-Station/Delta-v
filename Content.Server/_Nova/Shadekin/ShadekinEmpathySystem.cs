using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Shared._Nova.Shadekin;
using Content.Shared.Chat;
using Content.Shared.Database;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Server._Nova.Shadekin;

/// <summary>
/// Handles Shadekin empathic communication — a short-range telepathic chat
/// that only other Shadekin with ShadekinEmpathyComponent can hear.
/// Shadekin whispers are automatically broadcast as empathic messages to nearby Shadekin.
/// </summary>
public sealed class ShadekinEmpathySystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShadekinEmpathyComponent, EntitySpokeEvent>(OnEntitySpoke);
    }

    /// <summary>
    /// When a Shadekin whispers, also send the full message to nearby Shadekin as empathic speech.
    /// Normal whisper mechanics still apply (non-Shadekin hear garbled whispers at close range).
    /// </summary>
    private void OnEntitySpoke(EntityUid uid, ShadekinEmpathyComponent component, EntitySpokeEvent args)
    {
        // Only intercept whispers — normal speech works normally
        if (args.ObfuscatedMessage == null)
            return;

        SendEmpathicMessage(uid, args.Message, component);
    }

    private void SendEmpathicMessage(EntityUid source, string message, ShadekinEmpathyComponent empathy)
    {
        if (TryComp<MobStateComponent>(source, out var mobState) && mobState.CurrentState != MobState.Alive)
            return;

        var sourceTransform = Transform(source);
        var sourcePos = _transform.GetWorldPosition(sourceTransform);
        var sourceMap = sourceTransform.MapID;

        var recipients = new List<INetChannel>();
        var query = EntityQueryEnumerator<ShadekinEmpathyComponent, TransformComponent, MobStateComponent>();

        while (query.MoveNext(out var uid, out _, out var xform, out var mob))
        {
            if (uid == source)
                continue;

            if (mob.CurrentState != MobState.Alive)
                continue;

            if (xform.MapID != sourceMap)
                continue;

            var targetPos = _transform.GetWorldPosition(xform);
            var distance = (targetPos - sourcePos).Length();

            if (distance > empathy.Range)
                continue;

            if (TryComp<ActorComponent>(uid, out var actor))
                recipients.Add(actor.PlayerSession.Channel);
        }

        if (recipients.Count == 0)
            return;

        var wrappedMessage = Loc.GetString("shadekin-empathy-message-wrap",
            ("entity", source), ("message", message));

        _adminLogger.Add(LogType.Chat, LogImpact.Low, $"Shadekin empathy from {ToPrettyString(source):Player}: {message}");

        _chatManager.ChatMessageToMany(
            ChatChannel.Whisper,
            message,
            wrappedMessage,
            source,
            false,
            true,
            recipients,
            Color.FromHex("#a855f7")
        );
    }
}
