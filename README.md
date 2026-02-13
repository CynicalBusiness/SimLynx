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

.NET, while not the _most_ performant option, has an extremely powerful reflection system. This system gives assemblies from UGC much more freedom and power to manipulate not just game systems, but engine systems, too, especially when combined with patching tools like HarmonyLib.

C# is also very familiar to many game developers, while also being easy to learn for both new-to-software folks and those coming from other popular languages in game development (particularly Java and JavaScript).

### Can I use it? At what cost?

SimLynx is designed to be available for free under [LGPL-v3.0](./LICENSE), for both players and developers.

In general, you may use and distribute SimLynx provided that:

- The copy you distribute is unmodified from a version available in this repository
- Or, your modified copy is also available publicly under similar terms.
    - If you make improvements that are not specific to you, consider sharing your work with a PR?

See the [license](./LICENSE) for more details.

## General Concepts

TODO
