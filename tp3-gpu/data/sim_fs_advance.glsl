#version 330 core

uniform sampler2D constantsSampler;

uniform sampler2D posAndVelSampler;
uniform sampler2D timeToCollisionAndCollidesWithSampler;

uniform float deltaTime;

out vec4 nextPositionAndVelocity;

float square(float v) {
    return v * v;
}

void main() {
    ivec2 coords = ivec2(gl_FragCoord.xy);
    vec4 rawPosAndVel = texelFetch(posAndVelSampler, coords, 0);
    vec3 rawTimeToCollisionAndCollidesWith = texelFetch(timeToCollisionAndCollidesWithSampler, coords, 0).xyz;

    vec2 position = rawPosAndVel.xy;
    vec2 velocity = rawPosAndVel.zw;

    position += velocity * deltaTime;

    rawTimeToCollisionAndCollidesWith.x -= deltaTime;
    if (rawTimeToCollisionAndCollidesWith.x <= 0) {
        ivec2 otherCoords = ivec2(rawTimeToCollisionAndCollidesWith.yz);
        if (otherCoords.x < 0) {
            // Bounce against the wall
            velocity = reflect(velocity, normalize(position));
        } else {
            // Bounce against another particle
            vec2 rawConstants = texelFetch(constantsSampler, coords, 0).xy;
            vec2 otherRawConstants = texelFetch(constantsSampler, otherCoords, 0).xy;

            float mass = rawConstants.x;
            float radius = rawConstants.y;
            float otherMass = otherRawConstants.x;
            float otherRadius = otherRawConstants.y;

            vec4 otherRawPosAndVel = texelFetch(posAndVelSampler, coords, 0);
            vec2 otherVelocity = otherRawPosAndVel.zw;
            vec2 otherPosition = otherRawPosAndVel.xy + otherVelocity * deltaTime;

            vec2 deltaR = otherPosition - position;
            vec2 deltaV = otherVelocity - velocity;
            float sigma = radius + otherRadius;
            float j = (2 * mass * otherMass * dot(deltaV, deltaR)) / (sigma * (mass + otherMass));
            vec2 j2 = j * deltaR / sigma; // j_x and j_y

            velocity = velocity + j2 / mass;
        }
    }

    nextPositionAndVelocity = vec4(position, velocity);
}
