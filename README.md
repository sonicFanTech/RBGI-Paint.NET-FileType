# RBGI FileType Plugin for paint.NET

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![paint.NET](https://img.shields.io/badge/paint.NET-5.1%2B-purple)
![Framework](https://img.shields.io/badge/.NET-net9.0--windows-blueviolet)
![Status](https://img.shields.io/badge/status-public%20beta-orange)

A custom **paint.NET FileType plug-in** that adds support for opening and saving **`.RBGI` Render Background Image** files.

`.RBGI` is a custom image format made for **MozAluz RBGI Tools**, **Source BSP Explorer**, and other sonicFanTech projects that need a simple render-background / skybox-friendly image container. Internally, the current `.RBGI v1` format stores a small binary header, JSON metadata, and embedded PNG image data.

This plug-in lets paint.NET treat `.RBGI` files like a normal image format, so you can open them, edit them, and save them back out from paint.NET.

> [!IMPORTANT]
> This is an unofficial third-party paint.NET plug-in. It is not made by, endorsed by, or bundled with paint.NET.

---

## Features

- Adds `.rbgi` to paint.NET's **Open** dialog.
- Adds `.rbgi` to paint.NET's **Save As** file type list.
- Opens real `.RBGI v1` files.
- Saves images as real `.RBGI v1` files.
- Supports legacy `.RBGI` files that are actually PNG files renamed to `.rbgi`.
- Uses pure C# for the paint.NET plug-in side.
- Does **not** require the original Qt/C++ `SharedRBGIformat.dll` to be installed.
- Stores image pixels using embedded PNG data.
- Writes basic/default metadata when saving.

---

## What is `.RBGI`?

**RBGI** stands for **Render Background Image**.

The format is designed to be a lightweight image container for render backgrounds, flat preview images, and future skybox-style workflows. The current v1 layout is:

```txt
[Header][Metadata JSON][Embedded PNG]
```

The file begins with the ASCII magic value:

```txt
RBGI
```

After that, it stores version/header information, metadata size, PNG size, compact JSON metadata, and then the actual PNG image bytes.

### Current image modes

The RBGI SDK defines these image modes:

| Mode value | Name | Meaning |
|---:|---|---|
| `0` | `FlatImage` | Normal flat image |
| `1` | `RenderBackground` | Render background image |
| `2` | `SkyBoxSingleTexture` | Future/skybox single-texture use |
| `3` | `SkyBoxCubeTexture` | Future/skybox cube-texture use |

At the moment, this paint.NET plug-in saves using `RenderBackground` mode by default.

---

## Requirements

### For normal users

- Windows
- paint.NET 5.1 or newer is recommended
- A copy of `RBGIFileType.dll`

This plug-in has been tested with:

```txt
paint.NET 5.1.12
```

It may work on other paint.NET 5.1+ builds, but older paint.NET versions may require changes because paint.NET's plug-in API and .NET target changed over time.

### For developers/building from source

- Windows
- Visual Studio 2022 or newer, or Visual Studio 2026
- .NET SDK 9 or newer
- paint.NET installed locally
- C#/.NET desktop build tools

The project currently targets:

```txt
net9.0-windows
```

---

## Installation

### Classic paint.NET install

1. Close paint.NET.
2. Download `RBGIFileType.dll` from the latest release.
3. Copy `RBGIFileType.dll` into paint.NET's `FileTypes` folder:

```txt
C:\Program Files\paint.net\FileTypes
```

Depending on how your copy of paint.NET is installed, the folder may also appear as:

```txt
C:\Program Files\Paint.NET\FileTypes
```

Windows paths are usually not case-sensitive, so both names may point to the same place depending on your setup.

4. Start paint.NET again.
5. Go to **File > Open** or **File > Save As**.
6. Look for:

```txt
RBGI Render Background Image (*.rbgi)
```

### Microsoft Store paint.NET install

For the Microsoft Store version, put the DLL here instead:

```txt
Documents\paint.net App Files\FileTypes
```

If the `FileTypes` folder does not exist, create it manually.

### Important install notes

- FileType plug-ins go in the `FileTypes` folder, not the `Effects` folder.
- Restart paint.NET after copying the DLL.
- The plug-in will not appear under the **Effects** menu.
- FileType plug-ins appear in the **Open** and **Save As** dialogs.

Official paint.NET plug-in installation documentation:

https://www.getpaint.net/doc/latest/InstallPlugins.html

---

## How to use

### Opening `.RBGI` files

1. Open paint.NET.
2. Go to **File > Open**.
3. Select an `.rbgi` file.
4. paint.NET should load the embedded image.

The plug-in supports two kinds of `.RBGI` files:

| File kind | Supported? | Notes |
|---|---:|---|
| Real `.RBGI v1` file | Yes | Uses the RBGI header + metadata + PNG layout |
| Legacy renamed PNG `.RBGI` | Yes | If the file is really a PNG with a `.rbgi` extension, the plug-in tries to load it as PNG |
| Future unknown `.RBGI` versions | Not guaranteed | May require a plug-in update |
| Multi-layer paint.NET project data | No | `.RBGI` is an image format, not a `.pdn` project format |

### Saving `.RBGI` files

1. Open or create an image in paint.NET.
2. Go to **File > Save As**.
3. Set **Save as type** to:

```txt
RBGI Render Background Image (*.rbgi)
```

4. Save the file.

The saved file will be a real `.RBGI v1` file with PNG image data embedded inside it.

> [!NOTE]
> paint.NET will flatten the final visible image when saving to normal image file formats. If you need to preserve layers, keep a `.pdn` copy too.

---

## Current limitations

This first release focuses on getting reliable open/save support working. Some extra format features are planned but not finished yet.

Current limitations:

- No save-options dialog yet.
- No metadata editor yet.
- Saves with default/basic metadata.
- Saves as `RenderBackground` mode by default.
- Does not currently let the user pick `FlatImage`, `SkyBoxSingleTexture`, or `SkyBoxCubeTexture` from the Save dialog.
- Does not preserve original custom metadata when saving over a file.
- Does not store paint.NET layers.
- Does not include an installer yet.

Recommended workflow for now:

1. Keep a `.pdn` master copy if you need layers.
2. Export/save a `.rbgi` copy when you need the RBGI file.
3. Back up important `.rbgi` files before overwriting them.

---

## Building from source

Clone the repository:

```bat
git clone https://github.com/sonicFanTech/RBGI-Paint.NET-FileType.git
cd RBGI-Paint.NET-FileType
```

Build in Release mode:

```bat
dotnet build -c Release
```

The output DLL should be created at something like:

```txt
bin\Release\net9.0-windows\RBGIFileType.dll
```

Copy that DLL to:

```txt
C:\Program Files\paint.net\FileTypes
```

Then restart paint.NET.

### Building when paint.NET is installed somewhere else

The project file uses this default path:

```txt
C:\Program Files\paint.net
```

If your paint.NET install is in a different folder, build with:

```bat
dotnet build -c Release -p:PaintNetInstallDir="D:\Path\To\paint.net"
```

Example:

```bat
dotnet build -c Release -p:PaintNetInstallDir="C:\Program Files\Paint.NET"
```

### Auto-install after build

The project includes an optional MSBuild target that can copy the DLL directly into the classic paint.NET `FileTypes` folder.

Run:

```bat
dotnet build -c Release -p:AutoInstallToPaintNet=true
```

If paint.NET is installed somewhere else:

```bat
dotnet build -c Release -p:AutoInstallToPaintNet=true -p:PaintNetInstallDir="D:\Path\To\paint.net"
```

You may need to run the terminal as Administrator if copying into `C:\Program Files`.

---

## Project structure

Typical source layout:

```txt
RBGI-Paint.NET-FileType/
├─ README.md
├─ RBGIFileTypePlugin.csproj
├─ src/
│  ├─ RbgiFileTypeFactory.cs
│  ├─ RbgiFileType.cs
│  └─ RbgiCodec.cs
└─ docs/
   └─ RBGI_PLUGIN_NOTES.md
```

### Main source files

| File | Purpose |
|---|---|
| `RbgiFileTypeFactory.cs` | Registers the file type with paint.NET |
| `RbgiFileType.cs` | Handles paint.NET load/save integration |
| `RbgiCodec.cs` | Reads and writes the `.RBGI` binary format |
| `RBGIFileTypePlugin.csproj` | Build/project file |

---

## Technical details

### RBGI v1 file layout

The plug-in is designed around the current `.RBGI v1` layout:

| Part | Description |
|---|---|
| Header | Binary header containing magic, version, size, dimensions, mode, flags, PNG size, and metadata size |
| Metadata JSON | Compact JSON metadata block |
| Embedded PNG | PNG image bytes |

### Header summary

| Field | Description |
|---|---|
| `magic` | ASCII `RBGI` |
| `version` | Current format version, usually `1` |
| `headerSize` | Header size, currently `38` bytes |
| `width` | Image width |
| `height` | Image height |
| `mode` | RBGI image mode value |
| `flags` | Reserved, currently `0` |
| `pngSize` | Size of the embedded PNG data |
| `metadataSize` | Size of the JSON metadata block |

### Legacy PNG fallback

Older test `.RBGI` files may simply be PNG files renamed to `.rbgi`.

If the plug-in opens a file that does not start with `RBGI`, it attempts to load the file as PNG image data. This allows older/legacy `.rbgi` files to still open instead of failing immediately.

---

## Troubleshooting

### The file type does not show up in paint.NET

Try these steps:

1. Make sure paint.NET is fully closed.
2. Make sure `RBGIFileType.dll` is inside the `FileTypes` folder, not `Effects`.
3. Restart paint.NET.
4. Check **File > Open** or **File > Save As**.
5. Make sure you are using a supported paint.NET version.

Correct folder for classic paint.NET:

```txt
C:\Program Files\paint.net\FileTypes
```

Correct folder for Microsoft Store paint.NET:

```txt
Documents\paint.net App Files\FileTypes
```

### The plug-in does not appear in the Effects menu

That is normal.

This is a **FileType** plug-in, not an **Effect** plug-in. It appears in Open/Save file dialogs.

### I get permission denied when copying the DLL

`C:\Program Files` is protected by Windows.

Try one of these:

- Copy the DLL as Administrator.
- Open File Explorer as Administrator.
- Use the Microsoft Store plug-in folder if you are using the Store version.
- Build with `AutoInstallToPaintNet=true` from an Administrator terminal.

### The project does not build because paint.NET DLLs are missing

Make sure paint.NET is installed and that this path exists:

```txt
C:\Program Files\paint.net
```

If paint.NET is installed somewhere else, pass the path to MSBuild:

```bat
dotnet build -c Release -p:PaintNetInstallDir="D:\Path\To\paint.net"
```

### I see MSB3277 warnings while building

Some paint.NET plug-in projects can produce assembly version conflict warnings when building against paint.NET's installed DLLs. The project suppresses `MSB3277` by default because it is usually warning noise in this context.

If the build succeeds and the DLL loads in paint.NET, those warnings are usually not a problem.

### Opening a file fails

Possible reasons:

- The file is not a real `.RBGI` file.
- The file is damaged.
- The embedded PNG data is corrupt.
- The file uses a future `.RBGI` version that this plug-in does not understand yet.
- The metadata or embedded PNG section is too large or invalid.

---

## Release packaging

A normal release ZIP should include:

```txt
RBGIFileType.dll
README.md
```

Optional extras:

```txt
LICENSE
CHANGELOG.md
sample-files/
```

Suggested release naming:

```txt
RBGIFileTypePlugin-v1.0.0-beta.zip
RBGIFileTypePlugin-v1.0.0.zip
RBGIFileTypePlugin-v1.1.0.zip
```

Suggested GitHub release description:

```md
## RBGI FileType Plugin for paint.NET v1.0.0-beta

First public beta release.

### Features
- Adds .RBGI open/save support to paint.NET.
- Supports real RBGI v1 files.
- Supports legacy renamed-PNG .RBGI files.
- Saves as RBGI v1 with embedded PNG data.

### Install
Copy `RBGIFileType.dll` to your paint.NET `FileTypes` folder, then restart paint.NET.
```

---

## Roadmap

Planned or possible future upgrades:

- Save-options dialog.
- Metadata editor.
- Preserve metadata when opening and saving.
- Let users choose RBGI image mode when saving.
- Add a small installer.
- Add sample `.rbgi` files.
- Add automated build/release workflow.
- Add better validation/testing for corrupted files.
- Add a changelog.
- Add better documentation for the `.RBGI` format.

---

## For paint.NET forum posting

Suggested short description:

```txt
RBGI FileType Plugin adds support for opening and saving .RBGI Render Background Image files in paint.NET.
```

Suggested forum category:

```txt
Plugins - Publishing ONLY! > FileType Plugins
```

Recommended forum post info:

- Plug-in name: `RBGI FileType Plugin`
- File extension: `.rbgi`
- Type: FileType plug-in
- paint.NET version tested: `5.1.12`
- Install folder: `FileTypes`
- Download: GitHub Releases
- Source code: This repository

paint.NET forum:

https://forums.getpaint.net/

---

## Credits

Created by **sonicFanTech**.

Made for the `.RBGI` Render Background Image format used by sonicFanTech projects such as MozAluz RBGI Tools and Source BSP Explorer.

paint.NET is created and maintained by its own developers. This project is only a third-party plug-in.

---

## License

No license has been selected yet.

Until a license is added to this repository, the source code and binaries should be treated as **all rights reserved** by default.

Suggested future license options:

- MIT License, if you want people to freely use, modify, and redistribute the plug-in.
- GPL-style license, if you want modified versions to stay open-source.
- Custom license, if you want more control.

---

## Version history

### v1.0.0-beta / v1.0.1 test build

- First working public/test build.
- Adds `.RBGI` open support.
- Adds `.RBGI` save support.
- Supports RBGI v1 header + metadata + embedded PNG.
- Supports legacy renamed-PNG `.RBGI` files.
- Confirmed working in paint.NET 5.1.12.
