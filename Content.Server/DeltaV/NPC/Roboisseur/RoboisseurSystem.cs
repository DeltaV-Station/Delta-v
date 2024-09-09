using Content.Server.Administration.Logs;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Server.Chat.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared.Random.Helpers;
using Content.Shared.Kitchen;
using Robust.Server.GameObjects;
using Content.Server.Materials;
using Content.Shared.Database;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server.Roboisseur.Roboisseur
{
    public sealed partial class RoboisseurSystem : EntitySystem
    {
        [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
        [Dependency] private readonly IRobustRandom _random = default!;
        [Dependency] private readonly ChatSystem _chat = default!;
        [Dependency] private readonly MaterialStorageSystem _material = default!;
        [Dependency] private readonly AppearanceSystem _appearance = default!;
        [Dependency] private readonly IGameTiming _timing = default!;
        [Dependency] private readonly IAdminLogManager _adminLog = default!;

        public override void Initialize()
        {
            base.Initialize();


            SubscribeLocalEvent<RoboisseurComponent, ComponentInit>(OnInit);
            SubscribeLocalEvent<RoboisseurComponent, InteractHandEvent>(OnInteractHand);
            SubscribeLocalEvent<RoboisseurComponent, InteractUsingEvent>(OnInteractUsing);
        }


        private void OnInit(EntityUid uid, RoboisseurComponent component, ComponentInit args)
        {
            NextItem(component);
        }

        public override void Update(float frameTime)
        {
            base.Update(frameTime);


            foreach (var roboisseur in EntityQuery<RoboisseurComponent>())
            {
                roboisseur.Accumulator += frameTime;
                roboisseur.BarkAccumulator += frameTime;
                if (roboisseur.BarkAccumulator >= roboisseur.BarkTime.TotalSeconds)
                {
                    roboisseur.BarkAccumulator = 0;
                    string message = Loc.GetString(_random.Pick(roboisseur.DemandMessages), ("item", roboisseur.DesiredPrototype.Name));
                    if (roboisseur.ResetTime.TotalSeconds - roboisseur.Accumulator < 120)
                    {
                        roboisseur.Impatient = true;
                        message = Loc.GetString(_random.Pick(roboisseur.ImpatientMessages), ("item", roboisseur.DesiredPrototype.Name));
                    }
                    else if (CheckTier(roboisseur.DesiredPrototype.ID, roboisseur) > 2)
                        message = Loc.GetString(_random.Pick(roboisseur.DemandMessagesTier2), ("item", roboisseur.DesiredPrototype.Name));
                    _chat.TrySendInGameICMessage(roboisseur.Owner, message, InGameICChatType.Speak, true);
                }

                if (roboisseur.Accumulator >= roboisseur.ResetTime.TotalSeconds)
                {
                    roboisseur.Impatient = false;
                    NextItem(roboisseur);
                }
            }
        }

        private void RewardServicer(EntityUid uid, RoboisseurComponent component, int tier)
        {
            var r = new Random();
            int rewardToDispense = r.Next(500, 1000) + 350 * tier;

            _material.SpawnMultipleFromMaterial(rewardToDispense, "Credit", Transform(uid).Coordinates);
            if(tier > 1)
            {
                while (tier != 0)
                {
                    EntityManager.SpawnEntity(_random.Pick(component.RobossuierRewards), Transform(uid).Coordinates);
                    tier--;
                }
            }
            return;
        }

        private void OnInteractHand(EntityUid uid, RoboisseurComponent component, InteractHandEvent args)
        {
            if (!TryComp<ActorComponent>(args.User, out var actor))
                return;

            if (_timing.CurTime < component.StateTime) // Literally stolen from the sophie code
                return;

            component.StateTime = _timing.CurTime + component.StateCD;

            string message = Loc.GetString(_random.Pick(component.DemandMessages), ("item", component.DesiredPrototype.Name));
            if (CheckTier(component.DesiredPrototype.ID, component) > 1)
                message = Loc.GetString(_random.Pick(component.DemandMessagesTier2), ("item", component.DesiredPrototype.Name));

            _chat.TrySendInGameICMessage(component.Owner, message, InGameICChatType.Speak, true);
        }

        private void OnInteractUsing(EntityUid uid, RoboisseurComponent component, InteractUsingEvent args)
        {
            if (HasComp<MobStateComponent>(args.Used) ||
                MetaData(args.Used)?.EntityPrototype == null)
                return;

            var validItem = CheckValidity(MetaData(args.Used).EntityName, component.DesiredPrototype);
            var nextItem = true;

            if (!validItem)
            {
                _chat.TrySendInGameICMessage(uid, Loc.GetString(_random.Pick(component.RejectMessages)), InGameICChatType.Speak, true);
                return;
            }

            component.Impatient = false;
            EntityManager.QueueDeleteEntity(args.Used);

            int tier = CheckTier(component.DesiredPrototype.ID, component);

            string message = Loc.GetString(_random.Pick(component.RewardMessages));
            if (tier > 1)
                message = Loc.GetString(_random.Pick(component.RewardMessagesTier2));
            _chat.TrySendInGameICMessage(uid, message, InGameICChatType.Speak, true);

            _adminLog.Add(LogType.InteractHand,
                LogImpact.Medium,
                $"{ToPrettyString(args.User):player} sold {ToPrettyString(args.Used)} to {ToPrettyString(uid)}.");

            RewardServicer(args.User, component, tier);

            if (nextItem)
                NextItem(component);
        }

        private bool CheckValidity(String name, EntityPrototype target)
        {
            // 1: directly compare Names
            // name instead of ID because the oracle asks for them by name
            // this could potentially lead to like, labeller exploits maybe but so far only mob names can be fully player-set.
            if (name == target.Name)
                return true;

            return false;
        }

        private int CheckTier(String target, RoboisseurComponent component)
        {
            if (component.Tier2Protos.Contains(target))
                return 2;
            if (component.Tier3Protos.Contains(target))
                return 3;
            return 1;
        }

        private void NextItem(RoboisseurComponent component)
        {
            component.Accumulator = 0;
            component.BarkAccumulator = 0;
            var protoString = GetDesiredItem(component);

            if (_prototypeManager.TryIndex<EntityPrototype>(protoString, out var proto))
                component.DesiredPrototype = proto;
            else
                Log.Error("Roboisseur can't index prototype " + protoString);
        }

        private string GetDesiredItem(RoboisseurComponent component)
        {
            return _random.Pick(GetAllProtos(component));
        }

        public List<string> GetAllProtos(RoboisseurComponent component)
        {

            var allRecipes = _prototypeManager.EnumeratePrototypes<FoodRecipePrototype>();
            var allProtos = new List<String>();

            foreach (var recipe in allRecipes)
                allProtos.Add(recipe.Result);

            foreach (var proto in component.BlacklistedProtos)
                allProtos.Remove(proto);

            return allProtos;
        }
    }

    public enum RobossieurVisualLayers : byte
    {
        Angry
    }
}
