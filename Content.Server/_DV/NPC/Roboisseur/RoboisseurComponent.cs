using Robust.Shared.Prototypes;

namespace Content.Server.Roboisseur.Roboisseur
{
    [RegisterComponent]
    public sealed partial class RoboisseurComponent : Component
    {
        [ViewVariables]
        [DataField]
        public float Accumulator = 0f;

        [ViewVariables(VVAccess.ReadOnly)]
        [DataField]
        public Boolean Impatient { get; set; } = false;

        [ViewVariables]
        [DataField]
        public TimeSpan ResetTime = TimeSpan.FromMinutes(10);

        [DataField]
        public float BarkAccumulator = 0f;

        [DataField]
        public TimeSpan BarkTime = TimeSpan.FromMinutes(1);

        /// <summary>
        ///     Antispam.
        /// </summary>
        public TimeSpan StateTime = default!;

        [DataField]
        public TimeSpan StateCD = TimeSpan.FromSeconds(5);

        [ViewVariables(VVAccess.ReadWrite)]
        public EntityPrototype DesiredPrototype = default!;

        [DataField]
        public List<LocId> DemandMessages = new()
        {
            "roboisseur-request-1",
            "roboisseur-request-2",
            "roboisseur-request-3",
            "roboisseur-request-4",
            "roboisseur-request-5",
            "roboisseur-request-6"
        };

        [DataField]
        public List<LocId> ImpatientMessages = new()
        {
            "roboisseur-request-impatient-1",
            "roboisseur-request-impatient-2",
            "roboisseur-request-impatient-3",
        };

        [DataField]
        public List<LocId> DemandMessagesTier2 = new()
        {
            "roboisseur-request-second-1",
            "roboisseur-request-second-2",
            "roboisseur-request-second-3"
        };

        [DataField]
        public List<LocId> RewardMessages = new()
        {
            "roboisseur-thanks-1",
            "roboisseur-thanks-2",
            "roboisseur-thanks-3",
            "roboisseur-thanks-4",
            "roboisseur-thanks-5"
        };

        [DataField]
        public List<LocId> RewardMessagesTier2 = new()
        {
            "roboisseur-thanks-second-1",
            "roboisseur-thanks-second-2",
            "roboisseur-thanks-second-3",
            "roboisseur-thanks-second-4",
            "roboisseur-thanks-second-5"
        };

        [DataField]
        public List<LocId> RejectMessages = new()
        {
            "roboisseur-deny-1",
            "roboisseur-deny-2",
            "roboisseur-deny-3"
        };
        /// <summary>
        ///    these protos need to be updated when new food is added
        /// </summary>
        [DataField]
        public List<EntProtoId> Tier2Protos = new()
        {
            "FoodBurgerEmpowered",
            "FoodSoupClown",
            "FoodPiePumpkin",
            "FoodSoupTomato",
            "FoodBreadMeat",
            "FoodBreadCreamcheese",
            "FoodBreadTofu",
            "FoodCheeseCurds",
            "FoodBurgerSuper",
            "FoodNoodlesCopy",
            "FoodSoupMonkey",
            "FoodCakeCarrot",
            "FoodBreadBaguette",
            "FoodTartGrape",
            "FoodMealSashimi",
            "FoodBakedChevreChaud",
            "FoodMealPotatoLoaded",
            "FoodMealRibs",
            "FoodMealQueso",
            "FoodSoupNettle",
            "FoodMealEnchiladas",
            "FoodBurgerBaseball",
            "FoodMealNachosCheesy",
            "FoodSoupChiliHot",
            "FoodMothCapreseSalad",
        };

        [DataField]
        public List<EntProtoId> Tier3Protos = new()
        {
            "FoodSoupChiliClown",
            "FoodCakeCheese",
            "FoodCakeLemoon",
            "FoodTartGapple",
            "FoodMealNachosCuban",
            "FoodSaladWatermelonFruitBowl",
            "FoodBakedDumplings",
            "FoodMealCubancarp",
            "FoodBakedCannabisBrownieBatch",
            "FoodBreadFrenchToast",
            "FoodMothSeedSoup",
            "FoodPieFrosty",
            "FoodBreadBanana",
            "FoodBreadCotton",
            "FoodBurgerCarp",
            "FoodBurgerMcguffin",
            "FoodBurgerMcrib",
            "FoodMothFleetSalad",
            "FoodCakeSuppermatter",
            "FoodBurgerFive",
            "FoodPieBaklava",
            "FoodNoodlesMeatball",
            "FoodSaladValid",
            "FoodSaladKimchi",
            "FoodSaladCitrus",
            "FoodSoupMeatball",
            "FoodSoupWingFangChu",
            "FoodTacoChickenSupreme",
            "FoodTacoBeefSupreme",
            "FoodBakedGrilledCheeseSandwich",
            "FoodMothCheesecakeBalls",
            "FoodSoupChiliCold",
            "FoodMothKachumbariSalad",
            "FoodMothChiliCabbageWrap",
            "FoodMothHeartburnSoup",
            "FoodSoupBisque",
            "FoodCakeSlime",
            "FoodBurgerCrazy",
            "FoodMealPoachedPears",
            "FoodMealPearsBelleHelene",
            "FoodTartPearCheese",
            "FoodMeatSnailCooked",
            "FoodSoupEscargot",
            "FoodMealNachosCuban",
            "FoodSaladHerb",
            "FoodSaladColeslaw",
            "FoodSaladCaesar",
            "FoodSaladFruit",
        };

        [DataField]
        public List<EntProtoId> RobossuierRewards = new()
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

        [DataField]
        public List<EntProtoId> BlacklistedProtos = new()
        {
            "FoodBurgerSpell",
            "FoodMothSqueakingFry",
            "FoodBurgerMime",
            "FoodPizzaCorncob",
            "FoodBurgerGhost",
            "FoodCakeClown",
            "FoodCakeSpaceman",
            "MobCatCake",
            "MobBreadDog",
            "FoodBreadMimana",
            "FoodBreadMeatSpider",
            "FoodBurgerHuman",
            "FoodNoodlesBoiled",
            "FoodPizzaDonkpocket",
            "FoodMothOatStew",
            "FoodDonkpocketBerryWarm",
            "FoodBreadButteredToast",
            "FoodMothCottonSoup",
            "LeavesTobaccoDried",
            "FoodSoupEyeball",
            "FoodBurgerCorgi",
            "FoodBreadPlain",
            "FoodBreadBun",
            "FoodBurgerCat",
            "FoodSoupTomatoBlood",
            "FoodMothSaladBase",
            "FoodPieXeno",
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
            "FoodBurgerAppendix",
            "FoodBurgerRat",
            "FoodBurgerRobot",
            "RegenerativeMesh",
            "FoodDonkpocketHonkWarm",
            "FoodOatmeal",
            "FoodBreadJellySlice",
            "FoodMothCottonSalad",
            // "FoodBreadMoldy",
            "FoodDonkpocketSpicyWarm",
            "FoodPizzaDank",
            "FoodCannabisButter",
            "FoodNoodles",
            "LeavesCannabisDried",
            "FoodBurgerCheese",
            "FoodDonkpocketDankWarm",
            "FoodDonkpocketDank",
            "FoodSpaceshroomCooked",
            "FoodMealFries",
            "MedicatedSuture",
            "FoodDonkpocketWarm",
            "FoodCakePlain",
            "DisgustingSweptSoup",
            "FoodBurgerPlain",
            "FoodSoupMushroom",
            "FoodDonkpocketCarp",
            "FoodDonkpocketCarpWarm",
            "FoodDonkpocketDink",
            "FoodDonkpocketStonkWarm",
            "FoodDonkpocketStonk",
            "FoodDonkpocketBerryWarm",
            "FoodDonkpocketBerry",
            "FoodDonkpocketHonkWarm",
            "FoodDonkpocketHonk",
            "FoodDonkpocketPizzaWarm",
            "FoodDonkpocketPizza",
            "FoodBreadMeatXeno",
            "FoodBakedNugget",
            "FoodBakedPancake",
            "FoodBakedPancakeBb",
            "FoodBakedPancakeCc",
            "FoodBakedWaffle",
            "FoodBakedWaffleSoy",
            "FoodBakedWaffleSoylent",
            "FoodBakedWaffleRoffle",
            "FoodBakedBrownieBatch",
            "FoodBakedBrownie",
            "FoodBakedCannabisBrownieBatch",
            "FoodBakedCannabisBrownie",
            "FoodTartMime",
            "FoodPieAmanita",
            "FoodPizzaMargherita", // pizza lovers in shambles
            "FoodPizzaMeat",
            "FoodPizzaMushroom",
            "FoodPizzaVegetable",
            "FoodPizzaDank",
            "FoodPizzaSassysage",
            "FoodPizzaPineapple",
            "FoodPizzaArnold",
            "FoodPizzaMoldySlice",
            "FoodPizzaUranium",
            "FoodPizzaCotton",
            "FoodMothPizzaFirecracker",
            "FoodMothPizzaFiveCheese",
            "FoodMothPizzaPesto",
            "FoodBurgerDuck",
            "FoodBurgerBear",
            "FoodBurgerClown",
            "FoodBurgerCrab",
            "FoodBurgerXeno",
            "FoodMealMemoryleek",
            "FoodMothMacBalls",
            "FoodJellyAmanita",
            "FoodSoupMiso",
            "FoodSoupTomatoBlue",
            "FoodBoritoPie",
            "LeavesCannabisRainbowDried",
            "LeavesCannabisDried",
            "FoodCakeBrain",
            "FoodBurgerBrain",
            "FoodMeatAnomaly",
            "FoodMothBeanStew",
            "FoodPizzaWorldpeas",
        };
    }
}
