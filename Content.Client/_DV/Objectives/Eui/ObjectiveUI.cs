using Content.Shared._DV.Objectives.Eui;
using Robust.Shared.Utility;

namespace Content.Client._DV.Objectives.Eui;

// TODO(Barry): Think of a better name for this
public sealed class ObjectiveUI(ObjectiveData data)
{
    private readonly ObjectiveData _current = data;
    private readonly ObjectiveData _original = data.Clone();
    public bool HasChanges { get; private set; } = false;
    public ObjectiveData Current => _current;
    public bool Deleted = false;

    public NetEntity Entity
    {
        get => _current.Entity;
        set
        {
            _current.Entity = value;
            HasChanges = true;
        }
    }
    public string Issuer
    {
        get => _current.Issuer;
        set
        {
            _current.Issuer = value;
            HasChanges = true;
        }
    }
    public string Title
    {
        get => _current.Info.Title;
        set
        {
            _current.Info.Title = value;
            HasChanges = true;
        }
    }
    public string Description
    {
        get => _current.Info.Description;
        set
        {
            _current.Info.Description = value;
            HasChanges = true;
        }
    }
    public SpriteSpecifier Icon
    {
        get => _current.Info.Icon;
        set
        {
            _current.Info.Icon = value;
            HasChanges = true;
        }
    }

    public void Reset()
    {
        _current.CopyFrom(_original);
        HasChanges = false;
        Deleted = false;
    }
}
