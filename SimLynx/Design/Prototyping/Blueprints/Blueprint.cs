using System.Linq;
using Autofac;
using Autofac.Core;

namespace SimLynx.Design.Prototyping.Blueprints;

internal class Blueprint<TSubject>(BlueprintBuilder<TSubject> builder) : IBlueprint<TSubject>
    where TSubject : class, IPrototypeSubject
{
    private readonly BlueprintPreCreateHandler[] preCreateHandlers = [.. builder.BeforeCreateHandlers];
    private readonly BlueprintPostCreateHandler<TSubject>[] postCreateHandlers = [.. builder.AfterCreateHandlers];

    private readonly TypeDictionary baseOptions = builder.Options.Clone();
    private readonly Parameter[] injectionParameters = [.. builder.InjectionParameters];

    /// <inheritdoc/>
    public IPrototype<TSubject> Prototype { get; } = builder.Prototype;

    /// <inheritdoc/>
    public TSubject CreateInstance(ILifetimeScope scope)
    {
        var context = new BlueprintBuilder<TSubject>.BuildContext(baseOptions.Clone(), scope, Prototype.SubjectType);
        var parameters = injectionParameters.Concat(preCreateHandlers.SelectMany(handler => handler.Invoke(context)));

        var instance = scope.Resolve<TSubject>(parameters);

        foreach (var handler in postCreateHandlers)
        {
            handler.Invoke(context, instance);
        }

        return instance;
    }
}
