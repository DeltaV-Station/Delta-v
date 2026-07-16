using Content.Server.Administration;
using Content.Shared._DV.IonLaws;
using Content.Shared.Administration;
using Robust.Shared.Random;
using Robust.Shared.Toolshed;

namespace Content.Server._DV.IonLaws;

[ToolshedCommand, AdminCommand(AdminFlags.Fun)]
public sealed class IonLawCommand : ToolshedCommand
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private DVIonLawSystem? _ionLaw;

    [CommandImplementation("generate")]
    public IEnumerable<string> GenerateNames(int count)
    {
        _ionLaw ??= GetSys<DVIonLawSystem>();

        for (var i = 0; i < count; i++)
        {
            yield return _ionLaw.GenerateLaw(_random);
        }
    }
}
