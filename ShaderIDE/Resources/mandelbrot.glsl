#version 330 core

uniform float time;
in vec3 vertexPosition;
out vec4 FragColor;

void main()
{    
    float iterations = 250;
    vec2 translation = vec2(-0.5f, 0.0f);
    vec2 z = vertexPosition.xy + translation;
    vec2 c = z;
    float r = 0.0f;

    for (float i = 0.0; i< iterations && r < 2.0; i++)
    {
        z = vec2(z.x * z.x - z.y * z.y, 2.0f * z.x * z.y) + c;
        r = length(z);
    }
    if (r > 2)
        discard;

    FragColor = vec4(r * r, sin(r * sqrt(time)), cos(r + time), 1.0f);
}
