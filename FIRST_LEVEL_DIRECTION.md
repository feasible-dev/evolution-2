# First-level design decisions

Current baseline and art decisions: [CURRENT_STATE](docs/CURRENT_STATE.md).

The opening opponent is **Nico Valdes**, not the earlier Shenzhen character idea.
The active game offers **Turn, Thrust, Direction**. Direction pushes along an
inherited absolute angle; agents do not perceive the safe region or seek a target.
Direction parameters evolve through survival and reproduction.

Target was rejected as too capable and dominant for an introductory card.
A future target-seeking ability should combine perception and action, cost more,
and arrive when a level creates a real need for it. The long-term vocabulary is
Condition / Action / Rule / Program / GeneticLanguage / MutationPolicy; see
MUTATION_CARD_DESIGN.md. Card points currently weight available behavior choices;
do not silently reinterpret them as physical thrust or speed multipliers.

## Preserve

- Identical physical rules for both teams; explain strategy and evolutionary
  effects through measured statistics rather than hidden team bonuses.
- Two team colors, orange and cyan; no agent color per card type.
- Circular arena corners under resizing, a small reserved lower strip, and a
  dominant arena. Separate survival histories remain in the side columns.
- Compact active cards. Composition percentages are removed from the UI.
- Fixed LED dots on black, predominantly orange/blue, rare red event effects.
  Text scrolls right to left; short messages can hold or flash.
- Natural irregular presenter glances, delayed partner responses and attention
  to the action. Standing is reserved for extraordinary measured events.
- Consistent head size, subtle mouth motion, emotion carried through sitting
  down; no permanent grin or synchronized idle head turns.

## Still design work, not implemented by cleanup

- Consider widening the first survival strip from 5% to 10%; not approved as a
  cleanup change. Check current settings before changing the challenge.
- Build progressive discovery around obstacles, viscosity, energy, hazards and
  perception; every new card should solve a specific newly encountered problem.
- Clarify impulse, sustained thrust, maximum speed and energy in card language.
- Distinct card costs and eventual perception/action composition.
- Live deck changes in PvP are a future feature, not current locked-round play.

Commentary must use actual measured events and completed-generation comparisons.
Avoid calling temporary occupancy a survival result or announcing an unsupported
winner. Prioritize major events, allow reading time, expire stale remarks and
apply cooldowns. Proposed additional triggers include recovery, sustained lead
changes and plateaus; do not assume all are implemented.
