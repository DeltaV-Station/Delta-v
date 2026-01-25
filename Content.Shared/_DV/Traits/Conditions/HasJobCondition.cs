using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Traits.Conditions;

/// <summary>
/// Condition that checks if the player has a specific job.
/// Use Invert = true to check if the player does NOT have the job.
/// </summary>
public sealed partial class HasJobCondition : BaseTraitCondition
{
    /// <summary>
    /// The job prototype ID to check for.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<JobPrototype> Job = string.Empty;

    protected override bool EvaluateImplementation(TraitConditionContext ctx)
    {
        if (ctx.JobId is not { } jobId)
            return false;

        return jobId == Job;
    }

    public override string GetTooltip(IPrototypeManager proto, ILocalizationManager loc, int depth)
    {
        var jobName = Job.Id;
        var jobColor = "#ffffff";

        if (proto.TryIndex(Job, out var jobProto))
        {
            jobName = loc.GetString(jobProto.Name);

            // Try to find the job's department color
            foreach (var dept in proto.EnumeratePrototypes<DepartmentPrototype>())
            {
                if (dept.Roles.Contains(Job))
                {
                    jobColor = dept.Color.ToHex();
                    break;
                }
            }
        }

        var tooltip = Invert
            ? loc.GetString("trait-condition-job-not", ("job", jobName), ("color", jobColor))
            : loc.GetString("trait-condition-job-is", ("job", jobName), ("color", jobColor));

        return new string(' ', depth * 2) + "- " + tooltip + Environment.NewLine;
    }
}
