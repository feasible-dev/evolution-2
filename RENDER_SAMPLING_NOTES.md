# Render Sampling Notes

## Context

We tried to add a "show 1 generation out of N" mode to make long rounds feel faster while still preserving some visual feedback.

The goal was:

- simulate all generations
- render only one generation out of `N`
- keep the bottom graph fully up to date
- visually save time by not drawing skipped generations

## What we learned

### 1. Skipping rendering is not the same as speeding up the simulation

The expensive part is not only drawing dots on screen.
The simulation still has to:

- update every agent
- resolve overlaps
- evaluate rules
- apply selection/reproduction
- update analytics

So hiding frames did not automatically produce the large speedup we wanted.

### 2. Partial visualization created confusing feedback

The sampled mode tended to produce:

- static snapshots for long stretches
- abrupt visual jumps
- confusing relationship between world view and graph
- clunky selection transitions

This made the simulation feel broken even when the underlying data was still evolving.

### 3. The graph and the world view need different timing rules

The graph should reflect all generations.
The world view should reflect smooth motion.

Trying to sample one but not the other introduced UX problems:

- the graph looked "ahead"
- or the graph lagged behind on purpose
- both options felt wrong in practice

### 4. Selection screens are especially disruptive in sampled mode

If selection is still shown while generations are skipped, the result feels jerky.
If selection is hidden, the transition between visible generations becomes harder to read.

That means selection visualization should probably be redesigned as a separate presentation layer, not mixed into a generation-skipping feature.

## Decision

For now, we reverted the "render every N generations" feature and went back to rendering every generation normally.

This keeps behavior simple and trustworthy while we continue building the core game.

## Safer future directions

If we revisit this problem later, better options are likely:

- improve motion readability with trails / ghost traces
- add clearer per-agent path visualization
- use a dedicated fast-forward presentation mode, not a partial live mode
- speed up the simulation itself, not only the rendering
- show post-round summaries rather than trying to sample live generations

## Guideline

Do not reintroduce generation-sampling in the live sandbox unless:

- the simulation loop is measurably faster in hidden generations
- the graph/world synchronization is explicitly designed
- selection presentation has a dedicated UX plan
