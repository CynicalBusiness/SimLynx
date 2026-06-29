namespace SimLynx.Simulation.ComponentModel;

/// <summary>
/// Handle to a particular type of state on a entity instance.
/// </summary>
/// <typeparam name="TValue">The type of the state value.</typeparam>
public interface IInstanceState<out TValue>
    where TValue : notnull { }
