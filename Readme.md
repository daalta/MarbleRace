# Marble Race
Prefab for VRChat.

- Watch marbles roll!
- Bet on marbles and win or lose (useless, virtual) money!
- Make your own courses - it's all modular!

# Requirements
- [VRChat SDk 3](https://docs.vrchat.com/docs/setting-up-the-sdk)
- [UdonSharp](github.com/MerlinVR)
- [TextMeshPro](https://docs.unity3d.com/Packages/com.unity.textmeshpro@1.3/manual/index.html)

# Adding it to your scene
1. Import the `MarbleRace.unitypackage`
2. Drag `MarbleRace/Prefabs/MarbleRace` prefab into your scene.
  - Unity may prompt you to import TextMeshPro.
  - **Keep the the Y rotation at 0° or 180°.** The prefab uses Unity's 2D Physics and does not work correctly unless it it parallel to your scene's X axis.

# Customization
## Make your own track

Unhappy with the layout of the race? Want to shorten something, or make the race longer? Re-arrange things as you like! All track pieces use Unity's [2D Physics](https://learn.unity.com/tutorial/2d-physics). It's all modular, so go nuts.

## Marbles
To change the marbles, first find them in your scene hierarchy. You can tweak the following settings:
- The **name** of the marble object determines how it will show up in the scoreboard.
- The **UI color** setting on the Marble's script determines which color it will have in the betting UI.
- You can change the **mesh** or **material**, they do not affect gameplay.
- If you're brave, you can tweak the **Physics** of the `MarblePhysics` Physics Material, found in `MarbleRace/PhysicsMaterials`.

## Wait, what did I just drag into my scene?
If you'd like to understand how this prefab works, here's a brief overview of the prefab's hierarchy after you drag it into your scene.
- **MarbleRace** is a script controls the race. It has references to all other important objects. You *probably* don't need to touch this.
- **Spawn** contains:
  - A script and transforms for getting the marbles' starting locations
  - A betting screen for the start of the race
- **Finish** notifies the `MarbleRace` script if a marble has finished the race.
- **Marbles** contains the marbles. Currently there's six, but we plan to add (easy) support for more marbles in the future. 
  - Each marble has a special `Marble` script for synchronizing its position for all players.
- **Courses** is the entire racetrack between the start and finish. No fancy scripts here - it's all just 2D colliders. This is the easiest part of the prefab to tweak and remix.
- **StartGameButton** starts the game. It calls `StartPreRace` on the `MarbleRace` script. You can customize the button.

# Credits
- Models, Textures by [Hughesy](https://twitter.com/lachie_hughes)
- Code by [Faxmashine](twitter.com/faxmashine)
- [UdonSharp](github.com/MerlinVR)
- [VRWorld Toolkit](github.com/oneVR)
- [CyanEmu](github.com/cyanLaser)
