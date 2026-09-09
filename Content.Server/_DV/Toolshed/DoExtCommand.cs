using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Maths;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Errors;
using Robust.Shared.Utility;

namespace Content.Server._DV.Toolshed;

/// <summary>
/// An extended version of the engine's <c>do</c> command. Runs a console command once per piped value,
/// substituting <c>$NAME</c> tokens in the command string first.
/// </summary>
/// <remarks>
/// On top of the built-in tokens that <c>do</c> supports (<c>$ID</c>, <c>$PID</c>, <c>$WX</c>, <c>$WY</c>,
/// <c>$LX</c>, <c>$LY</c>, <c>$SELF</c>), any other <c>$name</c> is resolved as a toolshed variable and
/// converted to a string. Tokens that don't resolve to anything are left untouched.
///
/// Where the command actually runs is explicit:
/// <list type="bullet">
/// <item><c>doext:client</c> sends it back down to the calling user's own client to run locally. This is what
/// you want for client-side commands like <c>exec</c>, which resolve paths against the client's user data.</item>
/// <item><c>doext:server</c> runs it server-side as the calling session, like <c>do</c> does.</item>
/// </list>
/// </remarks>
/// <example><code>
/// self doext:client "exec /Script_$var.txt"
/// i 5 => $count; self doext:server "somecommand $count"
/// </code></example>
[ToolshedCommand(Name = "doext"), AdminCommand(AdminFlags.Debug)]
public sealed class DoExtCommand : ToolshedCommand
{
    /// <summary>
    /// Matches a <c>$name</c> token. Names use the same character set as toolshed variable names, so this
    /// matches whole identifiers - <c>$IDLE</c> resolves as "IDLE" rather than being mangled into <c>$ID</c>.
    /// </summary>
    private static readonly Regex TokenRegex = new(@"\$(\w+)", RegexOptions.Compiled);

    [Dependency] private readonly IConsoleHost _console = default!;

    private SharedTransformSystem? _xform;

    /// <summary>
    /// Sends the command back down to the calling user's client, which runs it locally.
    /// </summary>
    [CommandImplementation("client"), TakesPipedTypeAsGeneric]
    public IEnumerable<T> Client<T>(IInvocationContext ctx, [PipedArgument] IEnumerable<T> input, string command)
    {
        // No session means there's no client to hand this back to - e.g. the server console.
        if (ctx.Session is not { } session)
        {
            ctx.ReportError(new NoClientSessionError());
            yield break;
        }

        foreach (var value in input)
        {
            _console.RemoteExecuteCommand(session, Substitute(command, value, ctx));
            yield return value;
        }
    }

    /// <summary>
    /// Runs the command on the server as the calling session.
    /// </summary>
    [CommandImplementation("server"), TakesPipedTypeAsGeneric]
    public IEnumerable<T> Server<T>(IInvocationContext ctx, [PipedArgument] IEnumerable<T> input, string command)
    {
        foreach (var value in input)
        {
            _console.ExecuteCommand(ctx.Session, Substitute(command, value, ctx));
            yield return value;
        }
    }

    private string Substitute<T>(string command, T value, IInvocationContext ctx)
    {
        return TokenRegex.Replace(command, match =>
        {
            var name = match.Groups[1].Value;

            if (TryGetBuiltin(name, value, ctx, out var builtin))
                return builtin;

            // Anything else is looked up as a toolshed variable and implicitly stringified.
            if (ctx.ReadVar(name) is { } variable)
                return Stringify(variable);

            // Unresolved, so leave the token alone rather than silently blanking it.
            return match.Value;
        });
    }

    private bool TryGetBuiltin<T>(string name, T value, IInvocationContext ctx, [NotNullWhen(true)] out string? result)
    {
        switch (name)
        {
            case "SELF":
                result = Stringify(value);
                return true;
            case "PID":
                result = (ctx.Session?.AttachedEntity ?? EntityUid.Invalid).ToString();
                return true;
        }

        // The remaining tokens are all positional, so they only apply when piping entities.
        if (value is not EntityUid uid)
        {
            result = null;
            return false;
        }

        switch (name)
        {
            case "ID":
                result = uid.ToString();
                return true;
            case "WX":
                result = Number(WorldPosition(uid).X);
                return true;
            case "WY":
                result = Number(WorldPosition(uid).Y);
                return true;
            case "LX":
                result = Number(Transform(uid).Coordinates.X);
                return true;
            case "LY":
                result = Number(Transform(uid).Coordinates.Y);
                return true;
        }

        result = null;
        return false;
    }

    private Vector2 WorldPosition(EntityUid uid)
    {
        _xform ??= GetSys<SharedTransformSystem>();
        return _xform.GetWorldPosition(uid);
    }

    private static string Number(float value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Stringify(object? value) => value?.ToString() ?? string.Empty;
}

public sealed class NoClientSessionError : IConError
{
    public FormattedMessage DescribeInner()
    {
        return FormattedMessage.FromUnformatted("There is no client to run this on. doext:client must be run by a player, not the server console.");
    }

    public string? Expression { get; set; }
    public Vector2i? IssueSpan { get; set; }
    public StackTrace? Trace { get; set; }
}
