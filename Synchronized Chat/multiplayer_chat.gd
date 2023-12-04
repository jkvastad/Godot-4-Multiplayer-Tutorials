extends Control

@onready
var chat_box = $ChatBox
@onready
var multiplayer_synchronizer = $MultiplayerSynchronizer

func _ready():			
	_update_self_ID()

func _process(_delta):
	_update_ID_display()

func _update_ID_display():
	var chat_box_authority_ID = chat_box.get_multiplayer_authority()
	var synchronizer_authority_ID = multiplayer_synchronizer.get_multiplayer_authority()
	$ChatBoxAuthorityIDLine.set_text(str(chat_box_authority_ID))
	$SynchronizerAuthorityIDLine.set_text(str(synchronizer_authority_ID))
	
func _set_synchronizer_ID():	
	multiplayer_synchronizer.set_multiplayer_authority(int($SetSynchAuthorityIDInput.text))
	
func _set_chat_box_ID():	
	chat_box.set_multiplayer_authority(int($SetChatBoxAuthorityIDInput.text))
	
func _update_self_ID():	
	$SelfAuthorityIDLine.text = str(multiplayer.get_unique_id())
