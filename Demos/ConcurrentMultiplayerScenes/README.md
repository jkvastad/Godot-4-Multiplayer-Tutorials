# Concurrent Multiplayer Scenes
Demo project showing how to handle multiple concurrent scenes using visibility.

![Concurrent multiplayer scenes](https://github.com/jkvastad/Godot-4-Multiplayer-Tutorials/assets/9295196/4d662516-c03b-4635-8a2c-06a6d8129547)


The main features to check out are

* Clients can change the color of the cube/sphere while other clients are in different scenes.
* Handling VisibilityChanged signal to update e.g. Control nodes below a Node3D - Node3D does not get its visibility from CanvasItem like Node2D and Control, so they must be manually synced.
* MultiplayerSynchronizers are placed close to building block scenes, e.g. cube and sphere, allowing looser coupling.
* Scenes can be created and run stand alone (F6 in editor) before being integrated into a scene, allowing faster iteration.
