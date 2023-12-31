using System.Linq;
using Content.Shared.DeltaV.CartridgeLoader.Cartridges;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.DeltaV;

[TestFixture]
public sealed class CrimeAssistTest
{
    [Test]
    public async Task CrimeAssistValid()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        await server.WaitIdleAsync();

        var mapManager = server.ResolveDependency<IMapManager>();
        var prototypeManager = server.ResolveDependency<IPrototypeManager>();

        var testMap = await pair.CreateTestMap();

        // Get all CrimeAssistPage
        var allProtos = prototypeManager.EnumeratePrototypes<CrimeAssistPage>().ToArray();

        await server.WaitAssertion(() =>
        {
            foreach (var proto in allProtos)
            {
                // Assert that ID is unique, and not present a second time in allProtos
                Assert.That(allProtos.Count(p => p.ID == proto.ID), Is.EqualTo(1),
                    $"CrimeAssistPage {proto.ID} is not unique!");

                if (proto.LocKey != null)
                {
                    Assert.That(Loc.TryGetString(proto.LocKey, out var _),
                        $"CrimeAssistPage {proto.ID} has invalid LocKey {proto.LocKey}!");
                }

                if (proto.LocKeyTitle != null)
                {
                    Assert.That(Loc.TryGetString(proto.LocKeyTitle, out var _),
                        $"CrimeAssistPage {proto.ID} has invalid LocKeyTitle {proto.LocKeyTitle}!");
                }

                if (proto.LocKeyDescription != null)
                {
                    Assert.That(Loc.TryGetString(proto.LocKeyDescription, out var _),
                        $"CrimeAssistPage {proto.ID} has invalid LocKeyDescription {proto.LocKeyDescription}!");
                }

                if (proto.LocKeySeverity != null)
                {
                    Assert.That(Loc.TryGetString(proto.LocKeySeverity, out var _),
                        $"CrimeAssistPage {proto.ID} has invalid LocKeySeverity {proto.LocKeySeverity}!");
                }

                if (proto.LocKeyPunishment != null)
                {
                    Assert.That(Loc.TryGetString(proto.LocKeyPunishment, out var _),
                        $"CrimeAssistPage {proto.ID} has invalid LocKeyPunishment {proto.LocKeyPunishment}!");
                }

                if (proto.OnStart != null)
                {
                    Assert.That(allProtos.Any(p => p.ID == proto.OnStart),
                        $"CrimeAssistPage {proto.ID} has invalid OnStart {proto.OnStart}!");
                }

                if (proto.OnYes != null)
                {
                    Assert.That(allProtos.Any(p => p.ID == proto.OnYes),
                        $"CrimeAssistPage {proto.ID} has invalid OnYes {proto.OnYes}!");
                }

                if (proto.OnNo != null)
                {
                    Assert.That(allProtos.Any(p => p.ID == proto.OnNo),
                        $"CrimeAssistPage {proto.ID} has invalid OnNo {proto.OnNo}!");
                }
            }

            // Because Server/Client pairs can be re-used between Tests, we
            // need to clean up anything that might affect other tests,
            // otherwise this pair cannot be considered clean, and the
            // CleanReturnAsync call would need to be removed.
            mapManager.DeleteMap(testMap.MapId);
        });

        await pair.CleanReturnAsync();
    }
}
