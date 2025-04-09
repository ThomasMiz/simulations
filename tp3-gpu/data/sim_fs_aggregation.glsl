#version 330 core

uniform sampler2D timeToCollisionAndCollidesWithSampler;

flat in int pointId;
out float minTimeToCollision;

void main() {
    ivec2 bufferSize = textureSize(timeToCollisionAndCollidesWithSampler, 0);
    ivec2 coords = ivec2(pointId % bufferSize.x, pointId / bufferSize.x);
    vec3 rawTimeToCollisionAndCollidesWith = texelFetch(timeToCollisionAndCollidesWithSampler, coords, 0).xyz;

    minTimeToCollision = rawTimeToCollisionAndCollidesWith.x;
}
