FFVI Pixel Remaster Switch support patch
========================================

What changed
- Opens raw Nintendo Switch save files directly (no Base64 conversion needed).
- Saves raw Switch files back in the same format.
- Adds File > Open Switch JKSV folder...
- Scans the folder and lists readable slots with slot ID, playtime, gil,
  timestamp and original hashed filename.

Build
1. Install Visual Studio 2022 or newer with .NET desktop development.
2. Install the .NET 10 SDK if Visual Studio does not already include it.
3. Open src/Ffvi.SaveTool.slnx.
4. Build Ffvi.SaveTool.Gui in Release mode.

Use
1. Make an untouched JKSV backup first.
2. Choose File > Open Switch JKSV folder...
3. Select the slot from the list.
4. Edit it and choose Save. The original hashed filename is retained and
   the file remains raw Switch binary.
5. Restore that JKSV backup on the Switch.

This source package has been patched but not compiled in this environment.
