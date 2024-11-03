using Content.Server.Administration;
using Content.Server.DeltaV.Cargo.Components;
using Content.Server.DeltaV.Cargo.Systems;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.DeltaV.Cargo;

[AdminCommand(AdminFlags.Fun)]
public sealed class ChangeStocksPriceCommand : IConsoleCommand
{
    public string Command => "changestocksprice";
    public string Description => Loc.GetString("cmd-changestocksprice-desc");
    public string Help => Loc.GetString("cmd-changestocksprice-help", ("command", Command));

    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!float.TryParse(args[1], out var newPrice))
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-number"));
            return;
        }

        var name = args[0];

        var stockMarket = _entitySystemManager.GetEntitySystem<StockMarketSystem>();

        var query = _entityManager.EntityQueryEnumerator<StationStockMarketComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (stockMarket.TryChangeStocksPrice(uid, comp, newPrice, name))
                continue;
            shell.WriteLine(Loc.GetString("cmd-changestocksprice-invalid-company"));
            return;
        }

        shell.WriteLine(Loc.GetString("shell-command-success"));
    }
}

[AdminCommand(AdminFlags.Fun)]
public sealed class AddStocksCompanyCommand : IConsoleCommand
{
    public string Command => "addstockscompany";
    public string Description => Loc.GetString("cmd-addstockscompany-desc");
    public string Help => Loc.GetString("cmd-addstockscompany-help", ("command", Command));

    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 2)
        {
            shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
            return;
        }

        if (!float.TryParse(args[1], out var basePrice))
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-number"));
            return;
        }

        var name = args[0];

        var stockMarket = _entitySystemManager.GetEntitySystem<StockMarketSystem>();

        var query = _entityManager.EntityQueryEnumerator<StationStockMarketComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (stockMarket.TryAddCompany(uid, comp, basePrice, name))
                continue;
            shell.WriteLine(Loc.GetString("cmd-addstockscompany-already-exists")); // Assume it can't fail for other reasons :blunt:
            return;
        }

        shell.WriteLine(Loc.GetString("shell-command-success"));
    }
}
