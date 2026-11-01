Changes as I coded (I listed these as a I updated, see the assignment requirement under HW Requirement section of the readme)
1) Aliens and alien bullet speed up by 5% per alien death (capped)
2) Spaceship can not go off screen anymore, will wrap around instead
3) Keyboard is optional; gamepad and touch controls are supported.
4) Updated the fleets dimensions - if a column of aliens dies, the fleet size changes
5) The fleet now reaches the camera's screen edges before descending and reversing, adapting to screen size without overshooting. No longer a fixed value (dynamic)
6) Win and game over states: Win when all aliens are defeated; lose when the ship is destroyed or aliens reach its height(the red line). 
7) Added a restart button.
8) Added a highscore that is saved locally. Award points for kills and save the best score between sessions.
9) Pause and controls screen: Let players pause and see the controls for their device.
10)Added a shield. Shield blocks 3 shots. shields can be refilled with a blue shield drop (if we already have a shield it fully regenerates current shield)
11)Added a background. Also added an indicator of were the aliens need to cross to win
12) Improved graphics: improved alien design, bullet design and spaceship design by using some free online assets.
13) Added collision animations:
- Green explosions when aliens are destroyed.
- Blue impact pulses when the shield blocks a shot.
- Blue expanding bursts when collecting shield refills.
- Orange sparks when projectiles collide.
- Orange explosion when the ship is destroyed.
14) increased difficulty, gave 3 aliens a shield. Also made the aliens shoot faster and the bullets faster every time an alien is killed.
15) added as start screen and game over screen
16) added more levels (for each level, adds more aliens, increases ship speed and fire rate)
17) Added lives (3 lives). Allows to retry the current level as long as their are remaining lives.
18) Adjusted Rigidbody settings to prevent collision forces from pushing or rotating the ship and aliens


#-----------------------------------------------------------------------------------HW Requirements-----------------------------------------------------------------------------------#
#The 1st part of the homework that said complete all of the following
1) Aliens speed up as aliens get destroyed
2) Added logic to detect when alines have reached a point on the screen that the game is no longer possible to win (the red line).
2.1) player wins now if all aliens are defeated, player loses if ship is destroyed or aliens reach the red line.
3) Spaceship does not move off screen anymore, now wraps to the other side
4) For ways in which the code was inefficient, non-idiomatic or poorly written
4.1) The way it detected input assumed a keyboard was used as input with keyboard.current, if a keyboard failed to be detected, its possible a runtime error could have occured. Updated it to prevent the runtime error and allowed other input controls as well. I did this by replacing direct keyboard access with Input System actions, supporting keyboard, gamepad, and touch controls
4.2) Collisions destroyed any object they touched. Destroy(coll.gameObject) didn’t check whether the target was a ship, projectile, or another alien. If for some reason two aliens collide, they would destroy each other. Added logic so an alien can only be destroyed by a dart.
4.3) AlienCounter.Update() ran every rendered frame. updated it so the display can update once when the count changes. This avoids unnecessary string creation and UI work. The difference is small for one counter, but the pattern matters as an interface grows.
4.4) fleet.GetComponent<AlienFleet>() asked Unity to find that same component again since fleet was already an AlienFleet object. I removed the redundant lookup and now call fleet.AlienKilled(gameObject) directly.
Homework said 4 but I found a few more I fixed just while updating the code for my preference
4.5) Responsibility issue on how the alien were removed. The Alien handled editing the fleet internal list. By OOP standard, that bad practice. Updated it so the alien reports to the fleet it was killed and the fleet handles the removal. used AlienKilled() for this. This improves encapsulation and keeps fleet management in one place.
4.6) Dead code and unused imports: lowerBound was unused, its related logic was commented out, and several using directives weren't needed. I just removed these.
4.7) Misleading names: AlienCounter.score actually meant "remaining aliens", and bulletSpawnRate was a probability per physics tick rather than a rate over time. changed it to remainingAliens and fireProbabilityPerPhysicsTick
4.8) the fleet dimension/boundary calculation. originally the code tracked the horizontal edges using alien centers but the dimensions felt very weird when the aliens changed directions. I added bounds based on the surviving aliens. Clearing an outside column now reduces the fleet's width, while clearing the top or bottom row reduces its height.
4.9) the original fleet dimension was very far from the edge of the screen, updated it so it would almost touch the edge before changing the direction (visually much more satisfying now and just feels smoother)
5) Increased difficulty, gave 3 aliens a shield (for level 1, after its based on the number of rows of aliens). Those aliens have to be hit 4 times before they are killed (3 for the shield)
-Also added that the aliens shoot faster and the bullets move faster every time an alien is killed. 
-These changes with the earlier requirement of the aliens moving faster after an alien is kill increased the difficulty. 
-Also balanced the ship fire rate and ship movement speed (balanced till level 3, gets really hard after level 5)
-Also changed the aliens initial movement speed, fire rate and bullet speed to make them a bit faster.

#The 2nd part that says complete 4 of the following
1) added as start screen and end screen
2) Added a shield. Shield blocks 3 shots. shields can be refilled with a blue shield drop (if we already have a shield it fully regenerates current shield)
3) Added a background. Also added an indicator of were the aliens need to cross to trigger defeat (a red line)
4) Improved graphics: improved alien design, bullet design and spaceship design by using some free online assets.
I added more then 4, see an additional 3 below. Felt the game was incomplete without these
5) Added collision animations:
- Green explosions when aliens are destroyed.
- Blue impact pulses when the shield blocks a shot.
- Blue expanding bursts when collecting shield refills.
- Orange sparks when projectiles collide.
- Orange explosion when the ship is destroyed.
6) added some levels (will add more aliens, increase ship speed and fire rate)
7) Added lives (3 lives)

#-----------------------------------------------------------------------------------Not homework changes-----------------------------------------------------------------------------------#
1) added a highscore
2) fixed the rigid bodies... aliens and spaceship do not move on collision
3) Pause and controls screen: Let players pause and see the controls for their device.
4) Added a restart button.

#-----------------------------------------------------------------------------------Credits-----------------------------------------------------------------------------------#

### Artwork
- **Alien Stare** by **Lorc**, from [Game-icons.net](https://game-icons.net/1x1/lorc/alien-stare.html).
  Licensed under [CC BY 3.0](https://creativecommons.org/licenses/by/3.0/).
  Displayed with a green tint and resized in-game.
- **Player spaceship and laser sprites** by **Kenney Vleugels**, from the
  [Space Shooter pack](https://kenney.nl/assets/space-shooter-remastered).
  Released under [CC0 1.0](https://creativecommons.org/publicdomain/zero/1.0/).

### Development
Developed with AI-assisted programming support from **OpenAI Codex**