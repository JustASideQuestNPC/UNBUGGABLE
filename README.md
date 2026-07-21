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
- Arrow keys to move a selection.
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
All settings for UNBUGGABLE can be changed by editing `config.json`. After editing the config file,
either restart the editor or hit the "Reload Config" button in the top left corner to reload most
settings. **Note:** Currently, the color theme and hit sound tick rate will not change until you
completely restart the editor.

## colorTheme
Which color theme to use for the editor. This must be the name of one of the themes in `themes.json`
(case sensitive).

## useBeatFiles
Whether UNBUGGABLE should default to saving charts as .beat.txt files, or as standard .txt files.

## enhancedPreview
If true, the in-game preview shows an indicator of where doubles will land, and gives mash notes a
"tail" like hold notes have. Set this to false to make the preview more closely match what you
actually see in-game.

## alwaysShowAllNoteFlags
If true, the note viewer will always show the letters for all note flags on all notes. Normally,
flags that determine the note type are not shown (for example, spikes and doubles always have the
Whistle flag, so that flag is not shown for them).

## enableBreakpoints
Enables breakpoints to start the game midway through the chart.

## useLane2AsMarkers
If true, loading a chart file will convert all notes that are in lane 2 into UNBUGGABLE markers
(unless a marker there already exists).

## saveMarkersAsLane2Notes
If true, all markers are saved in a chart file as a lane 2 note. Markers are always saved in the
UNBUGGABLE section of a .beat.txt file, regardless of this setting.

## alwaysEnableCustomDifficultyName
If true, you can set a custom name for every difficulty slot, not just Star.

## autoSelectPastedNotes
If true, pasting notes will automatically select those notes (this is useful for pasting and then
mirroring sections).

## allowTopLaneCopMashes
Whether to allow placing cop mashes on the top lane. In-game, cop mashes are always on the bottom
lane, regardless of what lane they were actually placed in.

## showFreestyleSubNotesWhilePlacing
If true, freestyle notes appear smaller in the note viewer if they will be subnotes of another
freestyle note in-game.

## beatSnaps
A list of every value that the chart editor can snap to. For some reason, the official editor
multiplies all snap values by 4. The default snap values for UNBUGGABLE match the ones in the
official editor.

## minZoom
Minimum possible zoom. Smaller values zoom out, larger values zoom in.

## maxZoom
Maximum possible zoom. Smaller values zoom out, larger values zoom in.

## zoomIncrement
How much to increase or decrease zoom by when scrolling the mouse. Negative values invert scroll
direction.

## laneOrder
What order (from left to right) the note viewer displays lanes. The default setting places the
center lane in between the top and bottom to match the in-game order. To use the official editor's
order where the center lane is on the right, change this setting to 
`["top", "bottom", "camera", "center"]`.

## hitSoundOffset
Constant offset to make hit sounds play slightly earlier or later than the actual note.

## hardChartOffset
Constant offset applied to all charts, to match up with the one hard-coded into the official editor.
This value is only in the config file because I don't like hard-coding things. ***Do not change***
***this number unless you know what you're doing.***

## hitSoundTickRate
While the song is playing, how many times to check for whether any notes should play a hit sound.
Lower values can improve performance, but may cause hit sounds to desync.