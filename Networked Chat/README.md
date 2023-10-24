# Networked Chat
Tutorial project showing a minimal multiplayer chat in Godot.

# RPC vs. MultiplayerSpawner vs. MultiplayerSynchronizer
When to use what and how?
MS for convencience replication
RPC for everything else

## RPC
Described in High Level multiplayer

## MultiplayerSpawner vs. MultiplayerSynchronizer
Not described in High Level multiplayer, but described in a Godot scene replication article: https://godotengine.org/article/multiplayer-in-godot-4-0-scene-replication/. Mentioned in SceneMultiplayer docs, along with SceneReplicationConfig.

### MultiplayerSynchronizer
Synchronization is not supported for Object type properties, like Resource.
Properties that are unique to each peer, like the instance IDs of Objects (see Object.get_instance_id) or RIDs, will also not work in synchronization.
TODO: What CAN be synchronized? Built in types?
Add things to the property window when the synchronizer is selected in the editor (screenshot). Properties will be synchronized if a matching node path exists in connected clients scene trees. (TODO: test removing nodes at client/host, what is the synchronizer behavior?)
Pushes selected properties from the multiplayer authority of the synchronizer. Note how in the project the authority of the box itself does not matter - only the synchronizer ID matters.
Note that if you have 3 peers; 1 host and 2 clients, setting the clients synchronizers to the same ID while leaving the hosts on a separate ID will not synchronize the clients. If the host sets its synchronizer ID to the same as the clients, all peers will now synchronize - synchronization must go via the host, but the peer with the authority decides what the target property state is.

### MultiplayerSpawner
Pushes nodes under the spawn path and allowed in the auto spawn list from the multiplayer authority.