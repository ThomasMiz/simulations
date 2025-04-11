#version 330 core

uniform float containerRadius;

uniform sampler2D constantsSampler;

uniform sampler2D posAndVelSampler;

layout (location = 0) out vec4 nextPositionAndVelocity;
layout (location = 1) out vec3 nextTimeToCollisionAndCollidesWith;

float square(float v) {
    return v * v;
}

const float InfiniteTime = 999999999999999.9;

float timeToCollisionWithSphere(in vec2 pos0, in float rad0, in vec2 vel0, in vec2 pos1, in float rad1, in vec2 vel1) {
    vec2 deltaR = pos1 - pos0;
    vec2 deltaV = vel1 - vel0;
    float deltaVDotDeltaR = dot(deltaV, deltaR);
    if (deltaVDotDeltaR >= 0) return InfiniteTime;

    float sigma = rad0 + rad1;
    float deltaVDotDeltaV = dot(deltaV, deltaV);
    float d = square(deltaVDotDeltaR) - deltaVDotDeltaV * (dot(deltaR, deltaR) - square(sigma));
    if (d < 0) return InfiniteTime;

    return -(deltaVDotDeltaR + sqrt(d)) / deltaVDotDeltaV;
}

float timeToCollisionWithBorder(in vec2 pos, in float rad, in vec2 vel) {
    float a = dot(vel, vel);
    float b = 2 * dot(pos, vel);
    float c = dot(pos, pos) - square(containerRadius - rad);

    float t = (-b + sqrt(square(b) - 4 * a * c)) / (2 * a);
    return t;
}

float findNextTimeToCollision(in vec2 pos, in float rad, in vec2 vel) {
    return timeToCollisionWithBorder(pos, rad, vel);
}

void main() {
    ivec2 coords = ivec2(gl_FragCoord.xy);
    vec2 rawConstants = texelFetch(constantsSampler, coords, 0).xy;
    vec4 rawPosAndVel = texelFetch(posAndVelSampler, coords, 0);

    float mass = rawConstants.x;
    float radius = rawConstants.y;

    vec2 position = rawPosAndVel.xy;
    vec2 velocity = rawPosAndVel.zw;

    nextPositionAndVelocity = rawPosAndVel;
    nextTimeToCollisionAndCollidesWith = vec3(findNextTimeToCollision(position, radius, velocity), vec2(69, 420));
}
