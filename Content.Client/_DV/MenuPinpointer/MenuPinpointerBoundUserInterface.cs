using Content.Shared.Pinpointer;
using Content.Shared._DV.Pinpointer;
using Robust.Shared.Prototypes; 
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;


// this is a list of things from the varyinng programs because i a, unsure which exactly is usefull 

namespace Content.Client._DV.MenuPinpointer;
public sealed class MenuPinpointerBoundUserInterface : BoundUserInterface
{
    
    private MenuPinpointerTargetWindow? _window;
    public MenuPinpointerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<MenuPinpointerTargetWindow>();
        
        _window.OnTargetSelected += OnTargetSelect;
        _window.OnToggleSelected += OnToggleSelect;
    }
    
    //<summary>uses the given name to find the person with that name and tracks them
    private void OnTargetSelect(String targetName)
    {
        SendMessage(new MenuPinpointerOnTargetSelectedMessage(targetName));
    }

    //<summary> toggles the pinpointer and runs set activate without runing regular code because that 
    private void OnToggleSelect()
    {
        SendMessage(new MenuPinpointerOnToggleSelectedMessage());
    }
}
