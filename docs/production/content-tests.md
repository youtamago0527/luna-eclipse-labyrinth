# Gameplay content verification

Entry point: `LunaEclipse.EditorTools.GameplayContentTests.Run`

Editor menu: `Luna/Validate Gameplay Content`

Batch (root owns invocation): add `-batchmode -quit -projectPath <project> -executeMethod LunaEclipse.EditorTools.GameplayContentTests.Run -logFile <log>` to the installed Unity Editor command.

Report: `Library/GameplayContentTests.json`. The method throws on the first failed expectation, writes a failure report, and logs a pass summary on success. Creating this test source does not claim that the Editor run has passed.

Coverage:

- Six distinct monster archetypes and positive combat data.
- 120 seeds: all four generated item kinds, unique IDs within a floor and across adjacent floors, reachable/non-overlapping placements, -3..3 equipment rolls, +1/+10 permanent additions, specifically -3→-2, consumables unaffected.
- RunModifiers equipment bonus reaches generated equipment; each expedition has its own RunId.
- Sword/shield equipping, negative roll preservation, replacement retention, already-equipped no-op, dropping equipped items clears the slot, re-pick preserves item identity without auto-equipping.
- One fixed adjacent enemy: exact attack/defense effects, precisely one retaliation and one turn per accepted action, lethal attack prevents retaliation. Unrelated enemies are cleared for deterministic expectations.
- Herb heals eight HP, potion caps at max HP, neither restores satiety; full-HP use preserves the item and turn.
- Bag capacities 20/30/40 and out-of-range clamping; full-bag refusal retains floor item and consumes no turn.
- Satiety decreases each five actions; +2 endurance changes it to seven. Satiety clamps to zero.
- B50 fresh start has full satiety; three subsequent floor transitions preserve HP, bag, equipped IDs/effects, run ID, capacity and satiety action counter; immediate movement remains accepted.
- Missing item actions and the unimplemented return scroll are rejected; scroll is absent from generated items across 100 floors.

Limits: pure model checks only. UI interaction, rendering, audio, platform build, save migration, and lifecycle across separate gameplay sessions require their respective validation. No gameplay source, scene, or project setting is changed by the tests.

Review finding resolved by root: DungeonRun now prefixes generated item IDs with RunId. The tests additionally assert that two expeditions with identical seeds have disjoint generated item IDs, preventing collisions when inventory is carried across runs.
