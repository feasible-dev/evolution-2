shader_type canvas_item;
render_mode unshaded;

uniform sampler2D message_texture : filter_nearest, repeat_disable;
uniform vec2 panel_size = vec2(300.0, 38.0);
uniform vec2 message_size = vec2(8.0, 12.0);
uniform float pitch = 2.5;
uniform float scroll_columns = 0.0;
uniform float effect_time = 0.0;

void fragment() {
    vec2 position = UV * panel_size;
    vec2 cell = floor(position / pitch);
    // Geometry depends only on the panel, never on time or text scroll.
    vec2 in_cell = fract(position / pitch) - vec2(0.5);
    float radius = length(in_cell);
    float bulb = 1.0 - smoothstep(0.33, 0.43, radius);
    float halo = 1.0 - smoothstep(0.25, 0.49, radius);
    float rows = floor(panel_size.y / pitch);
    float text_y = cell.y - floor((rows - message_size.y) * 0.5);
    float text_x = mod(cell.x + floor(scroll_columns), message_size.x);
    vec4 lamp = vec4(0.0);
    if (text_y >= 0.0 && text_y < message_size.y) {
        lamp = texture(message_texture, (vec2(text_x, text_y) + 0.5) / message_size);
    }
    float phase = mod(effect_time, 8.0);
    float crest = phase / 2.2 * panel_size.x;
    float pulse = phase < 2.2 ? max(0.0, 1.0 - abs(position.x - crest) / 50.0) : 0.0;
    vec3 lit = mix(lamp.rgb, vec3(1.0), pulse * 0.28);
    vec3 inactive = vec3(0.055, 0.063, 0.07) * bulb;
    COLOR = vec4(mix(inactive, lit * (bulb * 0.87 + halo * 0.13), lamp.a), 1.0);
}
