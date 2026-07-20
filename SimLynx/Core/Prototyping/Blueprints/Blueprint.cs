using System.Linq;
using Autofac;
using Autofac.Core;

namespace SimLynx.Core.Prototyping.Blueprints;

internal class Blueprint<TSubject>(BlueprintBuilder<TSubject> builder) : IBlueprint<TSubject>
    where TSubject : class, IPrototypeSubject
{
    private readonly BlueprintBuilder<TSubject>.PreCreateHandler[] preCreateHandlers = builder.OnBeforeCreate
        is not null
        ? [.. builder.OnBeforeCreate.GetInvocationList().Cast<BlueprintBuilder<TSubject>.PreCreateHandler>()]
        : [];

    private readonly BlueprintBuilder<TSubject>.PostCreateHandler[] postCreateHandlers = builder.OnCreate is not null
        ? [.. builder.OnCreate.GetInvocationList().Cast<BlueprintBuilder<TSubject>.PostCreateHandler>()]
        : [];

    private readonly TypeDictionary baseOptions = builder.Options.Clone();
    private readonly Parameter[] injectionParameters = [.. builder.InjectionParameters];

    /// <inheritdoc/>
    public IPrototype<TSubject> Prototype { get; } = builder.Prototype;

    /// <inheritdoc/>
    public TSubject CreateInstance(ILifetimeScope scope)
    {
        var context = new BlueprintBuilder<TSubject>.BuildContext(baseOptions.Clone(), scope);
        var parameters = injectionParameters.Concat(preCreateHandlers.SelectMany(handler => handler.Invoke(context)));

        var instance = scope.Resolve<TSubject>(parameters);

        foreach (var handler in postCreateHandlers)
        {
            handler.Invoke(context, instance);
        }

        return instance;
    }
}
