# Prototype Implementation Plan

Status: active WIP. This document describes the current implementation, known gaps, and the next work needed. It is
not a finalized implementation contract.

Last reviewed against the current `SimLynx/Design/Prototyping` and
`SimLynx/Simulation/ComponentModel/Prototyping` implementations.

## Intent

Prototypes provide runtime-defined "is a" relationships, a code-first configuration surface, and a place to move
reflection and configuration work out of the simulation hot path.

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
- `PrototypeConfigSlotCatalog` snapshots slot definitions in Autofac registration order and resolves slot instances
  through Autofac by their unique slot ID or declared config-type aliases.
- `PrototypeConfigModule` registers an open-generic slot under its unique ID and adds its definition to the catalog.
- Slot configuration order is deterministic: definitions are stable-sorted by descending priority, so equal-priority
  slots retain Autofac registration order. The catalog records both this sort index and the original registration
  index.
- Slot IDs are unique. Duplicate IDs fail catalog construction rather than silently overriding an earlier
  registration.
- Config-type aliases are intentionally non-unique. Alias lookup ignores slot priority and tries the latest
  registration first, falling back to earlier registrations that support the prototype. An exact closed-generic alias
  is more specific and is tried before an alias for its open-generic type definition.
- Autofac determines whether an open-generic slot can be closed for a subject type. An incompatible generic constraint
  is treated as an unavailable registration by `TryResolveService`; activation and registration failures are not
  interpreted as unsupported slots and propagate to the caller. `IPrototypeConfigSlot.IsSupported` handles only
  contextual applicability after successful construction.
- Compilation uses the nearest resolved slot for each slot ID across the prototype hierarchy, so inherited-only slots
  participate without requiring a getter call on the derived prototype.

The first complete slot is `Properties`.

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

Registry compilation validates the prototype graph, omits abstract prototypes, and compiles each concrete prototype
into the catalog. Direct compilation of an abstract prototype fails.

### Blueprints and catalog

- `Prototype<TSubject>.Compile()` creates a `BlueprintBuilder<TSubject>`, lets each effective local or inherited slot
  configure it, and builds an immutable `Blueprint<TSubject>`.
- `BlueprintBuilder<TSubject>` collects Autofac injection parameters, shared typed options, and pre-/post-create
  handlers.
- `Blueprint<TSubject>` snapshots those inputs. Each `CreateInstance(...)` clones the instance options, gathers dynamic
  parameters, resolves the subject, and runs post-create handlers.
- `BlueprintCatalog<TBaseSubject>` is implemented as a read-only dictionary from prototype ID to blueprint.
- `PrototypeRegistry.Compile()` creates the catalog from all registry entries.

The catalog is structurally read-only. Blueprints retain their source prototype for identity and hierarchy inspection;
the relevant structural state (`Id`, `Base`, and `SubjectType`) is stable after initialization. `IsAbstract` may be
extended by specialized prototype types, but is checked before compilation and is not consulted when creating
instances from an existing blueprint.

Each config slot must snapshot any mutable state used by its compiled parameters, options, and handlers. Compiled
behavior must not read mutable slot or config objects after compilation. Property configs satisfy this contract by
copying their effective state before applying it to the blueprint builder, and `Blueprint<TSubject>` snapshots the
builder's parameters, options, and handlers.

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
3. [x] Define the compilation contract:
    - [x] specify which state is snapshotted and which references may remain live;
    - [x] make slot discovery deterministic and independent of which code-first getters happened to run;
    - [x] define duplicate slot/config registration behavior;
    - [x] prevent mutation from changing already compiled blueprints.
4. [ ] Add focused registry and lifecycle tests:
    - [ ] creation, compatible reuse, duplicate IDs, base lookup, and `OnPrototypeAdded`;
    - [ ] Design-to-Simulation catalog creation;
    - [ ] catalog lookup and subject creation through Autofac;
    - [ ] abstract and failure cases.
