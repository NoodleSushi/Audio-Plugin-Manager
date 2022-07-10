extends Tree

func _ready() -> void:
	var root = self.create_item()
	self.set_hide_root(true)
	var child1 = self.create_item(root)
	var child2 = self.create_item(root)
	var subchild1 = self.create_item(child1)
	subchild1.set_text(0, "Subchild1")
