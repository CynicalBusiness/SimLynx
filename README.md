# SimLynx

High-performance Hybrid-ECS simulation engine

⚠️ SimLynx is still early in development and probably not suitable for production use yet! ⚠️ _But I ain't gonna tell you how to ~~live your life~~ build your game._

## Intro

SimLynx is a "Hybrid ECS" simulation engine designed for high performance in video games which heavily rely on data processing.

Key features:

- Dedicated to highly-concurrent and/or highly-detailed data-driven games, such as:
    - City/colony/settlement builders
    - Factory and automation games
    - Detailed world sims
    - Any probably many others
- User-generated Content (UGC, i.e. "mods") as a first-class citizen
- Powered by high-performance but also highly-versatile .NET
- Free! (See below)

Of note, SimLynx is **not** a complete game engine (and does not replace Unity/Godot/Unreal/etc.), but rather the core data processing engine "backend". It is intended to be layered with a visual view of some kind, like one of the prior mentioned, but is designed to be agnostic to any one in particular.

### But why?

In general, I was unhappy with the options available for building "colony sim" style games (al. a. Dwarf Fortress, RimWorld, etc.) when looking for a solution for my own project.

My main gripes were:

- Support amongst the available products was extremely limited: most engines were simply not designed for this and either cannot or perform slowly.
    - The few options that _were_ designed for this had horrible developer experience.
    - ...and many of those options were expensive
- Native UGC (i.e. mods) support is apparently a far-fetched idea that does not exist in any solution I was able to find.
- Most games in the genre roll their own solutions to the above, or an entirely custom engine, so I am clearly not the only one experiencing this trouble.

Since I'm going to build my own anyway, I'm opting to give back to the Open Source community I heavily rely on.

#### BuT wHy NoT rUsT!?!?!

.NET, while not the _most_ performant option, has an extremely powerful reflection system while still hitting above-average in performance. This system gives assemblies from UGC much more freedom and power to manipulate not just game systems, but engine systems, too, especially when combined with patching tools like HarmonyLib.

C# is also very familiar to many game developers and compatible with most popular game engines, while also being easy to learn for both new-to-software folks and those coming from other popular languages in game development (particularly Java and JavaScript).

### Can I use it? At what cost?

SimLynx is designed to be available for free under [LGPL-v3.0](./LICENSE), for both players and developers.

Generally, to use and distribute copies of SimLynx itself, you must either:

- Distribute an unmodified copy from a released version available in this repository.
    - You should indicate somewhere, ideally both in the file structure and inside the app, what version of SimLynx you are utilizing. Inside a "credits" or "about" section is sufficient.
- Or, if SimLynx _itself_ is modified (i.e. a fork), provide either the source of directly, or a link to, these modifications under similar terms.
    - If you make improvements that are not specific to you, consider sharing your work with a PR?

In general, no restrictions are imposed on your app/game's licensing by using SimLynx.

See the [license](./LICENSE) for more details.

## Getting Started

### For Game/App Developers

Install SimLynx into your project:

**!TODO**

The easiest and recommended way to use SimLynx is to extend the `SimLynxApp` class.

```csharp
public class MyGameApp : SimLynxApp
{
    // nothing yet
}
```

Then, start it:

**!TODO**

### For UGC Developers

**User-Generated Content** (UGC, sometimes called "mods") is a first-class citizen in SimLynx, and general support is enabled by default. While what kinds of content you can implement are app-specific, some content, like C# code assemblies or [Prototype](#prototypes) configs, are native to SimLynx and should be available.

**!TODO**

## General Concepts

More detailed information and references on the ~~wiki~~ (coming soon).

### Dependency Injection and IoC

SimLynx utilizes [Autofac](https://autofac.org/) as an IoC container, which allows for a highly versatile architecture, especially when UGC is involved.

If you are unfamiliar with Autofac, Dependency Injection in general, or are already using another DI container, SimLynx can run self-contained and manages its own container. **This is the recommended use for most people,** as well as a brief read up on [Autofac's general concepts](https://autofac.readthedocs.io/en/latest/getting-started/index.html).

#### Usage with external DI containers

If your application already uses Autofac, SimLynx can be used as a module in your existing container; this may be preferred instead. SimLynx offers a registration extension for this:

```csharp
containerBuilder.RegisterSimLynx<MyGameApp>();

// or, the module directly, if you prefer
// this is essentially what the extension is doing
containerBuilder.RegisterModule<SimLynxModule<MyGameApp>>();
```

For other containers, you can use the relevant container bridge or simply run SimLynx in self-contained mode wrapped in the method of your choosing.

```csharp

vat cts = new CancellationTokenSource();
var handle = SimLynx.Run<MyGameApp>(cts.token);

// handle can be used to manage your running app
// TODO examples

// then cancel the token to stop
ctx.Cancel();

```

### Phases of Operation

The operation of SimLynx is broken into "phases," each with their own purpose.

From a technical standpoint, each phase is its own dependency scope, as a child scope of the previous. For the uninitiated, this means that each phase can essentially "build" the next phase and define its structure, with both its services and the newly-built ones being available to the next phase.

By default, SimLynx is broken into three phases, in this order:

- **Discovery**: Discover and prepare content to be used.
- **Design**: Load and initialize content to create various configurations, such as [Prototypes](#prototypes).
- **Simulation**: Using designed configurations, assemble and run the main simulation.

The phases can also be treated as "checkpoints" that can be jumped to to "reload" the application at a specific point. For example, in the case of a game, the design phase only needs to run once and can start simulations as needed when loading different saves. SimLynx can also be "reloaded" at the discovery phase, should content change, to potentially reload content without having to restart the entire application.

#### Advanced: Custom Phases

SimLynx allows for custom phases to by defined by your application. While the use-cases of this are few and far between, if it does come up, you _can_ adjust phases to suit your needs, including introducing your own new phases. For more information, see ~~here~~. **!TODO**

### Prototypes

Prototypes are one of the core ingredients in SimLynx and a primary component behind the built-in UGC support.

In essence, prototypes define configuration for various components which can be defined and configured at design-time by loaded content, either programmatically or through configuration files (i.e. YAML/JSON). These configurations remain mutable through the design phase, allowing for content to both introduce new prototypes as well as augment those of other content. Once these prototypes are assembled, they can be used as-is or act as a blueprint for making simulation-time objects.

Most of SimLynx's core components are derived from prototypes, meaning most can be configured by content out-of-the-box. It is recommended, then, that your app's core components also be built using prototypes to benefit from these features.

**!TODO** More detailed explanation once things are more fleshed out.

### Hooks

Hooks are IoC tools for dispatching asynchronous events to a subscribers while also providing customizable control over how those subscriber's resulting tasks are handled. They also solve a similar problem to a message bus: provides a method of communicating _downward_ to descendant scopes rather than relying on interfaces registers in current/ancestor scopes.

#### Why is this useful?

In SimLynx's case, the most obvious use-case is the simulation and its update loop.

Traditionally, one would create an interface for types that are interested in getting simulation updates, `ISimulationService` for example, that the simulation itself would then inject all components with that matching service. In a flatter scope hierarchy, this may work, but for SimLynx, the Simulation actually creates multiple layers of child scopes. Such an interface would get missed for child scopes without extra dedicate logic to resolve them from that new scope, and all child scopes, each time any are created, then properly dispose of them when those scopes do. This creates a highly complex structure of hard dependencies on child scopes, which risks capturing services from them and just adds a lot of code that would need to be duplicated for each instance.

Hooks solve this problem, while also introducing two additional features: customizing of Task-management for async subscribers, and a priority system for organizing subscribers.

#### How do they work?

Interested parties inject the hook and register to it:

```csharp
public class MySimulationService : IDisposable
{

    private readonly IDisposable _simulationUpdateSubscription;

    public MySimulationService(IHook<OnSimulationUpdate> onSimulationUpdate)
    {
        // subscription returns a handle that can be used to unsubscribe
        // the second argument is a priority
        // `Priorities` provides some constants, but any sbyte is accepted
        _simulationUpdateSubscription = onSimulationUpdate.Subscribe(HandleSimulationUpdate, Priorities.Normal);
    }

    public Task HandleSimulationUpdate(OnSimulationUpdate payload, HookContext ctx)
    {
        // ...
    }

    public void Dispose()
    {
        // ...

        // unsubscribe from the hook when you're done
        _simulationUpdateSubscription.Dispose();
    }

}
```

Alternatively, if your service's subscription will last for the same lifetime as itself, you can also attach the hook during registration:

```csharp
builder.RegisterType<MySimulationService>()
    .InstancePerPhase<SimulationPhase>()
    .OnHook(e => e.Instance.HandleSimulationUpdate, Priorities.Normal); // "e" here is an Autofac `IActivatedEventArgs<T>`
// disposal is handled automatically by the relevant scope's lifetime
```

Of course, the same interface-style injection is available for services registered _above_ the hook in the hierarchy, for example, to hook into discovery phase init from your main app:

```csharp
public class MyGameApp : SimLynxApp, IHookHandler<OnPhaseInit<DiscoveryPhase>>
{
    public Task HandleHook(OnPhaseInit<DiscoveryPhase> payload, HookContext context)
    {
        // ...
        // disposal is handled automatically here since this service outlives the hook
    }

    // you can *optionally* override the priority this way, too
    sbyte IHookHandler<OnPhaseInit<DiscoveryPhase>>.Priority => Priorities.High;
}
```

#### Creating Hooks

To create a hook to invoke yourself, first, create a "payload" type that your hook will carry (and be identified by). For consistencies sake, SimLynx prefers to use the convention of starting all hook payload types with `On`, such as `OnSimulationUpdate` or `OnPhaseInit<T>`.

```csharp
public record OnMyHook(bool IsAwesome);
// record's aren't required, but they *are* less code to write and can save time
```

Hooks are defined by their payload type, which may be any valid C# "object type" (classes, records, structs, interfaces, etc.). In the case of generic types, different generic parameters create different types and therefore _different_ hooks!

Then, register one to the container _to the relevant scope_ using the available extension to create the default type of hook:

```csharp
builder.RegisterHook<OnMyHook>()
    .InstancePerPhase<SimulationPhase>();
```

From your dispatching service, inject the hook then invoke it:

```csharp
public class MySimulationService(IHook<OnMyHook> onMyHook)
{

    public async Task DoThingAsync(CancellationToken token)
    {
        // async delegation and priority control is all handled for you
        // reduced to one single task that is the entire invocation
        await onMyHook.Invoke(new OnMyHook(true), token);
    }

}

```

By default, hooks will execute the subscribers serially first ordered by priority (higher numbers win), then subscription order. However, there are other strategies, which can be selected per-hook. For most hooks, this can be done by overriding the provided `IHookDeliveryStrategy` during registration:

```csharp
builder.RegisterHook<OnMyHook>()
    .InstancePerPhase<SimulationPhase>()
    .WithDeliveryStrategy(ConcurrentHookDeliveryStrategy.Default);
    // use the "concurrent" (i.e. Task.WhenAll) strategy instead
```

Note: due to limitations with C# generics and extension methods, these methods will broaden the registration limit type for this builder to `IHook<T>`, so they should be placed near the end of their builder chain.

---

**!TODO** More info soon
