using Content.Shared.Examine;
using Content.Shared.Interaction.Events;
using Robust.Shared.GameStates;
using Content.Shared.Popups;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
//using Content.Shared.Stray.Utilitys.LogMessage;
//using Robust.Shared.GameObjects;
//using Robust.Client.GameObjects;

namespace Content.Shared.Stray.Utilitys.PopupOnInteract;

public abstract class SharedPopupOnInteractSystem : EntitySystem
{
    //[Dependency] private readonly SharedLogMessageSystem _logMessage = default!;

    //public string getCKEY = "";
    public override void Initialize()
    {
        base.Initialize();
        //SubscribeLocalEvent<CustomGhostsComponent, PlayerAttachedEvent>(OnAtt);
        //SubscribeLocalEvent<CustomGhostsComponent, AfterAutoHandleStateEvent>(OnAfterHandleState);
        //SetRand();
        SubscribeLocalEvent<PopupOnInteractComponent, UseInHandEvent>(OnUseInHand);
        //SubscribeLocalEvent<CustomGhostComponent, LandEvent>(OnLand);
        //SubscribeLocalEvent<CustomGhostsComponent, ExaminedEvent>(OnExamined);
        //SubscribeLocalEvent<CustomGhostsComponent, AppearanceChangeEvent>(UpdVis2);
    }

    //private void OnAfterHandleState(EntityUid uid, CustomGhostsComponent component, ref AfterAutoHandleStateEvent args)
    //{
    //    UpdVis(uid, component);
    //}
//
    private void OnUseInHand(EntityUid uid, PopupOnInteractComponent component, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if(!MakePopup(uid,component)){
            //Logger.Error("Что-то пошло не так. Возможно не указан какой-нибудь параметр. пример написания: Message1|0&Message2|1.  & - разделение сообщений, | - разделение сообщения и степени его важности");
            //Log.Error("Что-то пошло не так. Возможно не указан какой-нибудь параметр. пример написания: Message1|0&Message2|1.  & - разделение сообщений, | - разделение сообщения и степени его важности");
            //LogError("Что-то пошло не так. Возможно не указан какой-нибудь параметр. пример написания: Message1|0&Message2|1.  & - разделение сообщений, | - разделение сообщения и степени его важности");
        }
        //string[] popups = component.Popups.Split('&');
        //if (popups.Length == 1){
//
        //}else{
        //    _popup.PopupEntity(, uid);
        //}
        //args.Handled = true;
        //=Roll(uid, component);
    }
    public virtual bool MakePopup(EntityUid uid, PopupOnInteractComponent? component = null)
    {
        return true;
        // See the server system, client cannot predict rolling.
    }
    //public virtual void LogError(string message){
//
    //}
//
    //private void OnLand(EntityUid uid, DiceComponent component, ref LandEvent args)
    //{
    //    Roll(uid, component);
    //}
//
    //private void OnExamined(EntityUid uid, CustomGhostsComponent CGC, ExaminedEvent args)
    //{
    //    //No details check, since the sprite updates to show the side.
    //    //using (args.PushGroup(nameof(DiceComponent)))
    //    //{
    //    //    args.PushMarkup(Loc.GetString("dice-component-on-examine-message-part-1", ("sidesAmount", dice.Sides)));
    //    //    args.PushMarkup(Loc.GetString("dice-component-on-examine-message-part-2",
    //    //        ("currentSide", dice.CurrentValue)));
    //    //}
    //    SetRand2(uid, getCKEY, CGC);
    //    SetCurrVal(uid, CGC.currRV, CGC);
    //}
//
    //public void SetCurrentSide(EntityUid uid, int side, DiceComponent? die = null)
    //{
    //    if (!Resolve(uid, ref die))
    //        return;
//
    //    if (side < 1 || side > die.Sides)
    //    {
    //        Log.Error($"Attempted to set die {ToPrettyString(uid)} to an invalid side ({side}).");
    //        return;
    //    }
//
    //    die.CurrentValue = (side - die.Offset) * die.Multiplier;
    //    Dirty(uid, die);
    //    UpdateVisuals(uid, die);
    //}
//
    //public void SetCurrentValue(EntityUid uid, int value, DiceComponent? die = null)
    //{
    //    if (!Resolve(uid, ref die))
    //        return;
//
    //    if (value % die.Multiplier != 0 || value/ die.Multiplier + die.Offset < 1)
    //    {
    //        Log.Error($"Attempted to set die {ToPrettyString(uid)} to an invalid value ({value}).");
    //        return;
    //    }
//
    //    SetCurrentSide(uid, value / die.Multiplier + die.Offset, die);
    //}

    //public void SetCurrVal(EntityUid uid, string chTo, CustomGhostsComponent? CGC= null){
    //    if (!Resolve(uid, ref CGC))
    //        return;
//
//
    //    CGC.currRV = chTo;
    //    //_popup.PopupEntity("chTo:"+chTo+" cc:"+ CGC.currRV+"                                                                             ", uid);
    //    Dirty(uid, CGC);
    //    UpdVis(uid, CGC);
//
    //    //_popup.PopupEntity(".                                                                                                               chTo:___________________________________________________________________________________________________________________________________________", uid);
    //    //if (!Resolve(uid, ref CGC)|| !TryComp(uid, out SpriteComponent? sprite))
    //    //    return;
    //    //_popup.PopupEntity(".                                                                                                               chTo: "+CGC.currRV, uid);
    //    ////// TODO maybe just move each diue to its own RSI?
    //    //var state = sprite.LayerGetState(0).Name;
    //    //if (state == null)
    //    //    return;
//////
    //    //string? prefix = "animated";//state.Substring(0, state.IndexOf(''));
    //    //_popup.PopupEntity($"{prefix}{CGC.currRV}                                         .", uid);
    //    //sprite.LayerSetState(0, $"{prefix}{CGC.currRV}");
//
    //}
    ////public void ChangeCHTO(EntityUid uid, string chTo, CustomGhostsComponent? CGC= null){
    ////    if (!Resolve(uid, ref CGC))
    ////        return;
    ////    CGC.currRV = chTo;
    ////    //_popup.PopupEntity("                                                                             "+"chTo:"+chTo+" cc:"+ CGC.currRV, uid);
    ////    SetCurrVal(uid, chTo, CGC);
    ////}
    //public virtual void UpdVis(EntityUid uid, CustomGhostsComponent? CGC = null)
    //{
    //    // See client system.
    //}
    ////public virtual void UpdVis2(EntityUid uid, CustomGhostsComponent CGC, AppearanceChangeEvent args)
    ////{
    ////    // See client system.
    ////}
    ////public void OnStart(EntityUid uid, CustomGhostsComponent CGC, PlayerAttachedEvent args){
    ////    //getCKEY = args.Player.Name;
    ////    SetRand(uid, CGC, args);
    ////}
    //private void OnAtt(EntityUid uid, CustomGhostsComponent CGC, PlayerAttachedEvent args){
    //    SetRand(uid, args, CGC);
    //    //args.Player.AttachedEntity.
    //}
    //public virtual void SetRand(EntityUid uid, PlayerAttachedEvent args, CustomGhostsComponent? CGC = null)
    //{
    //    // See the server system, client cannot predict rolling.
    //}
    ////public virtual void SetRand2(EntityUid uid, string ckey, CustomGhostsComponent CGC)
    ////{
    ////    // See the server system, client cannot predict rolling.
    ////}
}//
