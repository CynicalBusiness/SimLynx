using System.Collections.Generic;
using Autofac.Core;

namespace SimLynx.Design.Prototyping.Blueprints;

/// <summary>
/// Handler delegate blueprints before the subject value is created.
/// </summary>
/// <param name="context">The context for the build operation.</param>
/// <returns>Additional injection parameters to be used when creating the subject.</returns>
public delegate IEnumerable<Parameter> BlueprintPreCreateHandler(IBlueprintBuildContext context);

/// <summary>
/// Handler delegate for blueprints after the subject value is created.
/// </summary>
/// <typeparam name="TSubject">The type of the subject having been created.</typeparam>
/// <param name="context">The context for the build operation.</param>
/// <param name="subject">The newly-created subject instance.</param>
public delegate void BlueprintPostCreateHandler<in TSubject>(IBlueprintBuildContext context, TSubject subject)
    where TSubject : class, IPrototypeSubject;
