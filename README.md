<div class="header" align="center">

# NovaSector

</div>

NovaSector is a furry-focused high roleplay fork of [Delta-V](https://github.com/DeltaV-Station/Delta-v), which is itself a fork of [Space Station 14](https://github.com/space-wizards/space-station-14).

Space Station 14 is a remake of SS13 that runs on [Robust Toolbox](https://github.com/space-wizards/RobustToolbox), a homegrown engine written in C#.

### Any work done in a non-base namespace may contain incorrect attributions due to rewrites and recommitting.

## Links

#### Space Station 14
<div class="header" align="center">

[Website](https://spacestation14.io/) | [Discord](https://discord.ss14.io/) | [Forum](https://forum.spacestation14.io/) | [Steam](https://store.steampowered.com/app/1255460/Space_Station_14/) | [Standalone Download](https://spacestation14.io/about/nightlies/)

</div>

## Documentation/Wiki

The [docs site](https://docs.spacestation14.io/) has documentation on SS14s content, engine, game design and more.
Additionally, see these resources for license and attribution information:
- [Robust Generic Attribution](https://docs.spacestation14.com/en/specifications/robust-generic-attribution.html)
- [Robust Station Image](https://docs.spacestation14.com/en/specifications/robust-station-image.html)

## Contributing

We are happy to accept contributions from anybody!

All NovaSector-specific content goes in the `_Nova` namespace. Make sure to read [CONTRIBUTING.md](/CONTRIBUTING.md) if you are new!

## Building

1. Clone this repo:
```shell
git clone --recurse-submodules https://github.com/YourOrg/NovaSector.git
```
2. Go to the project folder and run `RUN_THIS.py` to initialize the submodules and load the engine:
```shell
cd NovaSector
python RUN_THIS.py
```
3. Compile the solution:

Build the server using `dotnet build`.

[More detailed instructions on building the project.](https://docs.spacestation14.com/en/general-development/setup.html)

## License

Read [LEGAL.md](/LEGAL.md) for legal information regarding the code licensing.

Most assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and the copyright in the metadata file.

Code taken from [Project Starlight](https://github.com/ss14Starlight/space-station-14) was taken in accordance with the [Starlight License](./LICENSE-Starlight.txt).

> [!NOTE]
> Some assets are licensed under the non-commercial [CC-BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) or similar non-commercial licenses and will need to be removed if you wish to use this project commercially.
