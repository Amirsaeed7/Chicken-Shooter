# Architecture

`GameManager` is the persistent singleton for player state and JSON save/load. It uses `DontDestroyOnLoad` and scene names for transitions.

`LevelDataSO` contains stage and ordered wave data. `WaveDataSO` defines formations, enemy type, count, speed, health, egg rate, asteroid flags, and boss flags. `WeaponDataSO` defines projectile and firing properties.

`RoundManager` reads the current level and round data, creates formations or asteroid waves, and advances rounds or stages. `Enemy` implements normal behavior and drops. `BossController` is an explicit finite-state machine.

The New Input System asset supplies Move, Attack, Bomb, and Pause actions. The HUD listens to `GameManager.StateChanged`.
