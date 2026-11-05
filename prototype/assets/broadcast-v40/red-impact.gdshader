shader_type canvas_item;
render_mode unshaded;
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform float strength = 0.0;
void fragment() {
    vec3 original = texture(screen_texture, SCREEN_UV).rgb;
    float light = dot(original, vec3(0.299, 0.587, 0.114));
    vec3 red = vec3(0.06 + light * 1.15, light * 0.025, light * 0.015);
    COLOR = vec4(mix(original, red, strength * 0.9), 1.0);
}
