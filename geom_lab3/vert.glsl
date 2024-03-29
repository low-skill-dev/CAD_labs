﻿#version 430 core
layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aBarycentric;

uniform mat4 rotate;
uniform mat4 scale;

out vec3 vbc;


void main()
{
    vbc = aBarycentric;

    gl_Position = vec4(aPosition, 1) * scale * rotate;
}