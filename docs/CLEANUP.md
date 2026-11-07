# Repository cleanup — 2026-11-07

The art working set went from 2,703 files / 526 MB to approximately 111 files /
99 MB (excluding installed Node dependencies and regenerable frame exports).
About **427 MB of art trials** were removed from the working tree. Old tracked
prototype logs were also removed. Git history was not rewritten.

Retained: all runtime assets, approved source sheets and prompts, earlier useful
presenter style references, both opponent ending directions, and the selected
black hangar with **option 02 red-orange neon WARS**. The new sign and entrances
remain review assets awaiting integration.

Documentation now distinguishes the playable `prototype/` project from the root
sandbox. Capture destinations moved to ignored `.cache/captures/`. Sprite tools
use a pinned Sharp dependency with a lockfile instead of a private cache path.
An obsolete smoke-test assertion was updated to check presenter adjacency and
centering at their actual display size, rather than the former 192-pixel width.

Validation: 131 runtime asset files remained byte-identical; all four retained
intro atlases rebuilt with identical pixels; six preview pages, 31 local links
and the four timing manifests were checked. Shared greeting frames match between
the two ending orientations. The gallery and selected image respond over local HTTP.

Both .NET projects built successfully. The root simulation console suite passed.
The playable smoke test passed 28,579 checks through 40 generations, including
studio, presenter, marquee and opponent checks. Godot printed an ObjectDB leak
warning on shutdown despite exit code 0; the root console build also emitted its
Godot source-generator warning. Neither warning was investigated in this cleanup.

Optional research/tool caches under `.cache/` remain local and ignored. Bulk cache
deletion was rejected by automatic tool policy; this does not affect a fresh clone.
Earlier committed experiments can be recovered from `cf310fd`. Rejected untracked
drafts are not recoverable from that Git revision.
