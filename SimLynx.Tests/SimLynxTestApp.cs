using Autofac;
using SimLynx.Discovery.Content;

namespace SimLynx.Tests;

public class SimLynxTestApp(ILifetimeScope scope) : SimLynxApp(scope, ContentPackageManifest.From<SimLynxTestApp>()) { }
