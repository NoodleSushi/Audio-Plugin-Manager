extends AcceptDialog

const ExportArg: GDScript = preload("ExportArg.gd")
const ExporterData: GDScript = preload("ExporterData.gd")
var ExportArgIns: ExportArg = ExportArg.new()

onready var exporter_options: OptionButton = $"%ExporterOptions"
onready var exporter_input: LineEdit = $"%ExporterInput"
onready var path_input: LineEdit = $"%PathInput"
onready var arg_options: OptionButton = $"%ArgOptions"
onready var args_editor: VBoxContainer = $"%ArgsEditor"
onready var editor: Control = $"%Editor"

var exporter_list: Array = []
var exporter_data: ExporterData = null
var selected_arg: ExportArg = null

func get_export_args() -> Array:
	if not exporter_data is ExporterData:
		return []
	return exporter_data.export_args

func _ready() -> void:
	exporter_options.connect("item_selected", self, "_on_exporter_options_item_selected")
	$"%DeleteExporterButton".connect("pressed", self, "_on_DeleteExporterButton_pressed")
	$"%AddExporterButton".connect("pressed", self, "_on_AddExporterButton_pressed")
	exporter_input.connect("text_changed", self, "_on_exporter_input_text_changed")
	path_input.connect("text_changed", self, "_on_path_input_text_changed")
	for type in ExportArg.new().ARG_TYPE.values():
		arg_options.add_item(ExportArgIns.get_name(type), type)
	$"%AddArgButton".connect("pressed", self, "_on_AddArgButton_pressed")
	$"%DeleteArgButton".connect("pressed", self, "_on_DeleteArgButton_pressed")
	$"%UpButton".connect("pressed", self, "_on_MoveButton_pressed", [-1])
	$"%DownButton".connect("pressed", self, "_on_MoveButton_pressed", [1])
	connect("confirmed", self, "_on_confirmed")
	serialize(ConfigServer.Exporters)
	update_exporter_options()
	update_editor()

func update_exporter_options() -> void:
	var selected: int = exporter_options.selected
	exporter_options.clear()
	exporter_options.add_item("--SELECT EXPORTER--", -2)
	var id: int = 0
	for _exporter in exporter_list:
		var exporter: ExporterData = _exporter
		exporter_options.add_item(exporter.name, id)
		id += 1
	exporter_options.selected = 0 if selected == -1 else selected

func update_editor() -> void:
	if not exporter_data is ExporterData:
		editor.hide()
		return
	editor.show()
	exporter_input.text = exporter_data.name
	path_input.text = exporter_data.exe_path
	update_tree()

func update_tree() -> void:
	var export_args: Array = get_export_args()
	for child in args_editor.get_children():
		child.queue_free()
	if not exporter_data is ExporterData:
		return
	for _arg in export_args:
		var arg: ExportArg = _arg
		args_editor.add_child(arg.generate_field())
		_arg.connect("selected", self, "_on_selected_arg_selected", [_arg], CONNECT_REFERENCE_COUNTED)
	if selected_arg in export_args:
		selected_arg.select()

func _on_selected_arg_selected(new_selected_arg: ExportArg) -> void:
	selected_arg = new_selected_arg
	for _arg in get_export_args():
		var arg: ExportArg = _arg
		if arg != new_selected_arg:
			arg.deselect()

func _on_exporter_options_item_selected(idx: int) -> void:
	var id: int = exporter_options.get_item_id(idx)
	selected_arg = null
	exporter_data = null if id == -2 else exporter_list[id]
	update_editor()

func _on_DeleteExporterButton_pressed() -> void:
	var idx: int = exporter_list.find(exporter_data)
	if idx < 0 or idx > exporter_list.size()-1:
		return
	exporter_list.erase(exporter_data)
	if exporter_list.size() > 0:
		exporter_data = exporter_list[min(idx, exporter_list.size()-1)]
	else:
		exporter_data = null
	update_exporter_options()
	update_editor()

func _on_AddExporterButton_pressed() -> void:
	selected_arg = null
	var new_exporter_data: ExporterData = ExporterData.new()
	new_exporter_data.name = "New Exporter"
	exporter_list.append(new_exporter_data)
	exporter_data = new_exporter_data
	update_exporter_options()
	exporter_options.selected = exporter_options.get_item_index(exporter_list.size()-1)
	update_editor()

func _on_exporter_input_text_changed(new_text: String) -> void:
	if not exporter_data is ExporterData:
		return
	exporter_data.name = new_text
	update_exporter_options()

func _on_path_input_text_changed(new_text: String) -> void:
	if not exporter_data is ExporterData:
		return
	exporter_data.exe_path = new_text

func _on_AddArgButton_pressed() -> void:
	var new_arg = ExportArg.new()
	new_arg.type = arg_options.get_item_id(arg_options.selected)
	get_export_args().append(new_arg)
	update_tree()

func _on_DeleteArgButton_pressed() -> void:
	var export_args: Array = get_export_args()
	if selected_arg in export_args:
		export_args.erase(selected_arg)
		selected_arg = null
		update_tree()

func _on_MoveButton_pressed(offset: int) -> void:
	var export_args: Array = get_export_args()
	var idx: int = export_args.find(selected_arg)
	if idx+offset < 0 or idx+offset > export_args.size()-1:
		return
	var temp = export_args[idx]
	export_args[idx] = export_args[idx+offset]
	export_args[idx+offset] = temp
	update_tree()

func _on_confirmed() -> void:
	ConfigServer.Exporters = deserialize()

func deserialize() -> Array:
	var out: Array = []
	for _exporter in exporter_list:
		var exporter: ExporterData = _exporter
		out.append(exporter.deserialize())
	return out

func serialize(arr: Array) -> void:
	exporter_list.clear()
	for item in arr:
		if not item is Dictionary:
			continue
		var exporter: ExporterData = ExporterData.new()
		exporter.serialize(item)
		exporter_list.append(exporter)
	exporter_options.selected = 0
	update_exporter_options()
	update_editor()
