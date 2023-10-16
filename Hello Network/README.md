# Hello Network
Tutorial project showing minimal connection in Godot. A callback is also connected to the peer_connected signal to print connection status to the editor. 

See comments in Main.gd for code explanation and the step by step recreation file for recreation details.

## Example output from connecting 3 clients to one host, sorted by peer

Peer 1 (host) connects to the first client, which also connects to the host:
```
Peer 1 now connected to 1888539480
Peer 1888539480 now connected to 1
```
The previous peers connect to the new peer, the new peer connects to all previous peers (host and first client):
```
Peer 1 now connected to 520450720
Peer 1888539480 now connected to 520450720
Peer 520450720 now connected to 1
Peer 520450720 now connected to 1888539480
```
The previous peers connect to the new peer, the new peer connects to all previous peers (host, first client and second client):
```
Peer 1 now connected to 2024518808
Peer 1888539480 now connected to 2024518808
Peer 520450720 now connected to 2024518808
Peer 2024518808 now connected to 1
Peer 2024518808 now connected to 1888539480
Peer 2024518808 now connected to 520450720
```
