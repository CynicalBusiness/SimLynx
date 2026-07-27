using Autofac;

namespace SimLynx.Tests;

public class SimLynxTestApp(ILifetimeScope scope) : SimLynxApp(scope) { }
