extends Resource

signal selected()
signal deselected()

enum ARG_TYPE {
	ARGUMENT
	PROJECT_PATH,
	FILE_PATH,
	FOLDER_PATH,
	SELECTED_FOLDER,
	SELECTED_DAW,
}

var ARG_KEY: Dictionary = {}
var ARG_NAME: Dictionary = {}

var type: int = -1
var is_custom: bool = true
var dir: String = ""
var arg: String = ""

func _init() -> void:
	add(ARG_TYPE.ARGUMENT, "arg", "Argument")
	add(ARG_TYPE.PROJECT_PATH, "project", "Project Path")
	add(ARG_TYPE.FILE_PATH, "file_path", "File Path")
	add(ARG_TYPE.FOLDER_PATH, "folder_path", "Folder Path")
	add(ARG_TYPE.SELECTED_FOLDER, "folder", "Selected Folder")
	add(ARG_TYPE.SELECTED_DAW, "daw", "Selected DAW")

func set_is_custom(val: bool) -> void:
	is_custom = val

func set_dir(val: String) -> void:
	dir = val

func set_arg(val: String) -> void:
	arg = val

func add(key: int, value: String, name: String) -> void:
	ARG_KEY[key] = value
	ARG_KEY[value] = key
	ARG_NAME[key] = name

func get_key(key: int = -1) -> String:
	if key == -1:
		return ARG_KEY[type]
	return ARG_KEY[key]

func get_name(key: int = -1) -> String:
	if key == -1:
		return ARG_NAME[type]
	return ARG_NAME[key]

func get_type(key: String) -> int:
	return ARG_KEY[key]
	
func deselect() -> void:
	emit_signal("deselected")

func select() -> void:
	emit_signal("selected")

func generate_field() -> Control:
	var new_field = HBoxContainer.new()
	new_field.connect("gui_input", self, "_on_field_gui_input")
	
	var label = Label.new()
	new_field.add_child(label)
	label.text = get_name()
	label.align = Label.ALIGN_LEFT
	label.valign = Label.VALIGN_CENTER
	label.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	label.size_flags_vertical = Control.SIZE_FILL
	label.mouse_filter = Control.MOUSE_FILTER_PASS
	
	var select_highlight = ColorRect.new()
	label.add_child(select_highlight)
	select_highlight.set_anchors_and_margins_preset(Control.PRESET_WIDE)
	select_highlight.show_behind_parent = true
	select_highlight.mouse_filter = Control.MOUSE_FILTER_PASS
	select_highlight.color = Color(1, 1, 1, 0.25)
	select_highlight.visible = false
	connect("selected", select_highlight, "set_visible", [true])
	connect("deselected", select_highlight, "set_visible", [false])
	
	var arg_edit = LineEdit.new()
	new_field.add_child(arg_edit)
	arg_edit.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	arg_edit.text = arg
	arg_edit.placeholder_text = "-arg"
	arg_edit.connect("text_changed", self, "set_arg")
	
	var param = Control.new()
	if type == ARG_TYPE.FILE_PATH or type == ARG_TYPE.FOLDER_PATH:
		param = HBoxContainer.new()
		var custom_box = CheckBox.new()
		custom_box.pressed = is_custom
		custom_box.connect("toggled", self, "set_is_custom")
		param.add_child(custom_box)
		var dir_edit = LineEdit.new()
		dir_edit.text = dir
		dir_edit.visible = is_custom
		dir_edit.size_flags_horizontal = Control.SIZE_EXPAND_FILL
		dir_edit.connect("text_changed", self, "set_dir")
		custom_box.connect("toggled", dir_edit, "set_visible")
		param.add_child(dir_edit)
		if type == ARG_TYPE.FILE_PATH:
			dir_edit.placeholder_text = "C:/path/to/file.exe"
		elif type == ARG_TYPE.FOLDER_PATH:
			dir_edit.placeholder_text = "C:/path/to/folder"
	new_field.add_child(param)
	param.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	param.size_flags_vertical = Control.SIZE_FILL
	return new_field

func _on_field_gui_input(event: InputEvent) -> void:
	if event is InputEventMouseButton and event.pressed and event.button_index == BUTTON_LEFT:
		select()

func serialize(dict: Dictionary) -> void:
	type = get_type(dict["type"])
	arg = dict["arg"]
	if type == ARG_TYPE.FILE_PATH or type == ARG_TYPE.FOLDER_PATH:
		is_custom = dict["custom"]
		dir = dict["dir"]

func deserialize() -> Dictionary:
	var out: Dictionary = {}
	out["type"] = get_key()
	out["arg"] = arg
	if type == ARG_TYPE.FILE_PATH or type == ARG_TYPE.FOLDER_PATH:
		out["custom"] = is_custom
		out["dir"] = dir
	return out
