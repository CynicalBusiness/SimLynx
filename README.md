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

#### Learning by Example

Taking for example a colony-builder-style game, let's make a basic building.

**!TODO**
