using System.Numerics;
using Content.Shared.Camera;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Client.GameObjects;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.CustomControls;
using Content.Client.Drunk;


namespace Content.Client.Camera;

public sealed class CameraRecoilSystem : SharedCameraRecoilSystem
{
    [Dependency] private readonly DrunkSystem _drunk = default!;
    [Dependency] private readonly IConfigurationManager _configManager = default!;
    [Dependency] private readonly InputSystem _inputSystem = default!;
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    private float _intensity;
    public float tim;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<CameraKickEvent>(OnCameraKick);
        //_inputSystem.CmdStates.GetState(EngineKeyFunctions.UseSecondary);
        Subs.CVar(_configManager, CCVars.ScreenShakeIntensity, OnCvarChanged, true);
    }

    private void OnCvarChanged(float value)
    {
        _intensity = value;
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        //var query = EntityQueryEnumerator<CameraRecoilComponent>();

        //while (query.MoveNext(out var uid, out var comp)){
        //    if(_inputSystem.CmdStates.GetState(EngineKeyFunctions.CameraOffset) == BoundKeyState.Down){
        //        //_ui.MousePositionScaled.Position
        //        Vector2 mp = new Vector2(0,0);
        //        if(_ui.MainViewport.Viewport!=null&&_ui.ActiveScreen!=null){
        //            Vector2 scrSize = new Vector2(_ui.ActiveScreen.Width, _ui.ActiveScreen.Height);
        //            //_popup.PopupEntity(""+_ui.MainViewport.Viewport.Size+" "+_ui.MousePositionScaled+_ui.ActiveScreen.Width+" "+_ui.ActiveScreen.Height,uid);
        //            mp = (_ui.MousePositionScaled.Position - scrSize/2);
        //            mp = new Vector2(mp.X>250?250:mp.X<-250?-250:mp.X,mp.Y>250?250:mp.Y<-250?-250:mp.Y)/100;
        //            //mp = new Vector2( Vector2.Distance(new Vector2(mp.X,scrSize.Y/200),scrSize/200)>5*_ui.MainViewport.Viewport.Size.X?5*_ui.MainViewport.Viewport.Size.X:mp.X,Vector2.Distance(new Vector2(scrSize.X/200,mp.Y),scrSize/200)>5*_ui.MainViewport.Viewport.Size.Y?5*_ui.MainViewport.Viewport.Size.Y:mp.Y   );
        //            //mp = new Vector2(MathF.Max(MathF.Min(mp.X,5),-5),MathF.Max(MathF.Min(mp.Y,5),-5));
        //            //Vector2.
        //        }//(_ui.MousePositionScaled.Position-_ui.MainViewport.ViewportResolution/2)/_ui.MainViewport.ViewportResolution;
        //        comp.CurrentLookOffset = new Vector2(MathHelper.Lerp(comp.CurrentLookOffset.X, mp.X + MathF.Sin(tim)*MathF.Sqrt(MathF.Max(0,_drunk._overlay.CurrentBoozePower-50))/2000,0.01f),MathHelper.Lerp(comp.CurrentLookOffset.Y, -mp.Y + MathF.Cos(tim)*MathF.Sqrt(MathF.Max(0,_drunk._overlay.CurrentBoozePower-50))/2000,0.01f));
        //    }else{
        //        comp.CurrentLookOffset = new Vector2(MathHelper.Lerp(comp.CurrentLookOffset.X, MathF.Sin(tim)*MathF.Sqrt(MathF.Max(0,_drunk._overlay.CurrentBoozePower-50))/2000,0.05f),MathHelper.Lerp(comp.CurrentLookOffset.Y, MathF.Cos(tim)*MathF.Sqrt(MathF.Max(0,_drunk._overlay.CurrentBoozePower-50))/2000,0.05f));
        //    }
        //}
        //tim+=frameTime/50*(MathF.Pow(MathF.Min(0,_drunk._overlay.CurrentBoozePower-50),1.1f)/100+1);
        // 120  54   120-54 = 66    54 - 120   -66 abs 66

        // 120 120   50    -10   60
    }

    private void OnCameraKick(CameraKickEvent ev)
    {
        KickCamera(GetEntity(ev.NetEntity), ev.Recoil);
    }

    public override void KickCamera(EntityUid uid, Vector2 recoil, CameraRecoilComponent? component = null)
    {
        if (_intensity == 0)
            return;

        if (!Resolve(uid, ref component, false))
            return;

        recoil *= _intensity;

        // Use really bad math to "dampen" kicks when we're already kicked.
        var existing = component.CurrentKick.Length();
        var dampen = existing / KickMagnitudeMax;
        component.CurrentKick += recoil * (1 - dampen);

        if (component.CurrentKick.Length() > KickMagnitudeMax)
            component.CurrentKick = component.CurrentKick.Normalized() * KickMagnitudeMax;

        component.LastKickTime = 0;
    }
}
