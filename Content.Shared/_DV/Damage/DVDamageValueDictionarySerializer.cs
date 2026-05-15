using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;

namespace Content.Shared._DV.Damage;

/// <summary>
/// Reads a float dictionary with support for named <see cref="DVDamageConstantPrototype" /> constants
/// </summary>
[UsedImplicitly]
public sealed class DVDamageValueDictionarySerializer<TKey> : ITypeReader<Dictionary<TKey, float>, MappingDataNode> where TKey : notnull
{
    public Dictionary<TKey, float> Read(ISerializationManager serializationManager,
        MappingDataNode node, IDependencyCollection dependencies, SerializationHookContext hookCtx, ISerializationContext? context,
        ISerializationManager.InstantiationDelegate<Dictionary<TKey, float>>? instanceProvider)
    {
        var dict = instanceProvider != null ? instanceProvider() : new Dictionary<TKey, float>();
        var proto = dependencies.Resolve<IPrototypeManager>();

        var keyNode = new ValueDataNode();
        foreach (var (key, value) in node.Children)
        {
            keyNode.Value = key;
            var tKey = serializationManager.Read<TKey>(keyNode, hookCtx, context);

            if (value is not ValueDataNode nValue)
            {
                throw new Exception($"{value} is not a scalar");
            }

            if (float.TryParse(nValue.Value, out var num))
            {
                dict[tKey] = num;
                continue;
            }

            dict[tKey] = proto.Index<DVDamageConstantPrototype>(nValue.Value).Value;
        }

        return dict;
    }

    public ValidationNode Validate(ISerializationManager serializationManager,
        MappingDataNode node,
        IDependencyCollection dependencies,
        ISerializationContext? context = null)
    {
        var mapping = new Dictionary<ValidationNode, ValidationNode>();
        var proto = dependencies.Resolve<IPrototypeManager>();

        foreach (var (key, value) in node.Children)
        {
            var keyNode = new ValueDataNode(key);
            ValidationNode valueNode;

            if (value is not ValueDataNode nValue)
            {
                valueNode = new ErrorNode(value, "Node is not a scalar");
            }
            else if (float.TryParse(nValue.Value, out _) || proto.HasIndex<DVDamageConstantPrototype>(nValue.Value))
            {
                valueNode = new ValidatedValueNode(nValue);
            }
            else
            {
                valueNode = new ErrorNode(value, $"{nValue.Value} is neither a damage constant or a literal value");
            }

            mapping.Add(serializationManager.ValidateNode<TKey>(keyNode, context), valueNode);
        }

        return new ValidatedMappingNode(mapping);
    }
}
