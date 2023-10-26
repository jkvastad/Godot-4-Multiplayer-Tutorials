extends Control

@onready
var chat_log = $ChatLog
@onready
var chat_input = $ChatInput

func _ready():
	$SelfIDDisplay.text = str(multiplayer.get_unique_id())	
	
func _set_authority_ID():
	var node_authority_ID = $NodeAuthorityIDInput.text
	self.set_multiplayer_authority(int(node_authority_ID))
	$NodeAuthorityIDDisplay.text = str(self.get_multiplayer_authority())

func _add_to_chat():
	var message = chat_input.text
	_rpc_add_to_chat.rpc(message)
	
func _direct_message():	
	_rpc_add_to_chat.rpc_id(int($DMRecipientID.text),"DM - " + $DMInput.text)

@rpc("any_peer", "call_local", "reliable", 0)
func _rpc_add_to_chat(message):	
	var last_line = chat_log.get_line_count()
	var timestamp = Time.get_time_string_from_system()
	chat_log.insert_line_at(last_line - 1,timestamp + 
	" " + str(multiplayer.get_remote_sender_id()) + ": " + message)
