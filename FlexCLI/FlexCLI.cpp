// This is the main DLL file.
#include "stdafx.h"
#include "FlexCLI.h"

namespace FlexCLI {

	int multiplicator = 8;

	int maxParticles = 65536 * multiplicator;
	int maxDiffuseParticles = 0;
	int maxNeighborsPerParticle = INT8_MAX * multiplicator;
	int maxCollisionShapeNumber = 65536;			//some geometries requires more entries (sphere: 2, box: 3, mesh: arbitrary), therefore this is NOT the max nr. of collision objects! 
	int maxCollisionMeshVertexCount = 65536;		//max nr. of vertices in a single collision mesh
	int maxCollisionMeshIndexCount = 65536;			//max nr. of faces in a single collision mesh
	int maxCollisionConvexShapePlanes = 65536;		//max nr. of faces in all convex collision meshes combined
	int maxRigidBodies = 65536 * multiplicator;		//max nr. of rigid bodies
	int maxSprings = 65536 * multiplicator;			//max nr. of springs
	int maxDynamicTriangles = 65536 * multiplicator;//needed for cloth

	NvFlexLibrary* Library;
	NvFlexSolver* Solver;
	NvFlexParams Params;
	NvFlexExtForceFieldCallback* ForceFieldCallback; 
	int n; //The particle count in this very iteration
	float dt;
	int subSteps;
	int numFixedIter;

	struct SimBuffers {
		NvFlexBuffer* Particles;
		NvFlexBuffer* Velocities;
		NvFlexBuffer* Phases;
		NvFlexBuffer* Active;
		NvFlexBuffer* CollisionGeometry;
		NvFlexBuffer* Position;
		NvFlexBuffer* PrevPosition;
		NvFlexBuffer* Rotation;
		NvFlexBuffer* PrevRotation;
		NvFlexBuffer* Flags;
		//Buffers for collision geometry
		NvFlexBuffer* CollisionMeshVertices;
		NvFlexBuffer* CollisionMeshIndices;
		NvFlexBuffer* CollisionConvexMeshPlanes;
		//Buffers for Rigid Bodies
		NvFlexBuffer* RigidOffets;
		NvFlexBuffer* RigidIndices;
		NvFlexBuffer* RigidRestPositions;
		NvFlexBuffer* RigidRestNormals;
		NvFlexBuffer* RigidStiffnesses;
		NvFlexBuffer* RigidRotations;
		NvFlexBuffer* RigidTranslations;
		//Buffers for Springs
		NvFlexBuffer* SpringPairIndices;
		NvFlexBuffer* SpringLengths;
		NvFlexBuffer* SpringCoefficients;
		//Buffers for dynamic triangles
		NvFlexBuffer* DynamicTriangleIndices;
		NvFlexBuffer* DynamicTriangleNormals;
		//Buffers for inflatables
		NvFlexBuffer* InflatableStartIndices;
		NvFlexBuffer* InflatableNumTriangles;
		NvFlexBuffer* InflatableRestVolumes;
		NvFlexBuffer* InflatableOverPressures;
		NvFlexBuffer* InflatableConstraintScales;

		///Tells the host upon startup, how much memory it will need and reserves this memory
		void Allocate() {
			Particles = NvFlexAllocBuffer(Library, maxParticles, sizeof(float4), eNvFlexBufferHost);
			Velocities = NvFlexAllocBuffer(Library, maxParticles, sizeof(float3), eNvFlexBufferHost);
			Phases = NvFlexAllocBuffer(Library, maxParticles, sizeof(int), eNvFlexBufferHost);
			Active = NvFlexAllocBuffer(Library, maxParticles, sizeof(int), eNvFlexBufferHost);
			CollisionGeometry = NvFlexAllocBuffer(Library, maxCollisionShapeNumber, sizeof(float4), eNvFlexBufferHost);
			Position = NvFlexAllocBuffer(Library, maxCollisionShapeNumber, sizeof(float4), eNvFlexBufferHost);
			PrevPosition = NvFlexAllocBuffer(Library, maxCollisionShapeNumber, sizeof(float4), eNvFlexBufferHost);
			Rotation = NvFlexAllocBuffer(Library, maxCollisionShapeNumber, sizeof(float4), eNvFlexBufferHost);
			PrevRotation = NvFlexAllocBuffer(Library, maxCollisionShapeNumber, sizeof(float4), eNvFlexBufferHost);
			Flags = NvFlexAllocBuffer(Library, maxCollisionShapeNumber, sizeof(int), eNvFlexBufferHost);
			CollisionMeshVertices = NvFlexAllocBuffer(Library, maxCollisionMeshVertexCount, sizeof(float3), eNvFlexBufferHost);
			CollisionMeshIndices = NvFlexAllocBuffer(Library, maxCollisionMeshIndexCount, sizeof(int) * 3, eNvFlexBufferHost);
			CollisionConvexMeshPlanes = NvFlexAllocBuffer(Library, maxCollisionConvexShapePlanes, sizeof(float4), eNvFlexBufferHost);
			RigidOffets = NvFlexAllocBuffer(Library, maxRigidBodies + 1, sizeof(int), eNvFlexBufferHost);
			RigidIndices = NvFlexAllocBuffer(Library, maxRigidBodies, sizeof(int), eNvFlexBufferHost);
			RigidRestPositions = NvFlexAllocBuffer(Library, maxRigidBodies, sizeof(float3), eNvFlexBufferHost);
			RigidRestNormals = NvFlexAllocBuffer(Library, maxRigidBodies, sizeof(float4), eNvFlexBufferHost);
			RigidStiffnesses = NvFlexAllocBuffer(Library, maxRigidBodies, sizeof(float), eNvFlexBufferHost);
			RigidRotations = NvFlexAllocBuffer(Library, maxRigidBodies, sizeof(float4), eNvFlexBufferHost);
			RigidTranslations = NvFlexAllocBuffer(Library, maxRigidBodies, sizeof(float3), eNvFlexBufferHost);
			SpringPairIndices = NvFlexAllocBuffer(Library, maxSprings * 2, sizeof(int), eNvFlexBufferHost);
			SpringLengths = NvFlexAllocBuffer(Library, maxSprings, sizeof(float), eNvFlexBufferHost);
			SpringCoefficients = NvFlexAllocBuffer(Library, maxSprings, sizeof(float), eNvFlexBufferHost);
			DynamicTriangleIndices = NvFlexAllocBuffer(Library, maxDynamicTriangles * 3, sizeof(int), eNvFlexBufferHost);
			DynamicTriangleNormals = NvFlexAllocBuffer(Library, maxDynamicTriangles, sizeof(float3), eNvFlexBufferHost);
			InflatableStartIndices = NvFlexAllocBuffer(Library, maxDynamicTriangles, sizeof(int), eNvFlexBufferHost);
			InflatableNumTriangles = NvFlexAllocBuffer(Library, maxDynamicTriangles, sizeof(int), eNvFlexBufferHost);
			InflatableRestVolumes = NvFlexAllocBuffer(Library, maxDynamicTriangles / 4, sizeof(float), eNvFlexBufferHost);
			InflatableOverPressures = NvFlexAllocBuffer(Library, maxDynamicTriangles / 4, sizeof(float), eNvFlexBufferHost);
			InflatableConstraintScales = NvFlexAllocBuffer(Library, maxDynamicTriangles / 4, sizeof(float), eNvFlexBufferHost);
		}

		///<summary>
		///Performs the following steps for every buffer: Check if pointer is 0; if it is, do nothing. If it is not, free buffer (NvFlex function) and set pointer to 0.
		///</summary>
		void Destroy() {
			if (Particles) {
				NvFlexFreeBuffer(Particles);
				Particles = NULL;
			}
			if (Velocities) {
				NvFlexFreeBuffer(Velocities);
				Velocities = NULL;
			}
			if (Phases) {
				NvFlexFreeBuffer(Phases);
				Phases = NULL;
			}
			if (Active) {
				NvFlexFreeBuffer(Active);
				Active = NULL;
			}
			if (CollisionGeometry) {
				NvFlexFreeBuffer(CollisionGeometry);
				CollisionGeometry = NULL;
			}
			if (Position) {
				NvFlexFreeBuffer(Position);
				Position = NULL;
			}
			if (PrevPosition) {
				NvFlexFreeBuffer(PrevPosition);
				PrevPosition = NULL;
			}
			if (Rotation) {
				NvFlexFreeBuffer(Rotation);
				Rotation = NULL;
			}
			if (PrevRotation) {
				NvFlexFreeBuffer(PrevRotation);
				PrevRotation = NULL;
			}
			if (Flags) {
				NvFlexFreeBuffer(Flags);
				Flags = NULL;
			}
			if (CollisionMeshVertices) {
				NvFlexFreeBuffer(CollisionMeshVertices);
				CollisionMeshVertices = NULL;
			}
			if (CollisionMeshIndices) {
				NvFlexFreeBuffer(CollisionMeshIndices);
				CollisionMeshIndices = NULL;
			}
			if (CollisionConvexMeshPlanes) {
				NvFlexFreeBuffer(CollisionConvexMeshPlanes);
				CollisionConvexMeshPlanes = NULL;
			}
			if (RigidOffets) {
				NvFlexFreeBuffer(RigidOffets);
				RigidOffets = NULL;
			}
			if (RigidIndices) {
				NvFlexFreeBuffer(RigidIndices);
				RigidIndices = NULL;
			}
			if (RigidRestPositions) {
				NvFlexFreeBuffer(RigidRestPositions);
				RigidRestPositions = NULL;
			}
			if (RigidRestNormals) {
				NvFlexFreeBuffer(RigidRestNormals);
				RigidRestNormals = NULL;
			}
			if (RigidStiffnesses) {
				NvFlexFreeBuffer(RigidStiffnesses);
				RigidStiffnesses = NULL;
			}
			if (RigidRotations) {
				NvFlexFreeBuffer(RigidRotations);
				RigidRotations = NULL;
			}
			if (RigidTranslations) {
				NvFlexFreeBuffer(RigidTranslations);
				RigidTranslations = NULL;
			}
			if (SpringPairIndices) {
				NvFlexFreeBuffer(SpringPairIndices);
				SpringPairIndices = NULL;
			}
			if (SpringLengths) {
				NvFlexFreeBuffer(SpringLengths);
				SpringLengths = NULL;
			}
			if (SpringCoefficients) {
				NvFlexFreeBuffer(SpringCoefficients);
				SpringCoefficients = NULL;
			}
			if (DynamicTriangleIndices) {
				NvFlexFreeBuffer(DynamicTriangleIndices);
				DynamicTriangleIndices = NULL;
			}
			if (DynamicTriangleNormals) {
				NvFlexFreeBuffer(DynamicTriangleNormals);
				DynamicTriangleNormals = NULL;
			}
			if (InflatableStartIndices) {
				NvFlexFreeBuffer(InflatableStartIndices);
				InflatableStartIndices = NULL;
			}
			if (InflatableNumTriangles) {
				NvFlexFreeBuffer(InflatableNumTriangles);
				InflatableNumTriangles = NULL;
			}
			if (InflatableRestVolumes) {
				NvFlexFreeBuffer(InflatableRestVolumes);
				InflatableRestVolumes = NULL;
			}
			if (InflatableOverPressures) {
				NvFlexFreeBuffer(InflatableOverPressures);
				InflatableOverPressures = NULL;
			}
			if (InflatableConstraintScales) {
				NvFlexFreeBuffer(InflatableConstraintScales);
				InflatableConstraintScales = NULL;
			}
		}
	};

	SimBuffers Buffers;

	///<summary>Create a default Flex engine object. This will initialize a solver, create buffers and set up default NvFlexParams.</summary>
	Flex::Flex() {
		if (Solver)
			Destroy();

		Library = NvFlexInit();

		//set default Params
#pragma region Params
		Params.gravity[0] = 0.0f;
		Params.gravity[1] = 0.0f;
		Params.gravity[2] = -9.81f;

		Params.wind[0] = 0.0f;
		Params.wind[1] = 0.0f;
		Params.wind[2] = 0.0f;

		Params.radius = 0.15f;
		Params.viscosity = 0.0f;
		Params.dynamicFriction = 0.0f;
		Params.staticFriction = 0.0f;
		Params.particleFriction = 0.0f; // scale friction between particles by default
		Params.freeSurfaceDrag = 0.0f;
		Params.drag = 0.0f;
		Params.lift = 0.0f;
		Params.numIterations = 3;
		Params.fluidRestDistance = 0.0f;
		Params.solidRestDistance = 0.0f;

		Params.anisotropyScale = 1.0f;
		Params.anisotropyMin = 0.1f;
		Params.anisotropyMax = 2.0f;
		Params.smoothing = 1.0f;

		Params.dissipation = 0.0f;
		Params.damping = 0.0f;
		Params.particleCollisionMargin = 0.0f;
		Params.shapeCollisionMargin = 0.0f;
		Params.collisionDistance = 0.0f;
		Params.plasticThreshold = 0.0f;
		Params.plasticCreep = 0.0f;
		Params.fluid = true;
		Params.sleepThreshold = 0.0f;
		Params.shockPropagation = 0.0f;
		Params.restitution = 0.0f;

		Params.maxSpeed = FLT_MAX;
		Params.maxAcceleration = 100.0f;	// approximately 10x gravity

		Params.relaxationMode = eNvFlexRelaxationLocal;
		Params.relaxationFactor = 1.0f;
		Params.solidPressure = 1.0f;
		Params.adhesion = 0.0f;
		Params.cohesion = 0.025f;
		Params.surfaceTension = 0.0f;
		Params.vorticityConfinement = 0.0f;
		Params.buoyancy = 1.0f;
		Params.diffuseThreshold = 100.0f;
		Params.diffuseBuoyancy = 1.0f;
		Params.diffuseDrag = 0.8f;
		Params.diffuseBallistic = 16;
		Params.diffuseSortAxis[0] = 0.0f;
		Params.diffuseSortAxis[1] = 0.0f;
		Params.diffuseSortAxis[2] = 0.0f;
		Params.diffuseLifetime = 2.0f;

		// planes created after particles
		Params.numPlanes = 0;
#pragma endregion

		Buffers.Allocate();

		FlexForceFields = gcnew List<FlexForceField^>();

		Solver = NvFlexCreateSolver(Library, maxParticles, maxDiffuseParticles, maxNeighborsPerParticle);

		if (ForceFieldCallback)
			NvFlexExtDestroyForceFieldCallback(ForceFieldCallback);

		ForceFieldCallback = NvFlexExtCreateForceFieldCallback(Solver);
	}

	///<summary>Returns true if pointers to library and solver objects are valid</summary>
	bool Flex::IsReady() {
		return Library && Solver;
	}

	///Registration methods private and public

	///<summary>Register different collision geometries wrapped into the FlexCollisionGeometry class.</summary>
	void Flex::SetCollisionGeometry(FlexCollisionGeometry^ flexCollisionGeometry) {

		//PLANES
		//if (flexCollisionGeometry->NumPlanes > 0 && flexCollisionGeometry->Planes) {
			//unlike other collision geometries, planes are registered in the Flex param (NvFlexParams)
			Params.numPlanes = flexCollisionGeometry->NumPlanes;
			for (int i = 0; i < flexCollisionGeometry->NumPlanes; i++) {
				Params.planes[i][0] = flexCollisionGeometry->Planes[i * 4];
				Params.planes[i][1] = flexCollisionGeometry->Planes[i * 4 + 1];
				Params.planes[i][2] = flexCollisionGeometry->Planes[i * 4 + 2];
				Params.planes[i][3] = flexCollisionGeometry->Planes[i * 4 + 3];
			}
			NvFlexSetParams(Solver, &Params);
		//}


		//EVERYTHING ELSE
		//prepare generic buffers, shape specific buffers are handled in the respective field 
		NvFlexCollisionGeometry* geometry = (NvFlexCollisionGeometry*)NvFlexMap(Buffers.CollisionGeometry, 0);
		float4* positions = (float4*)NvFlexMap(Buffers.Position, 0);
		float4* rotations = (float4*)NvFlexMap(Buffers.Rotation, 0);
		int* flags = (int*)NvFlexMap(Buffers.Flags, 0);
		int numShapes = 0;

		// add sphere
		for (int i = 0; i < flexCollisionGeometry->NumSpheres; i++) {
			flags[numShapes] = NvFlexMakeShapeFlags(eNvFlexShapeSphere, false);
			geometry[numShapes].sphere.radius = flexCollisionGeometry->SphereRadii[i];
			positions[numShapes] = float4(flexCollisionGeometry->SphereCenters[i * 3], flexCollisionGeometry->SphereCenters[i * 3 + 1], flexCollisionGeometry->SphereCenters[i * 3 + 2], 0.0);
			rotations[numShapes] = float4(0.0f, 0.0f, 0.0f, 0.0f);
			numShapes++;
		}

		// add boxes
		for (int i = 0; i < flexCollisionGeometry->NumBoxes; i++) {
			flags[numShapes] = NvFlexMakeShapeFlags(eNvFlexShapeBox, false);
			geometry[numShapes].box.halfExtents[0] = flexCollisionGeometry->BoxHalfHeights[i * 3];
			geometry[numShapes].box.halfExtents[1] = flexCollisionGeometry->BoxHalfHeights[i * 3 + 1];
			geometry[numShapes].box.halfExtents[2] = flexCollisionGeometry->BoxHalfHeights[i * 3 + 2];
			positions[numShapes] = float4(flexCollisionGeometry->BoxCenters[i * 3], flexCollisionGeometry->BoxCenters[i * 3 + 1], flexCollisionGeometry->BoxCenters[i * 3 + 2], 0.0);
			rotations[numShapes] = float4(flexCollisionGeometry->BoxRotations[i * 4], flexCollisionGeometry->BoxRotations[i * 4 + 1], flexCollisionGeometry->BoxRotations[i * 4 + 2], flexCollisionGeometry->BoxRotations[i * 4 + 3]);
			numShapes++;
		}

		//add capsules
		for (int i = 0; i < flexCollisionGeometry->NumCapsules; i++) {
			flags[numShapes] = NvFlexMakeShapeFlags(eNvFlexShapeCapsule, false);
			geometry[numShapes].capsule.halfHeight = flexCollisionGeometry->CapsuleHalfHeights[i];
			geometry[numShapes].capsule.radius = flexCollisionGeometry->CapsuleRadii[i];
			positions[numShapes] = float4(flexCollisionGeometry->CapsuleCenters[i * 4], flexCollisionGeometry->CapsuleCenters[i * 4 + 1], flexCollisionGeometry->CapsuleCenters[i * 4 + 2], 0.0);
			rotations[numShapes] = float4(flexCollisionGeometry->CapsuleRotations[i * 4], flexCollisionGeometry->CapsuleRotations[i * 4 + 1], flexCollisionGeometry->CapsuleRotations[i * 4 + 2], flexCollisionGeometry->CapsuleRotations[i * 4 + 3]);
			numShapes++;
		}

		//add meshes
		for (int i = 0; i < flexCollisionGeometry->NumMeshes; i++) {
			// create a triangle mesh
			NvFlexTriangleMeshId mesh = NvFlexCreateTriangleMesh(Library);



			//assign vertex and face lists accordingly
			float3* vertices = (float3*)NvFlexMap(Buffers.CollisionMeshVertices, 0);
			int* faces = (int*)NvFlexMap(Buffers.CollisionMeshIndices, 0);
			array<float>^ v = flexCollisionGeometry->MeshVertices[i];
			array<int>^ f = flexCollisionGeometry->MeshFaces[i];
			for (int j = 0; j < v->Length / 3; j++)
				vertices[j] = float3(-v[j * 3], -v[j * 3 + 1], -v[j * 3 + 2]);
			for (int j = 0; j < f->Length; j++)
				faces[j] = f[j];

			//get upper and lower bounds of the mesh
			array<float>^ u = flexCollisionGeometry->MeshUpperBounds[i];
			array<float>^ l = flexCollisionGeometry->MeshLowerBounds[i];
			float* upper = new float[3];
			float* lower = new float[3];
			for (int j = 0; j < 3; j++) {
				upper[j] = -u[j];
				lower[j] = -l[j];
			}
			NvFlexUnmap(Buffers.CollisionMeshVertices);
			NvFlexUnmap(Buffers.CollisionMeshIndices);

			//set mesh
			NvFlexUpdateTriangleMesh(Library, mesh, Buffers.CollisionMeshVertices, Buffers.CollisionMeshIndices, (int)(v->Length / 3), (int)(f->Length / 3), upper, lower);

			delete(upper);
			upper = NULL;
			delete(lower);
			lower = NULL;

			// add triangle mesh instance
			flags[numShapes] = NvFlexMakeShapeFlags(eNvFlexShapeTriangleMesh, false);
			geometry[numShapes].triMesh.mesh = mesh;
			geometry[numShapes].triMesh.scale[0] = 1.0f;
			geometry[numShapes].triMesh.scale[1] = 1.0f;
			geometry[numShapes].triMesh.scale[2] = 1.0f;
			positions[numShapes] = float4(0.0f, 0.0f, 0.0f, 0.0f);
			rotations[numShapes] = float4(0.0f, 0.0f, 0.0f, 0.0f);

			numShapes++;
		}

		//add convex shapes
		for (int i = 0; i < flexCollisionGeometry->NumConvex; i++) {
			//create convex mesh
			NvFlexConvexMeshId mesh = NvFlexCreateConvexMesh(Library);

			//assign planes accordingly
			float4* planes = (float4*)NvFlexMap(Buffers.CollisionConvexMeshPlanes, 0);
			array<float>^ p = flexCollisionGeometry->ConvexPlanes[i];
			for (int j = 0; j < p->Length / 4; j++)
				planes[j] = float4(p[j * 4], p[j * 4 + 1], p[j * 4 + 2], -p[j * 4 + 3]);

			//get upper and lower bounds of the mesh
			array<float>^ u = flexCollisionGeometry->ConvexUpperBounds[i];
			array<float>^ l = flexCollisionGeometry->ConvexLowerBounds[i];
			float* upper = new float[3];
			float* lower = new float[3];
			for (int j = 0; j < 3; j++) {
				upper[j] = -u[j];
				lower[j] = -l[j];
			}

			NvFlexUnmap(Buffers.CollisionConvexMeshPlanes);

			//set convex mesh
			NvFlexUpdateConvexMesh(Library, mesh, Buffers.CollisionConvexMeshPlanes, p->Length / 4, lower, upper);

			delete(upper);
			upper = NULL;
			delete(lower);
			lower = NULL;

			flags[numShapes] = NvFlexMakeShapeFlags(eNvFlexShapeConvexMesh, false);
			geometry[numShapes].convexMesh.mesh = mesh;
			geometry[numShapes].convexMesh.scale[0] = 1.0f;
			geometry[numShapes].convexMesh.scale[1] = 1.0f;
			geometry[numShapes].convexMesh.scale[2] = 1.0f;
			positions[numShapes] = float4(0.0f, 0.0f, 0.0f, 0.0f);
			rotations[numShapes] = float4(0.0f, 0.0f, 0.0f, 0.0f);
			numShapes++;
		}

		//TO DO: add SDF

		// unmap buffers
		NvFlexUnmap(Buffers.CollisionGeometry);
		NvFlexUnmap(Buffers.Position);
		NvFlexUnmap(Buffers.Rotation);
		NvFlexUnmap(Buffers.Flags);

		// send shapes to Flex
		NvFlexSetShapes(Solver,
			Buffers.CollisionGeometry,
			Buffers.Position,
			Buffers.Rotation,
			NULL,
			NULL,
			Buffers.Flags, numShapes);
	}

	///<summary>Register simulation parameters using the FlexCLI.FlexParams class</summary>
	void Flex::SetParams(FlexParams^ flexParams) {
		if (flexParams->IsValid()) {
#pragma region set all
			Params.adhesion = flexParams->Adhesion;
			Params.anisotropyMax = flexParams->AnisotropyMax;
			Params.anisotropyMin = flexParams->AnisotropyMin;
			Params.anisotropyScale = flexParams->AnisotropyScale;
			Params.buoyancy = flexParams->Buoyancy;
			Params.cohesion = flexParams->Cohesion;
			Params.collisionDistance = flexParams->CollisionDistance;
			Params.damping = flexParams->Damping;
			Params.diffuseBallistic = flexParams->DiffuseBallistic;
			Params.diffuseBuoyancy = flexParams->DiffuseBuoyancy;
			Params.diffuseDrag = flexParams->DiffuseDrag;
			Params.diffuseLifetime = flexParams->DiffuseLifetime;
			Params.diffuseSortAxis[0] = flexParams->DiffuseSortAxisX;
			Params.diffuseSortAxis[1] = flexParams->DiffuseSortAxisZ;
			Params.diffuseSortAxis[2] = flexParams->DiffuseSortAxisY;
			Params.diffuseThreshold = flexParams->DiffuseThreshold;
			Params.dissipation = flexParams->Dissipation;
			Params.drag = flexParams->Drag;
			Params.dynamicFriction = flexParams->DynamicFriction;
			Params.fluid = flexParams->Fluid;
			Params.fluidRestDistance = flexParams->FluidRestDistance;
			Params.freeSurfaceDrag = flexParams->FreeSurfaceDrag;
			Params.gravity[0] = flexParams->GravityX;
			Params.gravity[1] = flexParams->GravityY;
			Params.gravity[2] = flexParams->GravityZ;
			Params.lift = flexParams->Lift;
			Params.maxAcceleration = flexParams->MaxAcceleration;
			Params.maxSpeed = flexParams->MaxSpeed;
			Params.particleCollisionMargin = flexParams->ParticleCollisionMargin;
			Params.particleFriction = flexParams->ParticleFriction;
			Params.plasticCreep = flexParams->PlasticCreep;
			Params.plasticThreshold = flexParams->PlasticThreshold;
			Params.radius = flexParams->Radius;
			Params.relaxationFactor = flexParams->RelaxationFactor;
			Params.relaxationMode = NvFlexRelaxationMode(flexParams->RelaxationMode);
			Params.restitution = flexParams->Restitution;
			Params.shapeCollisionMargin = flexParams->ShapeCollisionMargin;
			Params.shockPropagation = flexParams->ShockPropagation;
			Params.sleepThreshold = flexParams->SleepThreshold;
			Params.smoothing = flexParams->Smoothing;
			Params.solidPressure = flexParams->SolidPressure;
			Params.solidRestDistance = flexParams->SolidRestDistance;
			Params.staticFriction = flexParams->StaticFriction;
			Params.surfaceTension = flexParams->SurfaceTension;
			Params.viscosity = flexParams->Viscosity;
			Params.vorticityConfinement = flexParams->VorticityConfinement;
			Params.wind[0] = flexParams->WindX;
			Params.wind[1] = flexParams->WindY;
			Params.wind[2] = flexParams->WindZ;
#pragma endregion
			NvFlexSetParams(Solver, &Params);
		}
		else
			throw gcnew Exception("FlexCLI: void Flex::SetParams(FlexParams^ flexParams) ---> Invalid flexParams");
	}

	///<summary>Register a simulation scenery using the FlexCLI.FlexScene class</summary>
	void Flex::SetScene(FlexScene^ flexScene) {
		//TO DO!!!! Create a deep copy of flexScene to avoid changing the original lists in flexScene
		FlexScene^ s = flexScene;
		if (!s->IsValid())
			return;

		if (s->Particles->Count > maxParticles)
			throw gcnew Exception("void Flex::SetScene() ---> Exceeded maximum particle count. Contact benjamin@felbrich.com for more info.");
		//set particles
		SetParticles(s->Particles);

		//set constraints
		//Rigids
		SetRigids(s->RigidOffsets,
			s->RigidIndices,
			s->RigidRestPositions,
			s->RigidRestNormals,
			s->RigidStiffnesses,
			s->RigidRotations,
			s->RigidTranslations);

		//Springs
		SetSprings(s->SpringPairIndices,
			s->SpringLengths,
			s->SpringStiffnesses);

		//Dynamic Triangles for cloth inflatable and dynamic collision objects
		SetDynamicTriangles(
			s->DynamicTriangleIndices,
			s->DynamicTriangleNormals
		);

		//Inflatables
		SetInflatables(
			s->InflatableStartIndices,
			s->InflatableNumTriangles,
			s->InflatableRestVolumes,
			s->InflatableOverPressures,
			s->InflatableConstraintScales);

		//save scene globally
		Scene = s;
		Scene->Flex = this;
	}

	void Flex::SetSolverOptions(FlexSolverOptions^ flexSolverOptions) {
		if (flexSolverOptions->IsValid()) {
			dt = flexSolverOptions->dT;
			subSteps = flexSolverOptions->SubSteps;
			Params.numIterations = flexSolverOptions->NumIterations;
			numFixedIter = flexSolverOptions->FixedTotalIterations;
		}
		else
			throw gcnew Exception("Invalid solver options: Both dt and subSteps have to be > 0");

	}

	void Flex::SetForceFields(List<FlexForceField^>^ flexForceFields) {

		if (flexForceFields->Count == 0)
			return;
		std::vector<NvFlexExtForceField> forceFields(flexForceFields->Count);
		NvFlexExtForceField forceField;

		for (int i = 0; i < flexForceFields->Count; i++) {
			NvFlexExtForceField ff;
			ff.mPosition[0] = flexForceFields[i]->Position[0];
			ff.mPosition[1] = flexForceFields[i]->Position[1];
			ff.mPosition[2] = flexForceFields[i]->Position[2];
			ff.mRadius = flexForceFields[i]->Radius;
			ff.mStrength = flexForceFields[i]->Strength;
			if (flexForceFields[i]->Mode == 0)
				ff.mMode = NvFlexExtForceMode::eNvFlexExtModeForce;
			else if (flexForceFields[i]->Mode == 1)
				ff.mMode = NvFlexExtForceMode::eNvFlexExtModeImpulse;
			else if (flexForceFields[i]->Mode == 2)
				ff.mMode = NvFlexExtForceMode::eNvFlexExtModeVelocityChange;
			else
				throw gcnew Exception("void Flex::SetForceFields() ---> Invalid mode! Mode must be either 0, 1 or 2.");
			ff.mLinearFalloff = flexForceFields[i]->LinearFallOff;

			forceFields[i] = ff;
		}

		NvFlexExtSetForceFields(ForceFieldCallback, &forceFields[0], flexForceFields->Count);
	}

	void Flex::SetParticles(List<FlexParticle^>^ flexParticles) {
		//create buffers
		n = flexParticles->Count;
		if (!n) return;
		int nActive = 0;

		float4* particles = (float4*)NvFlexMap(Buffers.Particles, eNvFlexMapWait);
		float3* velocities = (float3*)NvFlexMap(Buffers.Velocities, eNvFlexMapWait);
		int* phases = (int*)NvFlexMap(Buffers.Phases, eNvFlexMapWait);
		int* actives = (int*)NvFlexMap(Buffers.Active, eNvFlexMapWait);

		for (int i = 0; i < n; i++) {
			if (flexParticles[i]->IsValid()) {
				particles[i] = float4(flexParticles[i]->PositionX, flexParticles[i]->PositionY, flexParticles[i]->PositionZ, flexParticles[i]->InverseMass);
				velocities[i] = float3(flexParticles[i]->VelocityX, flexParticles[i]->VelocityY, flexParticles[i]->VelocityZ);
				phases[i] = flexParticles[i]->Phase;
				if (flexParticles[i]->IsActive)
				{
					actives[i] = i;
					nActive++;
				}
			}
			else
				throw gcnew Exception("FlexCLI: void Flex::SetParticles(array<FlexParticle^>^ flexParticles ---> particle nr. " + i + " is invalid!\n" + flexParticles[i]->ToString());
		}

		NvFlexUnmap(Buffers.Particles);
		NvFlexUnmap(Buffers.Velocities);
		NvFlexUnmap(Buffers.Phases);
		NvFlexUnmap(Buffers.Active);

		NvFlexSetParticles(Solver, Buffers.Particles, n);
		NvFlexSetVelocities(Solver, Buffers.Velocities, n);
		NvFlexSetPhases(Solver, Buffers.Phases, n);
		NvFlexSetActive(Solver, Buffers.Active, nActive);
	}

	List<FlexParticle^>^ Flex::GetParticles() {

		List<FlexParticle^>^ parts = gcnew List<FlexParticle^>;
		NvFlexGetParticles(Solver, Buffers.Particles, n);
		NvFlexGetVelocities(Solver, Buffers.Velocities, n);
		NvFlexGetPhases(Solver, Buffers.Phases, n);

		float4* particles = (float4*)NvFlexMap(Buffers.Particles, eNvFlexMapWait);
		float3* velocities = (float3*)NvFlexMap(Buffers.Velocities, eNvFlexMapWait);
		int* phases = (int*)NvFlexMap(Buffers.Phases, eNvFlexMapWait);

		for (int i = 0; i < n; i++) {
			array<float>^ pos = gcnew array<float>{particles[i].x, particles[i].y, particles[i].z};
			array<float>^ vel = gcnew array<float>{velocities[i].x, velocities[i].y, velocities[i].z};

			int gi = 0;
			bool sc = false;
			bool fl = false;

			DecomposePhase(phases[i], gi, sc, fl);
			parts->Add(gcnew FlexParticle(pos, vel, particles[i].w, phases[i], true));
		}

		NvFlexUnmap(Buffers.Particles);
		NvFlexUnmap(Buffers.Velocities);
		NvFlexUnmap(Buffers.Phases);

		return parts;
	}

	void Flex::SetRigids(List<int>^ offsets, List<int>^ indices, List<float>^ restPositions, List<float>^ restNormals, List<float>^ stiffnesses, List<float>^ rotations, List<float>^ translations) {
		if (offsets[0] != 0)
			throw gcnew Exception("FlexCLI: void Flex::SetRigids(...) Invalid input: ");
		int numRigids = offsets->Count - 1;

		if (indices->Count < 2)
			return;

		//create buffers	
		int* off = (int*)NvFlexMap(Buffers.RigidOffets, eNvFlexMapWait);
		int* ind = (int*)NvFlexMap(Buffers.RigidIndices, eNvFlexMapWait);
		float3* restPos = (float3*)NvFlexMap(Buffers.RigidRestPositions, eNvFlexMapWait);
		float4* restNor = (float4*)NvFlexMap(Buffers.RigidRestNormals, eNvFlexMapWait);
		float* sti = (float*)NvFlexMap(Buffers.RigidStiffnesses, eNvFlexMapWait);
		float4* rot = (float4*)NvFlexMap(Buffers.RigidRotations, eNvFlexMapWait);
		float3* tra = (float3*)NvFlexMap(Buffers.RigidTranslations, eNvFlexMapWait);

		//assign everything
		off[0] = 0;
		for (int i = 0; i < numRigids; i++) {
			off[i + 1] = offsets[i + 1];
			for (int j = offsets[i]; j < offsets[i + 1]; j++) {
				restPos[j] = float3(restPositions[j * 3], restPositions[j * 3 + 1], restPositions[j * 3 + 2]);
				restNor[j] = float4(restNormals[j * 4], restNormals[j * 4 + 1], restNormals[j * 4 + 2], restNormals[j * 4 + 3]);
			}
			sti[i] = stiffnesses[i];
			//for some weird reason rotations always returns zeros unless w is initialized with some tvalue from the beginning. if x, y, or z are initialized as non-zero values, intitial rotation is applied which is wrong.
			if (rotations[i * 4] == 0.0f && rotations[i * 4 + 1] == 0.0f && rotations[i * 4 + 2] == 0.0f && rotations[i * 4 + 3] == 0.0f)
				rot[i] = float4(rotations[i * 4], rotations[i * 4 + 1], rotations[i * 4 + 2], rotations[i * 4 + 3] + 1);
			else
				rot[i] = float4(rotations[i * 4], rotations[i * 4 + 1], rotations[i * 4 + 2], rotations[i * 4 + 3]);
			tra[i] = float3(translations[i * 3], translations[i * 3 + 1], translations[i * 3 + 2]);
		}

		for (int i = 0; i < indices->Count; i++)
			ind[i] = indices[i];

		//unmap buffers
		NvFlexUnmap(Buffers.RigidOffets);
		NvFlexUnmap(Buffers.RigidIndices);
		NvFlexUnmap(Buffers.RigidRestPositions);
		NvFlexUnmap(Buffers.RigidRestNormals);
		NvFlexUnmap(Buffers.RigidStiffnesses);
		NvFlexUnmap(Buffers.RigidRotations);
		NvFlexUnmap(Buffers.RigidTranslations);

		//actual Nv function
		NvFlexSetRigids(Solver, Buffers.RigidOffets, Buffers.RigidIndices, Buffers.RigidRestPositions, Buffers.RigidRestNormals, Buffers.RigidStiffnesses, Buffers.RigidRotations, Buffers.RigidTranslations, numRigids, indices->Count);
	}

	void Flex::GetRigidTransformations(List<float>^ %translations, List<float>^ %rotations) {
		translations = gcnew List<float>();
		rotations = gcnew List<float>();

		NvFlexGetRigidTransforms(Solver, Buffers.RigidRotations, Buffers.RigidTranslations);

		float4* rot = (float4*)NvFlexMap(Buffers.RigidRotations, eNvFlexMapWait);
		float3* trans = (float3*)NvFlexMap(Buffers.RigidTranslations, eNvFlexMapWait);

		for (int i = 0; i < Scene->NumRigids(); i++) {
			rotations->Add(rot[i].x);
			rotations->Add(rot[i].y);
			rotations->Add(rot[i].z);
			rotations->Add(rot[i].w);
			translations->Add(trans[i].x);
			translations->Add(trans[i].y);
			translations->Add(trans[i].z);
		}

		NvFlexUnmap(Buffers.RigidRotations);
		NvFlexUnmap(Buffers.RigidTranslations);
	}

	void Flex::SetSprings(List<int>^ springPairIndices, List<float>^ springLengths, List<float>^ springCoefficients) {
		if (springPairIndices->Count != 2 * springLengths->Count || springPairIndices->Count != 2 * springCoefficients->Count)
			throw gcnew Exception("void Flex::SetSprings(...) ---> Invalid input!");

		int* spi = (int*)NvFlexMap(Buffers.SpringPairIndices, eNvFlexMapWait);
		float* sl = (float*)NvFlexMap(Buffers.SpringLengths, eNvFlexMapWait);
		float* sc = (float*)NvFlexMap(Buffers.SpringCoefficients, eNvFlexMapWait);

		for (int i = 0; i < springLengths->Count; i++) {
			spi[i * 2] = springPairIndices[i * 2];
			spi[i * 2 + 1] = springPairIndices[i * 2 + 1];
			sl[i] = springLengths[i];
			sc[i] = springCoefficients[i];
		}

		NvFlexUnmap(Buffers.SpringPairIndices);
		NvFlexUnmap(Buffers.SpringLengths);
		NvFlexUnmap(Buffers.SpringCoefficients);

		NvFlexSetSprings(Solver, Buffers.SpringPairIndices, Buffers.SpringLengths, Buffers.SpringCoefficients, springLengths->Count);
	}

	void Flex::SetDynamicTriangles(List<int>^ triangleIndices, List<float>^ triangleNormals) {
		if (triangleIndices->Count % 3 != 0 || triangleNormals->Count % 3 != 0)
			throw gcnew Exception("void Flex::SetDynamicTriangles(...) ---> Invalid input!");

		float3* nor = NULL;

		int* tri = (int*)NvFlexMap(Buffers.DynamicTriangleIndices, eNvFlexMapWait);
		if (triangleNormals->Count == triangleIndices->Count)
			nor = (float3*)NvFlexMap(Buffers.DynamicTriangleNormals, eNvFlexMapWait);

		for (int i = 0; i < triangleIndices->Count; i++)
			tri[i] = triangleIndices[i];

		if (nor)
			for (int i = 0; i < triangleNormals->Count / 3; i++)
				nor[i] = float3(triangleNormals[3 * i], triangleNormals[3 * i + 1], triangleNormals[3 * i + 2]);

		NvFlexUnmap(Buffers.DynamicTriangleIndices);
		if (nor) NvFlexUnmap(Buffers.DynamicTriangleNormals);

		NvFlexSetDynamicTriangles(Solver, Buffers.DynamicTriangleIndices, Buffers.DynamicTriangleNormals, triangleIndices->Count / 3);
	}

	void Flex::SetInflatables(List<int>^ startIndices, List<int>^ numTriangles, List<float>^ restVolumes, List<float>^ overPressures, List<float>^ constraintScales) {
		if (startIndices->Count != numTriangles->Count || startIndices->Count != restVolumes->Count || startIndices->Count != overPressures->Count || startIndices->Count != constraintScales->Count)
			throw gcnew Exception("void Flex::SetInflatables(...) ---> Invalid input!");

		int* si = (int*)NvFlexMap(Buffers.InflatableStartIndices, eNvFlexMapWait);
		int* nt = (int*)NvFlexMap(Buffers.InflatableNumTriangles, eNvFlexMapWait);
		float* rv = (float*)NvFlexMap(Buffers.InflatableRestVolumes, eNvFlexMapWait);
		float* op = (float*)NvFlexMap(Buffers.InflatableOverPressures, eNvFlexMapWait);
		float* cs = (float*)NvFlexMap(Buffers.InflatableConstraintScales, eNvFlexMapWait);

		for (int i = 0; i < startIndices->Count; i++) {
			si[i] = startIndices[i];
			nt[i] = numTriangles[i];
			rv[i] = restVolumes[i];
			op[i] = overPressures[i];
			cs[i] = constraintScales[i];
		}

		NvFlexUnmap(Buffers.InflatableStartIndices);
		NvFlexUnmap(Buffers.InflatableNumTriangles);
		NvFlexUnmap(Buffers.InflatableRestVolumes);
		NvFlexUnmap(Buffers.InflatableOverPressures);
		NvFlexUnmap(Buffers.InflatableConstraintScales);

		NvFlexSetInflatables(Solver, Buffers.InflatableStartIndices, Buffers.InflatableNumTriangles, Buffers.InflatableRestVolumes, Buffers.InflatableOverPressures, Buffers.InflatableConstraintScales, startIndices->Count);
	}

	//Utils
	void Flex::UpdateSolver() {
		if (numFixedIter < 2) {
			NvFlexUpdateSolver(Solver, dt, subSteps, false);
			Scene->Particles = GetParticles();
			GetRigidTransformations(Scene->RigidTranslations, Scene->RigidRotations);
		}
		else {
			for (int i = 0; i < numFixedIter; i++) {
				NvFlexUpdateSolver(Solver, dt, subSteps, false);
				Scene->Particles = GetParticles();
				GetRigidTransformations(Scene->RigidTranslations, Scene->RigidRotations);
			}
		}
	}

	void Flex::DecomposePhase(int phase, int %groupIndex, bool %selfCollision, bool %fluid) {

		if (phase < 16777216)
		{
			groupIndex = phase;
			selfCollision = false;
			fluid = false;
			return;
		}
		else if (phase < 33554432)
		{
			groupIndex = phase - 16777216;
			selfCollision = true;
			fluid = false;
			return;
		}
		else if (phase >= 67108864 && phase < 83886080)
		{
			groupIndex = phase - 67108864;
			selfCollision = false;
			fluid = true;
			return;
		}
		else if (phase >= 83886080)
		{
			groupIndex = phase - 83886080;
			selfCollision = true;
			fluid = true;
			return;
		}
		throw gcnew Exception("void Flex::DecomposePhase(...) Invalid input!");
	}

	void Flex::Destroy()
	{
		Buffers.Destroy();
		Params.numPlanes = 0;

		if (Solver) {
			NvFlexDestroySolver(Solver);
			Solver = 0;
		}
		if (Library) {
			NvFlexShutdown(Library);
			Library = 0;
		}
	}
}