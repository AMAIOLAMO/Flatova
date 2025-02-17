#version 450

vec2 pts[6] = {
    { -0.5, -0.5 },
    { 0.5,  -0.5 },
    { -0.5, 0.5 },

    { 0.5,  -0.5 },
    { 0.5, 0.5 },
    { -0.5, 0.5 },
};

vec3 colors[6] = {
    vec3(1.0, 0.0, 0.0),
    vec3(1.0, 1.0, 1.0),
    vec3(0.0, 1.0, 0.0),

    vec3(1.0, 1.0, 1.0),
    vec3(0.0, 0.0, 1.0),
    vec3(0.0, 1.0, 0.0)
};

layout (location = 0) out vec3 fragColor;

void main() {
    gl_Position = vec4(pts[gl_VertexIndex], 0.0, 1.0);
    fragColor = colors[gl_VertexIndex];
}
