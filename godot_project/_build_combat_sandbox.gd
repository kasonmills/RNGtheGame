cd C:\Users\Knox\repos\RNGtheGame
claudeextends SceneTree

func _make_label(name: String, text: String) -> Label:
	var l := Label.new()
	l.name = name
	l.text = text
	return l

func _init():
	var root := Control.new()
	root.name = "CombatSandbox"
	root.set_anchors_preset(Control.PRESET_FULL_RECT)

	var margin := MarginContainer.new()
	margin.name = "Margin"
	margin.set_anchors_preset(Control.PRESET_FULL_RECT)
	margin.add_theme_constant_override("margin_left", 24)
	margin.add_theme_constant_override("margin_top", 24)
	margin.add_theme_constant_override("margin_right", 24)
	margin.add_theme_constant_override("margin_bottom", 24)
	root.add_child(margin)
	margin.owner = root

	var layout := VBoxContainer.new()
	layout.name = "Layout"
	layout.add_theme_constant_override("separation", 12)
	margin.add_child(layout)
	layout.owner = root

	layout.add_child(_make_label("TitleLabel", "Combat Sandbox"))
	layout.get_child(-1).owner = root

	# Enemy row (top, facing the party)
	layout.add_child(_make_label("EnemyLabel", "Enemies"))
	layout.get_child(-1).owner = root

	var enemy_row := HFlowContainer.new()
	enemy_row.name = "EnemyRow"
	enemy_row.unique_name_in_owner = true
	enemy_row.add_theme_constant_override("h_separation", 16)
	enemy_row.custom_minimum_size = Vector2(0, 160)
	layout.add_child(enemy_row)
	enemy_row.owner = root

	# Party row (bottom, up to 8 slots)
	layout.add_child(_make_label("PartyLabel", "Your Party"))
	layout.get_child(-1).owner = root

	var party_row := HFlowContainer.new()
	party_row.name = "PartyRow"
	party_row.unique_name_in_owner = true
	party_row.add_theme_constant_override("h_separation", 16)
	party_row.custom_minimum_size = Vector2(0, 160)
	layout.add_child(party_row)
	party_row.owner = root

	# Combat log
	var log := RichTextLabel.new()
	log.name = "CombatLog"
	log.unique_name_in_owner = true
	log.bbcode_enabled = true
	log.scroll_following = true
	log.custom_minimum_size = Vector2(0, 180)
	log.size_flags_vertical = Control.SIZE_EXPAND_FILL
	layout.add_child(log)
	log.owner = root

	# Buttons
	var buttons_row := HBoxContainer.new()
	buttons_row.name = "ButtonsRow"
	buttons_row.add_theme_constant_override("separation", 12)
	layout.add_child(buttons_row)
	buttons_row.owner = root

	var attack_button := Button.new()
	attack_button.name = "AttackButton"
	attack_button.unique_name_in_owner = true
	attack_button.text = "Attack Selected Target"
	buttons_row.add_child(attack_button)
	attack_button.owner = root

	var reset_button := Button.new()
	reset_button.name = "ResetButton"
	reset_button.unique_name_in_owner = true
	reset_button.text = "Reset"
	buttons_row.add_child(reset_button)
	reset_button.owner = root

	var packed := PackedScene.new()
	packed.pack(root)
	var err := ResourceSaver.save(packed, "res://scenes/combat_sandbox.tscn")
	print("Save result: ", err)
	quit()
