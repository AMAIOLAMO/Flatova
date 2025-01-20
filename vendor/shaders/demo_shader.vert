#version 450

vec2 pts[3] = {
    { 0.5, 0.0 },
    {-0.5, 0.0 },
    { 0.5,-0.5 },
};

void main() {
    gl_Position = vec4(pts[gl_VertexIndex], 0.0, 1.0);
}
