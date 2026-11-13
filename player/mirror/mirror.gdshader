shader_type spatial;
render_mode unshaded, cull_back;

uniform sampler2D reflection_texture : source_color, filter_linear;
uniform vec4 tint : source_color = vec4(0.92, 0.95, 1.0, 1.0);
uniform float reflectivity = 0.9;
uniform float edge_darkness = 0.25;

void fragment() {
    vec2 uv = vec2(1.0 - UV.x, UV.y);
    vec3 col = texture(reflection_texture, uv).rgb * tint.rgb * reflectivity;
    float edge = length((UV - 0.5) * 2.0);
    col *= 1.0 - edge_darkness * smoothstep(0.6, 1.2, edge);
    ALBEDO = col;
}