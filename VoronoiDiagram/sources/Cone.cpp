#include "../includes/Cone.h"

Cone::Cone(float topAngle, float height) : topAngle{ topAngle }, height{ height }
{
}


Cone::~Cone() {}

float* Cone::CreateConeVertices()
{
	float* vertices = new float[numOfTriangles * 3 + 3];
	// top vertice
	vertices[0] = 0.0f;
	vertices[1] = height;
	vertices[2] = 0.0f;
	float alpha = (360.0f / numOfTriangles) * M_PI / 180.0f;
	float radius = height * tan(topAngle * M_PI / 180.0f);
	int j = 0;
	for (int i = 0; i < numOfTriangles * 3; i += 3)
	{
		float x = radius * cos(alpha * j);
		float z = -(radius * sin(alpha * j));
		float y = 0.0f;
		vertices[i + 3] = x;
		vertices[i + 1 + 3] = y;
		vertices[i + 2 + 3] = z;
		j += 1;
	}
	//for (int i = 0; i < numOfTriangles * 3 + 3; i += 3)
	//{
	//	std::cout << vertices[i] << ", " << vertices[i + 1] << ", " << vertices[i + 2] << std::endl;
	//}
	return vertices;
}

unsigned int* Cone::CreateIndices()
{
	unsigned int* indices = new unsigned int[numOfTriangles * 3];
	indices[0] = 0;
	indices[1] = 1;
	indices[2] = 2;
	for (int i = 3; i < numOfTriangles * 3 - 3; i += 3)
	{
		indices[i] = 0;
		indices[i + 1] = indices[i - 2] + 1;
		indices[i + 2] = indices[i - 1] + 1;
	}
	indices[numOfTriangles * 3 - 3] = 0;
	indices[numOfTriangles * 3 - 2] = numOfTriangles;
	indices[numOfTriangles * 3 - 1] = indices[1];
	return indices;
}