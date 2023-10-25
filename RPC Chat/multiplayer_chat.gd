extends Control

func _ready():
	$SelfIDDisplay.text = str(multiplayer.get_unique_id())	
