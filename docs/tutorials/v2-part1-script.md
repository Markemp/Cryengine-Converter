# Part 1: Converting Cryengine Assets — v2.0 Tutorial Script

**Format:** screen-recorded voiceover, casual conversational style matching the 2017 originals.
**Length target:** ~13 min (will run over, that's fine).
**Demo assets:**
- **Primary (MWO):** Adder mech — `D:\depot\mwo\Objects\mechs\adder\`
- **Secondary (SC #ivo):** AEGS Avenger — `D:\depot\SC4.6\Data\Objects\Spaceships\Ships\AEGS\Avenger\`
- **Animation demo:** KCD2 Boar (path TBD — fill in your actual KCD2 extraction location)

**Shell:** PowerShell inside Windows Terminal — encourage viewers to install Windows Terminal.
**No drag-and-drop:** intentionally omitted because dropping a `.cgf` on the .exe means no `-objectdir` and broken materials.

---

## 0:00 – 0:30 — Cold open

**ON SCREEN:** Quick title card: **"Cryengine Converter v2.0 — Part 1: Converting Assets"**. Then a fast cut: a `.cga` file in Explorer → an Adder mech in Blender, fully shaded, posed. Music can come up here for the first ~10 seconds.

**SPOKEN:**
> "Hi everyone, this is Jeff, and welcome to a new tutorial series on how to use Cryengine game art assets in modern 3D programs like Blender. If you've watched the old videos from a few years back — they're pretty out of date now. Cryengine Converter is on version 2, and a lot has changed. So I'm starting fresh.
>
> Same as before, this series is focused on Blender, but most of what I cover applies to Maya, 3DS Max, anything that takes USD or glTF or Collada. And same as before — please be aware of all applicable copyright rules, and don't steal. Thank you."

---

## 0:30 – 1:30 — What's new in v2 and what we'll cover

**ON SCREEN:** GitHub releases page on the right; bullet list overlaid on the left as Jeff talks.

**SPOKEN:**
> "Quick rundown of what changed since the old tutorials. Four big things.
>
> One — the default output format is now USD, Pixar's Universal Scene Description. Blender reads it natively, the materials come through, and it's the format I'd recommend going forward.
>
> Two — glTF and GLB are now supported alongside Collada. Four output formats total.
>
> Three — Star Citizen's newer binary material format (currently only works on SC 4.5 and newer; no backwards compatibility for SC until it is released), the one they call `#ivo`, is fully supported, except animations which are still a work in progress.
>
> Four — animations actually work end-to-end now. CAF, DBA, CAL animation files all parse. If you've got a rigged asset with multiple clips, you get one USD file per clip, and they import as separate actions in Blender. This will be explained more in the 2nd video.
>
> What we're going to cover today: setup, your first conversion, picking the right output format, the one argument you can't skip, bulk conversion, animations, and Star Citizen specifics. Part 2 will be the Blender side — fixing materials, working with rigs, prefabs, all that. Today is just about getting good output files."

---

## 1:30 – 3:00 — Setup

**ON SCREEN:**
1. Browser → `github.com/Markemp/Cryengine-Converter/releases`. Click latest release. Download `cgf-converter.exe`.
2. Move it into a `D:\scripts` directory.
3. Brief shot of System → Environment Variables → PATH containing `D:\scripts`. (Don't dwell — link in description.)
4. Open Windows Terminal. If you don't have it, install from Microsoft Store on screen.
5. PowerShell prompt. Type `cgf-converter`. Show the usage banner.

**SPOKEN:**
> "First thing, grab the latest release from the GitHub page — link's in the description. There's a single self-contained Windows executable, around 120 megs. It's got everything baked in, no .NET install needed, no dependencies.
>
> What I like to do is drop it into a `scripts` directory and put that on my PATH. I'm not going to walk through editing the PATH — there's a million tutorials for that already, link in the description if you need one — but you really do want to do it. Otherwise you're typing the full path to the .exe every single time, which is just a pain.
>
> Now, **about your terminal.** If you're still using the old Windows PowerShell window or the classic command prompt — install Windows Terminal. It's free in the Microsoft Store, it handles colors and Unicode and resizing properly, and it'll save you a lot of frustration. The Cryengine Converter is a command-line tool, and a good terminal makes a real difference. So that's my recommendation — Windows Terminal with a PowerShell tab.
>
> The last thing you need is a Cryengine game with the `.pak` files extracted. Mechwarrior Online, Star Citizen, Hunt: Showdown, Crysis, Kingdom Come Deliverance — anything Cryengine. Use 7-Zip — right-click on a `.pak` file, '7-Zip → Extract Here'. You'll end up with a directory tree that has folders like `Objects`, `Textures`, and `Materials` at the top. We'll call that the **data directory**. Remember it — we're going to point the converter at it constantly."

---

## 3:00 – 5:30 — Your first conversion (the Adder)

**ON SCREEN:**
1. PowerShell: `cd D:\depot\mwo\Objects\mechs\adder\body`.
2. `Get-ChildItem *.cga` — show the `.cga` files for the Adder body parts.
3. Pick one — say `adder_torso.cga`. Run:
   ```powershell
   cgf-converter adder_torso.cga -objectdir D:\depot\mwo
   ```
4. Show output: `adder_torso.usda` appears alongside the source.
5. Cut to Blender. File → Import → Universal Scene Description → pick `adder_torso.usda`.
6. Adder torso appears. Switch to Material Preview shading. Show textures wired up to Principled BSDF.
7. Tab into pose mode briefly if it's a CHR — show the rig.

**SPOKEN:**
> "Okay, let's do the simplest possible conversion. I'm in the directory for the Adder mech — which by the way is the best mech in the game, I will fight you on this. We're in the body folder. There's a bunch of `.cga` files in here, one per body part.
>
> The command is `cgf-converter`, the file name, then `-objectdir` pointing at the data directory — which for me is `D:\depot\mwo`. Hit enter. Done.
>
> By default in v2 you get a `.usda` file — that's USD in its text format.  You can open it in a text editor and see what's in there.
>
> Now over in Blender — File, Import, Universal Scene Description, find that `.usda`. There's the Adder torso. Switch to Material Preview shading and... textures. Wired up. Working. No script, no manual node setup, no fixing materials. That's the win for USD over the old Collada workflow.
>
> If you've used the old version of this tool, this is where you'd be opening the Node Editor and manually plugging in normal maps and unchecking the alpha channel and all that. Not anymore, unless you have to.  Let's just say "materials are hard"."

---

## 5:30 – 6:30 — Picking an output format (the 60-second segment)

**ON SCREEN:** A table on screen for the duration:

| Format    | Flag      | When to use                                                  |
|-----------|-----------|--------------------------------------------------------------|
| **USD**   | `-usd`    | **Default. Use this for Blender.**                           |
| glTF      | `-gltf`   | Game engines (Unity, Unreal, Godot), web viewers             |
| GLB       | `-glb`    | Same as glTF, single-file with embedded textures             |
| Collada   | `-dae`    | Older tools, Maya/Max workflows, anything that won't take USD |
| Wavefront | `-obj`    | **Don't.** Deprecated, no animation, no skinning.            |

**SPOKEN:**
> "Quick detour on output formats. v2 supports four — five if you count Wavefront, which I'll get to.
>
> **USD** is the default. Use this. It's where Blender is going, it's where the industry is going, and it's the most actively developed renderer in the tool right now.
>
> **glTF and GLB** are great if you're targeting a game engine — Unity, Unreal, Godot all take them. GLB is one self-contained file with the textures embedded, glTF is a folder of separate files. Both work in Blender too — USD just has slightly cleaner skeletons.
>
> **Collada** — `.dae` — is what the old videos used. Still fully supported except animations, still works fine, the renderer's mature. If you're on a Maya or 3DS Max workflow that already takes Collada, stick with it.
>
> **Wavefront `.obj`** is what this tool started as back in 2014. It's deprecated. No skinning, no animation, materials are limited. Don't use it.
>
> My rule — and the rule I'd recommend you follow: **start with USD. Switch to something else only if you hit a specific problem with it.** That's the order. USD first. Don't preemptively use Collada because it's familiar — give USD a real shot. It's better."

---

## 6:30 – 8:30 — The `-objectdir` rule (the most important segment)

**ON SCREEN:**
1. PowerShell: same Adder file. Run *without* `-objectdir`:
   ```powershell
   cgf-converter adder_torso.cga
   ```
2. Open the resulting `.usda` in Blender.
3. Show the Adder torso in flat default-magenta-or-grey — geometry fine, materials gone.
4. Cut back to PowerShell. Run *with* `-objectdir` again.
5. Reimport. Textures back. Side-by-side comparison freeze frame — broken vs working.

**SPOKEN:**
> "Okay, this is the most important part of the video. Watch carefully.
>
> Cryengine assets reference their materials and textures using paths *relative to the game's data directory*. The Adder file says 'my texture is at `Objects/mechs/adder/body/textures/adder_diff.dds`' — but it has no way of telling the converter where that path *starts*. That's what `-objectdir` is for.
>
> Watch what happens if I leave it off.
>
> [run command without -objectdir]
>
> [import in Blender — magenta or grey]
>
> Geometry's fine. Materials, gone. The converter couldn't find any of the texture files, so it wrote default placeholders.
>
> Now with `-objectdir`.
>
> [run with -objectdir]
>
> [reimport — textures back]
>
> Same asset. Same mesh. The only difference is the converter now knows where to look.
>
> **Always pass `-objectdir`.** Every single time. If you take one thing from this video, this is it.
>
> And — small soapbox moment — this is also why I don't recommend the drag-and-drop workflow you might have seen in older tutorials. Yes, you can drop a `.cgf` onto the .exe and it'll convert. But it can't pass `-objectdir` that way, so you get exactly what we just saw — flat materials, no textures. Use the command line. It's worth the thirty seconds."

---

## 8:30 – 10:00 — Bulk conversion

**ON SCREEN:**
1. PowerShell: `cd D:\depot\mwo\Objects\mechs\adder`.
2. Wildcard: `cgf-converter *.cga -objectdir D:\depot\mwo` — show it churning through Adder body parts.
3. Then the recursive one-liner in the body folder:
   ```powershell
   Get-ChildItem -Recurse -Include *.cga,*.cgf,*.chr,*.skin |
     ForEach-Object { cgf-converter $_.FullName -objectdir D:\depot\mwo }
   ```
4. Show ~30 files getting processed.

**SPOKEN:**
> "Single-file conversion is fine for one asset. For a whole mech, or a whole game, you want bulk.
>
> Wildcards work directly. `cgf-converter *.cga -objectdir [path]` converts every `.cga` in the current directory. Same for `*.cgf`, `*.chr`, `*.skin` — any of the supported extensions.
>
> For recursive — across an entire mech folder, or a whole game — PowerShell `Get-ChildItem -Recurse` piped into a `ForEach-Object` loop. I'll put this exact command in the description so you can paste it.
>
> One warning before you go run this on your entire `Objects` folder. A real Cryengine game has tens of thousands of assets. This will run for an hour. It'll produce twenty-plus gigs of output. Pick the subfolder you actually need. The whole Adder is fine — the whole MWO Objects folder is overkill."

---

## 10:00 – 12:00 — Animations (the KCD2 Boar)

**ON SCREEN:**
1. PowerShell: navigate to KCD2 boar asset folder. Show the `.chr` plus the `.chrparams`, `.dba`, `.caf` files alongside it.
2. Run:
   ```powershell
   cgf-converter boar.chr -anim -objectdir D:\depot\KCD2
   ```
3. Show the output — `boar.usda` plus a folder of animation USD files (one per clip).
4. Cut to Blender. Import the boar `.usda`. Animation appears. Scrub the timeline — the boar walks, runs, eats.
5. Optional: in the NLA editor, show the multiple action strips.

**SPOKEN:**
> "Animation support is the biggest reason to switch to v2, and to demo it I'm going to use a different game — Kingdom Come Deliverance 2. Specifically the boar. Because the boar has a *lot* of really nice animations — walking, running, eating, fighting — and it's a great way to see the whole pipeline working.
>
> Same command as before, but I'm adding `-anim`.  I'm also adding `-ut` to unsplit the textures, because KCD2 also does the split DDS thing like Star Citizen. The converter will pull all the animation files it finds — the `.caf`, `.dba`, `.cal` files — and convert them into separate USD files.
>
> [run conversion]
>
> Notice what came out. The boar geometry as USD, plus a folder full of separate USD files — one per animation clip. Walk, run, idle, attack, all individual files.
>
> That separate-files thing matters. The old Collada importer in Blender used to merge every animation into one giant action, which made multi-clip assets basically unusable. With USD and separate files, each clip is its own action, and you can drop them into the NLA editor as strips.
>
> [import in Blender, demo a clip]
>
> There's the boar walking. Pretty nice, right? The animation data comes from a few different file types — `.caf` is a single clip, `.dba` is a database of multiple clips, `.cal` is a list. The converter pulls all of them when you pass `-anim`, you don't need to know which is which.
>
> One thing to know — `-anim` only does anything for skeletal assets. `.chr`, `.skin`, sometimes `.cga` if it has a rig. Static meshes ignore the flag, no harm done."
> 
> Also, the 2nd tutorial video will show how to use the Cryengine Importer add-on to automatically get these animations set up properly in Blender, with the NLA editor and everything. So if you're interested in that, stay tuned for Part 2.
---

## 12:00 – 13:30 — Star Citizen specifics (the AEGS Avenger)

**ON SCREEN:**
1. Navigate to `D:\depot\SC4.6\Data\Objects\Spaceships\Ships\AEGS\Avenger\`.
2. Show the file listing — `AEGS_Avenger.cga`, `AEGS_Avenger.cgam`, the `.mtl` files.
3. Open one of the `.dds.0`, `.dds.1` split files in Explorer briefly to show what split textures look like. (Or any folder where they're visible.)
4. Run:
   ```powershell
   cgf-converter AEGS_Avenger.cga -unsplittextures -objectdir D:\depot\SC4.6\Data
   ```
5. Show the converter combining DDS files as it runs.
6. Open in Blender. Show the Avenger with working materials — including the binary `#ivo` ones.

**SPOKEN:**
> "Last segment — Star Citizen. SC needs two extra things you should know about.
>
> First, materials in Star Citizen 3.23 and later use a binary format internally called `#ivo`. v2 handles this transparently — same command, just works. If you've tried converting a recent SC ship and gotten empty or default materials, that's the bug, and v2 fixes it.
>
> Second, Star Citizen (and a lot of more modern games like KCD2) ships its DDS textures *split* across multiple files — you'll see `.dds.0`, `.dds.1`, `.dds.2` next to each other in the Textures folders. That's how their build system spits them out. Pass `-unsplittextures` or `-ut` and the converter will merge them back into single DDS files before it writes materials.
>
> Let's do the AEGS Avenger.
>
> [run conversion]
>
> Textures get combined as it runs — you can see it doing the work in the log. Output is `AEGS_Avenger.usda`, drop it into Blender, and there's the Avenger with full materials.
>
> If you're doing Star Citizen specifically, those two things — the `#ivo` support and the `-unsplittextures` flag — are why v2 exists for you."

---

## 13:30 – 14:30 — Outro and Part 2 teaser

**ON SCREEN:**
1. Quick recap slide listing the commands shown.
2. Cut to a finished scene in Blender — the Adder posed, the Avenger behind it, the boar mid-walk. Just to show the variety.
3. End card: "Part 2 — Importing & Setting Up in Blender" with subscribe button.

**SPOKEN:**
> "Quick recap. The four-piece command is: `cgf-converter`, the asset name, an output format flag if you want something other than USD, and *always* `-objectdir`. Add `-anim` if it's a skeletal asset. Add `-unsplittextures` if the DDS image files are split. That's the whole CLI for ninety percent of use cases.
>
> Part 2 is going to be the Cryengine Importer and Blender sides. We'll take what we just exported and actually do something with it — set up scenes, work with the rigs, fix the small material issues that always come up, look at importing prefabs. That's coming soon, link will be in the description when it's up.
>
> If this saved you some time, a thumbs up helps the algorithm find the next person trying to convert their first `.cga`. The Cryengine Converter is open source — issues and pull requests welcome on the GitHub repo, also linked below.
>
> Alright, thanks for watching, and go make some beautiful art."

---

## Production notes

- **Tone calibration:** the old videos are casual and conversational — "alright let's get started", "this is the best mech, I will fight you on this", "and that wasn't too bad". Match that energy. Don't over-script.
- **PowerShell font:** at least 16pt, ideally 18. Viewers are squinting at commands.
- **The `-objectdir` segment is the most important visual** in the video. Get a clean recording of the magenta-vs-textured side-by-side and consider holding on it for an extra beat.
- **Cuts to Blender** can be quick — viewers don't need to see Blender boot. Cut directly to the import dialog.
- **Description should include:**
  - GitHub repo: https://github.com/Markemp/Cryengine-Converter
  - Latest release page
  - 7-Zip
  - Windows Terminal (Microsoft Store)
  - Adding to PATH guide (any decent one)
  - The recursive PowerShell one-liner as plain text
  - Part 2 (placeholder until recorded)
- **YouTube chapters** — mirror the section headings: Setup, First Conversion, Output Formats, ObjectDir, Bulk Conversion, Animations, Star Citizen, Outro.

## Things deliberately *not* in this video

- Drag-and-drop onto the .exe — covered in the `-objectdir` segment as an anti-pattern.
- Material fixing in Blender — Part 2 territory.
- The CryEngine Importer Blender add-on — Part 2 territory (assuming it still exists / gets updated for v2).
- `-png` / `-tif` / `-tga` texture conversion flags — not needed for the demo, can be a short Part 1.5 if there's demand.
- `-excludenode` / `-excludemat` — advanced workflow stuff, not part-1 territory.
