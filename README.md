# PulseForge Studio 1.0

PulseForge Studio is an original native Windows music-production MVP. It is not affiliated with FL Studio or Image-Line.

## Included

- 8-track, 16-step sequencer
- interactive piano roll
- synthesized kick, snare, hat, bass and lead sounds
- tempo from 60–200 BPM
- looping playback and transport timer
- per-track volume and mute controls
- save/open `.pulse` project files
- render an eight-bar stereo WAV mix
- dark native Windows interface
- self-contained x64 publishing and Inno Setup installer

## Build on Windows 10/11

1. Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
2. Optional: install [Inno Setup 6](https://jrsoftware.org/isinfo.php) for the installer.
3. Right-click `build-installer.ps1` and choose **Run with PowerShell**, or run:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build-installer.ps1
```

The portable program appears in `publish`. With Inno Setup installed, the installer appears at `dist\PulseForgeStudio-Setup-x64.exe`.

## Controls

- Click sequencer pads to enable or disable beats.
- Click the piano-roll grid to add or remove notes.
- Press Space or use Play to start/stop.
- Change BPM and click elsewhere to apply it.
- Use **File → Export WAV** to render the current project.

## Scope

This is a functional first version, not a feature-complete commercial DAW. Planned next steps include microphone recording, audio-file clips, VST3 hosting, automation curves, undo/redo, MIDI-device input and ASIO output.
