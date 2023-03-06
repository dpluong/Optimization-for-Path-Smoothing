#ifndef CONE_H
#define CONR_H

#include <iostream>
#define _USE_MATH_DEFINES
#include <math.h>

const int numOfTriangles = 64;

class Cone
{
public:
	Cone(float topAngle, float height);
	~Cone();
	float* CreateConeVertices();
	unsigned int* CreateIndices();
private:
	float topAngle;
	float height;

};

#endif