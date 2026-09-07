using System.Collections.Immutable;
using System.Linq;
using Autofac;
using Autofac.Core;
using SimLynx.Core;
using SimLynx.Core.Hooks;
using SimLynx.Discovery.Content;

namespace SimLynx.Design.Prototyping.Blueprints;

internal class Blueprint<TSubject>(BlueprintBuilder<TSubject> builder) : IBlueprint<TSubject>
    where TSubject : class, IPrototypeSubject
{
    private readonly BlueprintPreCreateHandlerWithParams[] preCreateHandlers = [.. builder.BeforeCreateHandlers];
    private readonly BlueprintPostCreateHandler<TSubject>[] postCreateHandlers = [.. builder.AfterCreateHandlers];

    private readonly TypeDictionary<object> baseOptions = new(builder.Options);
    private readonly Parameter[] injectionParameters = [.. builder.InjectionParameters];

    public IPrototype<TSubject> Prototype { get; } = builder.Prototype;
    public ImmutableArray<IContentProvider> Attributions { get; } = builder.Attributions;

    /// <inheritdoc/>
    public TSubject CreateInstance(ILifetimeScope scope)
    {
        var context = new BlueprintBuilder<TSubject>.BuildContext(baseOptions.Clone(), scope, Prototype.SubjectType);

        var parameters = injectionParameters
            .Concat(preCreateHandlers.SelectMany(handler => handler.Invoke(context)))
            .ToList(); // TODO pooling?
        parameters.Add(new CovariantTypedParameter(Prototype, typeof(IPrototype)));

        if (scope.TryResolve(out IHook<OnBeforeCreateInstance>? onBeforeCreateHook))
        {
            onBeforeCreateHook.InvokeSync(new OnBeforeCreateInstance(this, context, parameters));
        }

        parameters.Reverse(); // Autofac resolves parameters first-wins, but we want last-wins.
        var instance = scope.Resolve<TSubject>(parameters);

        foreach (var handler in postCreateHandlers)
        {
            handler.Invoke(context, instance);
        }

        if (scope.TryResolve(out IHook<OnCreateInstance>? onCreateHook))
        {
            onCreateHook.InvokeSync(new OnCreateInstance(this, instance));
        }

        return instance;
    }
}
