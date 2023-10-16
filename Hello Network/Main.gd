extends Control

# Script mostly based on the high level multiplayer lobby implementation in the docs
# https://docs.godotengine.org/en/stable/tutorials/networking/high_level_multiplayer.html#example-lobby-implementation

# Default game server port. Can be any number between 1024 and 49151.
# Not present on the list of registered or common ports as of October 2023:
# https://en.wikipedia.org/wiki/List_of_TCP_and_UDP_port_numbers
const DEFAULT_PORT = 8910

func _ready():
	# Callback for running code when a new peer connects
	multiplayer.peer_connected.connect(_on_player_connected)

# Bare minimum to create a host:
# We make a network peer (e.g. a host), which creates a server.
# The peer then registers itself with the games MultiplayerAPI as the active peer
func _on_host_pressed():	
	# Create a network peer using ENet UDP network library
	# peers are actors/agents on a network, e.g. clients or the host
	var peer = ENetMultiplayerPeer.new()
	# IP for server is set using a peer's set_bind_ip method, but defaults to *
	# Setting the IP to * binds the game to all interfaces
	var error = peer.create_server(DEFAULT_PORT)	
	if error:
		print("error code " + str(error) + ", see create_server docs")
		return
	# register our hosting peer with the scene tree multiplayer API
	multiplayer.multiplayer_peer = peer
	_disable_buttons()


# Bare minimum to join a host:
# We make a network peer (e.g. a client), which joins a server.
# The peer then registers itself with the games MultiplayerAPI as the active peer
func _on_join_pressed():
	# Create a network peer using ENet UDP network library
	# peers are actors/agents on a network, e.g. clients or the host
	var peer = ENetMultiplayerPeer.new()
	var address = $Address.text
	var error = peer.create_client(address, DEFAULT_PORT)	
	if error:
		print("error code " + str(error) + ", see create_client docs")
		return
	# register our hosting peer with the scene tree multiplayer API
	multiplayer.multiplayer_peer = peer
	_disable_buttons()
	
# Prints the new id when a peer connects
# As an exercise, try using four instances, one host and three clients
# Inspect the print log as more peers join the server
func _on_player_connected(id):
	var self_id = multiplayer.multiplayer_peer.get_unique_id()
	print("Peer " + str(self_id) + " now connected to " + str(id))
	
func _disable_buttons():
	# Disable buttons so we don't accidentally click them again
	# '$NodePath' literal is shorthand for get_node("NodePath"), see gdscript docs
	# https://docs.godotengine.org/en/stable/tutorials/scripting/gdscript/gdscript_basics.html#literals
	$HostButton.disabled = true
	$JoinButton.disabled = true
