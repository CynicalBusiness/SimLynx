using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace SimLynx.Core.Logging;

internal class LoggingModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        var sc = new ServiceCollection();
        sc.AddLogging();

        builder.Populate(sc);
    }
}
