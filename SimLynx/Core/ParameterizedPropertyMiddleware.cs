using System;
using Autofac;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;

namespace SimLynx.Core;

/// <summary>
/// Autofac middleware that enables injection of non-required properties.
/// </summary>
/// <param name="selector">The property selector used to determine which properties are targeted for injection.</param>
public class ParameterizedPropertyMiddleware(IPropertySelector selector) : IResolveMiddleware
{
    /// <inheritdoc/>
    public PipelinePhase Phase => PipelinePhase.Activation;

    /// <inheritdoc/>
    public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
    {
        // let Autofac make the instance first
        next(context);

        var instance = context.Instance!;
        var instanceType = instance.GetType();
        var instanceMetaType = context.Resolve<MetaType.Resolver>().For(instanceType);

        foreach (var prop in instanceMetaType.Properties)
        {
            var setMethod = prop.SetMethod;
            if (setMethod is null || !selector.InjectProperty(prop, instance))
            {
                continue;
            }

            Func<object?>? valueProvider = null;
            foreach (var param in context.Parameters)
            {
                if (param is PropertyParameter propertyParam)
                {
                    // property params have priority over others, so overwrite and break here
                    if (propertyParam.CanSupplyValue(prop, context, out valueProvider))
                    {
                        break;
                    }
                }
                else if (valueProvider is null)
                {
                    // other params can still match, but don't overwrite and keep looking for a property param
                    if (param.CanSupplyValue(setMethod.GetParameters()[0], context, out valueProvider))
                    {
                        continue;
                    }
                }
            }

            if (valueProvider is not null)
            {
                var value = valueProvider();
                prop.SetValue(instance, value);
            }
        }
    }
}
