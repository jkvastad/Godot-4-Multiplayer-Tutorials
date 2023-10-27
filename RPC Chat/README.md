# RPC Chat
Tutorial project showing a minimal multiplayer chat in Godot, using Remote Procedure Calls (RPC).

## Scene replication: RPC vs. MultiplayerSpawner vs. MultiplayerSynchronizer
In this tutorial we will be looking at scene replication using RPCs. For MultiplayerSpawner and MultiplayerSynchronizer, see the tutorial Synchronized Chat.

### When to use what?
Spawners and Synchronizers sacrifice flexibility for convenience - they are useful when basic things such as transform position or existence of a node needs to be replicated.
RPCs sacrifice convenience for flexibility - you could rely solely on RPCs to replicate everything. The trade-off is boiler plate code and manual work for simple replication cases.

## RPC
Described in the docs under [High Level Multiplayer](https://docs.godotengine.org/en/stable/tutorials/networking/high_level_multiplayer.html#remote-procedure-calls).

To use an RPC, create a function and annotate it with @rpc e.g.

```gdscript
#the parameter names are mode, sync, transfer_mode, and transfer_channel
@rpc("authority", "call_local", "reliable", 0)
func _rpc_add_to_chat(message):	
	# relevant code...
```

then you can call it with .rpc or .rpc_id(id) as such:

```gdscript
#Sends the RPC to all connected peers
_rpc_add_to_chat.rpc(message)
#Sends the RPC to the peer with ID == id
_rpc_add_to_chat.rpc_id(id, message)
```

Central to understanding RPCs is the concept of the multiplayer authority and its corresponding ID number. The authority ID is both a per node parameter accessible via [Node methods](https://docs.godotengine.org/en/stable/classes/class_node.html#methods), as well as a per multiplayer peer property accessible via a [Node's multiplayer property](https://docs.godotengine.org/en/stable/classes/class_node.html#properties).

Note that it is thus possible to be a peer with one authority ID, and have nodes with other authority IDs (e.g. you are a client but the authority for the nodes is the host).

### Mode - authority vs. any_peer
When receiving an RPC call, a function with mode set to authority will only execute if the sending peer ID matches the multiplayer authority ID of the node to which the script is attached (the receiving node).

When receiving an RPC call, a function with mode set to any_peer will always execute.

### Sync
call_remote does not execute if the caller's ID matches the receiver's ID.

call_local puts no restriction on the caller's ID.

### Transfer Mode
From the docs:
>"unreliable" Packets are not acknowledged, can be lost, and can arrive at any order.
>
>"unreliable_ordered" Packets are received in the order they were sent in. This is achieved by ignoring packets that arrive later if another that was sent after them has already been received. Can cause packet loss if used incorrectly.
>
>"reliable" Resend attempts are sent until packets are acknowledged, and their order is preserved. Has a significant performance penalty.

### Transfer Channel
From the docs:
>transfer_channel is the channel index.
>
>The first 3 can be passed in any order, but transfer_channel must always be last.
>

[See examples from the docs for use cases](https://docs.godotengine.org/en/stable/tutorials/networking/high_level_multiplayer.html#channels).

## Testing things out
Of main interest is the various behaviors of .rpc and .rpc_id when varying the RPC Sync and Mode. Try different permuations of authority, any_peer, call_remote, and call_local to see how it works in Godot! 

(Note that i have implemented no error handling for failed connections to keep things minimal. Sometimes the host fails to create a server: if this happens, restart the project and try again.)
