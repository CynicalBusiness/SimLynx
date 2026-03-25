
using System;
using System.Collections.Generic;
using System.Threading;

namespace SimLynx.Core.Defs;

/// <summary>
/// Registry for
/// </summary>
public class DefRegistry<TBaseDef>
    where TBaseDef : class, IDef
{

    private readonly Dictionary<Type, IDefTypeInfo<TBaseDef>> _typeInfos = [];
    private readonly ReaderWriterLockSlim _lock = new();

    /// <summary>
    /// Checks if the given type is valid for use as a def type in this registry. The type must be a class assignable to
    /// <typeparamref name="TBaseDef"/>, but may be abstract or lack a default constructor and still be valid.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns>True if the type is valid, false otherwise.</returns>
    public static bool IsValidType(Type type)
    {
        return type.IsClass && type.IsAssignableTo(typeof(TBaseDef));
    }

    /// <summary>
    /// Gets the def type information for the given def type, creating it if it does not already exist in the registry.
    /// </summary>
    /// <param name="defType">The type of def to get the information for.</param>
    /// <returns>The def type information for the given def type.</returns>
    public IDefTypeInfo<TBaseDef> Get(Type defType)
    {
        ArgumentNullException.ThrowIfNull(defType, nameof(defType));

        if (!IsValidType(defType))
        {
            throw new ArgumentException(
                $"Type '{defType.FullName}' is not a class assignable to '{typeof(TBaseDef).FullName}'",
                nameof(defType));
        }

        _lock.EnterUpgradeableReadLock();
        try
        {
            if (_typeInfos.TryGetValue(defType, out var typeInfo))
            {
                return typeInfo;
            }

            _lock.EnterWriteLock();
            try
            {
                return CreateTypeInfo(defType);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        finally
        {
            _lock.ExitUpgradeableReadLock();
        }

    }

    /// <inheritdoc cref="Get(Type)"/>
    /// <typeparam name="TDef">The type of def to get the information for.</typeparam>
    public IDefTypeInfo<TDef> Get<TDef>()
        where TDef : class, TBaseDef
    {
        return (IDefTypeInfo<TDef>)Get(typeof(TDef));
    }

    private IDefTypeInfo<TBaseDef> CreateTypeInfo(Type defType)
    {
        if (_typeInfos.TryGetValue(defType, out var existingTypeInfo))
        {
            return existingTypeInfo;
        }

        var baseType = defType.BaseType;
        IDefTypeInfo<TBaseDef>? baseTypeInfo = null;
        if (baseType is not null && IsValidType(baseType))
        {
            baseTypeInfo = Get(baseType);
        }

        var typeInfoType = typeof(DefTypeInfo<,>).MakeGenericType(baseTypeInfo?.Type ?? typeof(TBaseDef), defType);
        var typeInfo = (IDefTypeInfo<TBaseDef>)Activator.CreateInstance(typeInfoType, defType, baseTypeInfo)!;

        _typeInfos[defType] = typeInfo;
        return typeInfo;
    }

}
