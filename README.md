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
- [Color Themes](#)

# Installation
To install UNBUGGABLE, download and run the installer from the
[latest release](https://github.com/JustASideQuestNPC/UNBUGGABLE/releases/latest). Have fun!

# Quickstart
If you've used the official editor, you already know how to use UNBUGGABLE. If you don't, go read
the [official instructions](https://app.notion.com/p/dcellgames/USING-THE-EDITOR-3485dc0d0e12804b8ad7fc31213a134f)
and then come back here to see the UNBUGGABLE-specific things.

### IMPORTANT!!!
Currently, UNBUGGABLE has 3 semi-major limitations:
1. Variable bitrate .mp3 files do not work correctly and will desync from everything else. If you're
   running into troubles with desync, use a .wav or make sure you convert to a constant bitrate.
   **Note:** If you rip an mp3 off of YouTube, it probably has a variable bitrate.
2. The in-game preview in UNBUGGABLE only shows where the camera is *supposed* to be. Unlike the
   official editor, it does not account for how long it takes the camera to actually move.
3. Because of rounding errors that I am doing my best to fix, pasting notes will sometimes place
   them 1ms before or after a snap line. On its own this has no effect on gameplay, but it prevents
   the notes from appearing in the placement priority list, and if you add new notes alongside them
   it can result in forced misses. You can fix this by selecting the notes and using the nudge
   keybinds to line them back up.

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
UNBUGGABLE supports every keybind found in the official editor's "shortcuts cheat sheet", *except*
*for* `Shift+Left/Right Arrows` to move a selection left and right (selections can still be moved up
and down with the arrow keys). Additionally, `PgUp` and `PgDn` now move to the previous and next
label.

There are also some UNBUGGABLE-specific keybinds:
- `,`/`.`: Cycle cop notes (in addition to the default `/` and `\`).
- `q`: Place a marker. Hold `Shift`, or `Ctrl`, to change the color of the marker.
- `b`: Place or move the breakpoint. Use `Ctrl+b` to delete it.
- `ctrl+1/2/3/4` without any notes selected will set the editor to place notes for that cop.
- `ctrl+0` or <code>ctrl+`</code>: If notes are selected, converts cop notes to normal notes.
  Otherwise, sets the editor to place normal notes.
- `n` while notes are selected: Set singles, spikes, holds, and doubles to spawn in the center of
  the screen, like in the base game's Noisz charts.
- `shift+c/f/w/n`: Lock the C/W/F flags or the Noisz spawn. While a flag is locked, placing a note
  will immediately give it that flag (does not apply to pasting notes). **Note:** Locking the W flag
  will make all singles and holds become spikes and doubles, and make all camera changes zoom
  in/out. Locking the F flag will make all freestyles become negative mashes (if conversion is
  enabled). Locking the C flag will make singles and holds become invisible notes, make all camera
  changes be instant swaps, and will make most other note types invalid. Locking Noisz spawns does
  nothing for camera changes and center lane notes.
- `alt+w/s` while notes are selected: Nudge those notes forward or back by 1ms. This only moves the
  start of holds, doubles, and mash notes; use `shift+alt+w/s` to move the end of them. **Note:**
  Nudging the end of notes will still move instant notes. This is primarily useful for when you use
  spikes or other notes to telegraph a double.
- Drag while holding right click to delete notes instead of selecting them.

## Editing Keybinds
All of UNBUGGABLE's keybinds can be found and edited in `configs/keybinds.json` Every action can
have as many keybinds as you want. To add a new keybind, add another string to the list. "ctrl",
"shift", and "alt" (in any order) can be used as modifiers. Each keybind can only have one main
key/mouse button bound to it (don't expect that to change any time soon, sorry).

For mouse buttons, use either "leftMouse", "rightMouse", "middleMouse", "scrollUp", or "scrollDown"
(**Note:** Keybinds for note placement cannot be used with the scroll wheel). For keyboard keys, the
best way to find them is to enable debug mode in config.yaml, then open the editor and press
whatever key you want. Whatever shows up next to "last pressed key" is what you need to use.
**Note:** Because my UI library is weird, the first letter in the name will be uppercase; in the
keybind file it should be lowercase (i.e., "pageUp" instead of "PageUp").

There are a few limitations with the keybinds:
- Actions can't share a keybind (technically they can and the editor will run, but it won't work
  like you want it to).
- What key is used to place notes on each lane can be rebound, but you always hold shift to place
  spikes and doubles.
- Mouse-specific actions can't be rebound - left click always selects things, right click always
  deletes things.

# Settings
All settings for UNBUGGABLE can be changed by editing `configs/config.json` (descriptions for each
setting are in the config file). After editing the config file, either restart the editor or hit the
"Reload Config" button in the top left corner to reload most settings. **Note:** For technical
reasons, these settings will not change until you fully restart the editor:
- hitSoundTickRate
- maxConcurrentHitSounds
- autosaveInterval

## colorTheme
Which color theme to use for the editor. This must be the name of one of the themes in the themes
folder (case sensitive).

## useBeatFiles
Whether UNBUGGABLE should default to saving charts as .beat.txt files, or as standard .txt files.

## enancedPreview
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
If true, you can set a difficulty name for every difficulty slot, not just Star.

## autoSelect
Determines when notes are automatically selected after being placed, either "none", "pasted", or
"all":
- "none": Notes are never automatically selected.
- "pasted": Pasting one or more notes automatically selects those notes.
- "all": Notes are automatically selected whenever you add them for any reason.

## selectHoldNotesFromTail
Determines whether you can select holds, doubles, and mashes by clicking on their tail, rather than
just their head, either "none", "first", "last", or "all":
- "none": Notes can never be selected by clicking on their tail.
- "first": When clicking on a point that has multiple tails overlapping (such as with double stacks
  or 4key holds), only the note that starts earliest in the chart will be selected.
- "last": When clicking on a point that has multiple tails overlapping, only the note that starts
  latest in the chart will be selected.
- "all": When clicking on a point that has multiple tails overlapping, all notes will be selected.
**Note:** Notes cannot be selected from the tail by dragging the mouse, only by clicking on them.

## allowTopLaneCopMashes
Whether to allow placing cop mashes on the top lane. In-game, cop mashes are always on the bottom
lane, regardless of what lane they were actually placed in.

## showFreestyleSubNotesWhilePlacing
If true, freestyles appear smaller in the note viewer if they will be subnotes of another freestyle
note in-game.

## enableNegativeMashConversion
If true, freestyles (not mashes) with the F flag will be converted to negative mashes when a chart
is saved. Negative mashes look like normal mashes in-game, but behave like freestyles and die
immediately upon being hit.

## enableLivePlacement
If true, you can place notes, camera changes, and markers while the song is playing. **Live**
**placement is extremely experimental and will not become an actual feature until a later version.**
The only reason this setting exists is because I fixed the bug that originally made it possible (all
this setting does is re-enable that bug for note placement only).

## beatSnaps
A list of every value that the chart editor can snap to. For some reason, the official editor
multiplies all snap values by 4. The default snap values for UNBUGGABLE match the ones in the
official editor.

## quickScrollBeats
While holding the quick scroll modifier (default `x`), you scroll this many beats at a time. Must be
a positive integer.

## pasteOverwrites
Determines what happens when pasting over existing notes, either "none", "notes", or "region":
- "none": If a note is pasted on top of an existing note, the existing note will stay and the new
          note will not be pasted.
- "notes": If a note is pasted on top of an existing note, the existing note will be removed and the
           new note will replace it.
- "region": When pasting, ALL notes between the start and the end of the pasted section will be
            removed, even if they weren't in the same place as one of the pasted notes.

## preserveNoiszFlag
If true, replacing/extending a note will preserve whether the N (noisz spawn) flag is on that note.
Does not apply to cop notes.

## holdExtensionSearchThreshold
When you place a hold, double, or mash note, the editor will check for a matching note that begins
or ends within this many milliseconds of the new note's start or end. If a note is found, that note
will be extended instead of placing the new note. Setting this to 1 or 2 milliseconds should account
for any rounding error-related issues.

## minZoom
Minimum possible zoom. Smaller values zoom out, larger values zoom in.

## maxZoom
Maximum possible zoom. Smaller values zoom out, larger values zoom in.

## zoomIncrement
How much to increase or decrease zoom by when scrolling the mouse. Negative values invert scroll
direction.

## sliderIncrement
Determines how many percent the volume and play speed sliders snap to.

## laneOrder
What order (from left to right) the note viewer displays lanes. The default setting places the
center lane in between the top and bottom to match the in-game order. To use the official editor's
order where the center lane is on the right, change this setting to 
`["top", "bottom", "camera", "center"]`.

## jumpTargets
Determines where the "jump to previous/next label" keybinds can send you to. Allowed values are
`"labels"`, `"bpmChanges"`, `"breakpoint"`, `"firstNote"`, `"lastNote"`, `"firstMarker"`,
`"lastMarker"`, `"chartStart"`, and `"chartEnd"`. **Note:** Values in this array can be in any
order. The editor will automatically sort them.

## doublePreviewAlpha
Opacity of doubles in the in-game preview while they are moving toward their landing point. Between
0 and 1, where 0 is fully transparent and 1 is fully opaque.

## currentTimePosition
How far (in pixels) from the top of the screen the line for the current time is. Higher values show
more notes before the current time and less notes after it.

## hitSoundOffset
Constant offset to make hit sounds play slightly earlier or later than the actual note.

## hardChartOffset
Constant offset applied to all charts, to match up with the one hard-coded into the official editor.
This is only in the config file because I don't like hard-coding things. **Do not change this**
**number unless you know what you're doing.**

## hitSoundTickRate
While the song is playing, how many times to check for whether any notes should play a hit sound.
Lower values can improve performance, but may cause hit sounds to desync.

## maxConcurrentHitSounds
How many hit sounds can be playing at once. Turn this up if you're placing long streams of notes and
the hit sounds are glitching.

## autosaveInterval
How often to autosave (only if the chart was loaded from an existing chart file and/or has been
saved to a file at least once), in seconds. Set to 0 (or less) to disable autosaves. 

## hitSounds
Contains toggles to enable or disable hit sounds for every note type. The official editor enables
hit sounds for everything except markers and camera changes.

## debug
Contains toggles to enable or disable debug overlays that display some technical info.

# Color Themes
Every color theme is its own `.json` file in the themes folder. The naming is (hopefully)
self-explanatory, but there are a few restrictions on what you can add:
- Colors must be in `#rgb`, `#rgba`, `#rrggbb`, or `#rrggbbaa` format.
- All numbers cannot be negative (see below for the only exception). Text sizes also cannot be 0.
- For the `selected` section of note themes, you can set color values to an empty string and numbers
  to `-1`. Doing this will make them copy the value used when the note isn't selected. This also
  applies to the `hovered` section of button themes.