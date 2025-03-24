#version 330 core

uniform sampler2D previous;
uniform vec2 pixelDelta;

uniform float p;
uniform float time;

in vec2 fTexCoords;

out vec4 FragColor;

vec2 rand2d(vec2 c) {
	return fract(sin(vec2(
		dot(c + vec2(0, time), vec2(52.9258, 76.3911)),
		dot(c + vec2(time, 0), vec2(66.7943, 33.1674))
	)) * vec2(49164.7641, 69761.6413));
}

void main() {
    vec4 prev = texture(previous, fTexCoords);

    vec4 sum = prev;
    //sum += texture(previous, fTexCoords + vec2(-pixelDelta.x, -pixelDelta.y));
    sum += texture(previous, fTexCoords + vec2(0, -pixelDelta.y));
    //sum += texture(previous, fTexCoords + vec2(pixelDelta.x, -pixelDelta.y));
    sum += texture(previous, fTexCoords + vec2(-pixelDelta.x, 0));
    sum += texture(previous, fTexCoords + vec2(pixelDelta.x, 0));
    //sum += texture(previous, fTexCoords + vec2(-pixelDelta.x, pixelDelta.y));
    sum += texture(previous, fTexCoords + vec2(0, pixelDelta.y));
    //sum += texture(previous, fTexCoords + vec2(pixelDelta.x, pixelDelta.y));

    float majorityOpinion = sign(sum.x - 2.5);

    float randy = rand2d(fTexCoords).x;
    float newOpinion = majorityOpinion * sign(randy - p);

    vec3 next = vec3(step(0.0, newOpinion));
    FragColor = vec4(next, 1.0);
}