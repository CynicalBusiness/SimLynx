
using System;

namespace SimLynx.Core.Defs;

/// <summary>
/// Standard base type for <typeparamref name="TObject"/> defs.
/// </summary>
/// <typeparam name="TObject">The type of object this definition creates.</typeparam>
public abstract class Def<TObject>
    : IDef<TObject>
    where TObject : class, IDefObject
{
    /// <inheritdoc/>
    public required Symbol Id { get; init; }

    /// <inheritdoc/>
    public Type ObjectType
    {
        get;
        set
        {
            if (value is null || !value.IsClass)
            {
                throw new ArgumentException($"ObjectType must be a class type, got: {value}");
            }

            var baseType = field ?? typeof(TObject);
            if (!value.IsAssignableTo(baseType))
            {
                throw new ArgumentException($"ObjectType must be assignable to base type: {baseType}");
            }

            field = value;
        }
    } = typeof(TObject);

    /// <inheritdoc/>
    public bool IsAbstract
    {
        get => field || ObjectType.IsAbstract;
        set;
    } = false;

    /// <inheritdoc/>
    public virtual void Init()
    {
        // no-op by default
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{GetType().Name}#{Id}[{ObjectType}]";
    }
}
