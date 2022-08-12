extends Resource

const ExportArg: GDScript = preload("ExportArg.gd")

var name: String = ""
var exe_path: String = ""
var export_args: Array = []

func serialize(dict: Dictionary) -> void:
	name = dict.get("name", "")
	exe_path = dict.get("exe_path", "")
	var args = dict.get("args", [])
	if args is Array:
		for arg in args:
			if not arg is Dictionary:
				continue
			var new_export_arg = ExportArg.new()
			new_export_arg.serialize(arg)
			export_args.append(new_export_arg)

func deserialize() -> Dictionary:
	var out: Dictionary = {}
	out["name"] = name
	out["exe_path"] = exe_path
	var export_args_deserialized: Array = []
	for _export_arg in export_args:
		var export_arg: ExportArg = _export_arg
		export_args_deserialized.append(export_arg.deserialize())
	out["args"] = export_args_deserialized
	return out
