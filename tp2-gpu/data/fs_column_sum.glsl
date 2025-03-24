#version 330 core

uniform sampler2D data;

out float FragColor;

void main() {
    ivec2 dataSize = textureSize(data, 0);

    int x = int(gl_FragCoord.x + 0.5);
    float total = 0;
    for (int y = 0; y < dataSize.y; y++) {
        vec4 texel = texelFetch(data, ivec2(x, y), 0);
        total += sign(texel.x - 0.5);
    }

    FragColor = total;
}
