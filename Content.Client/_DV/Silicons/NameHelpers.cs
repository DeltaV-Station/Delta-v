using Content.Shared.Labels.Components;

namespace Content.Client._DV.Silicons;

public static class NameHelpers
{
    extension(IEntityManager self)
    {
        public string PaperName(EntityUid paper)
        {
            if (self.TryGetComponent<LabelComponent>(paper, out var label) && label.CurrentLabel is { } labelText)
                return labelText;

            return self.GetComponent<MetaDataComponent>(paper).EntityName;
        }
    }
}
