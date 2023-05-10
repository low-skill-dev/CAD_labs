#version 430 core

in vec3 vbc;
uniform int allBlack;
uniform int grayPolygons;

void main()
{
    if(allBlack){
         gl_FragColor = vec4(0, 0, 0, 1);
    }
    else if(vbc.x < 0.01 || vbc.y < 0.01 || vbc.z < 0.01){
         gl_FragColor = vec4(0, 0, 1, 1);
    }
    else if(grayPolygons){
        gl_FragColor = vec4(0.85, 0.85, 0.85, 1);
    }
    else{
         gl_FragColor = vec4(vbc.x,vbc.y,vbc.z, 1);
    }
}