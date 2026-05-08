using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Autofac.Features.OwnedInstances;
using Microsoft.Extensions.Logging;

namespace SimLynx.Simulation;

internal class StageManager : IStageManager, IStartable, IDisposable
{
    private readonly Dictionary<string, Owned<IStage>> _stages;

    internal StageManager(ILifetimeScope scope, ILogger<StageManager> logger)
    {
        _stages = scope
            .ResolveKeyed<IEnumerable<Owned<IStage>>>(KeyedService.AnyKey)
            .ToDictionary(s => s.Value.Name, s => s);

        foreach (var stage in _stages.Values)
        {
            IStageManager.StageActivatedLogEvent.Log(logger, stage.Value.Name);
        }
    }

    public IStage? this[string stageName] =>
        _stages.TryGetValue(stageName, out var ownedStage) ? ownedStage.Value : null;

    public IEnumerable<IStage> GetAllStages()
    {
        return _stages.Values.Select(s => s.Value);
    }

    public bool TryGetStage(string stageName, out IStage? stage)
    {
        if (_stages.TryGetValue(stageName, out var ownedStage))
        {
            stage = ownedStage.Value;
            return true;
        }
        stage = null;
        return false;
    }

    public void Dispose()
    {
        foreach (var stage in _stages.Values)
        {
            stage.Dispose();
        }
    }

    public void Start()
    {
        // no-op, just need to be resolved to initialize the stages
    }
}
