#include "stdafx.h"
#include "FlexCLI.h"

//using namespace FlexCLI;
using namespace System::Runtime::InteropServices;

namespace FlexCLI {

	FlexCollisionGeometry::FlexCollisionGeometry() {
		NumPlanes = 0;
		Planes = gcnew array<float>(0);
		TimeStamp = 0;
	};

	///<summary>
	///Add up to eight collision planes, each in the form: Ax + By + Cz + D = 0. Anything beyond eight planes will be ignored.
	///</summary>
	void FlexCollisionGeometry::AddPlane(float A, float B, float C, float D) {
		if (!NumPlanes)
			Planes = gcnew array<float>(32);
		Planes[NumPlanes * 4] = A;
		Planes[NumPlanes * 4 + 1] = B;
		Planes[NumPlanes * 4 + 2] = C;
		Planes[NumPlanes * 4 + 3] = D;
		NumPlanes++;
		TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	/// <summary>
	/// Add a sphere by its center position and radius.
	/// </summary>
	void FlexCollisionGeometry::AddSphere(array<float>^ centerXYZ, float radius) {
		if (centerXYZ->Length != 3 || radius <= 0.0f)
			throw gcnew Exception("FlexCLI: FlexCollisionGeometry::AddSphere(array<float>^ centerXYZ, float radius) --->\nInvalid input: r <= 0 or centerXYZ doesn't supply three values!");
		if (!NumSpheres) {
			SphereCenters = gcnew List<float>();
			SphereRadii = gcnew List<float>();
		}
		SphereCenters->AddRange(centerXYZ);
		SphereRadii->Add(radius);
		NumSpheres++;
		TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	/// <summary>
	/// Add a box by its extends in each dimension, center position and orientation.
	/// </summary>
	void FlexCollisionGeometry::AddBox(array<float>^ halfHeightsXYZ, array<float>^ centerXYZ, array<float>^ rotationQuat) {
		if (halfHeightsXYZ->Length != 3
			|| centerXYZ->Length != 3
			|| rotationQuat->Length != 4
			|| halfHeightsXYZ[0] <= 0.0
			|| halfHeightsXYZ[1] <= 0.0
			|| halfHeightsXYZ[2] <= 0.0)
			throw gcnew Exception("FlexCollisionGeometry::AddBox(...) --->\nInvalid input: one array is not of correct length or at least one half height is <= 0.0!");
		if (!NumBoxes) {
			BoxHalfHeights = gcnew List<float>();
			BoxCenters = gcnew List<float>();
			BoxRotations = gcnew List<float>();
		}
		BoxHalfHeights->AddRange(halfHeightsXYZ);
		BoxCenters->AddRange(centerXYZ);
		BoxRotations->AddRange(rotationQuat);
		NumBoxes++;
		TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	/// <summary>
	/// UNTESTED: Add a capsule by its extends in X, radius, center position and orientation.
	/// </summary>
	void FlexCollisionGeometry::AddCapsule(float halfHeightX, float radius, array<float>^ centerXYZ, array<float>^ rotationQuat) {
		if (halfHeightX <= 0.0
			|| radius <= 0.0
			|| centerXYZ->Length != 3
			|| rotationQuat->Length != 4)
			throw gcnew Exception("FlexCollisionGeometry::AddCapsule(...) --->\nInvalid input: one array is not of correct length or half height or radius is <= 0.0!");
		if (!NumCapsules) {
			CapsuleHalfHeights = gcnew List<float>();
			CapsuleRadii = gcnew List<float>();
			CapsuleCenters = gcnew List<float>();
			CapsuleRotations = gcnew List<float>();
		}
		CapsuleHalfHeights->Add(halfHeightX);
		CapsuleRadii->Add(radius);
		CapsuleCenters->AddRange(centerXYZ);
		CapsuleRotations->AddRange(rotationQuat);
		NumCapsules++;
		TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	///<summary>
	/// Add a triangle mesh by its vertex position and faces both as flattened arrays. Make sure front face CCW is pointing outward otherwise results are unforeseen.
	///</summary>
	void FlexCollisionGeometry::AddMesh(array<float>^ vertices, array<int>^ faces) {
		if (vertices->Length % 3 != 0 || faces->Length % 3 != 0)
			throw gcnew Exception("FlexCollisionGeometry::AddMesh(...) --->\nInvalid input: at least one array is not of length n * 3!");
		if (!NumMeshes) {
			MeshVertices = gcnew List<array<float>^>();
			MeshFaces = gcnew List<array<int>^>();
			MeshLowerBounds = gcnew List<array<float>^>();
			MeshUpperBounds = gcnew List<array<float>^>();
		}
		MeshVertices->Add(vertices);
		MeshFaces->Add(faces);
		MeshLowerBounds->Add(gcnew array<float>{ vertices[0], vertices[1], vertices[2] });
		MeshUpperBounds->Add(gcnew array<float>{ vertices[0], vertices[1], vertices[2] });

		for (int i = 0; i < vertices->Length / 3; i++) {
			array<float>^ tmpLow = MeshLowerBounds[NumMeshes];
			array<float>^ tmpUpp = MeshUpperBounds[NumMeshes];

			if (vertices[i * 3] < tmpLow[0])
				tmpLow[0] = vertices[i * 3];
			if (vertices[i * 3 + 1] < tmpLow[1])
				tmpLow[1] = vertices[i * 3 + 1];
			if (vertices[i * 3 + 2] < tmpLow[2])
				tmpLow[2] = vertices[i * 3 + 2];

			if (vertices[i * 3] > tmpUpp[0])
				tmpUpp[0] = vertices[i * 3];
			if (vertices[i * 3 + 1] > tmpUpp[1])
				tmpUpp[1] = vertices[i * 3 + 1];
			if (vertices[i * 3 + 2] > tmpUpp[2])
				tmpUpp[2] = vertices[i * 3 + 2];
		}

		NumMeshes++;
		TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	///<summary>
	///Add a convex mesh by the plane of each mesh face in the form ABCD (z+ should point inward) in a flattened array. upper and lower limits (float[3]) refer to vertex positions
	///</summary>
	void FlexCollisionGeometry::AddConvexShape(array<float>^ planes, array<float>^ upperLimit, array<float>^ lowerLimit) {
		if (planes->Length % 4 != 0)
			throw gcnew Exception("FlexCollisionGeometry::AddConvexShape(array<float>^ planes) --->\nInvalid input: plane input must be multiple of four (ABCD structure)!");
		if (upperLimit->Length % 3 != 0 || lowerLimit->Length % 3 != 0)
			throw gcnew Exception("FlexCollisionGeometry::AddConvexShape(array<float>^ planes) --->\nInvalid input: upper and lower limit inputs must be three (XYZ structure)!");
		if (!NumConvex) {
			ConvexPlanes = gcnew List<array<float>^>();
			ConvexUpperBounds = gcnew List<array<float>^>();
			ConvexLowerBounds = gcnew List<array<float>^>();
		}

		ConvexPlanes->Add(planes);
		ConvexUpperBounds->Add(upperLimit);
		ConvexLowerBounds->Add(lowerLimit);

		NumConvex++;
		TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}
}