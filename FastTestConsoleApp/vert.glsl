﻿#version 430 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aColor;

out vec3 ourColor;

uniform mat4 rotation;

void main()
{
    gl_Position = vec4(aPosition, 1.0)*rotation;
    ourColor = aColor;
}