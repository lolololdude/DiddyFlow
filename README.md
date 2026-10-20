# Diddy Flow

Offline voice dictation for Windows, with maximum flow.

Diddy Flow listens on a global hotkey, transcribes your speech locally with a
Whisper model on your own GPU, and pastes the text straight into whatever app
you were typing in. No account, no cloud, no subscription.

> Parody project. Not affiliated with or endorsed by any artist or label.
> "Diddy" is used here as a joke.

## Features

- **Fully local** - Whisper runs on your machine via `whisper.cpp` (CUDA, Vulkan, or CPU).
- **Global hotkey** - default `Ctrl+Alt+D`, rebind it in Setup.
- **Paste or clipboard** - type into the focused window, or copy the transcript.
- **Floating mic orb** - always-on-top mic that pulses while you talk.
- **System tray** - start/stop dictation, toggle the orb, switch output mode.
- **Diddy Vision** - skippable ads made from your own video files.
- **Custom branding** - swap the logo, the hero image, and the Diddy wall.
- **Auto model install** - picks the best model for your hardware and downloads it on first run.
- **Draggable mic** - grab the orb anywhere to move it, click to dictate, right-click to hide it.

## Requirements

- Windows 10 or 11, x64
- .NET 10 SDK to build (the release build is self-contained, so end users need nothing)

## Build from source

```powershell
git clone https://github.com/<you>/DiddyFlow.git
cd DiddyFlow
dotnet build -c Release
```

## Editions

| Edition | Ads tab | Ad settings | Advanced page | Ad folders |
| --- | --- | --- | --- | --- |
| Consumer | No | No | No | `Ads` next to the exe + bundled |
| Normal | Yes | Yes | No | All folders |
| Developer | Yes | Yes | Yes | All folders |

A normal `dotnet build` produces the Developer edition. Release builds pick
their edition explicitly.

## Make a release

```powershell
.\build-release.ps1 -Edition Consumer -Version 1.1.0
```

That publishes a self-contained `win-x64` build into `publish\<Edition>\`, trims
the runtimes we do not need, copies only the ads you selected, and zips it into
`release\DiddyFlow-<version>-<edition>-win-x64.zip`.

To bundle ads, pass files or a folder:

```powershell
.\build-release.ps1 -Edition Normal -Version 1.1.0 -AdsFolder "C:\clips"
```

Nothing is bundled unless you ask for it.

### Build tool (Python)

```powershell
python build-tool.py
```

A small desktop app for the same job. Pick an edition, tick or untick the
features you want in that build, add the ads to bundle, then click **Build
edition**. It writes `build-tool.config.json` and runs the script for you.

## Ad videos

Diddy Vision plays whatever you put in these folders - no import step needed.
Consumer builds only look at the first two rows.

| Folder | Consumer | Normal / Developer |
| --- | --- | --- |
| `Videos\Diddy Ads` | No | Yes (click **Open ads folder**) |
| `%LOCALAPPDATA%\DiddyFlow\ads` | No | Yes |
| `Ads` next to `DiddyFlow.exe` | Yes (the secret folder) | Yes |
| `Assets\Ads` (bundled at build time) | Yes | Yes |

MP4, MKV, AVI, MOV, WMV, and WEBM all work.

**Remove** in the Ads tab hides a video without deleting it. Use **Restore
hidden** to bring it back. Ads close on their own when the video ends, and never
earlier than the skip delay: a 5 second video with a 10 second skip delay stays
on screen for 10 seconds, then closes.

## Where things live

| What | Where |
| --- | --- |
| Models | `%LOCALAPPDATA%\DiddyFlow\models` |
| Settings | `%LOCALAPPDATA%\DiddyFlow\settings.json` |
| Logs | `%LOCALAPPDATA%\DiddyFlow\logs\diddyflow.log` |
| Temp recordings | `%LOCALAPPDATA%\DiddyFlow\temp` |

## Credits

- [Whisper.net](https://github.com/sandrohanea/whisper.net) and
  [whisper.cpp](https://github.com/ggerganov/whisper.cpp) for the speech engine.
- [LibVLCSharp](https://github.com/videolan/LibVLCSharp) for ad playback.
- [NAudio](https://github.com/naudio/NAudio) for microphone capture.
