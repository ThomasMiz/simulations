#version 330 core

uniform mat4 view;
uniform mat4 projection;

uniform sampler2D consts;
uniform sampler2D particles;

in vec3 vPosition;
in vec4 vColor;

out vec4 fColor;

void main() {
    ivec2 bufferSize = textureSize(consts, 0);
    ivec2 coords = ivec2(gl_InstanceID % bufferSize.x, gl_InstanceID / bufferSize.x);
    vec2 cts = texelFetch(consts, coords, 0).xy;
    vec4 vrs = texelFetch(particles, coords, 0);

    float radius = cts.y;
    vec2 position = vrs.xy;

    gl_Position = projection * view * vec4(vec2(vPosition.xy * radius + position), vPosition.z, 1.0);
    fColor = vColor;
}
