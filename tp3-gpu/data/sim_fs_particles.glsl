#version 330 core

uniform sampler2D consts;
uniform sampler2D previous;

uniform float deltaTime;

out vec4 FragColor;

void main() {
    ivec2 coords = ivec2(gl_FragCoord.xy + 0.5);
    vec2 cts = texelFetch(consts, coords, 0).xy;
    vec4 vrs = texelFetch(previous, coords, 0);

    float mass = cts.x;
    float radius = cts.y;

    vec2 position = vrs.xy;
    vec2 velocity = vrs.zw;

    position += velocity * deltaTime;

    // use all the variables so they don't get optimized out xd
    position.x += (mass - radius) * 0.000000001;

    FragColor = vec4(position, velocity);
}
