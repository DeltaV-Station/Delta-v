using Content.Shared._DV.AccountLinking;
using Robust.Shared.Network;

namespace Content.Client._DV.AccountLinking;

public sealed class LinkAccountManager : IPostInjectInit
{
    [Dependency] private readonly INetManager _net = default!;

    private readonly List<SharedPatron> _allPatrons = [];

    public SharedPatronTier? Tier { get; private set; }
    public bool Linked { get; private set; }

    public event Action<Guid>? CodeReceived;
    public event Action? Updated;

    private void OnCode(LinkAccountCodeMsg message)
    {
        CodeReceived?.Invoke(message.Code);
    }

    private void OnStatus(LinkAccountStatusMsg ev)
    {
        Tier = ev.Patron?.Tier;
        Linked = ev.Patron?.Linked ?? false;
        Updated?.Invoke();
    }

    private void OnPatronList(PatronListMsg ev)
    {
        _allPatrons.Clear();
        _allPatrons.AddRange(ev.Patrons);
    }

    public IReadOnlyList<SharedPatron> GetPatrons()
    {
        return _allPatrons;
    }

    void IPostInjectInit.PostInject()
    {
        _net.RegisterNetMessage<LinkAccountCodeMsg>(OnCode);
        _net.RegisterNetMessage<LinkAccountRequestMsg>();
        _net.RegisterNetMessage<LinkAccountStatusMsg>(OnStatus);
        _net.RegisterNetMessage<PatronListMsg>(OnPatronList);
    }
}
