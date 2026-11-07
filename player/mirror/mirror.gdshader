shader_type spatial;
render_mode unshaded, cull_back;

uniform sampler2D reflection_texture : source_color, filter_linear;
uniform vec4 tint : source_color = vec4(1.0);

void fragment() {
    vec2 uv = vec2(1.0 - UV.x, UV.y);
    ALBEDO = texture(reflection_texture, uv).rgb * tint.rgb;
}