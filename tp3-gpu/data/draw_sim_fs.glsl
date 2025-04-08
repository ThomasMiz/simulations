#version 330 core

in vec4 fColor;

out vec4 FragColor;

void main() {
    FragColor = fColor + vec4(1, 0, 0, 1);
}
