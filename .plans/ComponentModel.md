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
  prototype state and `IComponentType.IsAbstract`.
- `IComponentConfig` and `ComponentConfigSlot` define the initial shape for attaching named child components. Config
  names combine the component type and optional given name.
- `ComponentModelModule` registers `PrototypeModule<Component>` with `ComponentPrototype<>` as its implementation.

`ComponentConfig`, its nested component prototype, and `ComponentConfigSlot.Configure(...)` remain unimplemented. The
current component slot is therefore an API scaffold, not usable blueprint behavior.

## Instance State

`IComponentBuilder.RegisterState<T>()` is an ergonomic and useful concept. The builder should record state declarations and return typed handles.

Example:

```csharp
public class Transform(IComponentBuilder builder) : Component(builder)
{
    private readonly IInstanceState<Matrix4x4> _matrix =
        builder.RegisterState<Matrix4x4>();
}
```

The handle can later be used by the component to expose safe APIs or dense query views.

## State Declaration Timing

Constructor-based state declaration has strong ergonomics:

- declaration and handle capture happen together
- primary constructors keep syntax compact
- component authors do not need a separate schema file or builder class

It does allow conditional state declarations:

```csharp
if (Prototype.HasPhysics)
{
    _velocity = builder.RegisterState<Vector3>();
}
```

This is not necessarily bad. Since components are instantiated per prototype, it may be acceptable for different prototypes of the same component type to have different layouts.

The likely rule:

- state registration is legal only during component construction/build
- after construction, the builder freezes the component layout
- no late state registration is allowed once the component enters simulation

## Possible Build Lifecycle

1. Create a `ComponentBuilder` for one compiled component prototype.
2. Construct the component.
3. Component constructor registers state handles.
4. Builder freezes the component layout.
5. Stage runtime allocates instance tables from the frozen layout.
6. Component exposes APIs and batch views over that layout.
7. Systems operate through those component APIs/views.

## Optional State Definition Classes

A separate state definition class could provide stronger separation and better tooling:

```csharp
public sealed class TransformState(ComponentStateBuilder builder)
{
    public IInstanceState<Matrix4x4> Matrix { get; } =
        builder.Register<Matrix4x4>();
}

public sealed class Transform(
    IComponentBuilder builder,
    TransformState state
) : Component(builder)
{
    private readonly TransformState _state = state;
}
```

Inheritance would need careful support. State definitions should mirror component inheritance:

```csharp
public class SpatialState(ComponentStateBuilder builder)
{
    public IInstanceState<Vector3> Position { get; } =
        builder.Register<Vector3>();
}

public class TransformState(ComponentStateBuilder builder) : SpatialState(builder)
{
    public IInstanceState<Quaternion> Rotation { get; } =
        builder.Register<Quaternion>();
}
```

Then:

```csharp
public class Spatial(IComponentBuilder builder, SpatialState state) : Component(builder);

public class Transform(
    IComponentBuilder builder,
    TransformState state
) : Spatial(builder, state);
```

This is more explicit but adds ceremony. It may be best treated as an optional advanced path rather than the default authoring style.

## Current Leaning

Prefer constructor-based state declaration as the primary path.

Add strict lifecycle enforcement:

- registering state is only allowed during component build
- layout freezes immediately after construction
- components expose dense batch views, not raw table ownership
- systems declare read/write access through those views

Consider state definition classes later for source generation, tooling, validation, or advanced inheritance scenarios.

## Initial Implementation Steps

1. Complete component prototype config behavior:
    - construct and expose nested component prototypes;
    - compile attachment behavior into parent blueprints;
    - define names, inheritance, removal, replacement, and duplicate-component rules;
    - integrate component build context and instance state.
2. Define component context tree and immutable runtime component graph.
3. Implement component builder with state registration and freeze semantics.
4. Implement typed `IInstanceState<T>` handles backed by layout slots.
5. Implement instance tables with dense column storage.
6. Add component-provided slice/query APIs.
7. Add access declaration metadata for read/write scheduling.
8. Implement system execution over component-provided batch views.
9. Add validation for illegal late state registration.
10. Explore optional state definition classes after the base model works.

## Open Questions

- Should component layouts be allowed to vary per prototype for the same component CLR type?
- How should component identity work when multiple components of the same type have no given name?
- How should systems express cross-component queries without bypassing component ownership?
- Should structural changes be deferred command buffers by default?
- Should query views expose `Span<T>` directly, or a safer wrapper that still optimizes well?
- How much should the scheduler know about component state handles versus higher-level component views?
