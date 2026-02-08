#version 330 core

in vec2 uv;

uniform sampler2D uGreenImage;
uniform sampler2D uBlueImage;
uniform sampler2D uRedImage;

out vec4 fragColor;

float amplify(float x, int pow)
{
    float a = 1;
    for(int i=0; i<pow; i++)
        a = a * (1-x);

    return 1-a;
}

void main()
{
    float green = texture(uGreenImage, uv).r;
    float blue = texture(uBlueImage, uv).r;
    float red = texture(uRedImage, uv).r;
    
    float r = amplify(red, 3);
    float g = amplify(green, 1);
    float b = amplify(blue, 3);
    
    fragColor = vec4(r, g, b ,1);
}