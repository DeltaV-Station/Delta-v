using System.Numerics;
using Content.Client.Ghost;
using Content.Shared.Popups;
using Content.Shared.Stray.CustomGhosts;
using Robust.Client.GameObjects;
using Robust.Shared.Player;
using Content.Shared.Ghost;

namespace Content.Client.Stray.CustomGhosts;

public sealed class CustomGhostsSystem : SharedCustomGhosts
{
    //[Dependency] private readonly SharedPopupSystem _popup = default!;


    //public override void Initialize()
    //{
    //    base.Initialize();
    //    SubscribeLocalEvent<CustomGhostsComponent, AppearanceChangeEvent>(UpdVis2);
    //    //SubscribeLocalEvent<CustomGhostsComponent, PlayerAttachedEvent>(UpdVis2);
    //    //SubscribeLocalEvent<CustomGhostsComponent, AppearanceChangeEvent>(UpdVis2);
    //}
    public override void UpdVis(EntityUid uid, CustomGhostsComponent? CGC = null){
        //_popup.PopupEntity("FUCK YEA", uid, PopupType.Large);
        if (!Resolve(uid, ref CGC)|| !TryComp(uid, out SpriteComponent? sprite))
            return;
        //_popup.PopupEntity(".                                                                                                               chTo: "+CGC.currRV, uid);
        //// TODO maybe just move each diue to its own RSI?
        //var state = sprite.LayerGetState(0).Name;
        //if (state == null)
       //     return;
///
        //string? prefix = "animated";//state.Substring(0, state.IndexOf(''));
        //_popup.PopupEntity($"{prefix}{CGC.currRV}                                         .", uid);
        //_popup.PopupEntity("                                  "+$"{CGC.currRV}", uid, PopupType.Large);
        sprite.LayerSetState(0, $"{CGC.currRV}");
    }/*
    public void UpdVis2(EntityUid uid, CustomGhostsComponent CGC, AppearanceChangeEvent args){

        _popup.PopupEntity("FUCK YEA2", uid, PopupType.Large);
        //if (!TryComp(uid, out SpriteComponent? sprite))
        //    return;
        //_popup.PopupEntity(".                                                                                                               chTo: "+CGC.currRV, uid);
        //// TODO maybe just move each diue to its own RSI?
        //var state = sprite.LayerGetState(0).Name;
        if (args.Sprite == null)
            return;
///
        string? prefix = "animated";//state.Substring(0, state.IndexOf(''));
        //_popup.PopupEntity($"{prefix}{CGC.currRV}                                         .", uid);
        args.Sprite.LayerSetState(0, $"{prefix}{CGC.currRV}");
    }*/
  // public void UpdVis2(EntityUid uid, CustomGhostsComponent CGC, PlayerAttachedEvent args){
    //    _popup.PopupEntity("FUCK YEA", uid, PopupType.Large);
    //    if (!TryComp(uid, out SpriteComponent? sprite))
    //        return;
    //    //_popup.PopupEntity(".                                                                                                               chTo: "+CGC.currRV, uid);
    //    //// TODO maybe just move each diue to its own RSI?
    //    var state = sprite.LayerGetState(0).Name;
    //    if (state == null)
    //        return;
/////
    //    string? prefix = "animated";//state.Substring(0, state.IndexOf(''));
    //    //_popup.PopupEntity($"{prefix}{CGC.currRV}                                         .", uid);
    //    sprite.LayerSetState(0, $"{prefix}{CGC.currRV}");
    //}
    //protected override void OnAppearanceChange(EntityUid uid, CustomGhostsComponent CGC, ref AppearanceChangeEvent args)
    //{
    //    _popup.PopupEntity("FUCK YEA", uid, PopupType.Large);
    //    if (!Resolve(uid, ref CGC)|| !TryComp(uid, out SpriteComponent? sprite))
    //        return;
    //    //_popup.PopupEntity(".                                                                                                               chTo: "+CGC.currRV, uid);
    //    //// TODO maybe just move each diue to its own RSI?
    //    var state = sprite.LayerGetState(0).Name;
    //    if (state == null)
    //        return;
/////
    //    string? prefix = "animated";//state.Substring(0, state.IndexOf(''));
    //    //_popup.PopupEntity($"{prefix}{CGC.currRV}                                         .", uid);
    //    sprite.LayerSetState(0, $"{prefix}{CGC.currRV}");
    //    base.OnAppearanceChange(uid, component, ref args);
//
    //    if(args.Sprite == null) return;
//
    //    if (AppearanceSystem.TryGetData<string>(uid, CustomGhostAppearance.Sprite, out var rsiPath, args.Component))
    //    {
    //        args.Sprite.LayerSetRSI(0, rsiPath);
    //    }
//
    //    if(AppearanceSystem.TryGetData<float>(uid, CustomGhostAppearance.AlphaOverride, out var alpha, args.Component))
    //    {
    //        args.Sprite.Color = args.Sprite.Color.WithAlpha(alpha);
    //    }
//
    //    if (AppearanceSystem.TryGetData<Vector2>(uid, CustomGhostAppearance.SizeOverride, out var size, args.Component))
    //    {
    //        args.Sprite.Scale = size;
    //    }
    //}
}
