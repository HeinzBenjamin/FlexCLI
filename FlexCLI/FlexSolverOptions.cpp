#include "stdafx.h"
#include "FlexCLI.h"

namespace FlexCLI {

	FlexSolverOptions::FlexSolverOptions() {
		dT = 0.01666667f;
		SubSteps = 3;
		NumIterations = 3;
		SceneMode = 0;
		FixedTotalIterations = -1;
		StabilityScalingFactor = 1.0f;
		TimeStamp = 0;
		MaxParticles = 131072;
		MaxNeighborsPerParticle = 96;
		MaxCollisionShapeNumber = 65536;			//some geometries requires more entries (sphere: 2, box: 3, mesh: arbitrary), therefore this is NOT the max nr. of collision objects! 
		MaxCollisionMeshVertexCount = 65536;		//max nr. of vertices in a single collision mesh
		MaxCollisionMeshIndexCount = 65536;			//max nr. of face indices in a single collision mesh
		MaxCollisionConvexShapePlanes = 65536;		//max nr. of face indices in all convex collision meshes combined
		MaxRigidBodies = 65536;						//max nr. of rigid bodies
		MaxSprings = 196608;						//max nr. of springs
		MaxDynamicTriangles = 131072;				//needed for cloth
	}
	
	FlexSolverOptions::FlexSolverOptions(float dt, int subSteps, int numIterations, int sceneMode, int fixedNumTotalIterations, array<int>^ memoryRequirements, float stabilityScalingFactor)
	{
		dT = dt;
		SubSteps = subSteps;
		NumIterations = numIterations;
		SceneMode = sceneMode;
		FixedTotalIterations = fixedNumTotalIterations;
		StabilityScalingFactor = stabilityScalingFactor;
		MaxParticles = memoryRequirements[0];
		MaxNeighborsPerParticle = memoryRequirements[1];
		MaxCollisionShapeNumber = memoryRequirements[2];			//some geometries requires more entries (sphere: 2, box: 3, mesh: arbitrary), therefore this is NOT the max nr. of collision objects! 
		MaxCollisionMeshVertexCount = memoryRequirements[3];		//max nr. of vertices in a single collision mesh
		MaxCollisionMeshIndexCount = memoryRequirements[4];			//max nr. of face indices in a single collision mesh
		MaxCollisionConvexShapePlanes = memoryRequirements[5];		//max nr. of face indices in all convex collision meshes combined
		MaxRigidBodies = memoryRequirements[6];						//max nr. of rigid bodies
		MaxSprings = memoryRequirements[7];							//max nr. of springs
		MaxDynamicTriangles = memoryRequirements[8];				//needed for cloth
		TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	bool FlexSolverOptions::IsValid() {
		return dT > 0.0f && SubSteps > 0 && NumIterations > 0;
	}

	String^ FlexSolverOptions::ToString() {
		String^ str = gcnew String("FlexScene:");
		str += "\ndt = " + dT.ToString();
		str += "\nSubSteps = " + SubSteps.ToString();
		str += "\nNumIter = " + NumIterations.ToString();
		str += "\nSceneMode = " + SceneMode.ToString();
		str += "\nTotalIterations = " + FixedTotalIterations.ToString();
		str += "\nMaxParticles = " + MaxParticles.ToString();
		str += "\nMaxNeighborsPerParticle = " + MaxNeighborsPerParticle.ToString();
		str += "\nMaxCollisionShapeNumber = " + MaxCollisionShapeNumber.ToString();
		str += "\nMaxCollisionMeshVertexCount = " + MaxCollisionMeshVertexCount.ToString();
		str += "\nMaxCollisionMeshIndexCount = " + MaxCollisionMeshIndexCount.ToString();
		str += "\nMaxCollisionConvexShapePlanes = " + MaxCollisionConvexShapePlanes.ToString();
		str += "\nMaxRigidBodies = " + MaxRigidBodies.ToString();
		str += "\nMaxSprings = " + MaxSprings.ToString();
		str += "\nMaxDynamicTriangles = " + MaxDynamicTriangles.ToString();
		str += "\n\nTimeStamp = " + TimeStamp.ToString();
		return str;
	}
}