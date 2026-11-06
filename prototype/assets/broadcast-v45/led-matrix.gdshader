shader_type canvas_item;
render_mode unshaded;

uniform sampler2D message_texture : filter_nearest, repeat_disable;
uniform vec2 panel_size = vec2(300.0, 38.0);
uniform vec2 message_size = vec2(8.0, 12.0);
uniform float pitch = 2.5;
uniform float scroll_columns = 0.0;
uniform float effect_time = 0.0;
uniform float glyph_scale = 1.0;
uniform float bold_amount = 0.0;
uniform bool centered = false;
uniform float red_amount = 0.0;
uniform bool repeat_message = true;
uniform float text_visibility = 1.0;
uniform float burst_progress = -1.0;
uniform bool burst_red = false;

vec4 read_lamp(vec2 p) {
    if (p.x < 0.0 || p.x >= message_size.x || p.y < 0.0 || p.y >= message_size.y) return vec4(0.0);
    return texture(message_texture, (floor(p) + 0.5) / message_size);
}

void fragment() {
    vec2 position = UV * panel_size;
    vec2 cell = floor(position / pitch);
    // Geometry depends only on the panel, never on time or text scroll.
    vec2 in_cell = fract(position / pitch) - vec2(0.5);
    float radius = length(in_cell);
    float bulb = 1.0 - smoothstep(0.33, 0.43, radius);
    float halo = 1.0 - smoothstep(0.25, 0.49, radius);
    float rows = floor(panel_size.y / pitch);
    float text_y = (cell.y - rows * 0.5) / glyph_scale + message_size.y * 0.5;
    float text_x = centered ? (cell.x - floor(panel_size.x / pitch) * 0.5) / glyph_scale + message_size.x * 0.5
        : cell.x + floor(scroll_columns);
    if (repeat_message) text_x = mod(text_x, message_size.x);
    vec4 lamp = read_lamp(vec2(text_x, text_y));
    if (bold_amount > 0.5 && lamp.a < 0.5) lamp = read_lamp(vec2(text_x - 1.0, text_y));
    lamp.a *= text_visibility;
    // Three little non-letter bursts are drawn by switching existing lamps.
    // Their rays grow across cells; bulb positions and diameters never change.
    if (burst_progress >= 0.0) {
        float reach = 1.0 + burst_progress * rows * 0.62;
        for (int i = 0; i < 3; i++) {
            vec2 origin = vec2(floor(panel_size.x / pitch * (0.2 + float(i) * 0.3)), floor(rows * 0.5));
            vec2 d = abs(cell - origin);
            float ring = max(d.x, d.y);
            bool ray = d.x < 0.6 || d.y < 0.6 || abs(d.x - d.y) < 0.6;
            if (ray && ring <= reach && ring > max(0.0, reach - 3.0)) {
                lamp = vec4(burst_red ? vec3(0.961,0.157,0.141)
                    : i == 1 ? vec3(0.212,0.616,1.0) : vec3(1.0,0.580,0.122), 1.0);
            }
        }
    }
    float phase = mod(effect_time, 8.0);
    float crest = phase / 2.2 * panel_size.x;
    float pulse = phase < 2.2 ? max(0.0, 1.0 - abs(position.x - crest) / 50.0) : 0.0;
    vec3 lit = min(lamp.rgb * (1.0 + pulse * 0.16), vec3(1.0));
    vec3 inactive = mix(vec3(0.055, 0.063, 0.07), vec3(0.075, 0.004, 0.002), red_amount) * bulb;
    COLOR = vec4(mix(inactive, lit * (bulb * 0.87 + halo * 0.13), lamp.a), 1.0);
}
