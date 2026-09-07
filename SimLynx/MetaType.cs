using System;
using Autofac;

namespace SimLynx;

/// <summary>
/// Static class for working with <see cref="IMetaType"/> instances.
/// </summary>
public static class MetaType
{
    /// <summary>
    /// Helper to dynamically resolve <see cref="IMetaType"/> instances from the DI container.
    /// </summary>
    /// <param name="container">The container to resolve meta types from.</param>
    public class Resolver(IComponentContext container)
    {
        /// <summary>
        /// Dynamically resolves the <see cref="IMetaType"/> for the given <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to resolve metadata for.</param>
        /// <returns>The <see cref="IMetaType"/> instance for the specified type.</returns>
        public IMetaType For(Type type)
        {
            return (IMetaType)container.Resolve(typeof(IMetaType<>).MakeGenericType(type));
        }

        /// <summary>
        /// Resolves the <see cref="IMetaType{T}"/> for the given <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">The type to resolve metadata for.</typeparam>
        /// <returns>The <see cref="IMetaType{T}"/> instance for the specified type.</returns>
        public IMetaType<T> For<T>()
        {
            return container.Resolve<IMetaType<T>>();
        }
    }
}

internal class MetaType<T> : IMetaType<T>
{
    public Type Type => typeof(T);
    public ITypeDictionary<object> Metadata => new TypeDictionary<object>();
}
