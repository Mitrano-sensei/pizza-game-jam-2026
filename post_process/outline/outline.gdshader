// Full-screen toon outline: edge detection on scene depth and normals.
// Requires Forward+ (normal buffer) and Godot 4.3+ (reversed-Z depth).
shader_type spatial;
// unshaded: no lighting on the quad. fog_disabled: fog must not tint the lines.
// depth_test_disabled: nothing may hide the quad. cull_disabled: POSITION is
// overridden below, so the quad's facing is meaningless.
render_mode unshaded, fog_disabled, depth_test_disabled, cull_disabled;

uniform bool enabled = true;
uniform vec4 outline_color : source_color = vec4(0.0, 0.0, 0.0, 1.0);
// Sampling distance in pixels, which is also the line width.
uniform float thickness : hint_range(0.5, 5.0, 0.1) = 1.0;
// Relative depth jump that counts as an edge (0.05 = 5 % of the pixel's distance).
uniform float depth_threshold : hint_range(0.001, 1.0, 0.001) = 0.05;
// 1 - cos(angle between normals): 0.3 is roughly 45 degrees.
uniform float normal_threshold : hint_range(0.01, 2.0, 0.01) = 0.3;
// Outlines fade out between these two distances (meters).
uniform float fade_start : hint_range(0.0, 500.0, 0.5) = 30.0;
uniform float fade_end : hint_range(0.0, 500.0, 0.5) = 80.0;
// 8 samples instead of 4: smoother diagonals, twice the texture reads.
uniform bool use_sobel = false;

// filter_nearest: blending depth or normals between two surfaces gives values
// that belong to neither.
uniform sampler2D depth_texture : hint_depth_texture, filter_nearest;
uniform sampler2D normal_roughness_texture : hint_normal_roughness_texture, filter_nearest;

const vec2 CROSS[4] = vec2[4](vec2(0.0, -1.0), vec2(-1.0, 0.0), vec2(1.0, 0.0), vec2(0.0, 1.0));
const vec2 RING[8] = vec2[8](
	vec2(-1.0, -1.0), vec2(0.0, -1.0), vec2(1.0, -1.0),
	vec2(-1.0, 0.0), vec2(1.0, 0.0),
	vec2(-1.0, 1.0), vec2(0.0, 1.0), vec2(1.0, 1.0));
const float SOBEL_X[8] = float[8](-1.0, 0.0, 1.0, -2.0, 2.0, -1.0, 0.0, 1.0);
const float SOBEL_Y[8] = float[8](-1.0, -2.0, -1.0, 0.0, 0.0, 1.0, 2.0, 1.0);

// Distance from the camera in meters. The depth buffer is non-linear, so it is
// unprojected back to view space. INV_PROJECTION_MATRIX only exists inside
// fragment(), hence the parameter.
float linear_depth(vec2 uv, mat4 inv_projection) {
	float raw = texture(depth_texture, uv).r;
	vec4 view = inv_projection * vec4(uv * 2.0 - 1.0, raw, 1.0);
	return -view.z / view.w;
}

vec3 view_normal(vec2 uv) {
	return texture(normal_roughness_texture, uv).xyz * 2.0 - 1.0;
}

void vertex() {
	// Skip the camera transform: the 2x2 quad maps straight onto clip space.
	POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}

void fragment() {
	float edge = 0.0;
	// Reversed-Z: the sky sits at raw depth 0, and it gets no outline. Silhouettes
	// against the sky are still drawn, from the object's side.
	if (enabled && texture(depth_texture, SCREEN_UV).r > 0.00001) {
		vec2 step_uv = thickness / VIEWPORT_SIZE;
		float depth = linear_depth(SCREEN_UV, INV_PROJECTION_MATRIX);
		float depth_edge = 0.0;
		float normal_edge = 0.0;

		if (use_sobel) {
			vec2 depth_gradient = vec2(0.0);
			vec3 normal_gradient_x = vec3(0.0);
			vec3 normal_gradient_y = vec3(0.0);
			for (int i = 0; i < 8; i++) {
				vec2 uv = SCREEN_UV + RING[i] * step_uv;
				float d = linear_depth(uv, INV_PROJECTION_MATRIX);
				vec3 n = view_normal(uv);
				depth_gradient += vec2(SOBEL_X[i], SOBEL_Y[i]) * d;
				normal_gradient_x += SOBEL_X[i] * n;
				normal_gradient_y += SOBEL_Y[i] * n;
			}
			// Scaled so a clean step edge gives the same value as the cross
			// version, which lets both modes share the thresholds.
			depth_edge = length(depth_gradient) / (4.0 * depth);
			normal_edge = (dot(normal_gradient_x, normal_gradient_x)
					+ dot(normal_gradient_y, normal_gradient_y)) / 32.0;
		} else {
			vec3 normal = view_normal(SCREEN_UV);
			for (int i = 0; i < 4; i++) {
				vec2 uv = SCREEN_UV + CROSS[i] * step_uv;
				// Relative difference: a 10 cm gap matters at 1 m, not at 50 m.
				depth_edge = max(depth_edge,
						abs(linear_depth(uv, INV_PROJECTION_MATRIX) - depth) / depth);
				normal_edge = max(normal_edge, 1.0 - dot(normal, view_normal(uv)));
			}
		}

		edge = max(step(depth_threshold, depth_edge), step(normal_threshold, normal_edge));
		// Distant foliage is all edges at pixel scale: fade lines out with distance.
		edge *= 1.0 - smoothstep(fade_start, fade_end, depth);
	}

	// Alpha-blend the line over the frame instead of copying the screen texture
	// back: transparent objects (laser beams, smoke) are not in that texture and
	// would be erased.
	ALBEDO = outline_color.rgb;
	ALPHA = edge * outline_color.a;
}
