# Mutation And Card Design Notes

## Goal

The sandbox and the future PvP game both need a coherent relationship between:

- the genetic language available to agents
- the mutation policy that transforms agent programs
- the cards the player can play
- the future expert-facing sandbox sliders

## Vocabulary

- `Condition`: the `if` part of a rule
- `Action`: the `then` part of a rule
- `Rule`: one `Condition -> Action` pair with parameters
- `Program`: the small set of rules carried by one agent
- `GeneticLanguage`: the set of conditions/actions/rule counts allowed in a run
- `MutationPolicy`: the probabilities that control how programs mutate

The player does not directly edit agent rules.
The player affects:

- the `GeneticLanguage`
- the `MutationPolicy`

## Design Principle

Cards must not directly edit rules already living inside agents.

Cards should instead:

- unlock condition families
- unlock action families
- increase the maximum number of rules per program
- bias mutation toward specific structural or parameter changes

This keeps the game scalable even when thousands of different rules exist across the population.

## Two Mutation Levels

### 1. Structural mutation

High-level changes to programs:

- add a rule
- remove a rule
- change the condition kind of a rule
- change the action kind of a rule

### 2. Parameter mutation

Lower-level changes inside a rule:

- time thresholds
- target points
- radii
- scalar strengths
- neighbor counts
- line offsets

## Planned Mutation Policy

The next version should replace the single global `MutationStrength` with a structured policy such as:

- `ProgramMutationChance`
- `AddRuleChance`
- `RemoveRuleChance`
- `ConditionKindMutationChance`
- `ActionKindMutationChance`
- `ConditionParameterMutationChance`
- `ActionParameterMutationChance`

Later, this can become more targeted:

- time-condition parameter mutation
- target-point mutation
- neighbor-radius mutation
- line-threshold mutation

## Planned Card Families

### Condition cards

Examples:

- `Temporal Logic`
- `Line Sense`
- `Target Radius`
- `Crowd Sense`

These unlock condition families in the genetic language.

### Action cards

Examples:

- `Relative Turn`
- `Relative Impulse`
- `Target Steering`
- `Brake`
- `Random Drift`
- `Vibration`

These unlock action families in the genetic language.

### Program cards

Examples:

- `Extra Rule Slot`
- later `Rule Slot +2`

These change the maximum number of rules in a program.

### Mutation cards

Examples:

- `Mutation Boost`
- `Structural Mutation`
- `Condition Mutation`
- `Action Mutation`
- `Parameter Mutation`

These change the mutation policy rather than the language itself.

### Budget cards

Not implemented yet, but planned:

- total budget
- targeted budget for rule count
- targeted budget for expensive mutation effects

## Sandbox Role

The sandbox should eventually expose expert sliders for mutation policy values so the player can build intuition before using cards in PvP.

Examples of sandbox controls:

- global program mutation chance
- add/remove rule chance
- condition kind mutation chance
- action kind mutation chance
- condition parameter mutation chance
- action parameter mutation chance

Long term, the sandbox may expose even finer sliders:

- time threshold mutation
- target point mutation
- neighbor radius mutation

This supports the intended learning loop:

1. understand mutation numerically in sandbox
2. learn what values are useful
3. reproduce those effects indirectly with cards during PvP

## Planned Inspection Feature

A future agent inspection panel should allow the player to click an agent and inspect:

- its current program
- the rules it carries
- the parameters of each rule
- eventually which rules were active during the last generation

This is meant for expert understanding, not for direct control.

## Next Recommended Step

Before adding many new cards or new rule families, implement a real `MutationPolicy` object and replace the single `MutationStrength` slider with structured mutation probabilities.

Then:

1. expose the mutation policy in the sandbox UI as sliders
2. introduce the first mutation cards that affect policy categories
3. only after that, add more conditions/actions

This keeps the code clean and makes future cards much more meaningful.

## Design discussion November 4 2026

The established vocabulary remains Condition, Action, Rule, Program,
GeneticLanguage and MutationPolicy. A rule associates a condition with an action;
a program contains the agent's inherited rules. Cards influence available
capabilities and evolution rather than directly scripting each individual.

The current design direction separates perception from conditions. Perception
provides information (position, a point, a region, distance or direction).
A condition tests information; an action uses it. Target navigation is therefore
a composed capability, not a basic action with free target knowledge and its own
hidden propulsion advantage. Region perception could support attraction,
avoidance, stopping at a boundary, or braking after entry. Detecting a region does
not automatically reveal whether it is safe or lethal.

Cards should have different costs. Exact costs and whether links themselves are
purchased remain open. Program capacity and mutation cards remain possible later
families; no additional family is approved for implementation yet.

The introductory movement should use an inherited absolute direction, before
target navigation. Distinguish initial velocity, instantaneous impulse,
continuous propulsion and maximum speed. A common initial launch and no initial
drag are proposals to test, not implemented settings. Viscosity can later create
a need for renewed impulses or sustained propulsion. Rotation requires a useful
relationship with heading-dependent propulsion or sensing; remove meaningless
rotation rather than inventing a reason to purchase it.

The first survival band may increase from 5 to 10 percent; this remains a tuning
proposal. Wall contact behavior must be considered alongside the launch model.
Both teams must retain equal physical capabilities under equal configurations.

Immediate focus returns to the first level and broadcast presentation. See
[First level direction](FIRST_LEVEL_DIRECTION.md) for the presentation backlog.
