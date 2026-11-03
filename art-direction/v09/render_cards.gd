extends SceneTree

class Sheet:
	extends Control
	var pictures: Array[Texture2D] = []
	var labels: Array[String] = []
	var title_font := SystemFont.new()
	var body_font := SystemFont.new()
	var ui_font := SystemFont.new()

	func configure(folder: String) -> void:
		title_font.font_names = PackedStringArray(["Baskerville Old Face"])
		body_font.font_names = PackedStringArray(["Book Antiqua"])
		ui_font.font_names = PackedStringArray(["Tahoma"])
		var models = JSON.parse_string(FileAccess.get_file_as_string(folder + "/modeles.json"))
		for model in models:
			var picture := Image.new()
			var error = picture.load_svg_from_string(FileAccess.get_file_as_string(folder + "/images/" + model.file), 2.0)
			if error != OK:
				push_error("SVG: " + model.file)
			pictures.append(ImageTexture.create_from_image(picture))
			labels.append(model.label)

	func _draw() -> void:
		draw_rect(Rect2(0, 0, 1440, 650), Color("#070e19"))
		draw_string(title_font, Vector2(40, 52), "Cible", HORIZONTAL_ALIGNMENT_LEFT, -1, 36, Color("#e5eadf"))
		draw_string(ui_font, Vector2(40, 82), "Carton sobre. Image nette. Slider moderne.", HORIZONTAL_ALIGNMENT_LEFT, -1, 15, Color("#90a5ba"))
		for i in pictures.size():
			var position := Vector2(40 + i * 350, 133)
			draw_string(ui_font, position + Vector2(0, -15), labels[i], HORIZONTAL_ALIGNMENT_LEFT, -1, 14, Color("#a8b8c6"))
			draw_texture_rect(pictures[i], Rect2(position, Vector2(300, 420)), false)
			draw_string(title_font, position + Vector2(22, 43), "Cible", HORIZONTAL_ALIGNMENT_LEFT, -1, 33, Color("#ece7d6"))
			draw_string(body_font, position + Vector2(23, 333), "Accélère vers sa cible.", HORIZONTAL_ALIGNMENT_LEFT, -1, 20, Color("#142a3b"))
			draw_string(ui_font, position + Vector2(24, 389), "Poids", HORIZONTAL_ALIGNMENT_LEFT, -1, 12, Color("#a7bcc9"))

func _initialize() -> void:
	call_deferred("_render_sheet")

func _render_sheet() -> void:
	root.size = Vector2i(1440, 650)
	root.content_scale_size = Vector2i(1440, 650)
	root.title = "Evolution — Cartes / v09"
	var sheet := Sheet.new()
	sheet.configure("C:/dev/projects/evolution-2/art-direction/v09")
	root.add_child(sheet)
	await RenderingServer.frame_post_draw
	await RenderingServer.frame_post_draw
	var picture := root.get_texture().get_image()
	print("CARD_SHEET:" + Marshalls.raw_to_base64(picture.save_png_to_buffer()))
	print("CARD_RENDER_OK: 4 vector cards")
	quit()
