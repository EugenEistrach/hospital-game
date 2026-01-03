extends Area2D

@onready var color_rect: ColorRect = $ColorRect
var original_color: Color = Color(0.2, 0.5, 0.8)
var click_color: Color = Color(0.9, 0.3, 0.3)

func _ready() -> void:
	color_rect.color = original_color
	input_event.connect(_on_input_event)

func _on_input_event(_viewport: Node, event: InputEvent, _shape_idx: int) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == MOUSE_BUTTON_LEFT:
		print("Square clicked!")
		_flash_color()

func _flash_color() -> void:
	color_rect.color = click_color
	await get_tree().create_timer(0.15).timeout
	color_rect.color = original_color
