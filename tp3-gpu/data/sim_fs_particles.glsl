#version 330 core

uniform float containerRadius;

uniform sampler2D constantsSampler;

uniform sampler2D posAndVelSampler;
uniform sampler2D timeToCollisionAndCollidesWithSampler;

uniform float deltaTime;

layout (location = 0) out vec4 nextPositionAndVelocity;
layout (location = 1) out vec3 nextTimeToCollisionAndCollidesWith;

float square(float v) {
    return v * v;
}

void main() {
    ivec2 coords = ivec2(gl_FragCoord.xy);
    vec2 rawConstants = texelFetch(constantsSampler, coords, 0).xy;
    vec4 rawPosAndVel = texelFetch(posAndVelSampler, coords, 0);
    vec3 rawTimeToCollisionAndCollidesWith = texelFetch(timeToCollisionAndCollidesWithSampler, coords, 0).xyz;

    float mass = rawConstants.x;
    float radius = rawConstants.y;

    vec2 position = rawPosAndVel.xy;
    vec2 velocity = rawPosAndVel.zw;

    float timeToCollision = rawTimeToCollisionAndCollidesWith.x;
    ivec2 destination = ivec2(rawTimeToCollisionAndCollidesWith.yz);

    position += velocity * deltaTime;

    float distanceFromOriginSquared = position.x*position.x + position.y*position.y;
    if (distanceFromOriginSquared > square(containerRadius - radius)) {
        velocity = reflect(velocity, normalize(position));
    }

    nextPositionAndVelocity = vec4(position, velocity);
    nextTimeToCollisionAndCollidesWith = vec3(timeToCollision - deltaTime, vec2(destination));
}
