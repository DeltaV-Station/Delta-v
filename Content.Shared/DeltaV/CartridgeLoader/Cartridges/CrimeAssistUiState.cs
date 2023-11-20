using Content.Shared.CartridgeLoader;
using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class CrimeAssistUiState : BoundUserInterfaceState
{
    public enum UiStates
    {
        MainMenu,
        IsItTerrorism,

        //assault branch
        WasSomeoneAttacked,
        WasItSophont,
        DidVictimDie,
        IsVictimRemovedFromBody,
        WasDeathIntentional,

        //Mindbreaking branch
        ForcedMindbreakerToxin,

        //theft branch
        HadIllegitimateItem,
        WasItAPerson,
        WasSuspectSelling,
        WasSuspectSeenTaking,
        IsItemExtremelyDangerous,

        //trespass branch
        WasSuspectInARestrictedLocation,
        WasEntranceLocked,

        //vandalism branch
        DidSuspectBreakSomething,
        WereThereManySuspects,
        WasDamageSmall,
        WasDestroyedItemImportantToStation,
        IsLargePartOfStationDestroyed,

        //sexual harrassment branch
        WasCrimeSexualInNature,

        //nuisance branch
        WasSuspectANuisance,
        FalselyReportingToSecurity,
        HappenInCourt,
        DuringActiveInvestigation,
        ToCommandStaff,
        WasItCommandItself,

        //Crime Endpoints
        Result_Innocent,
        Result_AnimalCruelty,
        Result_Theft,
        Result_Trespass,
        Result_Vandalism,
        Result_Hooliganism,
        Result_Manslaughter,
        Result_GrandTheft,
        Result_BlackMarketeering,
        Result_Sabotage,
        Result_Mindbreaking,
        Result_Assault,
        Result_AbuseOfPower,
        Result_Possession,
        Result_Endangerment,
        Result_BreakingAndEntering,
        Result_Rioting,
        Result_ContemptOfCourt,
        Result_PerjuryOrFalseReport,
        Result_ObstructionOfJustice,
        Result_Murder,
        Result_Terrorism,
        Result_GrandSabotage,
        Result_Decorporealisation,
        Result_Kidnapping,
        Result_Sedition,
        Result_SexualHarrassment
    }

    public UiStates CurrentState = UiStates.MainMenu;

    public CrimeAssistUiState(UiStates uiState = UiStates.MainMenu)
    {
        CurrentState = uiState;
    }
}

[Serializable, NetSerializable]
public sealed class CrimeAssistSyncMessageEvent : CartridgeMessageEvent
{
    public CrimeAssistSyncMessageEvent()
    { }
}
