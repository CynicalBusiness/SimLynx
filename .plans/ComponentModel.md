# Component Model Implementation Plan

Status: provisional. This captures current design direction and open questions, not a finalized implementation contract.

## Intent

The component model should preserve SimLynx's component boundary without giving up the cache-friendly linear iteration that makes traditional ECS designs performant.

Systems should not reach directly into raw instance tables. Instead, systems should operate through component-provided queries, slices, commands, or accessors. Components remain the authority over their state and invariants, while hot loops can still run over dense table-backed memory.

## Component Boundary

The refined rule should be:

> Systems should not access instance tables directly. They should operate through component-provided batch views with declared access semantics.

This means components are not per-instance wrappers. A component is a per-prototype, per-stage runtime object that owns:

- schema/state declarations
- invariants
- public API for its instances
- query/view surfaces
- component-specific logic

Instance data remains table-backed and suitable for dense iteration.

## Cache-Friendly System Operation

Components should expose linear views over their instance state.

Possible shapes:

```csharp
public readonly struct TransformSlice
{
    public ReadOnlySpan<InstanceId> Instances { get; init; }
    public Span<Matrix4x4> Matrices { get; init; }
}
```

Systems can then process dense memory:

```csharp
foreach (var slice in transform.GetWritableSlices())
{
    var matrices = slice.Matrices;

    for (var i = 0; i < matrices.Length; i++)
    {
        matrices[i] = UpdateMatrix(matrices[i]);
    }
}
```

The system is still operating through `Transform`, but the hot path remains linear and cache-friendly.

## Queries and Access Semantics

The runtime should understand read/write intent for scheduling and parallelism.

Possible query model:

```csharp
var query = Query
    .From(transform.Positions.ReadWrite)
    .With(velocity.Velocities.ReadOnly);
```

or component-local helpers:

```csharp
transform.ForEachWritable((Span<Matrix4x4> matrices) =>
{
    // hot loop
});
```

The scheduler can use declared access to determine which systems can run concurrently.

Important access modes:

- read-only state access
- read-write state access
- append/create instance commands
- remove/destroy instance commands
- structural component/entity changes, likely deferred

## Runtime Graph

Entity prototypes should compile into immutable component trees. During stage assembly, each concrete component prototype should produce one component instance per stage.

Runtime concepts:

- `EntityPrototype`: design/compiled component tree root.
- `ComponentPrototype`: static configuration for a component.
- `Component`: per-stage/per-prototype runtime object.
- `ComponentContext`: immutable runtime location in the entity/component tree.
- `InstanceId`: identifies a simulation instance, including its prototype identity.
- `InstanceTable`: dense state storage.

Component resolution should operate over the context tree rather than an arbitrary mutable collection.

## Current Prototype Integration

- A `Components` prototype config slot has been introduced, but its compilation and config behavior are still stubs.
- `ComponentPrototype<TComponent>` specializes `Prototype<TComponent>` and derives abstractness from both explicit
  prototype state and the abstractness of its component CLR type.
- `IComponentConfig` and `ComponentConfigSlot` define the initial shape for attaching named child components. Config
  names combine the component type and optional given name.
- `IComponentTypeConfig` and `IComponentTypeConfig<TComponent>` define the initial DI-discoverable configuration
  surface for component CLR types. They are not yet collected or applied to component prototypes.
- `ComponentModelModule` registers `PrototypeModule<Component>` with `ComponentPrototype<>` as its implementation.

`ComponentConfig`, its nested component prototype, and `ComponentConfigSlot.Configure(...)` remain unimplemented. The
current component slot is therefore an API scaffold, not usable blueprint behavior.

## Component Member Declarations

State and dependency members should be declared once, directly on the component. Their handle types are sufficient for
convention-based discovery:

```csharp
public class Mover(IComponentContextInfo context) : Component(context)
{
    protected readonly ComponentRef<Transform> transform = default;
    protected readonly InstanceState<Vector3> velocity = default;
    protected readonly InstanceState<Vector3> acceleration = default;
}
```

- `InstanceState<T>` fields and properties declare state slots with value type `T`.
- `ComponentRef<T>` fields and properties declare dependencies accepting components assignable to `T`.
- Member names provide default state tags and dependency names.
- Attributes may provide small overrides such as ignoring a member or assigning a stable external name.
- Rich configuration belongs in component type configs or prototype configuration, not attribute arguments.

The type hierarchy must be reflected from the oldest component base to the concrete type using declared-only member
enumeration so private base members are retained. Hidden fields are distinct declarations; overridden properties should
remain the same logical declaration. Conflicting final names or incompatible reconfiguration should fail compilation.

This metadata must be discovered and validated without constructing a component. A component constructor consumes
ordinary DI services and context information, but does not declare or alter its state layout or dependencies.

## Component Type Configuration

`IComponentTypeConfig<TComponent>` implementations may be registered in DI and collected as an enumerable or through a
small catalog. They provide defaults and richer configuration for the declarations discovered on a component CLR type.
The usual component should need no type config at all; a config only customizes convention-derived declarations.

Type configuration may be replayed into each `ComponentPrototype<TComponent>` rather than eagerly compiled into a
separate shared component-type model. The repeated work occurs during design and is not currently worth optimizing.
Reflection results may still be cached independently.

The prototype can expose or implement the builder API used by `IComponentTypeConfig<TComponent>`. A narrow
`IComponentTypeBuilder<TComponent>` facade remains useful even if it writes directly into a prototype-owned draft:

- it prevents type configs from modifying unrelated prototype behavior;
- it identifies changes as component-type defaults rather than prototype-local UGC configuration;
- it permits a temporary compilation draft or shared compiled model to be introduced later without changing the
  configuration API;
- it keeps compilation replayable and avoids mutating the source prototype during repeated `Compile()` calls.

Passing `ComponentPrototype<TComponent>` directly is viable if it preserves those same boundaries internally. In
particular, component-type defaults must not be stored as ordinary own-prototype overrides, or a derived prototype's
replayed defaults could incorrectly override explicit configuration inherited from its base prototype.

The preferred precedence is:

1. convention-derived member declarations and defaults;
2. component CLR base-type configs, from the oldest base to the concrete type;
3. component CLR concrete-type configs;
4. effective prototype configuration, from the root prototype to the current prototype.

`IEnumerable<IComponentTypeConfig<TComponent>>` naturally collects exact registrations, but component inheritance also
requires configs registered for component base types. The generic interface is contravariant, so DI contravariant
resolution may provide these registrations; otherwise, a catalog should index configs by `ComponentType` and return the
applicable hierarchy in deterministic base-to-derived and priority/registration order.

Type configs should be stateless or otherwise safe to replay. They should be applied during an explicit prototype
initialization or compilation phase, not from the prototype constructor body, where property-injected prototype state
may not yet be available.

## Blueprint Compilation

Component blueprint construction should be separated from runtime component construction. Because dependencies may
target parents, children, or siblings, they cannot be resolved while compiling a child component in isolation.

The entity-root compilation pipeline should be:

1. Discover effective state and dependency declarations for each component CLR type.
2. Apply applicable component type configs in CLR base-to-derived order.
3. Assemble the complete effective component prototype tree, including prototype inheritance.
4. Apply effective prototype-specific state and dependency configuration.
5. Resolve dependency selectors against the complete tree.
6. Validate missing, ambiguous, duplicate, or incompatible bindings.
7. Allocate and freeze state layouts and member-binding plans.
8. Produce an immutable component blueprint tree.

Dependency selectors should preferably be declarative data--contract type, name, search scope, cardinality, and matching
mode--rather than opaque callbacks. This keeps them statically resolvable during blueprint construction and makes the
same model accessible to data-driven UGC and tooling.

## Runtime Construction and Initialization

The runtime should populate the members that declared states and dependencies. Reflective assignment to non-public,
instance `readonly` fields is acceptable for the initial implementation because it happens once during graph assembly,
before the graph is published, and avoids declaring each member a second time. Generated binders can later replace
reflection for trimming, AOT, diagnostics, or startup performance without changing the authoring model.

The runtime lifecycle should be:

1. Allocate the component contexts/nodes for the complete blueprint tree.
2. Construct component instances top-down using ordinary dependency injection. State and component handles remain
   unbound defaults during constructors and must not be accessed or published.
3. After every component object exists, assign all `InstanceState<T>` and `ComponentRef<T>` fields/properties from the
   compiled binding plans. Binding may walk bottom-up, but its order should not affect correctness.
4. Invoke component initialization, normally bottom-up so contained children initialize before their parent.
5. Publish the completed component graph for simulation use.

All component references are guaranteed to be bound before initialization starts. Bottom-up tree order does not imply
that arbitrary sibling, parent, or cyclic dependencies have already initialized. If that guarantee becomes necessary,
add either a graph-wide ready phase after all initialization or explicit initialization-ordering dependency edges that
must be acyclic.

Any binding or initialization failure discards the incomplete graph. Once published, handles and layouts are immutable.

## Current Leaning

- Treat component fields/properties as the single declaration source for state and dependencies.
- Discover and validate declarations during prototype blueprint construction, without constructing components.
- Replay DI-discovered component type configs per prototype into a properly layered compilation draft.
- Compile and resolve components as a complete entity-root graph rather than independent child blueprints.
- Construct top-down, bind after the complete graph exists, initialize bottom-up, and publish only after success.
- Use reflective binding initially and retain source generation as an optional optimization/tooling path.
- Expose dense batch views from components rather than raw instance-table ownership.
- Require systems to declare read/write access through those views.

## Initial Implementation Steps

1. Complete component prototype config behavior:
    - construct and expose nested component prototypes;
    - compile attachments into an entity-root component graph draft;
    - define names, inheritance, removal, replacement, and duplicate-component rules;
    - integrate component contexts, instance state, and dependency binding.
2. Implement inheritance-aware reflection metadata for `InstanceState<T>` and `ComponentRef<T>` members.
3. Register, order, and replay `IComponentTypeConfig` implementations into a narrow component type builder/draft.
4. Define configuration-source precedence between conventions, CLR type configs, and prototype configs.
5. Implement entity-root, multi-pass component blueprint compilation and aggregated validation diagnostics.
6. Define the immutable runtime component context tree and member-binding plans.
7. Implement top-down construction, post-construction reflective binding, bottom-up initialization, and atomic graph
   publication/failure behavior.
8. Implement typed `InstanceState<T>` handles backed by frozen layout slots and instance tables with dense column
   storage.
9. Add component-provided slice/query APIs and access declaration metadata for read/write scheduling.
10. Implement system execution over component-provided batch views.
11. Explore generated binders and compile-time diagnostics after the reflection-based model works.

## Open Questions

- Should component layouts be allowed to vary per prototype for the same component CLR type?
- Should type configs be applied through Autofac contravariant enumeration or an explicit configuration catalog?
- Should `ComponentPrototype<T>` implement the type-builder interface directly or vend a short-lived facade over its
  compilation draft?
- How should component identity work when multiple components of the same type have no given name?
- Which dependency relationships, if any, impose initialization order beyond containment order?
- How should systems express cross-component queries without bypassing component ownership?
- Should structural changes be deferred command buffers by default?
- Should query views expose `Span<T>` directly, or a safer wrapper that still optimizes well?
- How much should the scheduler know about component state handles versus higher-level component views?
