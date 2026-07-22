using System;
using Autofac;
using SimLynx.Core.Phasing;
using SimLynx.Core.Prototyping;
using SimLynx.Core.Prototyping.Properties;
using SimLynx.Design;

namespace SimLynx.Tests.Core.Prototyping;

public class PropertyConfigSlotTests(PropertyConfigSlotTests.ContainerFixture containerFixture)
    : IClassFixture<PropertyConfigSlotTests.ContainerFixture>
{
    public static readonly Symbol OwnPrototypeId = Symbol.For("Own");
    public static readonly Symbol BasePrototypeId = Symbol.For("Base");
    public static readonly Symbol DerivedBaseTypePrototypeId = Symbol.For("DerivedBaseType");
    public static readonly Symbol DerivedTypePrototypeId = Symbol.For("DerivedType");

    [Fact]
    public void Compile_AppliesOwnPropertyConfigs()
    {
        using var scope = containerFixture.CreateDesignScope();
        var prototype = GetPrototype<PropertySubject>(scope, OwnPrototypeId);
        prototype.GetProperty(x => x.RequiredText).Configure(() => "required");
        prototype.GetProperty(x => x.Text).Configure(() => "configured");

        var subject = prototype.Compile().CreateInstance(scope);

        Assert.Equal("configured", subject.Text);
    }

    [Fact]
    public void Compile_AppliesInheritedSlotWhenDerivedHasNotResolvedIt()
    {
        using var scope = containerFixture.CreateDesignScope();
        var basePrototype = GetPrototype<PropertySubject>(scope, BasePrototypeId);
        basePrototype.GetProperty(x => x.RequiredText).Configure(() => "required");
        basePrototype.GetProperty(x => x.Text).Configure(() => "inherited");
        var derivedPrototype = GetPrototype<PropertySubject>(
            scope,
            DerivedBaseTypePrototypeId,
            basePrototypeId: BasePrototypeId
        );

        var subject = derivedPrototype.Compile().CreateInstance(scope);

        Assert.Equal("inherited", subject.Text);
    }

    [Fact]
    public void Compile_MergesPropertySlotsAcrossDifferentSubjectTypes()
    {
        using var scope = containerFixture.CreateDesignScope();
        var basePrototype = GetPrototype<PropertySubject>(scope, BasePrototypeId);
        basePrototype.GetProperty(x => x.RequiredText).Configure(() => "required");
        basePrototype.GetProperty(x => x.Text).Configure(() => "base");

        var context = scope.Resolve<PrototypeContext<DerivedPropertySubject>>();
        var derivedPrototype = GetPrototype<DerivedPropertySubject>(
            scope,
            DerivedTypePrototypeId,
            basePrototypeId: BasePrototypeId
        );
        derivedPrototype.GetProperty(x => x.Text).Configure(value => value + "-derived");

        var subject = derivedPrototype.Compile().CreateInstance(scope);

        Assert.Equal("base-derived", subject.Text);
    }

    [Fact]
    public void Compile_MergesSetConfigureAndClearFromBaseToDerived()
    {
        using var scope = containerFixture.CreateDesignScope();
        var root = GetPrototype<PropertySubject>(scope, BasePrototypeId);
        root.GetProperty(x => x.RequiredText).Configure(() => "required");
        root.GetProperty(x => x.Text).Configure(() => "root");
        root.GetProperty(x => x.Text).Configure(value => value + "-configured");

        var middle = GetPrototype<PropertySubject>(scope, DerivedBaseTypePrototypeId, basePrototypeId: BasePrototypeId);
        middle.GetProperty(x => x.Text).Configure(() => "middle");
        middle.GetProperty(x => x.Text).Configure(value => value + "-configured");

        var derived = GetPrototype<DerivedPropertySubject>(
            scope,
            DerivedTypePrototypeId,
            basePrototypeId: DerivedBaseTypePrototypeId
        );
        var cleared = derived.GetProperty(x => x.Text);
        cleared.Configure(() => "discarded");
        cleared.Clear();
        derived.GetProperty(x => x.Text).Configure(value => value + "-derived");

        var subject = derived.Compile().CreateInstance(scope);

        Assert.Equal("middle-configured-derived", subject.Text);
    }

    [Fact]
    public void Compile_HandlesRequiredNullableValueTypeDefaultConstructibleAndNonPublicSetterProperties()
    {
        using var scope = containerFixture.CreateDesignScope();
        var prototype = GetPrototype<PropertySubject>(scope, OwnPrototypeId);
        prototype.GetProperty(x => x.RequiredText).Configure(() => "required");
        prototype.GetProperty(x => x.NullableText).Configure(() => null);
        prototype.GetProperty(x => x.Count).Configure(value => value + 4);
        prototype.GetProperty(x => x.Options).Configure(options => options.Value = 7);
        prototype.GetProperty(x => x.Hidden).Configure(() => "hidden");

        var subject = prototype.Compile().CreateInstance(scope);

        Assert.Equal("required", subject.RequiredText);
        Assert.Null(subject.NullableText);
        Assert.Equal(4, subject.Count);
        Assert.Equal(7, subject.Options.Value);
        Assert.Equal("hidden", subject.Hidden);
    }

    private static IPrototype<TSubject> GetPrototype<TSubject>(
        ILifetimeScope scope,
        Symbol id,
        Maybe<bool> isAbstract = default,
        Maybe<Symbol> basePrototypeId = default
    )
        where TSubject : PropertySubject
    {
        var registry = scope.Resolve<PrototypeRegistry<PropertySubject>>();
        return registry.Configure<TSubject>(id, isAbstract, basePrototypeId);
    }

    public class ContainerFixture : IDisposable
    {
        public ContainerFixture()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule(new PrototypingModule());
            builder.RegisterModule(new PrototypeModule<PropertySubject>());

            builder.RegisterType<PropertySubject>().AsSelf();
            builder.RegisterType<DerivedPropertySubject>().AsSelf();

            Container = builder.Build();
        }

        public IContainer Container { get; }

        public ILifetimeScope CreateDesignScope()
        {
            return Container.BeginLifetimeScope(Phase.GetLifetimeScopeTag<DesignPhase>(DesignPhase.PhaseId));
        }

        public void Dispose()
        {
            Container.Dispose();
        }
    }

    private class PropertySubject : IPrototypeSubject
    {
        public IPrototype Prototype => null!;
        public required string RequiredText { get; set; }
        public string? NullableText { get; set; } = "initial";
        public int Count { get; set; }
        public Options Options { get; set; } = null!;

        [Configurable]
        public string Hidden { get; private set; } = "initial";

        public string Text { get; set; } = "initial";
    }

    private sealed class DerivedPropertySubject : PropertySubject;

    private sealed class Options
    {
        public int Value { get; set; }
    }
}
