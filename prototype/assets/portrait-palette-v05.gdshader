shader_type canvas_item;
render_mode unshaded;

// Shared inks. Quantize after sampling the illustration, without dithering.
const vec3 INKS[16] = vec3[16](
    vec3(9.0,20.0,35.0)/255.0, vec3(24.0,45.0,73.0)/255.0,
    vec3(48.0,77.0,112.0)/255.0, vec3(102.0,141.0,175.0)/255.0,
    vec3(169.0,215.0,234.0)/255.0, vec3(232.0,241.0,231.0)/255.0,
    vec3(112.0,72.0,63.0)/255.0, vec3(183.0,121.0,88.0)/255.0,
    vec3(229.0,177.0,128.0)/255.0, vec3(255.0,207.0,80.0)/255.0,
    vec3(216.0,148.0,54.0)/255.0, vec3(220.0,88.0,82.0)/255.0,
    vec3(102.0,117.0,129.0)/255.0, vec3(66.0,126.0,115.0)/255.0,
    vec3(120.0,170.0,132.0)/255.0, vec3(169.0,166.0,161.0)/255.0
);

uniform sampler2D portrait_atlas : filter_linear;
uniform bool continuous_portrait = false;
uniform float gaze_from = 0.0;
uniform float gaze_to = 0.0;
uniform float gaze_blend = 1.0;
uniform float mouth_open = 0.0;

vec4 portrait_pose(vec2 uv, float column) {
    vec4 closed = texture(portrait_atlas, (uv + vec2(column, 0.0)) / vec2(4.0, 2.0));
    vec4 talking = texture(portrait_atlas, (uv + vec2(column, 1.0)) / vec2(4.0, 2.0));
    // Only the lower face speaks: hands, elbows, shoulders and hair stay registered.
    float mouth = smoothstep(0.34, 0.40, uv.x) * (1.0 - smoothstep(0.62, 0.69, uv.x))
        * smoothstep(0.49, 0.54, uv.y) * (1.0 - smoothstep(0.73, 0.79, uv.y));
    return mix(closed, talking, mouth * mouth_open);
}

vec4 sample_portrait(vec2 uv) {
    vec4 body = texture(portrait_atlas, uv / vec2(4.0, 2.0));
    vec4 head = mix(portrait_pose(uv, gaze_from), portrait_pose(uv, gaze_to), gaze_blend);
    float mask = smoothstep(0.25, 0.30, uv.x) * (1.0 - smoothstep(0.72, 0.77, uv.x))
        * (1.0 - smoothstep(0.74, 0.83, uv.y));
    return mix(body, head, mask);
}

void fragment() {
    // Average the footprint before choosing an ink, so tiny source details do not speckle.
    vec2 step_uv = vec2(0.5 / 768.0, 0.5 / 256.0);
    vec4 source = vec4(0.0);
    for (int y = -1; y <= 1; y++) {
        for (int x = -1; x <= 1; x++) {
            vec2 sample_uv = UV + vec2(float(x), float(y)) * step_uv;
            source += continuous_portrait ? sample_portrait(sample_uv) : texture(TEXTURE, sample_uv);
        }
    }
    source /= 9.0;
    vec3 ink = INKS[0];
    float best = 100.0;
    for (int i = 0; i < 16; i++) {
        vec3 difference = source.rgb - INKS[i];
        float distance_squared = dot(difference, difference);
        if (distance_squared < best) {
            best = distance_squared;
            ink = INKS[i];
        }
    }
    COLOR = vec4(ink, source.a);
}
