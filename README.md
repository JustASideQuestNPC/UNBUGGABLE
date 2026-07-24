# UNBUGGABLE
**the chart editor where bugs are illegal and you...follow the law?**

UNBUGGABLE is a fan-made custom chart editor for the rhythm game
[UNBEATABLE](https://unbeatablegame.com). It feels the same as the official editor, has extra bells
and whistles, and even fixes the bugs! Windows only.

# Contents
- [Installation](#installation)
- [Quickstart](#quickstart)
- [Keybinds](#keybinds)
- [Settings](#settings)

# Installation
To install UNBUGGABLE, download and run the installer from the
[latest release](https://github.com/JustASideQuestNPC/UNBUGGABLE/releases/latest). Have fun!

# Quickstart
If you've used the official editor, you already know how to use UNBUGGABLE. If you don't, go read
the [official instructions](https://app.notion.com/p/dcellgames/USING-THE-EDITOR-3485dc0d0e12804b8ad7fc31213a134f)
and then come back here to see the UNBUGGABLE-specific things.

### IMPORTANT!!!
Currently, UNBUGGABLE has 2 semi-major limitations:
1. Variable bitrate .mp3 files do not work correctly and will desync from everything else. If you're
   running into troubles with desync, use a .wav or make sure you convert to a constant bitrate.
   **Note:** If you rip an mp3 off of YouTube, it probably has a variable bitrate.
2. The in-game preview in UNBUGGABLE only shows where the camera is *supposed* to be. Unlike the
   official editor, it does not account for how long it takes the camera to actually move.

Additionally, UNBUGGABLE uses milliseconds for offset, not seconds.

## .beat.txt Files
By default, UNBUGGABLE saves charts as a .beat.txt chart file. These files will still load in-game
and in the official editor, but have extra UNBUGGABLE-specific data. You can save as a standard .txt
file and/or save to a new path by right-clicking the save button.

## Breakpoints
If you have [Stefyfresh's Practice Mod](https://github.com/Stefyfresh/UNBEATABLE-practice-mode)
installed, pressing `b` will set your breakpoint. When you play the chart in-game, the song will
start wherever the breakpoint is, instead of at the beginning. Press `ctrl+b` to remove the
breakpoint and start at the beginning of the song.

## Placement Priority (for advanced users)
The placement priority list shows the order that notes at the same time will appear in the chart
fill. Drag the list items to reorder them.

# Keybinds
UNBUGGABLE supports every keybind found in the official editor's "shortcuts cheat sheet" except for
these three:
- Right-clicking a BPM change to delete it (notes can still be deleted with right click).
- `Ctrl+A+2/3/4/5` to select every note in a lane.
- `Shift+Left/Right Arrows` to move a selection left and right (selections can still be moved up
  and down with the arrow keys).
Additionally, `PgUp` and `PgDn` now move to the previous and next label.

There are also a few UNBUGGABLE-specific keybinds:
- `,`/`.`: Cycle cop notes (in addition to the default `/` and `\`).
- `q`: Place a marker. Hold `Shift`, or `Ctrl`, to change the color of the marker.
- `b`: Place or move the breakpoint. Use `Ctrl+b` to delete it.
- `ctrl+1/2/3/4` without any notes selected will set the editor to place notes for that cop.
- `ctrl+0` or <code>ctrl+`</code>: If notes are selected, converts cop notes to normal notes.
  Otherwise, sets the editor to place normal notes.
- Drag while holding right click to delete notes instead of selecting them.

# Settings
All settings for UNBUGGABLE can be changed by editing `config.yaml` (descriptions for each setting
are in the config file). After editing the config file, either restart the editor or hit the "Reload
Config" button in the top left corner to reload most settings. **Note:** For technical reasons, some
settings will not update until you fully restart the editor.