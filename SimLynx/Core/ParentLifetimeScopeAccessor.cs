using Autofac;
using Autofac.Core.Lifetime;

namespace SimLynx.Core;

/// <summary>
/// Helper accessor to get the parent lifetime scope of a given scope.
/// </summary>
public class ParentLifetimeScopeAccessor
{
    private readonly ILifetimeScope _currentScope;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParentLifetimeScopeAccessor"/> class for the given scope.
    /// </summary>
    /// <param name="currentScope">The current lifetime scope.</param>
    public ParentLifetimeScopeAccessor(ILifetimeScope currentScope)
    {
        _currentScope = currentScope;
        currentScope.ChildLifetimeScopeBeginning += HandleChildLifetimeScopeBeginning;
    }

    /// <summary>
    /// The parent lifetime scope of the current scope. This will be null if the current scope is the root scope.
    /// </summary>
    public ILifetimeScope? ParentScope { get; private set; }

    private void HandleChildLifetimeScopeBeginning(object? sender, LifetimeScopeBeginningEventArgs e)
    {
        e.LifetimeScope.Resolve<ParentLifetimeScopeAccessor>().ParentScope = _currentScope;
    }
}
