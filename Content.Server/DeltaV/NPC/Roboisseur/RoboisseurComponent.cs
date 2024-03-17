using Robust.Shared.Prototypes;

namespace Content.Server.Roboisseur.Roboisseur
{
    [RegisterComponent]
    public sealed partial class RoboisseurComponent : Component
    {
        [ViewVariables]
        [DataField("accumulator")]
        public float Accumulator = 0f;

        [ViewVariables(VVAccess.ReadOnly)]
        [DataField("impatient")]
        public Boolean Impatient { get; set; } = false;

        [ViewVariables]
        [DataField("resetTime")]
        public TimeSpan ResetTime = TimeSpan.FromMinutes(10);

        [DataField("barkAccumulator")]
        public float BarkAccumulator = 0f;

        [DataField("barkTime")]
        public TimeSpan BarkTime = TimeSpan.FromMinutes(1);

        /// <summary>
        ///     Antispam.
        /// </summary>
        public TimeSpan StateTime = default!;

        [DataField("stateCD")]
        public TimeSpan StateCD = TimeSpan.FromSeconds(5);

        [ViewVariables(VVAccess.ReadWrite)]
        public EntityPrototype DesiredPrototype = default!;

        [DataField("demandMessages")]
        public IReadOnlyList<string> DemandMessages = new[]
        {
            "roboisseur-request-1",
            "roboisseur-request-2",
            "roboisseur-request-3",
            "roboisseur-request-4",
            "roboisseur-request-5",
            "roboisseur-request-6"
        };

        [DataField("impatientMessages")]
        public IReadOnlyList<string> ImpatientMessages = new[]
        {
            "roboisseur-request-impatient-1",
            "roboisseur-request-impatient-2",
            "roboisseur-request-impatient-3",
        };

        [DataField("demandMessagesTier2")]
        public IReadOnlyList<string> DemandMessagesTier2 = new[]
        {
            "roboisseur-request-second-1",
            "roboisseur-request-second-2",
            "roboisseur-request-second-3"
        };

        [DataField("rewardMessages")]
        public IReadOnlyList<String> RewardMessages = new[]
        {
            "roboisseur-thanks-1",
            "roboisseur-thanks-2",
            "roboisseur-thanks-3",
            "roboisseur-thanks-4",
            "roboisseur-thanks-5"
        };

        [DataField("rewardMessagesTier2")]
        public IReadOnlyList<String> RewardMessagesTier2 = new[]
        {
            "roboisseur-thanks-second-1",
            "roboisseur-thanks-second-2",
            "roboisseur-thanks-second-3",
            "roboisseur-thanks-second-4",
            "roboisseur-thanks-second-5"
        };

        [DataField("rejectMessages")]
        public IReadOnlyList<String> RejectMessages = new[]
        {
            "roboisseur-deny-1",
            "roboisseur-deny-2",
            "roboisseur-deny-3"
        };

        [DataField("tier2Protos")]
        public List<String> Tier2Protos = new()
        {
            "FoodBurgerEmpowered",
            "FoodSoupClown",
            "FoodSoupChiliClown",
            "FoodBurgerSuper",
            "FoodNoodlesCopy",
            // "FoodMothMallow",
            "FoodPizzaCorncob",
            "FoodPizzaDonkpocket",
            "FoodSoupMonkey",
            "FoodMothSeedSoup",
            "FoodTartGrape",
            "FoodMealCubancarp",
            "FoodMealSashimi",
            "FoodBurgerCarp",
            "FoodMealSoftTaco",
            "FoodMothMacBalls",
            "FoodSoupNettle",
            "FoodBurgerDuck",
            "FoodBurgerBaseball"
        };

        [DataField("tier3Protos")]
        public List<String> Tier3Protos = new()
        {
            "FoodBurgerGhost",
            "FoodSaladWatermelonFruitBowl",
            "FoodBakedCannabisBrownieBatch",
            "FoodPizzaDank",
            "FoodBurgerBear",
            "FoodBurgerMime",
            "FoodCakeSuppermatter",
            "FoodSoupChiliCold",
            "FoodSoupBisque",
            "FoodCakeSlime",
            "FoodBurgerCrazy"
        };

        [DataField("robossuierRewards")]
        public IReadOnlyList<String> RobossuierRewards = new[]
        {
            "DrinkIceCreamGlass",
            "FoodFrozenPopsicleOrange",
            "FoodFrozenPopsicleBerry",
            "FoodFrozenPopsicleJumbo",
            "FoodFrozenSnowconeBerry",
            "FoodFrozenSnowconeFruit",
            "FoodFrozenSnowconeClown",
            "FoodFrozenSnowconeMime",
            "FoodFrozenSnowconeRainbow",
            "FoodFrozenCornuto",
            "FoodFrozenSundae",
            "FoodFrozenFreezy",
            "FoodFrozenSandwichStrawberry",
            "FoodFrozenSandwich",
        };

        [DataField("blacklistedProtos")]
        public IReadOnlyList<String> BlacklistedProtos = new[]
        {
            "FoodBurgerSpell",
            "FoodBreadBanana",
            "FoodMothSqueakingFry",
            "FoodMothFleetSalad",
            "FoodBreadMeatSpider",
            "FoodBurgerHuman",
            "FoodNoodlesBoiled",
            "FoodMothOatStew",
            "FoodMeatLizardtailKebab",
            "FoodSoupTomato",
            "FoodDonkpocketBerryWarm",
            "FoodBreadButteredToast",
            "FoodMothCottonSoup",
            "LeavesTobaccoDried",
            "FoodSoupEyeball",
            "FoodMothKachumbariSalad",
            "FoodMeatHumanKebab",
            "FoodMeatRatdoubleKebab",
            "FoodBurgerCorgi",
            "FoodBreadPlain",
            "FoodMeatKebab",
            "FoodBreadBun",
            "FoodBurgerCat",
            "FoodSoupTomatoBlood",
            "FoodMothSaladBase",
            "FoodPieXeno",
            "FoodPiePumpkin",
            "FoodPiePumpkinSlice",
            "FoodDonkpocketTeriyakiWarm",
            "FoodMothBakedCheese",
            "FoodMothPizzaCotton",
            "AloeCream",
            "FoodSnackPopcorn",
            "FoodBurgerSoy",
            "FoodMothToastedSeeds",
            "FoodMothCornmealPorridge",
            "FoodMothBakedCorn",
            // "FoodBreadMoldySlice",
            "FoodRiceBoiled",
            "FoodMothEyeballSoup",
            "FoodMeatRatKebab",
            "FoodBreadCreamcheese",
            "FoodSoupOnion",
            "FoodBurgerAppendix",
            "FoodBurgerRat",
            "RegenerativeMesh",
            "FoodCheeseCurds",
            "FoodDonkpocketHonkWarm",
            "FoodOatmeal",
            "FoodBreadJellySlice",
            "FoodMothCottonSalad",
            // "FoodBreadMoldy",
            "FoodDonkpocketSpicyWarm",
            "FoodCannabisButter",
            "FoodNoodles",
            "FoodBreadMeat",
            "LeavesCannabisDried",
            "FoodBurgerCheese",
            "FoodDonkpocketDankWarm",
            "FoodSpaceshroomCooked",
            "FoodMealFries",
            "MedicatedSuture",
            "FoodDonkpocketWarm",
            "FoodCakePlain",
            "DisgustingSweptSoup",
            "FoodBurgerPlain",
            "FoodBreadGarlicSlice",
            "FoodSoupMushroom",
            "FoodSoupWingFangChu",
            "FoodBreadMeatXeno",
            "FoodCakeBrain",
            "FoodBurgerBrain",
            "FoodSaladCaesar"
        };
    }
}
