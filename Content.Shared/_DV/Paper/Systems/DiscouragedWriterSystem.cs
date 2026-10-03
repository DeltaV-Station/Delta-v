using System.Diagnostics.CodeAnalysis;
using Content.Shared._DV.AACTablet.Events;
using Content.Shared._DV.Paper.Components;
using Content.Shared.Paper;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Paper.Systems;

/// <summary>
/// This discourages entities from writing by delaying their attempts with discouraging popups.
/// </summary>
public sealed class DiscouragedWriterSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DiscouragedWriterComponent, PaperWriteAttemptEvent>(OnPaperWriteAttempt);
        SubscribeLocalEvent<DiscouragedWriterComponent, AACTabletOpenAttemptEvent>(OnAACTabletOpenAttempt);
    }

    private void OnPaperWriteAttempt(Entity<DiscouragedWriterComponent> writer, ref PaperWriteAttemptEvent args)
    {
        args.Cancelled |= !CanWrite(writer, out var failReason);
        if (failReason != null)
            args.FailReason = failReason;
    }

    private void OnAACTabletOpenAttempt(Entity<DiscouragedWriterComponent> writer, ref AACTabletOpenAttemptEvent args)
    {
        args.Cancelled |= !CanWrite(writer, out var failReason);
        if (failReason != null)
            args.FailReason = failReason;
    }

    private bool CanWrite(Entity<DiscouragedWriterComponent> writer, [NotNullWhen(false)] out LocId? failReason)
    {
        failReason = null;

        // If the delay is active, do nothing.
        if (writer.Comp.LastAttemptTime + writer.Comp.Delay > _timing.CurTime)
        {
            failReason = GetFailReason(writer.Comp);
            return false;
        }

        if (writer.Comp.PreviousAttempts > writer.Comp.PunishStacks)
        {
            // Double the stacks each time they write.
            writer.Comp.PunishStacks = writer.Comp.PunishStacks == 0 ? 1 : writer.Comp.PunishStacks * 2;
            writer.Comp.NextStackDecay = _timing.CurTime + writer.Comp.DecayDelay;
            writer.Comp.PreviousAttempts = 0;
            Dirty(writer);
            return true;
        }

        failReason = GetFailReason(writer.Comp);
        writer.Comp.PreviousAttempts++;
        Dirty(writer);
        return false;
    }

    private LocId GetFailReason(DiscouragedWriterComponent writer)
    {
        var locIds = writer.PopupLocIds;
        var locId = locIds.Count > writer.PreviousAttempts
            ? locIds[writer.PreviousAttempts]
            : locIds[^1]; // Get the last entry if there's more attempts than entries.
        return locId;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DiscouragedWriterComponent>();
        while (query.MoveNext(out var uid, out var writer))
        {
            if (writer.NextStackDecay < _timing.CurTime)
                return;

            writer.NextStackDecay = _timing.CurTime + writer.DecayInterval;
            writer.PunishStacks--;
            Dirty(uid, writer);
        }
    }
}
