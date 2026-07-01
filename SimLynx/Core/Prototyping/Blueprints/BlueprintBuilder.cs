using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Autofac.Core;

namespace SimLynx.Core.Prototyping.Blueprints;

/// <summary>
/// Builder used by configs to construct a blueprint for a given subject type.
/// </summary>
/// <typeparam name="TSubject">The type of the subject for which the blueprint is being built.</typeparam>
public class BlueprintBuilder<TSubject>(IPrototype<TSubject> prototype)
    where TSubject : class, IPrototypeSubject
{
    private readonly List<Parameter> injectionParams = [];
    private readonly Dictionary<Type, object> context = [];

    /// <summary>
    /// The action to be invoked after a new instance of the subject is created.
    /// </summary>
    public Action<TSubject>? OnCreated { get; set; }

    /// <summary>
    /// The prototype for which the blueprint is being built.
    /// </summary>
    public IPrototype<TSubject> Prototype { get; } = prototype;

    /// <summary>
    /// The injection parameters to be used when creating the subject instance. These parameters can be used to provide
    /// values for injected dependencies.
    /// </summary>
    public IEnumerable<Parameter> InjectionParameters => injectionParams;

    /// <summary>
    /// Adds a new injection parameter to the blueprint builder. When assembling the subject, this parameter can be
    /// used to provide an alternate value for an injected dependency.
    /// </summary>
    /// <param name="parameter">The injection parameter to add.</param>
    public void Inject(Parameter parameter)
    {
        injectionParams.Add(parameter);
    }

    /// <summary>
    /// Gets a context object of the specified type. If the context object does not exist, a new instance is created.
    /// </summary>
    /// <remarks>
    /// Context objects are used to store state that is shared between configs during the blueprint building process and
    /// are not passed to the compiled blueprint nor subject.
    /// </remarks>
    /// <typeparam name="TContext">The type of the context object.</typeparam>
    /// <returns>The context object of the specified type.</returns>
    public TContext GetContext<TContext>()
        where TContext : notnull, new()
    {
        if (!TryGetContext<TContext>(out var contextObject))
        {
            contextObject = new TContext();
            SetContext(contextObject);
        }
        return contextObject;
    }

    /// <summary>
    /// Gets a context object of the specified type. If the context object does not exist, a new instance is created
    /// using the provided factory.
    /// </summary>
    /// <remarks>
    /// Context objects are used to store state that is shared between configs during the blueprint building process and
    /// are not passed to the compiled blueprint nor subject.
    /// </remarks>
    /// <typeparam name="TContext">The type of the context object.</typeparam>
    /// <param name="factory">The factory function to create a new instance of the context object if it does not exist.</param>
    /// <returns>The context object of the specified type.</returns>
    public TContext GetContext<TContext>(Func<TContext> factory)
        where TContext : notnull
    {
        if (!TryGetContext<TContext>(out var contextObject))
        {
            contextObject = factory();
            SetContext(contextObject);
        }
        return contextObject;
    }

    /// <summary>
    /// Sets the context object of the specified type. If a context object of that type already exists, it is replaced.
    /// </summary>
    /// <remarks>
    /// Context objects are used to store state that is shared between configs during the blueprint building process and
    /// are not passed to the compiled blueprint nor subject.
    /// </remarks>
    /// <typeparam name="TContext">The type of the context object.</typeparam>
    /// <param name="contextObject">The context object to set.</param>
    public void SetContext<TContext>(TContext contextObject)
        where TContext : notnull
    {
        context[typeof(TContext)] = contextObject;
    }

    /// <summary>
    /// Tries to get a context object of the specified type. If the context object does not exist, returns false.
    /// </summary>
    /// <remarks>
    /// Context objects are used to store state that is shared between configs during the blueprint building process and
    /// are not passed to the compiled blueprint nor subject.
    /// </remarks>
    /// <typeparam name="TContext">The type of the context object.</typeparam>
    /// <param name="contextObject">When this method returns, contains the context object of the specified type, ifit exists; otherwise, the default value for the type.</param>
    /// <returns>true if the context object exists; otherwise, false.</returns>
    public bool TryGetContext<TContext>([NotNullWhen(true)] out TContext? contextObject)
        where TContext : notnull
    {
        var type = typeof(TContext);
        if (context.TryGetValue(type, out var value))
        {
            contextObject = (TContext)value;
            return true;
        }
        contextObject = default;
        return false;
    }
}
