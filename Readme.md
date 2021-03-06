# Marble Race
Prefab for VRChat.

- Watch marbles roll!
- Bet on marbles and win or lose (useless, virtual) money!
- Make your own courses - it's all modular!

# Download
Please check the [releases](https://github.com/daalta/MarbleRace/releases) page!

# Requirements
- [VRChat SDk 3](https://docs.vrchat.com/docs/setting-up-the-sdk)
- [UdonSharp](github.com/MerlinVR)
- [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@1.3/manual/index.html)

# Adding it to your scene
1. Import the `MarbleRace.unitypackage`
2. Drag `MarbleRace/Prefabs/MarbleRaceCoolExample` or `MarbleRaceTemplate` prefab into your scene.
  - Unity may prompt you to import TextMeshPro. You may delete TextMeshPro's example font.
  - **Keep the the Y rotation at 0° or 180°.** The prefab uses Unity's 2D Physics and does not work correctly unless it it parallel to your scene's X axis.
  - 2D Physics interacts with 3D physics! Watch out for 3D collision, it might mess with the race track.

# Customization
## Make your own track

Unhappy with the layout of the race? Want to shorten something, or make the race longer? Re-arrange things as you like! All track pieces use Unity's [2D Physics](https://learn.unity.com/tutorial/2d-physics). It's all modular, so go nuts.

For course prefabs, go to `MarbleRace/Prefabs/Courses`.

If you'd like to make your own course & need some obstacles to get started, go to `MarbleRace/Prefabs/Obstacles`.

## Advanced obstacles

We made some scripts that let you create some *exciting* obstacles! Add them to any obstacle & tweak the settings.

- `SpinObstacle` spins the obstacle at a fixed rate.
- `DisappearingObstacle` disappears after a marble touches it. It respawns after some time. Great for roadblocks!

## Marbles
To change the marbles, first find them in your scene hierarchy. You can tweak the following settings:
- The **name** of the marble object determines how it will show up in the scoreboard.
- The **UI color** setting on the Marble's script determines which color it will have in the betting UI.
- You can change the **mesh** or **material**, they do not affect gameplay.
- If you're brave, you can tweak the **Physics** of the `MarblePhysics` Physics Material, found in `MarbleRace/PhysicsMaterials`.

## Wait, what did I just drag into my scene?
If you'd like to understand how this prefab works, here's a brief overview of the prefab's hierarchy after you drag it into your scene.
- **MarbleRace** is a script controls the race. It has references to all other important objects. You *probably* don't need to touch this unless you want to rearrange the balls list.
- **Marbles** contains the marbles. Currently there's six.
  - Each marble has a special `Marble` script for synchronizing its position for all players.
- **Courses** is the entire racetrack between the start and finish. It's mostly 2D colliders. This is the easiest part of the prefab to tweak and remix.
  - **Spawn** contains:
    - A script and transforms for getting the marbles' starting locations
    - A betting screen for the start of the race
  - **Finish** notifies the `MarbleRace` script if a marble has finished the race. 
- **StartGameButton** starts the game. It calls `StartPreRace` on the `MarbleRace` script. You can customize the button.
- **Physics2DSettings** lets you override some Unity 2D Physics settings. VRChat doesn't let you change them via your project settings, so a script like this is required.
  - **Velocity** threshold is the minimum speed at which balls bounce. If a ball is slower than this, it will 'stick' to a surface. 

## Known issues
- Disabling the prefab is not something I've tested. It should for fine for non-masters, but the prefab might go *bazinga* if the master disables it.

# Credits
- Models, Textures by [Hughesy](https://twitter.com/lachie_hughes)
- Code by [Faxmashine](twitter.com/faxmashine)
- [UdonSharp](github.com/MerlinVR)
- [VRWorld Toolkit](github.com/oneVR)
- [CyanEmu](github.com/cyanLaser)
