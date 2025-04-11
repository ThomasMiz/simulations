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

const float InfiniteTime = 999999999999999.9;

float timeToCollisionWithOther(in vec2 pos0, in float rad0, in vec2 vel0, in vec2 pos1, in float rad1, in vec2 vel1) {
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

vec3 findNextTimeToCollision(in ivec2 myCoords, in vec2 pos, in float rad, in vec2 vel) {
    float minTime = timeToCollisionWithBorder(pos, rad, vel);
    ivec2 minCoords = ivec2(-1.0, -1.0);

    ivec2 simSize = textureSize(posAndVelSampler, 0);
    for (int x = 0; x < simSize.x; x++) {
        for (int y = 0; y < simSize.y; y++) {
            ivec2 coords = ivec2(x, y);
            if (coords == myCoords) continue;
            vec4 otherRawPosAndVel = texelFetch(posAndVelSampler, coords, 0);
            vec2 otherRawConstants = texelFetch(constantsSampler, coords, 0).xy;
            vec2 otherPosition = otherRawPosAndVel.xy;
            vec2 otherVelocity = otherRawPosAndVel.zw;
            float otherRadius = otherRawConstants.y;
            float t = timeToCollisionWithOther(pos, rad, vel, otherPosition, otherRadius, otherVelocity);

            if (t > 0 && t < minTime) {
                minTime = t;
                minCoords = coords;
            }
        }
    }

    return vec3(minTime, vec2(minCoords));
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

    position += velocity * deltaTime;

    rawTimeToCollisionAndCollidesWith.x -= deltaTime;
    if (rawTimeToCollisionAndCollidesWith.x <= 0) {
        ivec2 otherParticleCoords = ivec2(rawTimeToCollisionAndCollidesWith.yz);
        if (otherParticleCoords.x < 0) {
            // Bounce against the wall
            velocity = reflect(velocity, normalize(position));
        } else {
            // Bounce against another particle
            vec4 otherRawPosAndVel = texelFetch(posAndVelSampler, coords, 0);
            vec2 otherPosition = otherRawPosAndVel.xy;
            velocity = reflect(velocity, normalize(position - otherPosition));
        }
    }

    rawTimeToCollisionAndCollidesWith = findNextTimeToCollision(coords, position, radius, velocity);

    nextPositionAndVelocity = vec4(position, velocity);
    nextTimeToCollisionAndCollidesWith = rawTimeToCollisionAndCollidesWith;
}
