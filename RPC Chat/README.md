# RPC Chat
Tutorial project showing a minimal multiplayer chat in Godot, Remote Procedure Calls (RPC).

## Scene replication: RPC vs. MultiplayerSpawner vs. MultiplayerSynchronizer
In this tutorial we will be looking at scene replication using RPCs. For MultiplayerSpawner and MultiplayerSynchronizer, see the tutorial Synchronized Chat.

### When to use what?
Spawners and Synchronizers sacrifice flexibility for convenience - they are useful when basic things such as transform position or existence of a node needs to be replicated.
RPCs sacrifice convenience for flexibility - you could rely solely on RPCs to replicate everything. The trade-off is boiler plate code and manual work for simple replication cases.

## RPC
Described in the docs under High Level Multiplayer: https://docs.godotengine.org/en/stable/tutorials/networking/high_level_multiplayer.html#remote-procedure-calls
