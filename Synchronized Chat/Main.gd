extends Control

var DEFAULT_PORT = 8910

func _process(_delta):
	_update_ID_display()

func _update_ID_display():
	$SetSpawnerAuthorityInput/SpawnerAuthorityDisplay.set_text(str($MultiplayerSpawner.get_multiplayer_authority()))

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
	
func _disable_buttons():
	$HostButton.disabled = true
	$JoinButton.disabled = true
	
func _spawn_chat():
	var chat_scene = load("res://multiplayer_chat.tscn")
	var chat = chat_scene.instantiate()
	$Chats.add_child(chat)

func _set_spawner_authority():
	$MultiplayerSpawner.set_multiplayer_authority(int($SetSpawnerAuthorityInput.text))
