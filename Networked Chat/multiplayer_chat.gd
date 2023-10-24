extends Control

@onready
var chat_box = $ChatBox
@onready
var multiplayer_synchronizer = $MultiplayerSynchronizer

func _ready():
	#TODO - should this get triggered after MultiplayerSpawner spawns the scene?
	multiplayer.connected_to_server.connect(_update_self_ID)	

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	_update_ID_display()

func _update_ID_display():
	var chat_box_authority_ID = chat_box.get_multiplayer_authority()
	var synchronizer_authority_ID = multiplayer_synchronizer.get_multiplayer_authority()
	$ChatBoxAuthorityIDLine.set_text(str(chat_box_authority_ID))
	$SynchronizerAuthorityIDLine.set_text(str(synchronizer_authority_ID))
	
func _set_synchronizer_ID():	
	multiplayer_synchronizer.set_multiplayer_authority(int($SetSynchAuthorityIDInput.text))
	
func _set_chat_box_ID():	
	$ChatBox.set_multiplayer_authority(int($SetChatBoxAuthorityIDInput.text))
	
func _update_self_ID():
	$SelfAuthorityIDLine.text = str(multiplayer.get_unique_id())
