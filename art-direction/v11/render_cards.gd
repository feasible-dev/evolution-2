extends SceneTree

class CardView:
	extends Control
	var face: Texture2D
	var reticle: Texture2D
	var reticle_name := ""
	var card_title := ""
	var front_text := ""
	var back_heading := ""
	var back_lines: Array = []
	var back_summary := ""
	var flipped := false
	var slider: HSlider
	var flip_button: Button
	var art_button: Button
	var title_font := SystemFont.new()
	var body_font := SystemFont.new()
	var ui_font := SystemFont.new()

	func configure(frame: Texture2D, symbol: Texture2D, label: String, spec: Dictionary) -> void:
		size = Vector2(300, 420)
		face = frame
		reticle = symbol
		reticle_name = label
		card_title = spec.title
		front_text = spec.front
		back_heading = spec.backHeading
		back_lines = spec.backLines
		back_summary = spec.backSummary
		title_font.font_names = PackedStringArray(["Baskerville Old Face"])
		body_font.font_names = PackedStringArray(["Book Antiqua"])
		ui_font.font_names = PackedStringArray(["Tahoma"])
		art_button = Button.new()
		art_button.flat = true
		art_button.position = Vector2(16, 72)
		art_button.size = Vector2(268, 214)
		art_button.tooltip_text = "Retourner la carte"
		art_button.pressed.connect(flip)
		add_child(art_button)
		flip_button = Button.new()
		flip_button.flat = true
		flip_button.text = "Verso ↪"
		flip_button.position = Vector2(207, 352)
		flip_button.size = Vector2(70, 21)
		flip_button.add_theme_font_override("font", ui_font)
		flip_button.add_theme_font_size_override("font_size", 10)
		flip_button.add_theme_color_override("font_color", Color("#adc5d0"))
		flip_button.pressed.connect(flip)
		add_child(flip_button)
		slider = HSlider.new()
		slider.position = Vector2(100, 374)
		slider.size = Vector2(164, 20)
		slider.min_value = 0
		slider.max_value = 10
		slider.step = 1
		slider.value = 4
		slider.tooltip_text = "Poids"
		var track := StyleBoxFlat.new()
		track.bg_color = Color("#38546b")
		track.content_margin_top = 2
		track.content_margin_bottom = 2
		track.set_corner_radius_all(2)
		var fill := track.duplicate()
		fill.bg_color = Color("#72d3e8")
		slider.add_theme_stylebox_override("slider", track)
		slider.add_theme_stylebox_override("grabber_area", fill)
		slider.add_theme_stylebox_override("grabber_area_highlight", fill)
		var knob_image := Image.new()
		knob_image.load_svg_from_string('<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16"><circle cx="8" cy="8" r="6" fill="#edf8f4" stroke="#72d3e8" stroke-width="2"/></svg>')
		var knob := ImageTexture.create_from_image(knob_image)
		slider.add_theme_icon_override("grabber", knob)
		slider.add_theme_icon_override("grabber_highlight", knob)
		add_child(slider)

	func flip() -> void:
		flipped = not flipped
		art_button.visible = not flipped
		flip_button.text = "↩ Recto" if flipped else "Verso ↪"
		queue_redraw()

	func _draw() -> void:
		draw_texture_rect(face, Rect2(0, 0, 300, 420), false)
		var title_width := title_font.get_string_size(card_title, HORIZONTAL_ALIGNMENT_LEFT, -1, 33).x
		draw_string(title_font, Vector2((300 - title_width) * 0.5, 43), card_title, HORIZONTAL_ALIGNMENT_LEFT, -1, 33, Color("#ece7d6"))
		if not flipped:
			draw_texture_rect(reticle, Rect2(50, 79, 200, 200), false)
			draw_string(body_font, Vector2(23, 333), front_text, HORIZONTAL_ALIGNMENT_LEFT, -1, 20, Color("#142a3b"))
		else:
			draw_texture_rect(reticle, Rect2(29, 84, 42, 42), false)
			draw_string(body_font, Vector2(29, 157), back_heading, HORIZONTAL_ALIGNMENT_LEFT, -1, 21, Color("#e8e0ca"))
			for line_index in back_lines.size():
				draw_string(body_font, Vector2(29, 190 + line_index * 26), back_lines[line_index], HORIZONTAL_ALIGNMENT_LEFT, -1, 18, Color("#d2e0e4"))
			draw_string(ui_font, Vector2(29, 253), reticle_name, HORIZONTAL_ALIGNMENT_LEFT, -1, 10, Color("#8aa4b9"))
			draw_string(body_font, Vector2(23, 333), back_summary, HORIZONTAL_ALIGNMENT_LEFT, -1, 19, Color("#142a3b"))
		draw_string(ui_font, Vector2(40, 389), "Poids", HORIZONTAL_ALIGNMENT_LEFT, -1, 12, Color("#a7bcc9"))

class Sheet:
	extends Control
	var cards: Array[CardView] = []
	var labels: Array[String] = []
	var codes: Array[String] = []
	var title_font := SystemFont.new()
	var ui_font := SystemFont.new()

	func texture_from_svg(file: String) -> Texture2D:
		var picture := Image.new()
		var error := picture.load_svg_from_string(FileAccess.get_file_as_string(file), 2.0)
		assert(error == OK, "SVG: " + file)
		return ImageTexture.create_from_image(picture)

	func build(folder: String) -> void:
		title_font.font_names = PackedStringArray(["Baskerville Old Face"])
		ui_font.font_names = PackedStringArray(["Tahoma"])
		var frame := texture_from_svg(folder + "/cadre.svg")
		var models = JSON.parse_string(FileAccess.get_file_as_string(folder + "/modeles.json"))
		for i in models.size():
			var model = models[i]
			var card := CardView.new()
			card.configure(frame, texture_from_svg(folder + "/illustrations/" + model.id + ".svg"), model.name, model)
			card.position = Vector2(40 + (i % 4) * 350, 133 + int(i / 4) * 480)
			add_child(card)
			cards.append(card)
			labels.append(model.code + " / " + model.name)
			codes.append(model.code.to_lower())
		queue_redraw()

	func _draw() -> void:
		draw_rect(Rect2(0, 0, 1440, 1080), Color("#070e19"))
		draw_string(title_font, Vector2(40, 52), "Poussée & Virage.", HORIZONTAL_ALIGNMENT_LEFT, -1, 34, Color("#e5eadf"))
		draw_string(ui_font, Vector2(40, 82), "Quatre illustrations par effet. Cadre de la carte Cible 06.", HORIZONTAL_ALIGNMENT_LEFT, -1, 14, Color("#9aafbe"))
		for i in cards.size():
			draw_string(ui_font, cards[i].position + Vector2(0, -15), labels[i], HORIZONTAL_ALIGNMENT_LEFT, -1, 14, Color("#a8b8c6"))

func _initialize() -> void:
	call_deferred("_build")

func _build() -> void:
	root.size = Vector2i(1440, 1080)
	root.content_scale_size = Vector2i(1440, 1080)
	root.title = "Évolution — Poussée et Virage / v11"
	var sheet := Sheet.new()
	root.add_child(sheet)
	sheet.build("C:/dev/projects/evolution-2/art-direction/v11")
	if not "--capture" in OS.get_cmdline_user_args():
		return
	await RenderingServer.frame_post_draw
	await RenderingServer.frame_post_draw
	# Exercise the actual flip buttons, retaining the same HSlider instance and value.
	for i in sheet.cards.size():
		var card = sheet.cards[i]
		card.slider.value = 7
		card.flip_button.pressed.emit()
		assert(card.flipped and card.slider.value == 7)
		card.flip_button.pressed.emit()
		assert(not card.flipped and card.slider.value == 7)
		card.slider.value = 4
	await RenderingServer.frame_post_draw
	await RenderingServer.frame_post_draw
	var image := root.get_texture().get_image()
	print("CARD_CAPTURE:01-poussee-et-virage.png:" + Marshalls.raw_to_base64(image.save_png_to_buffer()))
	var row_one := image.get_region(Rect2i(0, 105, 1440, 465))
	print("CARD_CAPTURE:02-poussee.png:" + Marshalls.raw_to_base64(row_one.save_png_to_buffer()))
	var row_two := image.get_region(Rect2i(0, 585, 1440, 465))
	print("CARD_CAPTURE:03-virage.png:" + Marshalls.raw_to_base64(row_two.save_png_to_buffer()))
	for i in sheet.cards.size():
		var one := image.get_region(Rect2i(Vector2i(sheet.cards[i].position), Vector2i(300, 420)))
		print("CARD_CAPTURE:" + sheet.codes[i] + ".png:" + Marshalls.raw_to_base64(one.save_png_to_buffer()))
	sheet.cards[0].slider.value = 7
	sheet.cards[0].flip_button.pressed.emit()
	await RenderingServer.frame_post_draw
	await RenderingServer.frame_post_draw
	var back := root.get_texture().get_image().get_region(Rect2i(40, 133, 300, 420))
	print("CARD_CAPTURE:04-verso-poussee.png:" + Marshalls.raw_to_base64(back.save_png_to_buffer()))
	sheet.cards[4].slider.value = 7
	sheet.cards[4].flip_button.pressed.emit()
	await RenderingServer.frame_post_draw
	await RenderingServer.frame_post_draw
	var second_back := root.get_texture().get_image().get_region(Rect2i(40, 613, 300, 420))
	print("CARD_CAPTURE:05-verso-virage.png:" + Marshalls.raw_to_base64(second_back.save_png_to_buffer()))
	print("CARD_REVIEW_OK: 8 illustrations, 16 button flips, slider values retained")
	quit()
