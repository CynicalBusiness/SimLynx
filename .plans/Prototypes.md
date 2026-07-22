# Prototype Implementation Plan

Status: active WIP. This document describes the current implementation, known gaps, and the next work needed. It is
not a finalized implementation contract.

Last reviewed against the current `SimLynx/Design/Prototyping` and
`SimLynx/Simulation/ComponentModel/Prototyping` implementations.

## Intent

Prototypes provide runtime-defined "is a" relationships, a code-first configuration surface that can later support
loaders and UGC, and a place to move reflection and configuration work out of the simulation hot path.

The implementation has an explicit phase split:

- Mutable prototypes and their config slots live in the Design lifetime.
- Prototypes compile into immutable blueprints and a read-only blueprint catalog for the Simulation lifetime.
- Blueprints use Autofac to construct subjects and run compiled pre-create and post-create behavior.

## Implemented Architecture

### Prototypes and inheritance

- `IPrototype` exposes `Id`, optional `Base`, `IsAbstract`, `SubjectType`, config-slot lookup, and `Compile()`.
- `Prototype<TSubject>` owns lazily resolved config slots keyed by `Symbol` and caches typed slot lookups.
- Base prototypes are direct object references. Assignment checks that the base subject type is assignable from the
  derived subject type.
- `Extends`, `GetAncestors`, and `IPrototypeSubject.IsOf(...)` traverse the direct base chain at runtime.
- `IPrototypeSubject.Prototype` is excluded from property configuration with `[Configurable(false)]`.

### Config slots

- Configuration is organized into `IPrototypeConfigSlot` instances instead of the previous
  provider/resolver/compiler services.
- `PrototypeConfigSlot<TSubject, TConfig>` stores named configs, creates them lazily, and configures a
  `BlueprintBuilder<TSubject>` during compilation.
- `PrototypeConfigSlotResolver<TSubject>` resolves slots through Autofac by slot ID or supported config type.
- `PrototypeConfigModule` registers an open-generic slot under its ID and config-type aliases.
- Compilation uses the nearest resolved slot for each slot ID across the prototype hierarchy, so inherited-only slots
  participate without requiring a getter call on the derived prototype.

The first complete slot is `Properties`. A `Components` slot has also been introduced, but its compilation and config
behavior are still stubs.

### Property configuration

- `PrototypePropertyTypeInfo<TSubject>` caches configurable properties per subject type.
- Public setters are configurable by default. `[Configurable(false)]` opts out, while `[Configurable]` can opt a
  non-public setter in.
- `PropertyConfig<TSubject, TValue>` stores an optional value factory and ordered configuration delegates.
- Setting a value clears earlier state in that config. `Clear()` removes both the value and all configurations.
- Expression-based `GetProperty`/`TryGetProperty` helpers provide typed code-first access.
- During blueprint compilation, non-required values become post-create assignment delegates. Required values are
  supplied to Autofac through a property-specific parameter. Modifier delegates run after construction.
- `PropertyConfigSlot` flattens inherited configs from oldest ancestor to the current prototype by copying them into
  stabilized effective configs stored on the blueprint builder.
- Property configs are merged across compatible base and derived CLR subject types, including inherited-only slots.

### Registry and construction

- `PrototypeRegistry<TBaseSubject>` is a mutable Design-lifetime dictionary keyed by prototype ID.
- `Configure<TSubject>(...)` creates a prototype through `PrototypeResolver`, optionally applying `IsAbstract` and a
  previously registered base prototype, or returns an existing compatible prototype for further configuration.
- Base IDs must already exist, so forward references are not supported.
- Existing prototypes cannot have their base or abstractness changed through `Configure`.
- `OnPrototypeAdded` is raised after a new prototype is stored.
- `PrototypeModule<TBaseSubject>` registers the registry, prototype implementation, automatic concrete subject
  registrations, and a Simulation-lifetime catalog compiled from the Design registry.
- `PrototypeSubjectRegistrationSource<TBaseSubject>` allows concrete subject types to be constructed through Autofac.

Registry compilation currently compiles every registered prototype, including abstract prototypes, and performs no
graph-wide validation before constructing the catalog.

### Blueprints and catalog

- `Prototype<TSubject>.Compile()` creates a `BlueprintBuilder<TSubject>`, lets each effective local or inherited slot
  configure it, and builds an immutable `Blueprint<TSubject>`.
- `BlueprintBuilder<TSubject>` collects Autofac injection parameters, shared typed options, and pre-/post-create
  handlers.
- `Blueprint<TSubject>` snapshots those inputs. Each `CreateInstance(...)` clones the instance options, gathers dynamic
  parameters, resolves the subject, and runs post-create handlers.
- `BlueprintCatalog<TBaseSubject>` is implemented as a read-only dictionary from prototype ID to blueprint.
- `PrototypeRegistry.Compile()` creates the catalog from all registry entries.

The catalog is structurally read-only, but prototypes and any delegate-captured config state remain mutable unless the
compilation path explicitly copies them. Property configs do make stabilized copies; this guarantee has not yet been
defined for every future slot.

### Component prototypes

- Component-model integration now exists and is no longer wholly out of scope.
- `ComponentPrototype<TComponent>` specializes `Prototype<TComponent>` and derives abstractness from both explicit
  prototype state and `IComponentType.IsAbstract`.
- `IComponentConfig` and `ComponentConfigSlot` define the initial shape for attaching named child components. Config
  names combine the component type and optional given name.
- `ComponentModelModule` registers `PrototypeModule<Component>` with `ComponentPrototype<>` as its implementation.

`ComponentConfig`, its nested component prototype, and `ComponentConfigSlot.Configure(...)` remain unimplemented. The
current component slot is therefore an API scaffold, not usable blueprint behavior.

## Design Gaps

The current code-first model does not yet provide the loader/UGC concerns from the broader design:

- source metadata such as package, loader, file, and line;
- ordered patches from multiple contributors;
- deferred base-ID resolution and forward references;
- conflict diagnostics and invalid override reporting;
- inheritance cycle detection and graph validation;
- a defined policy for compiling or instantiating abstract prototypes;
- a fully immutable compiled representation independent of mutable prototype objects;
- component add, remove, replace, and inherited merge semantics;
- file-based loaders.

## Next Implementation Steps

1. [x] Correct and test property-slot compilation:
    - include the current prototype's configs after its ancestors;
    - ensure inherited-only slots participate even when the derived prototype did not resolve that slot locally;
    - verify base-to-derived set/configure/clear ordering;
    - verify required, nullable, value-type, default-constructible, and non-public-setter behavior.
2. [x] Add prototype-graph validation before registry compilation:
    - reject inheritance cycles;
    - validate subject-type compatibility across the full chain;
    - define and enforce abstract prototype compilation/instantiation rules;
    - report errors with prototype IDs and the relevant chain.
3. [ ] Define the compilation contract:
    - specify which state is snapshotted and which references may remain live;
    - make slot discovery deterministic and independent of which code-first getters happened to run;
    - define duplicate slot/config registration behavior;
    - prevent mutation from changing already compiled blueprints.
4. [ ] Add focused registry and lifecycle tests:
    - creation, compatible reuse, duplicate IDs, base lookup, and `OnPrototypeAdded`;
    - Design-to-Simulation catalog creation;
    - catalog lookup and subject creation through Autofac;
    - abstract and failure cases.
5. [ ] Complete component config behavior:
    - construct and expose nested component prototypes;
    - compile attachment behavior into parent blueprints;
    - define names, inheritance, removal, replacement, and duplicate-component rules;
    - integrate component build context and instance state.
6. [ ] Add loader-facing design data only after the code-first compile semantics stabilize:
    - source-aware patches and deterministic ordering;
    - deferred ID resolution;
    - diagnostics suitable for content authors;
    - file formats and loaders.

## Open Questions

- Should abstract prototypes be omitted from the blueprint catalog, represented by non-instantiable blueprints, or
  cause direct `Compile()` to fail?
- Should slot participation be registered globally for every prototype type, or discovered from configs across the
  complete inheritance chain?
- Should `Base` remain a direct reference in the code-first API while loaders use deferred IDs, or should all design
  prototypes store IDs until validation?
- Should setting a property use a value factory, a captured value, or support both explicitly?
- Should `Clear()` mean "remove this patch" or emit a clearing operation that overrides inherited configuration?
- Should property configurability remain public-setter-by-default before untrusted UGC loaders exist?
- How much should blueprint creation depend on Autofac versus an engine-native factory abstraction?
- Should compiled blueprints retain their mutable source prototype for identity/inspection, or expose a frozen prototype
  descriptor?
- How should component identity work when multiple components of the same type have no given name?
- How should patches from multiple packages be ordered, diagnosed, and overridden?
