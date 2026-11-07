# Selected art — Evolution Wars

This folder retains approved reviews and useful production sources. Rejected
studies and repeated runtime captures were removed on 2026-11-07. Older committed
versions remain in Git history (`cf310fd` before cleanup).

## See the retained work

Serve this folder with `python -m http.server 8774 --bind 127.0.0.1 --directory art-direction`
from the repository root, then open <http://127.0.0.1:8774/>.
Previews are local HTML/canvas; no external generation service or API key is needed.

| Location | Purpose |
| --- | --- |
| `studio-approved/` | Approved black hangar + black wooden sign, neon option **02** |
| `opponent-endings-v01/` | Side-by-side approved entrances, both ending directions |
| `level-2-v18/`, `level-2-v19-camera/` | Nao double peace, then hands behind back; rink/camera |
| `nico-intro-v02/`, `nico-intro-v03-camera/` | Nico shy open-hand wave; rink/camera |
| `v34/`, `v35/` | Earlier detailed/simplified presenter sources retained for comparison |
| `v38/`, `v39/`, `v41/` | Presenter source sheets/builders and LED alphabet source |
| `v49/` | Original studio/card prompt provenance; runtime artwork is in prototype/assets |
| `v50/`, `v51/`, `v52/` | Current gallery/standing/sitting/opponent production sources |

The studio mockup and opponent entrances are review assets, **not game integration**.
The approved intro for Nao is the finale only; older dance frames in its source
atlas are historical material. The comparison player uses the correct finale range.
No final choice between camera-facing and rink-facing rests has been made.

## Optional regeneration

The checked-in atlases, timings and HTML work without Node. To rebuild images,
install Node.js and run from the repository root:

```powershell
npm ci --prefix art-direction
node art-direction/level-2-v18/build-preview.cjs
node art-direction/level-2-v19-camera/build-preview.cjs
node art-direction/nico-intro-v02/build-preview.cjs
node art-direction/nico-intro-v03-camera/build-preview.cjs
```

Sharp is pinned in `package-lock.json`; tools no longer depend on an undocumented
`.cache/sprite-tools` installation. Native frame sizes are 192x192 for Nao and
128x192 for Nico. Both are shown with integer nearest-neighbor scaling.
Generated `frames/` exports are ignored; the atlas is the canonical pixel output.
Source sheets, prompts, timing and copies needed to preserve shared frames remain.

Production builders in `v38`, `v39`, `v41`, and `v52` write into runtime assets;
do not execute them just to view the artwork. Their versioned paths remain stable
to preserve dependencies (including `v35/duo-scenes/scenes.json` event timing).
Archived generation metadata can mention removed historical references; those
are provenance, not live dependencies. Use the current-state document as the brief.

New gameplay captures belong in ignored `.cache/captures/`. Keep future reviews
small: a selected source, useful preview, prompt and decision note, not every draft.
