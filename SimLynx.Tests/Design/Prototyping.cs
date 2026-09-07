using System;
using Autofac;
using Autofac.Core.Registration;
using SimLynx.Design;
using SimLynx.Design.Prototyping;
using SimLynx.Design.Prototyping.Properties;
using SimLynx.Testing;

namespace SimLynx.Tests.Design;

public class Prototyping
{
    public class DesignTests(Fixture fixture) : IClassFixture<Fixture>, IDisposable
    {
        public static readonly Symbol OwnPrototypeId = Symbol.For("Own");
        public static readonly Symbol BasePrototypeId = Symbol.For("Base");
        public static readonly Symbol DerivedBaseTypePrototypeId = Symbol.For("DerivedBaseType");
        public static readonly Symbol DerivedTypePrototypeId = Symbol.For("DerivedType");
        public static readonly Symbol OtherPrototypeId = Symbol.For("Other");
        public static readonly Symbol UnknownPrototypeId = Symbol.For("Unknown");

        public ILifetimeScope Container => fixture.PhaseScope;

        public void Dispose()
        {
            // wipe the registry between tests to avoid cross-test contamination
            Container.Resolve<PrototypeRegistry<TestSubject>>().Clear();
        }

        [Fact]
        public void PrototypeModule_RegistersRequiredServices()
        {
            Assert.True(Container.TryResolve<PrototypeRegistry<TestSubject>>(out var registry));
            registry.Configure<TestSubject>(Symbol.For("test"));
        }

        [Fact]
        public void PrototypeModule_ThrowsIfSubjectTypeIsNotRegistered()
        {
            Assert.Throws<ComponentNotRegisteredException>(() =>
                Container
                    .Resolve<PrototypeRegistry<UnusedTestSubject>>()
                    .Configure<UnusedTestSubject>(Symbol.For("test"))
            );
        }

        [Fact]
        public void Registry_HandlesWriteOfInitProperties()
        {
            var registry = Container.Resolve<PrototypeRegistry<TestSubject>>();

            // valid: initial configuration allows setting
            var own = registry.Configure<TestSubject>(OwnPrototypeId, isAbstract: true);
            var derived = registry.Configure<DerivedTestSubject>(DerivedTypePrototypeId, baseId: OwnPrototypeId);

            Assert.True(own.IsAbstract);
            Assert.Equal(own, derived.Base);

            // valid: non-initial configuration ignores attempts to change abstractness or base prototype if they match the existing values
            registry.Configure<TestSubject>(OwnPrototypeId, isAbstract: true, baseId: new(Symbol.Empty));
            registry.Configure<DerivedTestSubject>(DerivedTypePrototypeId, baseId: OwnPrototypeId);

            // valid: non-initial configuration with no init-only is allowed
            registry.Configure<TestSubject>(OwnPrototypeId);

            // invalid: attempts to change abstractness or base prototype after initial configuration throw
            Assert.Throws<ArgumentException>(() => registry.Configure<TestSubject>(OwnPrototypeId, isAbstract: false));
            Assert.Throws<ArgumentException>(() =>
                registry.Configure<DerivedTestSubject>(DerivedTypePrototypeId, baseId: OtherPrototypeId)
            );

            // valid: can broaden a prototype to a base subject type
            registry.Configure<TestSubject>(DerivedTypePrototypeId);

            // invalid: cannot narrow a prototype to a derived subject type
            Assert.Throws<ArgumentException>(() => registry.Configure<DerivedTestSubject>(OwnPrototypeId));
        }

        [Fact]
        public void Registry_SetsBasePrototype()
        {
            var registry = Container.Resolve<PrototypeRegistry<TestSubject>>();

            // valid: not specifying base and explicitly empty base sets null base
            var ownPrototype = registry.Configure<TestSubject>(OwnPrototypeId, baseId: new(Symbol.Empty));
            var basePrototype = registry.Configure<TestSubject>(BasePrototypeId, isAbstract: true);
            Assert.Null(ownPrototype.Base);
            Assert.Null(basePrototype.Base);

            // valid: derived prototype configures with known base
            var derivedPrototype = registry.Configure<TestSubject>(DerivedBaseTypePrototypeId, baseId: BasePrototypeId);
            Assert.Equal(derivedPrototype.Base, basePrototype);

            // invalid: derived prototype fails where base prototype is unknown
            Assert.Throws<ArgumentException>(() =>
                registry.Configure<TestSubject>(OtherPrototypeId, baseId: UnknownPrototypeId)
            );
        }

        [Fact]
        public void Prototype_ProvidesPropertyConfigSlot()
        {
            var registry = Container.Resolve<PrototypeRegistry<TestSubject>>();
            var prototype = registry.Configure<TestSubject>(OwnPrototypeId);

            // valid: can get slot by ID or type, all should be the same instance
            var slotById = prototype.GetSlot(PropertyConfigSlot.SlotId);
            var slotByType = prototype.GetSlot<IPropertyConfig<TestSubject>>();
            var slotByBaseType = prototype.GetSlot<IPropertyConfig>();

            Assert.Equal(slotById, slotByType);
            Assert.Equal(slotById, slotByBaseType);

            // valid: unknown slot ID or type returns null
            var unknownSlotById = prototype.GetSlot(UnknownPrototypeId);
            var unknownSlotByType = prototype.GetSlot<IPrototypeConfig>();

            Assert.Null(unknownSlotById);
            Assert.Null(unknownSlotByType);
        }

        [Fact]
        public void Compile_AppliesOwnPropertyConfigs()
        {
            var prototype = GetPrototype<TestSubject>(Container, OwnPrototypeId);
            prototype.GetProperty(x => x.RequiredText).Configure(() => "required");
            prototype.GetProperty(x => x.Text).Configure(() => "configured");

            var subject = prototype.Compile().CreateInstance(Container);

            Assert.Equal("configured", subject.Text);
        }

        [Fact]
        public void Compile_AppliesInheritedSlotWhenDerivedHasNotResolvedIt()
        {
            var basePrototype = GetPrototype<TestSubject>(Container, BasePrototypeId);
            basePrototype.GetProperty(x => x.RequiredText).Configure(() => "required");
            basePrototype.GetProperty(x => x.Text).Configure(() => "inherited");
            var derivedPrototype = GetPrototype<TestSubject>(
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
            var basePrototype = GetPrototype<TestSubject>(Container, BasePrototypeId);
            basePrototype.GetProperty(x => x.RequiredText).Configure(() => "required");
            basePrototype.GetProperty(x => x.Text).Configure(() => "base");

            var derivedPrototype = GetPrototype<DerivedTestSubject>(
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
            var root = GetPrototype<TestSubject>(Container, BasePrototypeId);
            root.GetProperty(x => x.RequiredText).Configure(() => "required");
            root.GetProperty(x => x.Text).Configure(() => "root");
            root.GetProperty(x => x.Text).Configure(value => value + "-configured");

            var middle = GetPrototype<TestSubject>(
                Container,
                DerivedBaseTypePrototypeId,
                basePrototypeId: BasePrototypeId
            );
            middle.GetProperty(x => x.Text).Configure(() => "middle");
            middle.GetProperty(x => x.Text).Configure(value => value + "-configured");

            var derived = GetPrototype<DerivedTestSubject>(
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
            var prototype = GetPrototype<TestSubject>(Container, OwnPrototypeId);
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

        [Fact]
        public void Extensions_ProvideInheritanceChecks()
        {
            // valid: a prototype extends itself and its base prototypes
            var prototype = GetPrototype<TestSubject>(Container, BasePrototypeId);
            var derivedPrototype = GetPrototype<DerivedTestSubject>(
                Container,
                DerivedTypePrototypeId,
                basePrototypeId: BasePrototypeId
            );

            Assert.True(derivedPrototype.Extends(derivedPrototype));
            Assert.True(derivedPrototype.Extends(prototype));

            // valid: a subject of a prototype is of its own prototype and its base prototypes
            prototype.GetProperty(x => x.RequiredText).Configure(() => "required");
            var subject = derivedPrototype.Compile().CreateInstance(Container);

            Assert.True(subject.IsOf(derivedPrototype));
            Assert.True(subject.IsOf(prototype));
        }

        [Fact]
        public void Extensions_ProvideDirectPropertyAccessors()
        {
            var prototype = GetPrototype<TestSubject>(Container, OwnPrototypeId);

            // valid: extensions provide a shortcut
            prototype.GetProperty(x => x.RequiredText).Configure(() => "required");
            prototype.GetProperty(nameof(TestSubject.NullableText)).Configure(() => "nullable");

            Assert.Equal(typeof(int), prototype.GetProperty(nameof(TestSubject.Count)).ValueType);

            // invalid: extensions throw if the property is not configurable or does not exist
            Assert.Throws<ArgumentException>(() => prototype.GetProperty(x => x.NonConfigurable));
            Assert.Throws<ArgumentException>(() => prototype.GetProperty("unknown"));
        }

        private static IPrototype<TSubject> GetPrototype<TSubject>(
            ILifetimeScope scope,
            Symbol id,
            Maybe<bool> isAbstract = default,
            Maybe<Symbol> basePrototypeId = default
        )
            where TSubject : TestSubject
        {
            var registry = scope.Resolve<PrototypeRegistry<TestSubject>>();
            return registry.Configure<TSubject>(id, isAbstract, basePrototypeId);
        }
    }

    public class RegistrationTests
    {
        [Fact]
        public void PrototypeModule_RejectsInvalidPrototypeImplType()
        {
            Assert.Throws<ArgumentException>(() => new PrototypeModule<TestSubject> { PrototypeImpl = typeof(string) });
            Assert.Throws<ArgumentException>(() =>
                new PrototypeModule<TestSubject> { PrototypeImpl = typeof(BadPrototypeImpl<,>) }
            );
            Assert.Throws<ArgumentException>(() =>
                new PrototypeModule<TestSubject> { PrototypeImpl = typeof(DerivedPrototype<>) }
            );

            // should be valid:
            _ = new PrototypeModule<TestSubject> { PrototypeImpl = typeof(Prototype<>) };
            _ = new PrototypeModule<DerivedTestSubject> { PrototypeImpl = typeof(DerivedPrototype<>) };
        }

        private abstract class BadPrototypeImpl<T, U>(Symbol id, PrototypeContext<T> context)
            : Prototype<T>(id, context)
            where T : class, IPrototypeSubject { }
    }

    public class Fixture() : PhaseFixture<SimLynxTestApp, DesignPhase>(options: new() { ShouldRunPhase = false })
    {
        protected override void ConfigureContainer(ContainerBuilder builder)
        {
            base.ConfigureContainer(builder);

            builder.RegisterModule(new PrototypeModule<TestSubject>());

            // builder.RegisterType<TestSubject>().AsSelf();
            // builder.RegisterType<DerivedTestSubject>().AsSelf();
        }
    }

    private class TestSubject : IPrototypeSubject
    {
        public required IPrototype Prototype { get; init; }
        public required string RequiredText { get; init; }
        public string? NullableText { get; set; } = "initial";
        public int Count { get; set; }
        public Options Options { get; set; } = null!;

        [Configurable]
        public string Hidden { get; private set; } = "initial";

        public string Text { get; set; } = "initial";

        [Configurable(false)]
        public int NonConfigurable { get; set; } = 0;
    }

    private class DerivedTestSubject : TestSubject;

    private sealed class UnusedTestSubject : IPrototypeSubject
    {
        public IPrototype Prototype => null!;
    }

    private class DerivedPrototype<T>(Symbol id, PrototypeContext<T> context) : Prototype<T>(id, context)
        where T : DerivedTestSubject { }

    private sealed class Options
    {
        public int Value { get; set; }
    }
}
