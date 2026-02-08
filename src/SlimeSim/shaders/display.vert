#version 430

out vec2 uv;

uniform vec2 texSize;
uniform mat4 projection;
uniform vec2 worldMin;
uniform vec2 worldMax;

void main()
{
    vec2 verts[6] = vec2[](
        vec2(worldMin.x, worldMin.y),
        vec2(worldMax.x, worldMin.y),
        vec2(worldMax.x, worldMax.y),

        vec2(worldMin.x, worldMin.y),
        vec2(worldMax.x, worldMax.y),
        vec2(worldMin.x, worldMax.y)
    );

    vec2 p = verts[gl_VertexID];

    uv = p / texSize;          // UVs exceed [0,1] repeat
    gl_Position = projection * vec4(p, 0.0, 1.0);
}