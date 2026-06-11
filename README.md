# RBGI FileType Plug-in for Paint.NET

![Platform](https://img.shields.io/badge/platform-Windows-blue)
![Paint.NET](https://img.shields.io/badge/Paint.NET-5.2%20Preview-purple)
![Framework](https://img.shields.io/badge/.NET-net10.0--windows-blueviolet)
![Status](https://img.shields.io/badge/status-preview-orange)

A third-party **Paint.NET FileType plug-in** that adds support for opening and saving **`.RBGI` Render Background Image** files.

`.RBGI` is a custom image-container format made for **MozAluz RBGI Tools**, **Source BSP Explorer**, and other sonicFanTech projects that need render-background or skybox-friendly image assets. The current `.RBGI v1` format stores a small binary header, compact JSON metadata, and embedded PNG image data.

This version has been migrated to the **new Paint.NET 5.2 FileType plug-in system** and now includes a configurable Save Options window.

> [!IMPORTANT]
> This is an unofficial third-party Paint.NET plug-in. It is not made by, endorsed by, or bundled with Paint.NET.

> [!WARNING]
> This release is intended for **Paint.NET 5.2 Preview** testing. Keep using the earlier Paint.NET 5.1.12-compatible release if you have not installed Paint.NET 5.2 Preview yet.
>
## Full installation and usage demo

A complete demonstration video is now available on YouTube:

<p align="center">
  <a href="https://www.youtube.com/watch?v=gd56dtzv6XE">
    <img src="https://img.youtube.com/vi/gd56dtzv6XE/maxresdefault.jpg" width="720" alt=".RBGI FileType Plug-in for Paint.NET 5.2 Preview — Full Demo">
  </a>
</p>

**[Watch the `.RBGI` FileType Plug-in for Paint.NET 5.2 Preview full demo on YouTube](https://www.youtube.com/watch?v=gd56dtzv6XE)**

The video shows:

* Where to download the plug-in.
* The installer correctly detecting that Paint.NET `5.1.12` is not compatible with the new Paint.NET 5.2-preview build.
* Installing Paint.NET 5.2 Preview.
* Installing the `.RBGI` FileType plug-in.
* Confirming that `.RBGI` appears in Paint.NET's Open and Save As file-type lists.
* Creating a basic `256 × 256` black-and-orange gradient image.
* Saving the image as a real `.RBGI` file.
* Using the Save Options window and entering custom metadata.
* Opening Source BSP Explorer and using the exported `.RBGI` file as its rendered background.

---

---

## Features

- Adds `.rbgi` to Paint.NET's **Open** dialog.
- Adds `.rbgi` to Paint.NET's **Save As** file-type list.
- Opens real `.RBGI v1` files.
- Saves images as real `.RBGI v1` files.
- Supports older legacy `.RBGI` files that are actually PNG files renamed to `.rbgi`.
- Uses Paint.NET 5.2's built-in PNG FileType pipeline for the embedded PNG payload.
- Does **not** require the original Qt/C++ `SharedRBGIformat.dll`.
- Does **not** use the older `System.Drawing.Bitmap` / `Graphics` PNG workflow.
- Includes stronger validation for damaged, truncated, oversized, or inconsistent `.RBGI` files.
- Includes a Save Options window with selectable RBGI image modes and editable metadata.
- Includes an installer for classic Paint.NET installations.

---

## What is `.RBGI`?

**RBGI** stands for **Render Background Image**.

The current `.RBGI v1` file layout is:

```txt
[Header][Metadata JSON][Embedded PNG]
```

The file begins with the ASCII magic value:

```txt
RBGI
```

After that, it stores version and header information, dimensions, an image-mode value, flags, metadata size, PNG size, compact JSON metadata, and the embedded PNG image bytes.

### Current image modes

| Mode value | Name | Meaning |
|---:|---|---|
| `0` | `FlatImage` | Normal flat image |
| `1` | `RenderBackground` | Render-background image |
| `2` | `SkyBoxSingleTexture` | Skybox using one texture |
| `3` | `SkyBoxCubeTexture` | Skybox cube-texture workflow |

The Save Options window lets you choose the mode while exporting.

### Export metadata

The Save Options window currently allows you to set:

- Image name
- Author
- Description
- Content version

The exported metadata also records the RBGI format, selected image mode, and plug-in encoder information.

---

## Paint.NET version compatibility

| Plug-in release | Paint.NET version | Status |
|---|---|---|
| Earlier public beta | Paint.NET `5.1.12` | Available for users who remain on Paint.NET 5.1 |
| `v1.1.0 Preview` | Paint.NET `5.2 Preview` | Current preview-testing release |
| Future finalized release | Stable Paint.NET `5.2` | Planned after Paint.NET 5.2 is officially released |

> [!CAUTION]
> Do not install the old Paint.NET 5.1 DLL and the new Paint.NET 5.2-preview DLL at the same time. They both register the `.rbgi` extension and may conflict with each other.

### Tested preview build

This preview version has been tested successfully with:

```txt
Paint.NET 5.2 Alpha
5.200.9650.36619
```

---

## Requirements

### For normal users

- Windows
- Paint.NET 5.2 Preview or a compatible Paint.NET 5.2 build
- The included installer, or a copy of `RBGIFileType.Preview52.dll`

### For developers building from source

- Windows
- Visual Studio 2022 or newer
- .NET 10 SDK
- Paint.NET 5.2 Preview installed locally
- C#/.NET desktop build tools

The Paint.NET 5.2-preview project targets:

```txt
net10.0-windows
```

---

## Installation

### Recommended: installer

1. Close Paint.NET.
2. Download the Paint.NET 5.2-preview RBGI installer from the latest pre-release.
3. Run the installer.
4. The installer will look for Paint.NET, verify that a compatible Paint.NET 5.2 build is installed, and copy the plug-in into Paint.NET's `FileTypes` folder.
5. Start Paint.NET again.
6. Go to **File > Open** or **File > Save As** and look for:

```txt
RBGI Render Background Image (*.rbgi)
```

### Manual installation: classic Paint.NET install

1. Close Paint.NET.
2. Remove the older Paint.NET 5.1-compatible `RBGIFileType.dll` if it is already installed.
3. Download `RBGIFileType.Preview52.dll` from the latest Paint.NET 5.2-preview release.
4. Copy it into:

```txt
C:\Program Files\Paint.NET\FileTypes
```

Depending on your installation, the same location may appear as:

```txt
C:\Program Files\paint.net\FileTypes
```

5. Restart Paint.NET.

### Microsoft Store installation

For the Microsoft Store version of Paint.NET, FileType plug-ins are normally placed in:

```txt
Documents\paint.net App Files\FileTypes
```

The current Paint.NET 5.2-preview installer is intended for classic installations. Store-version preview installation has not yet been tested, so manual installation may be required.

### Important installation notes

- FileType plug-ins go in the `FileTypes` folder, not the `Effects` folder.
- Restart Paint.NET after installing or replacing the DLL.
- This plug-in will not appear under the **Effects** menu.
- FileType plug-ins appear in Paint.NET's **Open** and **Save As** dialogs.

Official Paint.NET plug-in installation documentation:

https://www.getpaint.net/doc/latest/InstallPlugins.html

---

## How to use

### Opening `.RBGI` files

1. Open Paint.NET.
2. Go to **File > Open**.
3. Select an `.rbgi` file.
4. Paint.NET should load the embedded image.

Supported file kinds:

| File kind | Supported? | Notes |
|---|---:|---|
| Real `.RBGI v1` file | Yes | Uses the RBGI header, metadata JSON, and embedded PNG layout |
| Legacy renamed-PNG `.RBGI` | Yes | If the file is really a PNG with an `.rbgi` extension, the plug-in attempts to load it as PNG data |
| Future unknown `.RBGI` versions | Not guaranteed | May require a plug-in update |
| Multi-layer Paint.NET project data | No | `.RBGI` is an exported image format, not a `.pdn` project format |

### Saving `.RBGI` files

1. Open or create an image in Paint.NET.
2. Go to **File > Save As**.
3. Set **Save as type** to:

```txt
RBGI Render Background Image (*.rbgi)
```

4. Choose the RBGI image mode.
5. Fill in any optional metadata fields.
6. Save the file.

The plug-in will export a real `.RBGI v1` file containing the selected mode, metadata JSON, and embedded PNG image data.

> [!NOTE]
> Paint.NET flattens the final visible image when exporting to normal image file formats. Keep a `.pdn` master copy if you need to preserve layers.

---

## Current limitations

The Paint.NET 5.2-preview release adds the planned Save Options window and metadata export fields, but some items are still unfinished:

- Original custom metadata is not automatically restored into the Save Options window when an existing `.rbgi` file is opened and saved again.
- Paint.NET layers are not stored inside `.RBGI` files.
- The current installer is focused on classic Paint.NET installations.
- Paint.NET 5.2 itself is still in preview, so API changes may require another plug-in update before the final stable release.

Recommended workflow:

1. Keep a `.pdn` master copy when layers matter.
2. Export an `.rbgi` copy for use in RBGI-compatible projects.
3. Back up important `.rbgi` files before overwriting them.
4. Use the Paint.NET 5.1-compatible DLL only with Paint.NET 5.1, and the Paint.NET 5.2-preview DLL only with Paint.NET 5.2 Preview.

---

## Building the Paint.NET 5.2-preview version from source

Clone the repository:

```bat
git clone https://github.com/sonicFanTech/RBGI-Paint.NET-FileType.git
cd RBGI-Paint.NET-FileType
```

Enter the Paint.NET 5.2-preview source folder:

```bat
cd preview-5.2-migration
```

Build in Release mode:

```bat
dotnet build -c Release
```

The expected output DLL is:

```txt
bin\Release\net10.0-windows\RBGIFileType.Preview52.dll
```

Copy it into:

```txt
C:\Program Files\Paint.NET\FileTypes
```

Then restart Paint.NET.

### Building when Paint.NET is installed somewhere else

The preview project uses this default path:

```txt
C:\Program Files\paint.net
```

If Paint.NET 5.2 Preview is installed somewhere else, build with:

```bat
dotnet build -c Release -p:PaintNetInstallDir="D:\Path\To\Paint.NET"
```

Example:

```bat
dotnet build -c Release -p:PaintNetInstallDir="C:\Program Files\Paint.NET"
```

### Auto-install after building

The preview project includes an optional MSBuild target that copies the newly built DLL directly into Paint.NET's `FileTypes` folder.

Run:

```bat
dotnet build -c Release -p:AutoInstallToPaintNet=true
```

If Paint.NET is installed elsewhere:

```bat
dotnet build -c Release -p:AutoInstallToPaintNet=true -p:PaintNetInstallDir="D:\Path\To\Paint.NET"
```

You may need to run the terminal as Administrator when copying into `C:\Program Files`.

---

## Project structure

The repository keeps the earlier Paint.NET 5.1-compatible source while the Paint.NET 5.2-preview migration is being tested.

```txt
RBGI-Paint.NET-FileType/
├─ README.md
├─ LICENSE
├─ preview-5.2-migration/
│  ├─ RBGIFileTypePlugin.Preview52.csproj
│  ├─ README_PREVIEW_5_2.md
│  ├─ TEST_CHECKLIST.md
│  ├─ Samples/
│  └─ src/
│     ├─ RbgiFileTypeFactory.cs
│     ├─ RbgiFileType.cs
│     ├─ RbgiCodec.cs
│     └─ RbgiPngBridge.cs
├─ RBGIFileTypePlugin.csproj
└─ src/
   ├─ RbgiFileTypeFactory.cs
   ├─ RbgiFileType.cs
   └─ RbgiCodec.cs
```

### Main Paint.NET 5.2-preview source files

| File | Purpose |
|---|---|
| `RbgiFileTypeFactory.cs` | Registers the new Paint.NET 5.2 FileType |
| `RbgiFileType.cs` | Provides the configurable Save Options window and handles Paint.NET load/save integration |
| `RbgiCodec.cs` | Reads, validates, and writes the `.RBGI v1` binary container |
| `RbgiPngBridge.cs` | Sends embedded PNG loading and saving through Paint.NET's built-in PNG FileType system |
| `RBGIFileTypePlugin.Preview52.csproj` | Configures the .NET 10 build and Paint.NET 5.2-preview assembly references |

---

## Technical details

### RBGI v1 file layout

| Part | Description |
|---|---|
| Header | Binary header containing magic, version, header size, dimensions, mode, flags, PNG size, and metadata size |
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
| `mode` | RBGI image-mode value |
| `flags` | Reserved, currently `0` |
| `pngSize` | Size of the embedded PNG data |
| `metadataSize` | Size of the JSON metadata block |

### Legacy PNG fallback

Some older test `.RBGI` files may simply be PNG files renamed to `.rbgi`.

When the plug-in opens a file that does not begin with the `RBGI` magic header, it attempts to read it as PNG image data. This allows older legacy `.rbgi` files to continue opening.

### Validation

The Paint.NET 5.2-preview plug-in performs checks for:

- Missing or invalid RBGI headers
- Unsupported RBGI versions
- Truncated or oversized files
- Oversized metadata blocks
- Missing or invalid embedded PNG signatures
- Missing PNG `IHDR` chunks
- Invalid PNG dimensions
- Header dimensions that do not match the embedded PNG dimensions
- Invalid metadata JSON

---

## Troubleshooting

### The installer says Paint.NET 5.2 Preview is not installed

The current Paint.NET 5.2 Alpha identifies itself with a version number similar to:

```txt
5.200.9650.36619
```

Use an up-to-date copy of the RBGI installer that recognizes the Paint.NET 5.2-preview version format.

### The file type does not appear in Paint.NET

1. Fully close Paint.NET.
2. Make sure `RBGIFileType.Preview52.dll` is inside the `FileTypes` folder, not `Effects`.
3. Remove the older Paint.NET 5.1-compatible `RBGIFileType.dll` if it is still installed.
4. Restart Paint.NET.
5. Check **File > Open** or **File > Save As**.
6. Confirm that you are using Paint.NET 5.2 Preview.

### The plug-in does not appear in the Effects menu

That is normal.

This is a **FileType** plug-in, not an **Effect** plug-in. It appears in Open and Save As dialogs.

### The project does not build because Paint.NET DLLs are missing

Make sure Paint.NET 5.2 Preview is installed and that the project points to the correct folder.

Example:

```bat
dotnet build -c Release -p:PaintNetInstallDir="C:\Program Files\Paint.NET"
```

### Visual Studio reports ambiguous Paint.NET FileType API references

Paint.NET 5.2 Preview keeps the older FileType API available for compatibility. The new source explicitly aliases the new `PaintDotNet.FileTypes` namespace to avoid collisions with the old API.

Make sure you are building the latest source from:

```txt
preview-5.2-migration
```

### Opening a file fails

Possible reasons:

- The file is not a real `.RBGI` file or legacy renamed PNG.
- The file is damaged.
- The embedded PNG data is corrupt.
- The file uses a future `.RBGI` version that this plug-in does not understand.
- The metadata or embedded PNG section is too large or inconsistent.

---

## Forum thread and acknowledgements

The original public-beta discussion is available on the Paint.NET forum:

https://forums.paint.net/topic/134445-beta-rbgi-filetype-plugin-rbgi-opensave-render-background-image-files/

Special thanks to:

- **Tactilis** — for pointing out that Paint.NET 5.2 introduces a new FileType plug-in system and recommending that this newly created plug-in migrate to it.
- **Rick Brewster** — Paint.NET author and developer — for explaining the new Paint.NET 5.2 FileType system, linking example conversions, recommending that the plug-in move away from `System.Drawing`, and suggesting Paint.NET's built-in PNG FileType pipeline through `IFileTypesService.CreatePngFileType()`.

Additional reference material:

- The Paint.NET 5.2 conversion branches for FileType plug-ins maintained by **null54 / 0xC0000054**, which were linked in the forum discussion and used as migration examples.

---

## Credits

Created by **sonicFanTech**.

Made for the `.RBGI` Render Background Image format used by sonicFanTech projects such as MozAluz RBGI Tools and Source BSP Explorer.

Paint.NET is created and maintained by its own developers. This repository is an unofficial third-party plug-in project.

---

## License

This project uses the custom **RBGI Paint.NET FileType Plug-in Source-Available License**.

See [`LICENSE`](LICENSE) for the complete terms.

This project is **not** distributed under the MIT License.

---

## Version history

### v1.1.0 Preview — Paint.NET 5.2 Preview

- Migrated to Paint.NET 5.2's new FileType plug-in system.
- Updated the project to target `.NET 10` / `net10.0-windows`.
- Added a Save Options window.
- Added selectable RBGI image modes.
- Added editable image-name, author, description, and content-version metadata.
- Moved embedded PNG handling through Paint.NET's built-in PNG FileType pipeline.
- Removed the older `System.Drawing` PNG workflow from the Paint.NET 5.2-preview source.
- Added stronger RBGI-container validation.
- Added an installer for classic Paint.NET installations.
- Retained legacy renamed-PNG `.rbgi` support.
- Confirmed working with Paint.NET 5.2 Alpha build `5.200.9650.36619`.

### v1.0.0-beta / v1.0.1 test build — Paint.NET 5.1.12

- First public beta and test builds.
- Added `.RBGI` open support.
- Added `.RBGI` save support.
- Supported RBGI v1 headers, metadata JSON, and embedded PNG data.
- Supported legacy renamed-PNG `.RBGI` files.
- Confirmed working in Paint.NET 5.1.12.

---

## Roadmap

Planned or possible future updates:

- Preserve original custom metadata automatically when opening and saving `.rbgi` files.
- Finalize the plug-in after the stable release of Paint.NET 5.2.
- Test and improve Microsoft Store installation support.
- Add more sample `.rbgi` files.
- Add automated build and release workflows.
- Add a changelog.
- Add more documentation for the `.RBGI` format.
