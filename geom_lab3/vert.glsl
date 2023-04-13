#version 430 core
layout (location = 0) in vec3 aPosition;
attribute vec3 barycentric;

uniform mat4 rotation;

out vec3 bcCoords;


void main()
{
    bcCoords = barycentric;
    gl_Position = vec4(aPosition, 1.0) * rotation;
}