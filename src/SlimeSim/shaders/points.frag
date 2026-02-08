#version 430

layout(location=0) in vec3 vColor;
layout(location=1) flat in int flag;
layout(location=2) flat in float pointSize;
layout(location=3) flat in float angle;
layout(location=4) flat in uint type;

out vec4 outputColor;

vec4 drawDirectionPointer(
    float size,
    float angle,          // radians
    vec2 pointCoord,      // gl_PointCoord
    vec4 color,             // RGBA color of the arrow,
    vec4 outsideColor     // color outside the triangle
) 
{
    // Move to [-1,1] space with origin at center
    vec2 p = pointCoord * 2.0 - 1.0;

    // Rotate by -angle so the arrow points along +X
    float c = cos(-angle);
    float s = sin(-angle);
    mat2 rot = mat2(c, -s, s, c);
    p = rot * p;

    // Triangle parameters (tweak freely)
    float length = size * 0.9;    // arrow length
    float width  = size * 0.4;    // base width

    // Triangle geometry:
    // Apex: ( length, 0 )
    // Base left:  ( -length * 0.5, -width )
    // Base right: ( -length * 0.5,  width )

    // Half-space tests
    bool inside =
        p.x <=  length &&
        p.x >= -length * 0.5 &&
        abs(p.y) <= width * (1.0 - (p.x + length * 0.5) / (length * 1.5));

    if (!inside)
        return outsideColor;

    return color;
}

void main()
{
    if ((vColor.r == 0 && vColor.g == 0 && vColor.b == 0) || flag == 0)
        discard;

    vec2 uv = gl_PointCoord * 2.0 - 1.0; 
    float r = length(uv); 
    float w = fwidth(r);

    outputColor = vec4(0);
    if (flag == 2 || flag == 3) // best performers marked with circle
    {
        // Outer circle alpha (soft discard)
        float alpha = 1.0 - smoothstep(1.0 - w, 1.0 + w, r);

        // White rim near the edge
        float rim = smoothstep(0.92 - w, 0.92 + w, r);

        vec4 baseColor = flag == 1 ? vec4(vColor, 1.0) :  vec4(0);
        vec4 rimColor  = vec4(1.0, 1.0, 1.0, 1.0);
        //vec4 rimColor  = flag == 3 ? vec4(vColor, 1.0) : vec4(1);

        outputColor = mix(baseColor, rimColor, rim);
        outputColor.a *= alpha;
    }

    //vec4 pointerColor = vec4(1.0);
    vec4 pointerColor = type == 1 ? vec4(0.8,0.8,1,1) : vec4(1,0.8,0.8,1);
    float pointerSize = 1;
    if (flag == 2) pointerSize = 0.66;
    if (flag == 3) pointerSize = 0.5;

    outputColor = drawDirectionPointer(pointerSize, angle, gl_PointCoord, pointerColor, outputColor);
}