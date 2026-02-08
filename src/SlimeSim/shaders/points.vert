#version 430 core

struct Agent
{
    vec2 position;
    float angle;
    uint type;
    float energy;
    uint age;
    int state;
    int nnOffset;
    int meals;
    int deaths;
    float energySpent;
    int flag;
    ivec2 currPixel;
    ivec2 prevPixel;
    float memory0;
    float memory1;
    float nearPrey;
    uint survivalDuration;
};

layout(std430, binding = 1) buffer AgentsBuffer {
    Agent agents[];
};

uniform mat4 projection;
uniform float zoom;
uniform vec2 offset;

layout(location=0) out vec3 vColor;
layout(location=1) flat out int flag;
layout(location=2) flat out float pointSize;
layout(location=3) flat out float angle;
layout(location=4) flat out uint type;

void main()
{
    uint id = gl_VertexID;
    Agent agent = agents[id];
    gl_Position = projection * vec4(agent.position + offset, 0.0, 1.0);

    //color
    vColor = vec3(0.0,1,0.0);
    if (agent.type == 1)
        vColor = vec3(0.0,0.0,1);
    else if (agent.type == 2)
        vColor = vec3(1,0.0,0.0);

    //size
    float baseSize = 0;
    if (agent.flag == 1)
        baseSize = 2.0;
    else if (agent.flag == 2)
        baseSize = 3.0;
    else if (agent.flag == 3)
        baseSize = 5.0;

    gl_PointSize = baseSize * zoom;

    flag = agent.flag;
    if (agent.state == 1 || agent.type == 0)
    {
        flag = 0;
        gl_PointSize = 0;
        vColor = vec3(0,0,0);
    }

    pointSize = gl_PointSize;
    angle = agent.angle;
    type = agent.type;
}