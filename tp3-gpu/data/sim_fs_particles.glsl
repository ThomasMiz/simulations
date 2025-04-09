#version 330 core

uniform float containerRadius;

uniform sampler2D constantsSampler;
uniform sampler2D previousPosAndVelSampler;

uniform float deltaTime;

layout (location = 0) out vec4 nextPositionAndVelocity;

float square(float v) {
    return v * v;
}

void main() {
    ivec2 coords = ivec2(gl_FragCoord.xy);
    vec2 cts = texelFetch(constantsSampler, coords, 0).xy;
    vec4 vrs = texelFetch(previousPosAndVelSampler, coords, 0);

    float mass = cts.x;
    float radius = cts.y;

    vec2 position = vrs.xy;
    vec2 velocity = vrs.zw;

    position += velocity * deltaTime;

    float distanceFromOriginSquared = position.x*position.x + position.y*position.y;
    if (distanceFromOriginSquared > square(containerRadius - radius)) {
        velocity = reflect(velocity, normalize(position));
    }

    nextPositionAndVelocity = vec4(position, velocity);
}
