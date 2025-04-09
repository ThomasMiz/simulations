#version 330 core

flat out int pointId;

void main() {
    pointId = gl_VertexID;
    gl_Position = vec4(0.0, 0.0, 0.0, 1.0);
}
