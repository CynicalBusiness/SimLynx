using System;

namespace SimLynx;

/// <summary>
/// Static class for working with <see cref="IMetaType"/> instances.
/// </summary>
public static class MetaType
{
    /// <summary>
    /// Gets/creates a <see cref="IMetaType{T}"/> for the <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The type to get/create metadata for.</typeparam>
    /// <returns>The <see cref="IMetaType{T}"/> instance for the specified type.</returns>
    public static IMetaType<T> For<T>()
    {
        return Of<T>.Instance;
    }

    /// <summary>
    /// Gets/creates a <see cref="IMetaType"/> for the specified <paramref name="type"/>.
    /// </summary>
    /// <param name="type">The type to get/create metadata for.</param>
    /// <returns>The <see cref="IMetaType"/> instance for the specified type.</returns>
    public static IMetaType For(Type type)
    {
        var genericType = typeof(Of<>).MakeGenericType(type);
        var instanceProperty = genericType.GetProperty(nameof(Of<>.Instance))!;
        return (IMetaType)instanceProperty.GetValue(null)!;
    }

    private class Of<T> : IMetaType<T>
    {
        public static Of<T> Instance { get; } = new();

        public Type Type => typeof(T);
        public TypeDictionary Metadata => new();
    }
}
