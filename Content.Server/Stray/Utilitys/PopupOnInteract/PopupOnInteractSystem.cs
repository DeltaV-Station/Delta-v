using Content.Shared.Stray.Utilitys.PopupOnInteract;
using Content.Shared.Popups;
using JetBrains.Annotations;
using Robust.Shared.Random;

namespace Content.Server.Stray.Utilitys.PopupOnInteract;

[UsedImplicitly]
public sealed class PopupOnInteractSystem : SharedPopupOnInteractSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    //[Dependency] private readonly SharedLogMessageSenderSystem _logger = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    //[Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    //[Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    //[Dependency] private readonly IPlayerManager _playerManager = default!;



    public override bool MakePopup(EntityUid uid, PopupOnInteractComponent? CGC = null)
    {
       // _popup.PopupEntity("P", uid);
        //if(CGC!=null){
        if (!Resolve(uid, ref CGC))
            return false;
        //No_CKEY_RECIVeD_15278456283765
        //string randV = _random.Next(1,CGC.mri+1)+""
        string picked = "";
        int value = 0;
        try{
            string[] popups = CGC.Popups.Split('&');
            picked = _random.Pick(popups);
            value = int.Parse(picked.Split('|')[1]);
            picked = picked.Split('|')[0];
        }
        catch{
            //Logger.Error("Что-то пошло не так. Возможно не указан какой-нибудь параметр. пример написания: Message1|0&Message2|1.  & - разделение сообщений, | - разделение сообщения и степени его важности");
            //_logger.LogError("Что-то пошло не так. Возможно не указан какой-нибудь параметр. пример написания: Message1|0&Message2|1.  & - разделение сообщений, | - разделение сообщения и степени его важности");
            return false;
        }
        PopupType type = PopupType.Small;
        switch(value){
            case 5:
                type = PopupType.LargeCaution;
                break;
            case 4:
                type = PopupType.Large;
                break;
            case 3:
                type = PopupType.MediumCaution;
                break;
            case 2:
                type = PopupType.Medium;
                break;
            case 1:
                type = PopupType.SmallCaution;
                break;
            default:
                type = PopupType.Small;
                break;
        }
        _popup.PopupEntity(picked, uid, type);
        return true;
        ////_popup.PopupEntity(".                                              2:"+args.Player.Name, uid);
        //for(int i = 0; i < ckeys.Length; i++){
        //    //_popup.PopupEntity(".                        3:"+ckeys[i], uid);
        //    if(args.Player.Name==ckeys[i]){
        //        randV = args.Player.Name;
        //        break;
        //    }
        //}
        ////_popup.PopupEntity(".                                                                                randV:"+randV+" "+ckeys.Length+" "+args.Player.Name+" "+ckeys[0], uid);
        ////CGC.currRV = randV;
        //SetCurrVal(uid, randV, CGC);
        //UpdVis(uid, CGC);
        //UpdVis(uid, CGC);
        //}
        // See client system.
    }/*
    public override void SetRand2(EntityUid uid, string ckey, CustomGhostsComponent CGC)
    {
       // _popup.PopupEntity("P", uid);
        if(CGC!=null){
        //if (!Resolve(uid, ref CGC))
        //    return;
        //No_CKEY_RECIVeD_15278456283765
        string randV = _random.Next(0,CGC.mri+1)+"";
        string[] ckeys = CGC.Ckeys.Split('&');
        //_popup.PopupEntity(".                                      1:"+CGC.Ckeys.Length, uid);
        //_popup.PopupEntity(".                                              2:"+args.Player.Name, uid);
        for(int i = 0; i < ckeys.Length; i++){
            //_popup.PopupEntity(".                        3:"+ckeys[i], uid);
            if(ckey==ckeys[i]){
                randV = ckey;
                break;
            }
        }
        _popup.PopupEntity(".                                                                                randV:"+randV+" "+ckeys.Length+" "+ckey+" "+ckeys[0], uid);
        CGC.currRV = randV;
        SetCurrVal(uid, randV, CGC);
        UpdVis(uid, CGC);
        //UpdVis(uid, CGC);
        }
        // See client system.
    }*/
    //private void OnShit(EntityUid uid, GhostComponent component, PlayerAttachedEvent args)
    //{
    //    if(!_playerManager.TryGetSessionByEntity(uid, out var session))
    //        return;
//
    //    TrySetCustomSprite(uid, session.Name);
    //}


    //public void TrySetCustomSprite(EntityUid ghostUid, string ckey)
    //{
        //var prototypes = _prototypeManager.EnumeratePrototypes<CustomGhostPrototype>();
//
        //foreach (var customGhostPrototype in prototypes)
        //{
        //    if (string.Equals(customGhostPrototype.Ckey, ckey, StringComparison.CurrentCultureIgnoreCase))
        //    {
        //        _appearanceSystem.SetData(ghostUid, CustomGhostAppearance.Sprite, customGhostPrototype.CustomSpritePath.ToString());
        //        _appearanceSystem.SetData(ghostUid, CustomGhostAppearance.SizeOverride, customGhostPrototype.SizeOverride);
//
        //        if(customGhostPrototype.AlphaOverride > 0)
        //        {
        //            _appearanceSystem.SetData(ghostUid, CustomGhostAppearance.AlphaOverride, customGhostPrototype.AlphaOverride);
        //        }
//
        //        if (customGhostPrototype.GhostName != string.Empty)
        //        {
        //            TryComp(ghostUid, out MetaDataComponent? mdc){
        //                mdc = customGhostPrototype.GhostName;
        //            }
        //            //MetaData(ghostUid) = customGhostPrototype.GhostName;
        //        }
//
        //        if (customGhostPrototype.GhostDescription != string.Empty)
        //        {
        //            MetaData(ghostUid).EntityDescription = customGhostPrototype.GhostDescription;
        //        }
//
//
//
//
        //        return;
        //    }
//
        //}
    //}
}
