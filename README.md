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
- **Diddy Vision** - fake skippable ads that are just your own video files.
- **Custom branding** - swap the logo, the hero image, and the Diddy wall.
- **Auto model install** - picks the best model for your hardware and downloads it on first run.

## Requirements

- Windows 10 or 11, x64
- .NET 10 SDK to build (the release build is self-contained, so end users need nothing)

## Build from source

```powershell
git clone https://github.com/<you>/DiddyFlow.git
cd DiddyFlow
dotnet build -c Release
```

## Make a release

```powershell
.\build-release.ps1 -Version 1.0.0
```

That publishes a self-contained `win-x64` build into `publish\`, trims the
runtimes we do not need, and zips it into `release\DiddyFlow-<version>-win-x64.zip`.
Attach the zip to a GitHub release.

## Ad videos

Diddy Vision plays whatever you put in these folders - no import step needed:

| Folder | What it is |
| --- | --- |
| `Videos\Diddy Ads` | The main drop folder (click **Open ads folder** in the app) |
| `%LOCALAPPDATA%\DiddyFlow\ads` | App data folder |
| `Ads` next to `DiddyFlow.exe` | Portable folder, great for USB sticks |
| `Assets\Ads` | Bundled sample jokes |

MP4, MKV, AVI, MOV, WMV, and WEBM all work.

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
