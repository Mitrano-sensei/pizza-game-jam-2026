shader_type spatial;

uniform vec3 color : source_color = vec3(1.0, 0.0, 0.0);
uniform float emission_strength = 1.75;
uniform float alpha = 0.63;
// Whole-beam radius breathing.
uniform float pulse_speed = 4.0;
uniform float pulse_amount = 0.05;
// Bright bands travelling from start to end (local +Y).
uniform float wave_frequency = 4.0;
uniform float wave_speed = 8.0;

varying float local_y;

void vertex() {
	local_y = VERTEX.y; // mesh units = meters, since Height resizes the mesh instead of scaling it
	VERTEX.xz *= 1.0 + pulse_amount * sin(TIME * pulse_speed);
}

void fragment() {
	float wave = 0.5 + 0.5 * sin(local_y * wave_frequency - TIME * wave_speed);
	ALBEDO = color;
	EMISSION = color * emission_strength * (0.7 + 0.6 * wave);
	ALPHA = alpha * (0.6 + 0.4 * wave);
}
