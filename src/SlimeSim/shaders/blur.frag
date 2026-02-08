#version 330 core

uniform sampler2D inGreen;
uniform sampler2D inBlue;
uniform sampler2D inRed;

uniform vec2 uTexelSize;         // (1.0/width, 1.0/height)
uniform float uKernelRed[25];
uniform float uKernelBlue[25];
uniform float uKernelGreen[25];

layout(location = 0) out vec4 outGreen;
layout(location = 1) out vec4 outBlue;
layout(location = 2) out vec4 outRed;

vec4 blur(sampler2D tex, vec2 uv, float kernel[25])
{
    vec4 sum = vec4(0,0,0,0);
    int k = 0;
    for (int j = -2; j <= 2; j++)
    {
        for (int i = -2; i <= 2; i++)
        {
            float dx = float(i);
            float dy = float(j);
            vec2 pixelOffset = vec2(dx, dy);
            vec2 offset = pixelOffset * uTexelSize;
            vec2 src = uv + offset;

            if (src.x < 0)
                src.x += 1.0;
            if (src.x > 1.0)
                src.x -= 1.0;
            if (src.y < 0)
                src.y += 1.0;
            if (src.y > 1.0)
                src.y -= 1.0;

            vec4 current = texture(tex, src);
            sum += current * kernel[k++];
        }
    }

    vec4 result = sum;
    if (result.r < 0)
      result.r = 0;
    if (result.r > 1.0)
      result.r=1.0;

    return result;
}

void main()
{
    vec2 uv = gl_FragCoord.xy * uTexelSize;
    outGreen = blur(inGreen, uv, uKernelGreen);
    outBlue = blur(inBlue, uv, uKernelBlue);
    outRed = blur(inRed, uv, uKernelRed);
}