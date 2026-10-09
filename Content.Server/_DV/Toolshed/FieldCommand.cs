using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Maths;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Errors;
using Robust.Shared.Toolshed.Syntax;
using Robust.Shared.Toolshed.TypeParsers;
using Robust.Shared.Utility;
using Robust.Shared.ViewVariables;

namespace Content.Server._DV.Toolshed;

/// <summary>
/// Reads the value of a single field or property off of the piped input, by name.
/// Member names are resolved the same way <c>vvread</c> does: the member must be exposed
/// via <c>[ViewVariables]</c>, <c>[DataField]</c>, or <c>[IncludeDataField]</c>.
/// </summary>
/// <example><code>ent 12345 . Name</code></example>
[ToolshedCommand(Name = "."), AdminCommand(AdminFlags.Debug)]
public sealed class FieldCommand : ToolshedCommand
{
    [CommandImplementation]
    public object? Field([PipedArgument] object? value, [CommandArgument(typeof(FieldNameParser))] string field, IInvocationContext ctx)
    {
        if (value == null)
        {
            ctx.ReportError(new NullInputFieldError());
            return null;
        }

        var type = value.GetType();
        var member = GetSingleMember(type, field);

        // Restrict to members that vvread would let you read, so this can't be used to peek at arbitrary internals.
        if (member == null || !ViewVariablesUtility.TryGetViewVariablesAccess(member, out _))
        {
            ctx.ReportError(new NoSuchFieldError(type, field));
            return null;
        }

        return member switch
        {
            FieldInfo f => f.GetValue(value),
            PropertyInfo p => p.GetValue(value),
            _ => null,
        };
    }

    /// <summary>
    /// Finds the field or property with the given name, mirroring the resolution used by <c>vvread</c>.
    /// </summary>
    private static MemberInfo? GetSingleMember(Type type, string member)
    {
        var members = type
            .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.Name == member && m is FieldInfo or PropertyInfo)
            .ToArray();

        if (members.Length == 0)
            return null;

        // In case there's member hiding going on, grab the one declared by the type of the object by default.
        return members.Length > 1
            ? members.FirstOrDefault(m => m.DeclaringType == type) ?? members[0]
            : members[0];
    }
}

/// <summary>
/// Parses a field name as a bare identifier (letters, digits, underscore) so that the <c>.</c> command doesn't
/// require the name to be wrapped in quotes like the default <see cref="string"/> parser does.
/// </summary>
public sealed class FieldNameParser : CustomTypeParser<string>
{
    public override bool TryParse(ParserContext ctx, [NotNullWhen(true)] out string? result)
    {
        ctx.ConsumeWhitespace();
        result = ctx.GetWord(ParserContext.IsToken);
        if (result != null)
            return true;

        if (ctx.PeekRune() is null)
            ctx.Error = new OutOfInputError();
        else
            ctx.Error = new InvalidFieldNameError();

        ctx.Error.Contextualize(ctx.Input, (ctx.Index, ctx.Index + 1));
        return false;
    }

    public override CompletionResult? TryAutocomplete(ParserContext ctx, CommandArgument? arg)
    {
        return CompletionResult.FromHint(GetArgHint(arg));
    }
}

public sealed class InvalidFieldNameError : IConError
{
    public FormattedMessage DescribeInner()
    {
        return FormattedMessage.FromUnformatted("Expected a field name (letters, digits, or underscores).");
    }

    public string? Expression { get; set; }
    public Vector2i? IssueSpan { get; set; }
    public StackTrace? Trace { get; set; }
}

public sealed class NullInputFieldError : IConError
{
    public FormattedMessage DescribeInner()
    {
        return FormattedMessage.FromUnformatted("Cannot read a field off of a null input.");
    }

    public string? Expression { get; set; }
    public Vector2i? IssueSpan { get; set; }
    public StackTrace? Trace { get; set; }
}

public sealed class NoSuchFieldError(Type type, string field) : IConError
{
    public FormattedMessage DescribeInner()
    {
        return FormattedMessage.FromUnformatted($"Type {type.Name} has no readable field or property named '{field}'. It must be exposed via [ViewVariables], [DataField], or [IncludeDataField].");
    }

    public string? Expression { get; set; }
    public Vector2i? IssueSpan { get; set; }
    public StackTrace? Trace { get; set; }
}
