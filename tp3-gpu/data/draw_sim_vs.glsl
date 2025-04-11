#version 330 core

uniform mat4 view;
uniform mat4 projection;

uniform sampler2D constantsSampler;
uniform sampler2D previousPosAndVelSampler;

uniform float timeSinceLastStep;

in vec3 vPosition;
in vec4 vColor;

out vec4 fColor;

void main() {
    ivec2 bufferSize = textureSize(constantsSampler, 0);
    ivec2 coords = ivec2(gl_InstanceID % bufferSize.x, gl_InstanceID / bufferSize.x);
    vec2 cts = texelFetch(constantsSampler, coords, 0).xy;
    vec4 vrs = texelFetch(previousPosAndVelSampler, coords, 0);

    float radius = cts.y;
    vec2 position = vrs.xy;
    vec2 velocity = vrs.zw;

    position += velocity * timeSinceLastStep;

    gl_Position = projection * view * vec4(vec2(vPosition.xy * radius + position), vPosition.z, 1.0);
    fColor = vColor;
}
