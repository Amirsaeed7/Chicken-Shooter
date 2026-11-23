# Manual Test Checklist

Open the project with Unity 6000.0.85f1, press Play in `MainMenu`, and use **New Game** for a clean run. Check each box only after confirming the expected result.

## Scenes and progression

- [ ] Main menu opens and its New Game, Continue, and Quit buttons work.
- [ ] New Game starts Level 1 at Round 1 with fresh score, lives, bombs, power, and food progress.
- [ ] Level 1 has 5 rounds, Level 2 has 7, and Level 3 has 10; each round loads its configured wave.
- [ ] All rounds feel slightly easier: one fewer enemy, slower/less durable chickens, less frequent eggs, 10% less boss health, and one fewer obstacle when the wave has more than three.
- [ ] Normal waves show the configured formation and enemies move/fire as expected.
- [ ] Asteroid waves contain falling asteroids, with none staying parked at the top of the screen.
- [ ] Enemies and dive-bombers visibly release falling eggs; eggs can damage the player and bombs clear them.
- [ ] Completing a round advances to the next round and saves progress.
- [ ] Completing each stage loads the next one; completing Level 3 opens Victory.
- [ ] Victory offers New Game and Main Menu; both buttons work.
- [ ] All six scenes are included in Build Settings and the Windows build launches.

## Controls and combat

- [ ] Move with WASD and arrow keys; movement stays within the play area.
- [ ] Fire with Space and left mouse button; shots travel upward and hit enemies.
- [ ] Standard, armored, and cyber enemies look like distinct chickens, not circles.
- [ ] Enemy formations move, dive-bombers dive, and enemies drop eggs.
- [ ] Player shots destroy enemies and asteroids; defeated chicken enemies can drop food or weapon gifts, but asteroids do not.
- [ ] Enemy eggs and obstacles damage the player on contact.
- [ ] Falling asteroid obstacles appear during rounds, can be shot, and can be dodged.
- [ ] The player starts with 3 hearts; taking damage removes one heart and briefly grants invincibility.
- [ ] Bomb input (X) works with the keyboard, consumes one bomb, clears enemies and eggs, and damages a boss.
- [ ] Using a bomb shows an expanding, fading explosion at the center of the screen.
- [ ] The HUD shows the current weapon power (1/3); collecting a weapon gift raises it and taking damage reduces it.
- [ ] All three weapon power levels change the firing pattern as intended.

## Boss rounds

- [ ] The boss has a distinct, larger chicken-like appearance.
- [ ] The boss health bar appears during boss rounds and updates when hit.
- [ ] The boss uses its egg-rain, dive, ranged, and enraged behaviors; each phase fires fewer eggs less often, including after low health triggers the Enraged phase.
- [ ] Defeating the boss awards score and advances the round.

## HUD, menus, and persistence

- [ ] HUD shows the correct level, round, and score.
- [ ] The HUD shows 3 hearts and the current bomb count as icons; icons decrease when resources are used.
- [ ] Food pickups update the food percentage and progress bar by 20%; five pickups award one bomb and reset progress.
- [ ] Escape and P pause/resume; the pause menu can also return to Main Menu.
- [ ] Taking the last hit opens Game Over; Retry and Main Menu work.
- [ ] Continue restores saved level, round, score, lives, bombs, power, and food progress.
- [ ] Projectiles are reused by the object pool without getting stuck or retaining stale state.

## Longer play and visual review

- [ ] Play through multiple rounds without errors, stuck objects, or broken scene transitions.
- [ ] Review the player ship, chicken variants, boss, obstacles, chicken-wing food pickup, hearts, and bombs at the target resolution.
- [ ] Note any remaining placeholder art or missing music; replacing final art and music is still a documented follow-up.
