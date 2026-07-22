using SimLynx.Design.Prototyping.Blueprints;

namespace SimLynx.Design.Prototyping;

/// <summary>
/// A slot for prototype configurations on a <typeparamref name="TSubject"/> prototype.
/// </summary>
/// <typeparam name="TSubject">The type of the prototype subject.</typeparam>
public interface IPrototypeConfigSlotFor<in TSubject> : IPrototypeConfigSlot
    where TSubject : class, IPrototypeSubject
{
    /// <summary>
    /// Configures this slot for the given <paramref name="builder"/>. This is called during prototype compilation to allow
    /// the slot to configure the blueprint with any necessary information.
    /// </summary>
    /// <param name="builder">The blueprint builder to configure.</param>
    public void Configure(IBlueprintBuilder<TSubject> builder);
}
