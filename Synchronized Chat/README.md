# Synchronized Chat
Tutorial project showing a minimal multiplayer "chat" in Godot, using Godot's MultiplayerSpawner and MultiplayerSynchronizer.

## Scene replication: RPC vs. MultiplayerSpawner vs. MultiplayerSynchronizer
In this tutorial we will be looking at scene replication using the MultiplayerSpawner and MultiplayerSynchronizer (Synchronizer/Spawner for short). We will look at RPCs in the RPC Chat tutorial.

### When to use what?
Spawners and Synchronizers sacrifice flexibility for convenience - they are useful when basic things such as transform position or existence of a node needs to be replicated.
RPCs sacrifice convenience for flexibility - you could rely solely on RPCs to replicate everything. The trade-off is boiler plate code and manual work for simple replication cases.

## MultiplayerSpawner vs. MultiplayerSynchronizer
Not described in the docs under High Level Multiplayer, but described in a Godot scene replication article: https://godotengine.org/article/multiplayer-in-godot-4-0-scene-replication/. Mentioned in SceneMultiplayer docs:https://docs.godotengine.org/en/stable/classes/class_scenemultiplayer.html

### MultiplayerSynchronizer
Synchronization is not supported for Object type properties, like Resource.
Properties that are unique to each peer, like the instance IDs of Objects (see Object.get_instance_id) or RIDs, will also not work in synchronization.
So what CAN be synchronized? Built-in types. Numerical values such as transform positions, hit points, basic stats. String values representing various text content.
To synchronize a property, add it to the property window which is visible when the synchronizer is selected in the editor (TODO: screenshot). Properties will be synchronized if a matching node path exists in connected clients scene trees.
The synchronizer pushes selected properties from the multiplayer authority of the synchronizer. Note how in the project the authority of the box itself does not matter - only the synchronizer authority ID matters. (Note also that when changing synchronizer ID there is a large variable delay ranging from a few seconds to above one minute before synchronization starts.)

### MultiplayerSpawner
Replicates nodes under the "Spawn Path" and allowed in the "Auto Spawn List" from the multiplayer authority. Similar behavior to the synchronizer but for node existence rather than node configuration.