// Draws the edges of a cube
// Uses cube specific vertex positions to calculate edges
shader_type spatial;
render_mode unshaded;
varying vec3 model_vertex;
uniform vec3 cube_color:source_color = vec3(1);

void vertex() {
	model_vertex = VERTEX;
}

void fragment() {
	float threshold = 0.99;
	float xy = abs(model_vertex.x) + abs(model_vertex.y);
	float xz = abs(model_vertex.x) + abs(model_vertex.z);
	float yz = abs(model_vertex.y) + abs(model_vertex.z);
	if(xy > threshold ||xz > threshold ||yz > threshold ){
		ALBEDO = vec3(0);
	}else{
		ALBEDO = cube_color;
	}
}
