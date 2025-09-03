using Content.Server._DV.AkashicFold.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Interaction;
using Content.Shared.Verbs;

namespace Content.Server._DV.AkashicFold;

/// <summary>
/// This handles...
/// </summary>
public sealed class AbnormalityEngineSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _link  = default!;

    private List<SyncPodComponent> _foldPods = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AbnormalityEngineComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<AbnormalityEngineComponent, NewLinkEvent>(OnNewLink);
        //SubscribeLocalEvent<AbnormalityEngineComponent, PortDisconnectedEvent>(OnPortDisconnected);
        //SubscribeLocalEvent<AbnormalityEngineComponent, AfterInteractEvent>(OnAfterInteract); // no idea if this is the right event
        SubscribeLocalEvent<AbnormalityEngineComponent, GetVerbsEvent<Verb>>(OnGetVerbs); // evidently not
    }

    private void OnComponentInit(Entity<AbnormalityEngineComponent> ent, ref ComponentInit args)
    {
        _link.EnsureSourcePorts(ent.Owner, ent.Comp.EnginePort);
    }

    private void OnGetVerbs(Entity<AbnormalityEngineComponent> ent,  ref GetVerbsEvent<Verb> args)
    {
        // todo: make this not fucking evil
        args.Verbs.Add(new Verb
        {
            Act = () =>
            {
                Log.Debug("waow we hit the button...");
                UpdateFoldPods();

                // evil loop of pain and suffering
                // just give each pod a linked pod if it doesn't have one
                foreach (var pod in ent.Comp.PortLinkedPods)
                {
                    if (pod.LinkedPod != null)
                        return;

                    Log.Debug("Starting pod link...");
                    foreach (var foldPod in _foldPods)
                    {
                        if (foldPod.LinkedPod != null || foldPod == pod) // second thing should literally never happen
                            continue;

                        pod.LinkedPod = foldPod;
                        foldPod.LinkedPod = pod;
                        Log.Debug("Linked a pod!");
                    }
                }
            },
            Text = "dude i hate this",
        });
    }

    private void OnNewLink(EntityUid uid, AbnormalityEngineComponent component, NewLinkEvent args)
    {
        Log.Debug("abnormality engine recieved new link waow...");
        if (TryComp<SyncPodComponent>(args.Sink, out var pod)/* && args.SourcePort == SyncPodComponent.PodPort*/)
        {
            Log.Debug("yeah looks good nuff");
            component.PortLinkedPods.Add(pod);
        }
    }

    private void UpdateFoldPods()
    {
        _foldPods.Clear();
        foreach (var pod in EntityManager.EntityQuery<SyncPodComponent>())
        {
            if (!pod.IsAkashic)
                continue;

            _foldPods.Add(pod);
        }
    }
}
