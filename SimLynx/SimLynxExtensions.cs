using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using SimLynx.Core;

namespace SimLynx;

/// <summary>
/// Utility extension methods for SimLynx.
/// </summary>
public static class SimLynxExtensions
{
    /// <summary>
    /// Delegate for <see cref="TrySelect{TItem, TResult}"/>
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="item"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public delegate bool TrySelectDelegate<TItem, TResult>(TItem item, [MaybeNullWhen(false)] out TResult result);

    /// <typeparam name="T">The type of elements in the source sequence.</typeparam>
    /// <param name="this">The source sequence.</param>
    extension<T>(IEnumerable<T> @this)
    {
        /// <summary>
        /// Performs the specified action on each element of the source sequence and yields the element.
        /// </summary>
        /// <param name="action">The action to perform on each element.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> that yields the elements of the source sequence after performing the action on each element.</returns>
        public IEnumerable<T> Tap(Action<T> action)
        {
            foreach (var item in @this)
            {
                action(item);
                yield return item;
            }
        }

        /// <summary>
        /// Enumerates the source sequence and performs the specified action on each element.
        /// </summary>
        /// <param name="action">The action to perform</param>
        public void ForEach(Action<T, int> action)
        {
            int index = 0;
            foreach (var item in @this)
            {
                action(item, index++);
            }
        }

        /// <inheritdoc cref="ForEach{T}(IEnumerable{T}, Action{T, int})"/>
        public void ForEach(Action<T> action)
        {
            @this.ForEach((item, _) => action(item));
        }

        /// <summary>
        /// Attempts to select a value from each element in the source sequence using the specified selector function.
        /// If the selector function returns <c>false</c> for an element, that element is skipped.
        /// </summary>
        /// <typeparam name="TResult">The type of the result value.</typeparam>
        /// <param name="selector">The selector function</param>
        /// <returns>An enumeration of successful selections</returns>
        public IEnumerable<TResult> TrySelect<TResult>(TrySelectDelegate<T, TResult> selector)
        {
            foreach (var item in @this)
            {
                if (selector(item, out var result))
                {
                    yield return result;
                }
            }
        }
    }

    extension(Type @this)
    {
        /// <summary>
        /// Gets an enumeration of all assignable generic types from <paramref name="this"/> type that match the
        /// specified <paramref name="genericTypeDefinition"/>
        /// </summary>
        /// <remarks>
        /// If <paramref name="this"/> type is an open generic type, the enumeration may also include
        /// the open constructed generic types which match the definition. For fully-closed class/struct/interface
        /// types the enumeration will only include closed generic types, for each closed type
        /// whose definition matches.
        /// <br/>
        /// All other varieties of types are considered invalid.
        /// </remarks>
        /// <param name="genericTypeDefinition">The definition to compare against.</param>
        /// <returns>The enumeration of all assignable generic types that match the specified generic type definition.</returns>
        public IEnumerable<Type> GetGenericTypesOf(Type genericTypeDefinition)
        {
            ArgumentNullException.ThrowIfNull(genericTypeDefinition);

            if (!genericTypeDefinition.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    "The provided type must be a generic type definition.",
                    nameof(genericTypeDefinition)
                );
            }

            if (!@this.IsClass && !@this.IsInterface && !@this.IsValueType)
            {
                throw new ArgumentException("The source type must be a class, struct, or interface.", nameof(@this));
            }

            for (Type? baseType = @this; baseType is not null; baseType = baseType.BaseType)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == genericTypeDefinition)
                {
                    yield return baseType;
                }
            }

            foreach (var interfaceType in @this.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == genericTypeDefinition)
                {
                    yield return interfaceType;
                }
            }
        }

        /// <summary>
        /// Determines if the specified type is either a subclass of, or implements, the specified
        /// <paramref name="genericType"/> definition, outputting the first found closed generic type that matches the
        /// definition.
        /// </summary>
        /// <remarks>
        /// The provided <paramref name="this"/> type may be either a concrete class/struct type, an interface, or an
        /// open generic type of such.
        /// </remarks>
        /// <param name="genericType">The generic type to check against.</param>
        /// <param name="found">The found closed generic type, if any.</param>
        /// <returns><c>true</c> if the type is a subclass of the specified open generic type; otherwise, <c>false</c>.</returns>
        public bool IsGenericTypeOf(Type genericType, [MaybeNullWhen(false)] out Type found)
        {
            if (!genericType.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    "The provided type must be a generic type definition.",
                    nameof(genericType)
                );
            }

            found = @this.GetGenericTypesOf(genericType).FirstOrDefault();
            return found is not null;
        }

        /// <summary>
        /// Determines if the specified type is either a subclass of, or implements, the specified
        /// <paramref name="genericType"/> definition.
        /// </summary>
        /// <inheritdoc cref="IsGenericTypeOf(Type, Type, out Type)"/>
        public bool IsGenericTypeOf(Type genericType)
        {
            return @this.IsGenericTypeOf(genericType, out _);
        }

        /// <summary>
        /// Enumerates the base types of the current class type, starting from but not including the type itself.
        /// </summary>
        /// <remarks>
        /// If the current type is not a class, this method returns an empty sequence.
        /// </remarks>
        /// <returns>An enumeration of the base types of the current class type.</returns>
        public IEnumerable<Type> GetBaseTypes()
        {
            if (!@this.IsClass)
            {
                yield break;
            }

            var next = @this;
            while ((next = next?.BaseType) is not null)
            {
                yield return next;
            }
        }

#if !NET5_0_OR_GREATER // this method is included in .NET 8, use it directly when available
        /// <summary>
        /// Determines whether the current type is assignable to the specified target type, inverse of
        /// <see cref="Type.IsAssignableFrom(Type)"/>.
        /// </summary>
        /// <param name="targetType">The target type to check against.</param>
        /// <returns><c>true</c> if the current type is assignable to the specified target type; otherwise, <c>false</c>.</returns>
        public bool IsAssignableTo(Type targetType)
        {
            return targetType.IsAssignableFrom(@this);
        }
#endif

        /// <inheritdoc cref="CreatePropertySetter{TInstance, TProperty}(PropertyInfo)"/>
        public Action<TInstance, TProperty> CreatePropertySetter<TInstance, TProperty>(
            Expression<Func<TInstance, TProperty>> propertyExpression
        )
        {
            if (propertyExpression.Body is not MemberExpression memberExpression)
            {
                throw new ArgumentException(
                    "The provided expression does not represent a property access.",
                    nameof(propertyExpression)
                );
            }

            if (memberExpression.Member is not PropertyInfo propertyInfo)
            {
                throw new ArgumentException(
                    "The provided expression does not represent a property access.",
                    nameof(propertyExpression)
                );
            }

            return propertyInfo.CreatePropertySetter<TInstance, TProperty>();
        }
    }

    extension(IMetaType @this)
    {
        /// <summary>
        /// Indicates whether the type represented by this meta-type is abstract.
        /// </summary>
        public bool IsAbstract => @this.Type.IsAbstract;
    }

    extension(MemberInfo member)
    {
        /// <summary>
        /// Indicates whether or not this property is required on its type.
        /// </summary>
        /// <remarks>
        /// A property is considered required if it uses the <see langword="required"/> modifier or marked with
        /// <see cref="RequiredAttribute"/>.
        /// </remarks>
        public bool IsRequired =>
            member.IsDefined(typeof(RequiredAttribute))
            || member.CustomAttributes.Any(attribute =>
                // in case a down-stream consumer uses their own shim, or the compiler includes it itself, look by name
                attribute.AttributeType.FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute"
            );
    }

    extension(PropertyInfo @this)
    {
        /// <summary>
        /// Indicates whether or not this property is init-only, meaning it should only be set during object
        /// initialization.
        /// </summary>
        /// <remarks>
        /// This method only returns <c>true</c> if the property has an "init only" setter. If the setter is not
        /// "init only" or this property <em>has no setter</em>, this method returns <c>false</c>.
        /// <br/>
        /// Init-only setters are a compiler feature; reflection ignores this behavior.
        /// </remarks>
        public bool IsInitOnly =>
            @this.SetMethod is not null
            && @this.SetMethod.ReturnParameter.CustomAttributes.Any(attribute =>
                // in case a down-stream consumer uses their own shim, or the compiler includes it itself, look by name
                attribute.AttributeType.FullName == "System.Runtime.CompilerServices.IsExternalInit"
            );

        /// <summary>
        /// Indicates whether or not this property is required on its type.
        /// </summary>
        /// <remarks>
        /// This differs from <see cref="get_IsRequired(MemberInfo)"/> in that it only considers the
        /// <see langword="required"/> modifier and nothing else, like Autofac.
        /// </remarks>
        public bool IsNativeRequired =>
            @this.CustomAttributes.Any(attribute =>
                // in case a down-stream consumer uses their own shim, or the compiler includes it itself, look by name
                attribute.AttributeType.FullName == "System.Runtime.CompilerServices.RequiredMemberAttribute"
            );

        /// <summary>
        /// Helper to create a strongly-typed setter delegate for the property, even if the property's setter is
        /// non-public or init-only.
        /// </summary>
        /// <typeparam name="TInstance">The instance type for the setter</typeparam>
        /// <typeparam name="TProperty">The property type for the setter</typeparam>
        /// <returns>A delegate that sets the property value on an instance of <typeparamref name="TInstance"/>.</returns>
        /// <exception cref="ArgumentException">If the instance/property types are invalid</exception>
        public Action<TInstance, TProperty> CreatePropertySetter<TInstance, TProperty>()
        {
            if (!typeof(TProperty).IsAssignableTo(@this.PropertyType))
            {
                throw new ArgumentException(
                    $"Property type '{typeof(TProperty)} is not assignable to property of type '{@this.PropertyType.FullName}'.",
                    nameof(TProperty)
                );
            }

            if (!typeof(TInstance).IsAssignableTo(@this.DeclaringType))
            {
                throw new ArgumentException(
                    $"Property '{@this.Name}' is not declared on type '{typeof(TInstance).FullName}'.",
                    nameof(TInstance)
                );
            }

            var setMethod =
                @this.GetSetMethod(true)
                ?? throw new ArgumentException(
                    $"Property '{@this.Name}' does not have a setter nor init.",
                    nameof(@this)
                );

            var instanceParamExpr = Expression.Parameter(typeof(TInstance), "instance");
            var valueParamExpr = Expression.Parameter(typeof(TProperty), "value");
            var bodyExpr = Expression.Call(instanceParamExpr, setMethod, valueParamExpr);
            return Expression
                .Lambda<Action<TInstance, TProperty>>(bodyExpr, instanceParamExpr, valueParamExpr)
                .Compile();
        }

        /// <summary>
        /// Helper to create a setter action for the property, even if the property's setter is non-public or init-only.
        /// </summary>
        /// <returns>The setter action for the property.</returns>
        /// <exception cref="ArgumentException">If the property does not have a setter nor init.</exception>
        public Action<object, object?> CreatePropertySetter()
        {
            var setMethod =
                @this.GetSetMethod(true)
                ?? throw new ArgumentException(
                    $"Property '{@this.Name}' does not have a setter nor init.",
                    nameof(@this)
                );

            var instanceParamExpr = Expression.Parameter(typeof(object), "instance");
            var valueParamExpr = Expression.Parameter(typeof(object), "value");
            var bodyExpr = Expression.Call(
                Expression.Convert(instanceParamExpr, @this.DeclaringType!),
                setMethod,
                Expression.Convert(valueParamExpr, @this.PropertyType)
            );
            return Expression.Lambda<Action<object, object?>>(bodyExpr, instanceParamExpr, valueParamExpr).Compile();
        }
    }

    extension<TLimit, TActivatorData, TRegistrationStyle>(
        IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> @this
    )
    {
        /// <summary>
        /// Configures the registration to be identified by the specified symbol, allowing
        /// <typeparamref name="TService"/> to be resolved by that symbol as a key.
        /// </summary>
        /// <typeparam name="TService">The type of the service being registered.</typeparam>
        /// <param name="id">The symbol to identify the registration with.</param>
        /// <returns>The updated registration builder.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> IdentifiedBy<TService>(Symbol id)
            where TService : notnull
        {
            return @this.Keyed<TService>(id);
        }

        /// <summary>
        /// Configures the registration to be identified by the specified symbol, allowing it to be resolved by that
        /// <paramref name="serviceType"/> and symbol as a key.
        /// </summary>
        /// <param name="id">The symbol to identify the registration with.</param>
        /// <param name="serviceType">The type of the service being registered.</param>
        /// <returns>The updated registration builder.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> IdentifiedBy(
            Symbol id,
            Type serviceType
        )
        {
            return @this.Keyed(id, serviceType);
        }

        /// <summary>
        /// Enables the injection of select properties via parameters using the provided <paramref name="middleware"/>
        /// instance.
        /// </summary>
        /// <param name="middleware">The parameterized property middleware instance.</param>
        /// <returns>This updated registration builder.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PropertiesParameterized(
            ParameterizedPropertyMiddleware middleware
        )
        {
            return @this.ConfigurePipeline(pipeline =>
            {
                pipeline.Use(middleware);
            });
        }

        /// <summary>
        /// Enables the injection of select properties via parameters matching the provided
        /// <paramref name="propertySelector"/>.
        /// </summary>
        /// <param name="propertySelector">The selector for properties.</param>
        /// <returns>This updated registration builder.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PropertiesParameterized(
            IPropertySelector propertySelector
        )
        {
            return @this.PropertiesParameterized(new ParameterizedPropertyMiddleware(propertySelector));
        }

        /// <summary>
        /// Enables the injection of select properties via parameters matching the provided
        /// <paramref name="propertyNames"/>.
        /// </summary>
        /// <param name="propertyNames">The names of the properties to be injected via parameters.</param>
        /// <returns>This updated registration builder.</returns>
        public IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> PropertiesParameterized(
            IEnumerable<string> propertyNames
        )
        {
            return @this.PropertiesParameterized(new NamedPropertySelector(propertyNames));
        }
    }

    extension(IComponentContext @this)
    {
        /// <summary>
        /// Resolves a <typeparamref name="T"/> service that is identified by the given symbol.
        /// </summary>
        /// <typeparam name="T">The type of the service to resolve.</typeparam>
        /// <param name="id">The symbol identifying the service.</param>
        /// <returns>The resolved service instance.</returns>
        public T ResolveIdentified<T>(Symbol id)
            where T : notnull
        {
            return @this.ResolveKeyed<T>(id);
        }

        /// <summary>
        /// Resolves a service of the specified <paramref name="serviceType"/> that is identified by the given symbol.
        /// </summary>
        /// <param name="id">The symbol identifying the service.</param>
        /// <param name="serviceType">The type of the service to resolve.</param>
        /// <returns>The resolved service instance.</returns>
        public object ResolveIdentified(Symbol id, Type serviceType)
        {
            return @this.ResolveKeyed(id, serviceType);
        }
    }

    extension(ParameterInfo @this)
    {
        /// <summary>
        /// Maps from a property-set-value parameter to the declaring property.
        /// </summary>
        /// <remarks>
        /// This method is copied from Autofac's internal helper by the same name.
        /// </remarks>
        /// <param name="prop">The property info on which the setter is specified.</param>
        /// <returns>True if the parameter is a property setter.</returns>
        public bool TryGetDeclaringProperty([NotNullWhen(returnValue: true)] out PropertyInfo? prop)
        {
            var mi = @this.Member as MethodInfo;
            if (
                mi is not null
                && mi.IsSpecialName
                && mi.Name.StartsWith("set_", StringComparison.Ordinal)
                && mi.DeclaringType is not null
            )
            {
                prop = mi.DeclaringType.GetDeclaredProperty(mi.Name[4..]);
                return true;
            }

            prop = null;
            return false;
        }
    }

    extension(ArgumentException)
    {
#if !NET6_0_OR_GREATER
        // polyfill the static ArgumentNullException methods

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the given argument is null.
        /// </summary>
        /// <remarks>
        /// This method is a polyfill for the static method included in .NET 6 and later.
        /// </remarks>
        /// <param name="argument">The argument to check for null.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">If the argument is null.</exception>
        public static void ThrowIfNull(object? argument, string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the given argument is null.
        /// </summary>
        /// <remarks>
        /// This method is a polyfill for the static method included in .NET 6 and later.
        /// </remarks>
        /// <typeparam name="T">The type of the argument.</typeparam>
        /// <param name="argument">The argument to check for null.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">If the argument is null.</exception>
        public static void ThrowIfNull<T>(T? argument, string? paramName = null)
            where T : class
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the given string argument is null or empty.
        /// </summary>
        /// <remarks>
        /// This method is a polyfill for the static method included in .NET 6 and later.
        /// </remarks>
        /// <param name="argument">The string argument to check for null or empty.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">If the argument is null or empty.</exception>
        public static void ThrowIfNullOrEmpty(string? argument, string? paramName = null)
        {
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentNullException(paramName);
            }
        }

        /// <summary>
        /// Throws an <see cref="ArgumentNullException"/> if the given string argument is null or consists only of white-space characters.
        /// </summary>
        /// <remarks>
        /// This method is a polyfill for the static method included in .NET 6 and later.
        /// </remarks>
        /// <param name="argument">The string argument to check for null or white-space.</param>
        /// <param name="paramName">The name of the parameter.</param>
        /// <exception cref="ArgumentNullException">If the argument is null or consists only of white-space characters.</exception>
        public static void ThrowIfNullOrWhiteSpace(string? argument, string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentNullException(paramName);
            }
        }
#endif
    }

    extension<TDelegate>(Expression<TDelegate> @this)
        where TDelegate : Delegate
    {
        /// <summary>
        /// Tries to interpret this lambda expression as a member selector and returns the corresponding
        /// <see cref="MemberInfo"/> that was selected.
        /// </summary>
        /// <param name="member">The member info if the expression is a valid member selector; otherwise, null.</param>
        /// <returns>True if the expression is a valid member selector; otherwise, false.</returns>
        public bool TryGetSelectedMember([MaybeNullWhen(false)] out MemberInfo member)
        {
            if (@this.Body is not MemberExpression memberExpression)
            {
                member = null!;
                return false;
            }

            member = memberExpression.Member;
            return true;
        }

        /// <summary>
        /// Interprets this lambda expression as a member selector and returns the corresponding
        /// <see cref="MemberInfo"/> that was selected, throwing an exception if the expression is not a valid
        /// member selector.
        /// </summary>
        /// <returns>The member info for the selected member.</returns>
        /// <exception cref="ArgumentException">If the expression is not a valid property selector.</exception>
        public MemberInfo GetSelectedMember()
        {
            if (!@this.TryGetSelectedMember(out var member))
            {
                throw new ArgumentException($"The given expression '{@this}' is not a valid selector.", nameof(@this));
            }

            return member;
        }

        /// <summary>
        /// Tries to interpret this lambda expression as a property selector and returns the corresponding
        /// <see cref="PropertyInfo"/> that was selected.
        /// </summary>
        /// <param name="property">The property info if the expression is a valid property selector; otherwise, null.</param>
        /// <returns>True if the expression is a valid property selector; otherwise, false.</returns>
        public bool TryGetProperty([MaybeNullWhen(false)] out PropertyInfo property)
        {
            if (!@this.TryGetSelectedMember(out var member) || member is not PropertyInfo propertyInfo)
            {
                property = null!;
                return false;
            }

            property = propertyInfo;
            return true;
        }

        /// <summary>
        /// Interprets this lambda expression as a property selector and returns the corresponding
        /// <see cref="PropertyInfo"/> that was selected, throwing an exception if the expression is not a valid
        /// property selector.
        /// </summary>
        /// <returns>The property info for the selected property.</returns>
        /// <exception cref="ArgumentException">If the expression is not a valid property selector.</exception>
        public PropertyInfo GetSelectedProperty()
        {
            if (!@this.TryGetProperty(out var property))
            {
                throw new ArgumentException(
                    $"The given expression '{@this}' is not a valid property selector.",
                    nameof(@this)
                );
            }

            return property;
        }
    }

    extension(AppDomain @this)
    {
        /// <summary>
        /// Enumerates all the types in all the assemblies loaded in the application domain.
        /// </summary>
        /// <returns>An enumerable of all types in all loaded assemblies.</returns>
        public IEnumerable<Type> GetTypes()
        {
            return @this.GetAssemblies().SelectMany(assembly => assembly.GetTypes());
        }

        private bool TryGetTypeByAssemblyQualifiedName(string assemblyQualifiedName, [NotNullWhen(true)] out Type? type)
        {
            type = Type.GetType(assemblyQualifiedName, throwOnError: false, ignoreCase: false);
            return type is not null;
        }

        private bool TryGetTypeByFullName(string fullName, [NotNullWhen(true)] out Type? type)
        {
            foreach (var assembly in @this.GetAssemblies())
            {
                type = assembly.GetType(fullName, throwOnError: false, ignoreCase: false);
                if (type is not null)
                {
                    return true;
                }
            }

            type = null;
            return false;
        }

        private bool TryGetTypeBySimpleName(string simpleName, [NotNullWhen(true)] out Type? type)
        {
            foreach (var assembly in @this.GetAssemblies())
            {
                type = assembly.GetTypes().FirstOrDefault(t => t.Name == simpleName);
                if (type is not null)
                {
                    return true;
                }
            }

            type = null;
            return false;
        }

        /// <summary>
        /// Attempts to find a type by a given <paramref name="typeName"/> in all loaded assemblies in this domain. The
        /// name may be type's <see cref="MemberInfo.Name"/>, <see cref="Type.FullName"/>, string representation
        /// (as from <see cref="Type.ToString"/>), an assembly-qualified name, or a "class name like"
        /// (i.e. <c>MyClass&lt;T&gt;</c> or <c>Dictionary&lt;,&gt;</c>). If multiple types match the given name, the first one found will be
        /// returned.
        /// </summary>
        /// <remarks>
        /// This method may return a concrete type, generic or otherwise, or a generic type definition, depending on
        /// the input.
        /// <br/>
        /// If this method successfully finds a generic type and all its generic parameters, but those parameters
        /// fail to satisfy the generic type's constraints, this method will throw the same
        /// <see cref="ArgumentException"/> that would be thrown by <see cref="Type.MakeGenericType(Type[])"/>.
        /// </remarks>
        /// <param name="typeName">The name of the type to find.</param>
        /// <param name="type">The type if found; otherwise, null.</param>
        /// <returns>True if the type was found; otherwise, false.</returns>
        public bool TryFindType(string typeName, [NotNullWhen(true)] out Type? type)
        {
            if (typeName.Contains('<'))
            {
                // angle bracket means this is a C# style generic type, which is a user thing.
                // we run it back through with standard brackets as the parsing rules are the same otherwise
                return @this.TryFindType(typeName.Replace('<', '[').Replace('>', ']'), out type);
            }

            var bracketIdx = typeName.IndexOf('[');
            if (bracketIdx >= 0)
            {
                // a bracket means this is a generic type, so its more complicated.
                if (typeName[bracketIdx + 1] == '[')
                {
                    // second bracket means this is a "FullName" string, and the generics are fully-qualified.
                    // this can be fed straight to Type.GetType or Assembly.GetType in this form
                    return @this.TryGetTypeByAssemblyQualifiedName(typeName, out type)
                        || @this.TryGetTypeByFullName(typeName, out type);
                }
                else
                {
                    // single bracket is a `ToString` style, meaning generics are present but not assembly-qualified OR this is a definition.
                    // start by finding the generic type definition...
                    var genericDefTypeName = typeName[..bracketIdx];
                    var genericArgsStr = typeName[(bracketIdx + 1)..^1]; // also drop the closing bracket

                    var genericArgNames = new Lazy<List<string>>(() =>
                    {
                        List<string> argNames = [];

                        int startIdx = 0;
                        int subGenericDepth = 0;
                        for (var i = 0; i < genericArgsStr.Length; i++)
                        {
                            switch (genericArgsStr[i])
                            {
                                case '[':
                                    subGenericDepth++;
                                    break;
                                case ']':
                                    subGenericDepth--;
                                    break;
                                case ',':
                                    if (subGenericDepth == 0)
                                    {
                                        argNames.Add(startIdx == i ? string.Empty : genericArgsStr[startIdx..i].Trim());
                                        startIdx = i + 1;
                                    }
                                    break;
                            }
                        }
                        argNames.Add(genericArgsStr[startIdx..].Trim());

                        return argNames;
                    });

                    if (!genericDefTypeName.Contains('`'))
                    {
                        // if there's no backtick, we have to infer the arity of the generic type for the search to work
                        // count commas at top-level depth
                        genericDefTypeName += $"`{genericArgNames.Value.Count}";
                    }

                    if (!@this.TryFindType(genericDefTypeName, out var genericDefType))
                    {
                        // no def means no type, bail
                        type = null;
                        return false;
                    }

                    // now try to pull out each generic argument
                    var firstGenericArgName = genericArgNames.Value[0];
                    if (
                        string.IsNullOrEmpty(firstGenericArgName)
                        || firstGenericArgName == genericDefType.GetGenericArguments()[0].Name
                    )
                    {
                        // if the first generic argument is empty or matches the def's first generic argument, this is a definition
                        // partial closings are not valid in C#, so we can successfully bail here
                        type = genericDefType;
                        return true;
                    }

                    Type[] genericTypeArgs = new Type[genericArgNames.Value.Count];
                    for (var i = 0; i < genericArgNames.Value.Count; i++)
                    {
                        var genericArgName = genericArgNames.Value[i];
                        if (string.IsNullOrEmpty(genericArgName))
                        {
                            // mixed empties are invalid, bail
                            type = null;
                            return false;
                        }

                        if (@this.TryFindType(genericArgName, out var genericArgType))
                        {
                            // good find, keep going
                            genericTypeArgs[i] = genericArgType;
                            continue;
                        }

                        // if we can't find the type, bail
                        type = null;
                        return false;
                    }

                    // make the generic type, letting constraint issues throw
                    type = genericDefType.MakeGenericType(genericTypeArgs);
                    return true;
                }
            }

            if (typeName.Contains(','))
            {
                // if we have a comma, we're assembly-qualified and can be fed straight to Type.GetType
                type = Type.GetType(typeName, throwOnError: false, ignoreCase: false);
                return type is not null;
            }

            // otherwise, try to find a type by name in all loaded assemblies
            return @this.TryGetTypeByFullName(typeName, out type) || @this.TryGetTypeBySimpleName(typeName, out type);
        }
    }

    extension(Assembly @this)
    {
        /// <summary>
        /// Returns an enumeration of all assemblies involved in the current call stack. The resulting enumeration
        /// will never yield the same assembly twice <em>in a row</em>, but may yield the same assembly multiple times
        /// if it appears in different parts of the call stack.
        /// </summary>
        /// <returns>An enumerable of all assemblies involved in the current call stack.</returns>
        /// <seealso cref="GetDistinctCallingAssemblies"/>
        public static IEnumerable<Assembly> GetCallingAssemblies()
        {
            Assembly? lastAssembly = null;
            var stack = new StackTrace(false);

            foreach (var frame in stack.GetFrames())
            {
                var assembly = frame.GetMethod()?.DeclaringType?.Assembly;
                if (assembly is not null && assembly != lastAssembly)
                {
                    yield return assembly;
                    lastAssembly = assembly;
                }
            }
        }

        /// <summary>
        /// Returns an enumeration of all distinct assemblies involved in the current call stack. The resulting
        /// enumeration will never yield the same assembly twice, even if it appears in different parts of the call
        /// stack.
        /// </summary>
        /// <returns>An enumerable of all distinct assemblies involved in the current call stack.</returns>
        /// <seealso cref="GetCallingAssemblies"/>
        public static IEnumerable<Assembly> GetDistinctCallingAssemblies()
        {
            return Assembly.GetCallingAssemblies().Distinct();
        }
    }
}
