using Content.Server.Access.Systems;
using Content.Server.Paper;
using Content.Server.Popups;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Shared.Player;

namespace Content.Server.DeltaV.Paper;

public sealed class SignatureSystem : EntitySystem
{
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly IdCardSystem _idCard = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    // The sprite used to visualize "signatures" on paper entities.
    private const string SignatureStampState = "paper_stamp-signature";

    public override void Initialize()
    {
        SubscribeLocalEvent<PaperComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
    }

    private void OnGetAltVerbs(EntityUid uid, PaperComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var pen = args.Using;
        if (pen == null || !_tagSystem.HasTag(pen.Value, "Write"))
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                TrySignPaper((uid, component), args.User);
            },
            Text = Loc.GetString("paper-sign-verb"),
            DoContactInteraction = true,
            Priority = 10
        };
        args.Verbs.Add(verb);
    }

    /// <summary>
    ///     Tries add add a signature to the paper with signer's name.
    /// </summary>
    public bool TrySignPaper(Entity<PaperComponent?> paper, EntityUid signer)
    {
        var paperComp = paper.Comp;
        if (!Resolve(paper, ref paperComp))
            return false;

        var signatureName = DetermineEntitySignature(signer);

        var stampInfo = new StampDisplayInfo()
        {
            StampedName = signatureName,
            StampedColor = Color.DarkSlateGray, // TODO: make configurable? Perhaps it should depend on the pen.
        };

        if (!paperComp.StampedBy.Contains(stampInfo) && _paper.TryStamp(paper, stampInfo, SignatureStampState, paperComp))
        {
            // Show popups and play a paper writing sound
            var signedOtherMessage = Loc.GetString("paper-signed-other", ("user", signer), ("target", paper));
            _popup.PopupEntity(signedOtherMessage, signer, Filter.PvsExcept(signer, entityManager: EntityManager), true);

            var signedSelfMessage = Loc.GetString("paper-signed-self", ("target", paper));
            _popup.PopupEntity(signedSelfMessage, signer, signer);

            _audio.PlayPvs(paperComp.Sound, signer);

            _paper.UpdateUserInterface(paper, paperComp);

            return true;
        }
        else
        {
            // Show an error popup
            _popup.PopupEntity(Loc.GetString("paper-signed-failure", ("target", paper)), signer, signer, PopupType.SmallCaution);

            return false;
        }
    }

    private string DetermineEntitySignature(EntityUid uid)
    {
        // If the entity has an ID, use the name on it.
        if (_idCard.TryFindIdCard(uid, out var id) && !string.IsNullOrWhiteSpace(id.Comp.FullName))
        {
            return id.Comp.FullName;
        }

        // Alternatively, return the entity name
        return Name(uid);
    }
}
