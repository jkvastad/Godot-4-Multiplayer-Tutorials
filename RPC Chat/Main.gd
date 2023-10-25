extends Control

var DEFAULT_PORT = 8910

func _on_host_pressed():
	var peer = ENetMultiplayerPeer.new()
	peer.create_server(DEFAULT_PORT)
	multiplayer.multiplayer_peer = peer
	_disable_buttons()
	_spawn_chat()
	
func _on_join_pressed():
	var address = $Address.text
	if not address.is_valid_ip_address():
		print ("'" + address +"' is not a valid ip")
		return
	var peer = ENetMultiplayerPeer.new()
	peer.create_client(address, DEFAULT_PORT)
	multiplayer.multiplayer_peer = peer
	_disable_buttons()
	_spawn_chat()
	
func _disable_buttons():
	$HostButton.disabled = true
	$JoinButton.disabled = true
	
func _spawn_chat():
	var chat_scene = load("res://multiplayer_chat.tscn")
	var chat = chat_scene.instantiate()
	$Chats.add_child(chat)
