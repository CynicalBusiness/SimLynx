using System;
using Autofac;
using SimLynx.Design;
using SimLynx.Design.Prototyping;
using SimLynx.Design.Prototyping.Properties;
using SimLynx.Testing;

namespace SimLynx.Tests.Design;

public class Prototyping(Prototyping.Fixture fixture) : IClassFixture<Prototyping.Fixture>, IDisposable
{
    public static readonly Symbol OwnPrototypeId = Symbol.For("Own");
    public static readonly Symbol BasePrototypeId = Symbol.For("Base");
    public static readonly Symbol DerivedBaseTypePrototypeId = Symbol.For("DerivedBaseType");
    public static readonly Symbol DerivedTypePrototypeId = Symbol.For("DerivedType");

    public ILifetimeScope Container => fixture.PhaseScope;

    public void Dispose()
    {
        // wipe the registry between tests to avoid cross-test contamination
        Container.Resolve<PrototypeRegistry<PropertySubject>>().Clear();
    }

    [Fact]
    public void Compile_AppliesOwnPropertyConfigs()
    {
        var prototype = GetPrototype<PropertySubject>(Container, OwnPrototypeId);
        prototype.GetProperty(x => x.RequiredText).Configure(() => "required");
        prototype.GetProperty(x => x.Text).Configure(() => "configured");

        var subject = prototype.Compile().CreateInstance(Container);

        Assert.Equal("configured", subject.Text);
    }

    [Fact]
    public void Compile_AppliesInheritedSlotWhenDerivedHasNotResolvedIt()
    {
        var basePrototype = GetPrototype<PropertySubject>(Container, BasePrototypeId);
        basePrototype.GetProperty(x => x.RequiredText).Configure(() => "required");
        basePrototype.GetProperty(x => x.Text).Configure(() => "inherited");
        var derivedPrototype = GetPrototype<PropertySubject>(
            Container,
            DerivedBaseTypePrototypeId,
            basePrototypeId: BasePrototypeId
        );

        var subject = derivedPrototype.Compile().CreateInstance(Container);

        Assert.Equal("inherited", subject.Text);
    }

    [Fact]
    public void Compile_MergesPropertySlotsAcrossDifferentSubjectTypes()
    {
        var basePrototype = GetPrototype<PropertySubject>(Container, BasePrototypeId);
        basePrototype.GetProperty(x => x.RequiredText).Configure(() => "required");
        basePrototype.GetProperty(x => x.Text).Configure(() => "base");

        var derivedPrototype = GetPrototype<DerivedPropertySubject>(
            Container,
            DerivedTypePrototypeId,
            basePrototypeId: BasePrototypeId
        );
        derivedPrototype.GetProperty(x => x.Text).Configure(value => value + "-derived");

        var subject = derivedPrototype.Compile().CreateInstance(Container);

        Assert.Equal("base-derived", subject.Text);
    }

    [Fact]
    public void Compile_MergesSetConfigureAndClearFromBaseToDerived()
    {
        var root = GetPrototype<PropertySubject>(Container, BasePrototypeId);
        root.GetProperty(x => x.RequiredText).Configure(() => "required");
        root.GetProperty(x => x.Text).Configure(() => "root");
        root.GetProperty(x => x.Text).Configure(value => value + "-configured");

        var middle = GetPrototype<PropertySubject>(
            Container,
            DerivedBaseTypePrototypeId,
            basePrototypeId: BasePrototypeId
        );
        middle.GetProperty(x => x.Text).Configure(() => "middle");
        middle.GetProperty(x => x.Text).Configure(value => value + "-configured");

        var derived = GetPrototype<DerivedPropertySubject>(
            Container,
            DerivedTypePrototypeId,
            basePrototypeId: DerivedBaseTypePrototypeId
        );
        var cleared = derived.GetProperty(x => x.Text);
        cleared.Configure(() => "discarded");
        cleared.Clear();
        derived.GetProperty(x => x.Text).Configure(value => value + "-derived");

        var subject = derived.Compile().CreateInstance(Container);

        Assert.Equal("middle-configured-derived", subject.Text);
    }

    [Fact]
    public void Compile_HandlesRequiredNullableValueTypeDefaultConstructibleAndNonPublicSetterProperties()
    {
        var prototype = GetPrototype<PropertySubject>(Container, OwnPrototypeId);
        prototype.GetProperty(x => x.RequiredText).Configure(() => "required");
        prototype.GetProperty(x => x.NullableText).Configure(() => null);
        prototype.GetProperty(x => x.Count).Configure(value => value + 4);
        prototype.GetProperty(x => x.Options).Configure(options => options.Value = 7);
        prototype.GetProperty(x => x.Hidden).Configure(() => "hidden");

        var subject = prototype.Compile().CreateInstance(Container);

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

    public class Fixture() : PhaseFixture<SimLynxTestApp, DesignPhase>(options: new() { ShouldRunPhase = false })
    {
        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);

            builder.RegisterModule(new PrototypeModule<PropertySubject>());

            builder.RegisterType<PropertySubject>().AsSelf();
            builder.RegisterType<DerivedPropertySubject>().AsSelf();
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
