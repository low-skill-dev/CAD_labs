#version 430 core

in vec3 bcCoords;
uniform vec4 ourColor;

void main()
{
    if(any(lessThan(bcCoords, vec3(0.1)))){
        if(false)
            gl_FragColor = vec4(0, 255, 0, 1);
        else
            gl_FragColor = ourColor;
    }
    else{
         gl_FragColor = vec4(0, 0, 0, 1);
    }

}