# Technical Decisions

- Procedural sprites provide an immediate playable baseline.
- Scene transitions use named scenes stored in `LevelDataSO`.
- JSON stores level, round, score, lives, bombs, power level, and food progress.
- Round completion reloads the level scene; stage completion loads the next stage.
- Game over resets to round 1 of the current level with 3 lives.
- Bosses use an explicit state enum.
- `ObjectPool` provides reusable projectiles.
