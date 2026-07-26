# FFVI Pixel Remaster Save Editor

A Windows save editor for **Final Fantasy VI Pixel Remaster**, written in C# and WinForms.

This fork adds native Nintendo Switch save support alongside the existing Steam/Windows support, together with inventory improvements and save-format fixes.

## About this fork

This fork is maintained by **Nova_Megami2113** (`protos813-spec`).

Changes added in this version include:

- Native Nintendo Switch save support
- Direct opening of extracted Switch save folders
- Switch save-slot browser
- Support for raw Switch saves without the Steam Base64 wrapper
- Inventory category and mass-editing improvements
- Additional save validation and format fixes

## Features

- Reads and writes Steam/Windows Pixel Remaster saves using Rijndael-256, custom padding, DEFLATE and Base64.
- Reads and writes raw Nintendo Switch save files.
- Opens extracted Nintendo Switch save folders and identifies readable save slots.
- Displays Switch slot ID, play time, gil, timestamp and filename.
- Edits party gil, total gil and step count.
- Edits character level, HP, MP and stat bonuses.
- Displays character stats using a Base + Total view.
- Supports all 54 spells with **Learn All** and **Forget All** options.
- Edits inventory using a 273-item lookup with item categories.
- Adds new inventory entries, removes entries and sets item quantities to 99.
- Edits weapons, shields, helmets, armour and relics.
- Edits owned and equipped Espers.

## Not currently supported

- Party composition and corps slots
- Story progression flags
- Treasure flags
- Esper spell-learning progress
- Keywords
- Warehouse items

## Installation

The editor supports Windows 10 and Windows 11.

The release build is self-contained, so no separate .NET installation is required.

1. Download the latest build from the [Releases](../../releases) page.
2. Extract the ZIP anywhere on your PC.
3. Run `Ffvi.SaveTool.Gui.exe`.

Windows SmartScreen may display an unrecognised-app warning because the executable is unsigned. Select **More info**, followed by **Run anyway**.

## Nintendo Switch usage

Always back up your save before editing.

1. Export your Final Fantasy VI Pixel Remaster save using JKSV.
2. Copy the exported save folder to your PC.
3. Run `Ffvi.SaveTool.Gui.exe`.
4. Select:

   ```text
   File → Open Switch JKSV folder...
   ```

5. Select the exported save folder.
6. Choose a slot from the save-slot browser.
7. Make your changes.
8. Select **File → Save**.
9. Copy the edited save folder back to your Switch.
10. Restore it using JKSV.

The editor ignores expected Switch metadata files and attempts to identify valid character save slots automatically.

## Steam/Windows usage

Always back up your save folder before editing.

The default Steam save location is:

```text
%USERPROFILE%\Documents\My Games\FINAL FANTASY VI PR\Steam\<steam-id>\
```

1. Close the game before editing.
2. Consider temporarily disabling Steam Cloud to prevent it from restoring an older save.
3. Run `Ffvi.SaveTool.Gui.exe`.
4. Select **File → Open**.
5. Choose a save-slot file. Slot files are generally larger than the configuration and metadata files in the same folder.
6. Select a character and make your changes using the available tabs.
7. Select **File → Save**.
8. Launch the game and verify the edited slot.

## Identifying Steam save files

Steam save filenames are Base64-hashed and are not human-readable.

After opening a file, the status bar displays its slot ID:

| Slot ID | In-game slot |
|---|---|
| 1–20 | Manual save slots 1–20 |
| 21 | Quick Save |
| 22 | Autosave |

Character save slots are generally larger than 50 KB and contain a `pictureData` field. Smaller files in the same folder contain configuration, slot occupancy and progression data.

## Safety notes

- Always retain an untouched backup.
- Close the game before editing.
- Steam Cloud may overwrite local changes.
- The editor does not currently create automatic backups.
- Restore your backup if an edited slot appears as empty or fails to load.
- Open a GitHub issue and describe the changes you made if a repeatable problem occurs.

## Save format

Steam/Windows saves are decoded using the following process:

```text
File bytes
  → Remove UTF-8 BOM when present
  → Restore missing Base64 padding
  → Base64 decode
  → Rijndael-256-CBC decrypt
  → Remove custom zero-byte padding
  → DEFLATE decompress
  → Decode UTF-8 JSON
```

Nintendo Switch saves store the save data without the Steam Base64 wrapper and are handled separately by the editor.

The JSON structure contains multiple escaped JSON strings nested inside other objects:

```text
top.userData
  .ownedCharacterList
    .target[N]
      .parameter
        .currentHP
        .currentMP
        .addtionalMaxHp
```

Field names are preserved from the game's data model, including misspellings such as `addtional` and `owendGil`.

### .NET implementation notes

`RijndaelManaged` on modern .NET versions only supports AES-compatible block sizes. This project uses BouncyCastle.NET for the game's 256-bit Rijndael block size.

Nested JSON strings must also be serialised using:

```csharp
JsonSerializerOptions
{
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
}
```

The game's parser may reject saves when nested quotation marks are escaped differently.

## Building from source

End users should normally use the prebuilt release.

Requirements:

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Windows, because the GUI uses WinForms

Clone and run:

```bash
git clone https://github.com/protos813-spec/AnotherFFVIPRSaveEditor.git
cd AnotherFFVIPRSaveEditor/src/Ffvi.SaveTool.Gui
dotnet run
```

Create a self-contained release:

```powershell
.\build-release.ps1
```

The release ZIP is written to:

```text
publish\Ffvi.SaveTool-YYYYMMDD.zip
```

## Project layout

```text
AnotherFFVIPRSaveEditor/
  src/
    Ffvi.SaveTool.Lib/   Save format, crypto, data models and editing logic
    Ffvi.SaveTool.Gui/   WinForms interface
    Ffvi.SaveTool.Diag/  Diagnostic console application
    Ffvi.SaveTool.slnx
  build-release.ps1
```

## Credits

- [KiameV/final-fantasy-vi-save-editor](https://github.com/KiameV/final-fantasy-vi-save-editor) — original save-format reverse engineering, data tables, Rijndael key, IV and padding information.
- [GiulioSamp/AnotherFFVIPRSaveEditor](https://github.com/GiulioSamp/AnotherFFVIPRSaveEditor) — original C# and WinForms implementation.
- **Nova_Megami2113** (`protos813-spec`) — Nintendo Switch support, inventory improvements, save-format fixes and maintenance of this fork.
- [Final Fantasy Wiki](https://finalfantasy.fandom.com/) — reference material for item, spell, Esper, character and stat metadata.
- [BouncyCastle.NET](https://www.bouncycastle.org/csharp/) — cryptography library.

## Licence

Licensed under the MIT Licence. See [`LICENSE`](LICENSE).

## Disclaimer

This project is not affiliated with or endorsed by Square Enix.

Final Fantasy VI Pixel Remaster is the property of Square Enix Holdings Co., Ltd. This editor only modifies save files supplied by the user and does not patch or modify the game executable.
