using Autofac.Core;
using SimLynx.Design.Prototyping.Blueprints;

namespace SimLynx.Design.Prototyping.Parameters;

/// <summary>
/// A prototype config which configures an Autofac injection parameter.
/// </summary>
/// <typeparam name="TSubject">The prototype subject type</typeparam>
/// <param name="name">The identifier for the parameter</param>
public class ParameterConfig<TSubject>(Identifier name)
    : PrototypeValueConfig<TSubject, Parameter>(name),
        IParameterConfig<TSubject>
    where TSubject : class, IPrototypeSubject
{
    /// <inheritdoc/>
    public override bool Apply(IBlueprintBuilder<TSubject> builder)
    {
        if (!TryCreateProvider(out var paramProvider))
        {
            return false;
        }

        builder.ConfigureBeforeCreate(_ => [paramProvider()]);
        return true;
    }
}
