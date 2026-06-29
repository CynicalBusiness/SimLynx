# Prototype Implementation Plan

Status: provisional. This captures current design direction and open questions, not a finalized implementation contract.

Last reviewed against WIP implementation: current `SimLynx/Core/Prototyping` code.

ComponentModel-specific prototype work is intentionally out of scope for this plan for now. The existing
ComponentModel implementation is known to be broken and is expected to be replaced.

## Intent

Prototypes should provide SimLynx with runtime-defined "is a" relationships, UGC-friendly configuration, and a way to move expensive reflection or validation work out of the simulation hot path.

The core design should separate mutable design-time data from immutable simulation-time data:

- Design-time prototypes are patchable records built by code, loaders, and UGC.
- Compiled prototypes are frozen descriptors consumed by simulation systems, component factories, and instance builders.

This avoids turning prototypes into half-built runtime objects and gives the engine a clear phase boundary between content authoring and simulation execution.

## Current WIP Shape

The current WIP implementation has moved away from the earlier `PrototypeInfo`/resolver shape and toward a lighter
runtime prototype object with named config slots:

- `IPrototype` exposes `Id`, optional `Base`, `IsAbstract`, `SubjectType`, `TryGetConfig(...)`, and a tentative
  `Compile()` entry point.
- `Prototype<TSubject>` stores configs in a two-level map: slot `Symbol` to config name to `IPrototypeConfig`.
- `IPrototypeConfigProvider` instances are resolved per slot and lazily create configs when requested.
- The first concrete slot is `Properties`, backed by `PrototypePropertyConfigProvider`.
- Property configurability is reflection-driven and cached per subject type in `PrototypePropertyTypeInfo<TSubject>`.
- Public settable properties are configurable by default; `[Configurable(false)]` opts out and `[Configurable]` can opt
  non-public setters in.
- Property configs currently store an optional assigned value plus ordered modifiers.
- `Extends` and `IPrototypeSubject.Is(...)` are implemented as runtime prototype-chain checks.
- `IPrototypeBlueprint` exists as the tentative compiled/factory surface, but compilation is still stubbed.

This is a reasonable near-term code-first authoring surface, but it is not yet the full design/compiled split described
below.

## Design-Time Prototype Model

A mutable design prototype should contain, or be able to derive:

- `Symbol Id`
- optional base prototype reference or base prototype id
- target CLR type metadata
- ordered configuration operations, currently represented by slotted `IPrototypeConfig` instances
- source metadata for diagnostics, such as package, loader, file, and line
- future entity/component child declarations, after ComponentModel is replaced

Loaders should not need to instantiate final runtime components or entities. Their job is to contribute configuration operations to the prototype registry.

The WIP implementation stores base prototypes as direct references rather than ids. That is sufficient for code-first
construction, but a registry/id-resolution layer is still likely needed before file loaders and UGC patches are added.

## Compiled Prototype Model

Tentative. Blueprint compilation is currently represented only by `IPrototypeBlueprint` and `IPrototype.Compile()`;
`Prototype<TSubject>.Compile()` is still unimplemented.

A compiled prototype should likely contain:

- resolved base prototype pointer
- flattened configuration plan
- cached type metadata
- compiled constructors or factories where appropriate
- compiled property setters
- ancestry chain or ancestry set for fast `Extends` checks
- immutable child component tree for entity prototypes

Compilation should happen during the Design phase before Simulation begins. The exact boundary between prototype,
blueprint, registry, and catalog remains open.

## Configuration Operations

Prototype configuration should be represented as replayable patches rather than immediate mutation of final runtime
objects.

Possible primitive operations:

- `SetProperty(name, value)`
- `ModifyProperty(name, Func<object?, object?>)`
- `AddComponent(name, componentPrototypeId)`
- `RemoveComponent(name or componentPrototypeId)`
- `ReplaceComponent(name, componentPrototypeId)`
- future collection operations, such as append, remove, merge, or clear

The current property config model covers `SetProperty`, `ClearProperty`, and ordered `ModifyProperty` operations. The
compiler should eventually replay property configs from base to derived prototype. Derived prototypes may override or
modify base configuration.

Open semantic detail: a new `SetProperty` currently clears prior modifiers in the same config. The inheritance/compiler
rules still need to define how inherited modifiers interact with derived sets, clears, and modifiers.

## Inheritance

Prototype inheritance should be independent from CLR inheritance.

A prototype should be able to answer:

- Does this prototype extend another prototype?
- Was this runtime object built from a prototype that extends this other prototype?

The WIP implementation supports prototype-chain traversal through direct `Base` references. Compilation should still
validate:

- missing base prototypes
- inheritance cycles
- incompatible target types
- invalid overrides
- abstract prototypes used as concrete build targets

## Reflection and Metadata

Reflection should be gathered once during design/compilation.

Current WIP status:

- `PropertyInfo.IsPrototypeConfigurable` centralizes the attribute/default configurability rule.
- `PrototypePropertyTypeInfo<TSubject>` caches configurable `PropertyInfo` instances by subject type.
- Property setter delegates are not compiled yet.
- Required-property validation is not wired into prototype compilation yet.

The compiler should cache:

- configurable properties
- property setter delegates
- constructor or factory delegates
- type compatibility information
- user-friendly diagnostics for failed configuration

Configurability rules can stay attribute-driven, convention-driven, or both, but the result should be compiled into
immutable metadata before simulation.

## Registry and Catalog

Tentative. No registry, compiler service, or compiled catalog exists yet. Likely services:

- `PrototypeRegistry`: mutable design-phase store of prototype definitions and patches.
- `PrototypeCompiler`: validates and freezes the registry.
- `CompiledPrototypeCatalog`: immutable simulation-phase lookup surface.

The registry should favor good diagnostics and loader friendliness. The catalog should favor fast lookup and immutable
access.

The current slotted provider approach may remain useful inside the registry as the code-first configuration API, but
the registry is still needed for id lookup, source diagnostics, patch ordering, and loader-friendly conflict reporting.

## Initial Implementation Steps

1. Stabilize the current core interfaces: `IPrototype`, `IPrototypeConfig`, `IPrototypeConfigProvider`, and
   `IPrototypeBlueprint`.
2. Fix and test the property config surface: provider resolution, value assignment, clearing, modifiers, and expression
   helpers.
3. Define inheritance semantics for property configs, especially derived set/clear/modify ordering.
4. Decide whether `Base` remains a direct reference for code-first APIs or whether ids are introduced at the design
   prototype layer.
5. Implement inheritance validation and cycle detection.
6. Implement property configuration replay into a blueprint or equivalent compiled plan.
7. Add compiled setter/factory metadata.
8. Add a registry and id lookup once code-first construction needs loader/UGC layering.
9. Add compiled prototype catalog once blueprints are real.
10. Revisit component/entity integration only after the ComponentModel replacement direction is clear.
11. Add file-based loaders later, after the internal model stabilizes.

## Open Questions

- Should prototype patch ordering be global per prototype, grouped by loader, or explicitly prioritized?
- Should derived prototypes be allowed to remove inherited components, or only disable/replace them?
- How should conflicting UGC patches be reported and resolved?
- Should compiled prototypes expose low-level metadata publicly, or keep it behind factory/build services?
- How much should prototype compilation depend on Autofac versus engine-native factories?
- Should config providers be singletons per slot, enumerable per slot, or selected through a keyed index?
- Should setting a property to `null` count as an explicit configured value for all property types?
- Should `ClearProperty` remove only explicit values, or both explicit values and modifiers?
- Should public setters be configurable by default, or should configurability become opt-in before UGC/file loaders exist?
