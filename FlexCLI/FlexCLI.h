// FlexCLI.h
#pragma once

#include <types.h>
#include <maths.h>

#include "NvFlex.h"
//#include "NvFlexExt.h"
//#include "NvFlexDevice.h"
#include <vector>
#include <map>
#include <iostream>
#include <stdio.h>
#include <DirectXMath.h>

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Runtime::InteropServices;

typedef DirectX::XMFLOAT3 float3;
typedef DirectX::XMFLOAT4 float4;

namespace FlexCLI {

	//Always declare all classes first here. Otherwise the compiler won't find anything.
	ref class Flex;
	ref class FlexCollisionGeometry;
	ref class FlexParams;
	ref class FlexScene;
	ref class FlexParticle;
	ref struct FlexSolverOptions;
	ref class FlexUtils;

	public ref class Flex
	{
	// public: Everything accessible from FlexHopper
	public:
		Flex();
		FlexScene^ Scene;
		void SetCollisionGeometry(FlexCollisionGeometry^ flexCollisionGeometry);
		void SetParams(FlexParams^ flexParams);
		void SetScene(FlexScene^ flexScene);
		void SetSolverOptions(FlexSolverOptions^ flexSolverOptions);
		bool IsReady();
		void UpdateSolver();
		void Destroy();			
	internal:
		void SetParticles(List<FlexParticle^>^ flexParticles);
		void AddParticles(List<FlexParticle^>^ flexParticles);
		void SetRigids(List<int>^ offsets, List<int>^ indices, List<float>^ restPositions, List<float>^ restNormals, List<float>^ stiffnesses, List<float>^ rotations, List<float>^ translations);
		void SetSprings(List<int>^ springPairIndices, List<float>^ springLengths, List<float>^ springCoefficients);
		void SetDynamicTriangles(List<int>^ triangleIndices, List<float>^ normals);
		void SetInflatables(List<int>^ startIndices, List<int>^ numTriangles, List<float>^ restVolumes, List<float>^ overPressures, List<float>^ constraintScales);

		static void DecomposePhase(int phase, int %groupIndex, bool %selfCollision, bool %fluid);
		
		//called in each update cycle
		List<FlexParticle^>^ GetParticles();
		void GetRigidTransformations(List<float>^ %translations, List<float>^ %rotations);
	};

	// Structs as they is presented to .Net
	public ref class FlexParams {
	public:		
		#pragma region fields
		int NumIterations;					//!< Number of solver iterations to perform per-substep

		float GravityX;						//!< Constant acceleration applied to all particles
		float GravityY;
		float GravityZ;
		float Radius;						//!< The maximum interaction radius for particles
		float SolidRestDistance;			//!< The distance non-fluid particles attempt to maintain from each other, must be in the range (0, radius]
		float FluidRestDistance;			//!< The distance fluid particles are spaced at the rest density, must be in the range (0, radius], for fluids this should generally be 50-70% of mRadius, for rigids this can simply be the same as the particle radius

											// common params
		float DynamicFriction;				//!< Coefficient of friction used when colliding against shapes
		float StaticFriction;				//!< Coefficient of static friction used when colliding against shapes
		float ParticleFriction;				//!< Coefficient of friction used when colliding particles
		float Restitution;					//!< Coefficient of restitution used when colliding against shapes, particle collisions are always inelastic
		float Adhesion;						//!< Controls how strongly particles stick to surfaces they hit, default 0.0, range [0.0, +inf]
		float SleepThreshold;				//!< Particles with a velocity magnitude < this threshold will be considered fixed

		float MaxSpeed;						//!< The magnitude of particle velocity will be clamped to this value at the end of each step
		float MaxAcceleration;				//!< The magnitude of particle acceleration will be clamped to this value at the end of each step (limits max velocity change per-second), useful to avoid popping due to large interpenetrations

		float ShockPropagation;				//!< Artificially decrease the mass of particles based on height from a fixed reference point, this makes stacks and piles converge faster
		float Dissipation;					//!< Damps particle velocity based on how many particle contacts it has
		float Damping;						//!< Viscous drag force, applies a force proportional, and opposite to the particle velocity

											// cloth params
		float WindX;							//!< Constant acceleration applied to particles that belong to dynamic triangles, drag needs to be > 0 for wind to affect triangles
		float WindY;
		float WindZ;
		float Drag;							//!< Drag force applied to particles belonging to dynamic triangles, proportional to velocity^2*area in the negative velocity direction
		float Lift;							//!< Lift force applied to particles belonging to dynamic triangles, proportional to velocity^2*area in the direction perpendicular to velocity and (if possible), parallel to the plane normal

											// fluid params
		bool Fluid;							//!< If true then particles with phase 0 are considered fluid particles and interact using the position based fluids method
		float Cohesion;						//!< Control how strongly particles hold each other together, default: 0.025, range [0.0, +inf]
		float SurfaceTension;				//!< Controls how strongly particles attempt to minimize surface area, default: 0.0, range: [0.0, +inf]    
		float Viscosity;					//!< Smoothes particle velocities using XSPH viscosity
		float VorticityConfinement;			//!< Increases vorticity by applying rotational forces to particles
		float AnisotropyScale;				//!< Control how much anisotropy is present in resulting ellipsoids for rendering, if zero then anisotropy will not be calculated, see NvFlexGetAnisotropy()
		float AnisotropyMin;				//!< Clamp the anisotropy scale to this fraction of the radius
		float AnisotropyMax;				//!< Clamp the anisotropy scale to this fraction of the radius
		float Smoothing;					//!< Control the strength of Laplacian smoothing in particles for rendering, if zero then smoothed positions will not be calculated, see NvFlexGetSmoothParticles()
		float SolidPressure;				//!< Add pressure from solid surfaces to particles
		float FreeSurfaceDrag;				//!< Drag force applied to boundary fluid particles
		float Buoyancy;						//!< Gravity is scaled by this value for fluid particles

											// diffuse params
		float DiffuseThreshold;				//!< Particles with kinetic energy + divergence above this threshold will spawn new diffuse particles
		float DiffuseBuoyancy;				//!< Scales force opposing gravity that diffuse particles receive
		float DiffuseDrag;					//!< Scales force diffuse particles receive in direction of neighbor fluid particles
		int DiffuseBallistic;				//!< The number of neighbors below which a diffuse particle is considered ballistic
		float DiffuseSortAxisX;				//!< Diffuse particles will be sorted by depth along this axis if non-zero
		float DiffuseSortAxisY;
		float DiffuseSortAxisZ;
		float DiffuseLifetime;				//!< Time in seconds that a diffuse particle will live for after being spawned, particles will be spawned with a random lifetime in the range [0, diffuseLifetime]

											// rigid params
		float PlasticThreshold;				//!< Particles belonging to rigid shapes that move with a position delta magnitude > threshold will be permanently deformed in the rest pose
		float PlasticCreep;					//!< Controls the rate at which particles in the rest pose are deformed for particles passing the deformation threshold 

											// collision params
		float CollisionDistance;			//!< Distance particles maintain against shapes, note that for robust collision against triangle meshes this distance should be greater than zero
		float ParticleCollisionMargin;		//!< Increases the radius used during neighbor finding, this is useful if particles are expected to move significantly during a single step to ensure contacts aren't missed on subsequent iterations
		float ShapeCollisionMargin;			//!< Increases the radius used during contact finding against kinematic shapes

		float* Planes;						//!< Collision planes in the form ax + by + cz + d = 0
		int NumPlanes;						//!< Num collision planes

		int RelaxationMode;					//!< How the relaxation is applied inside the solver; 0: global, 1: local
		float RelaxationFactor;				//!< Control the convergence rate of the parallel solver, default: 1, values greater than 1 may lead to instability
		#pragma endregion
		FlexParams();
		bool IsValid();	
		String^ ToString() override;
		int TimeStamp;
	};

	public ref class FlexCollisionGeometry {
	public:
		FlexCollisionGeometry();
		
		void AddPlane(float A, float B, float C, float D);
		void AddSphere(array<float>^ centerXYZ, float radius);
		void AddBox(array<float>^ halfHeightsXYZ, array<float>^ centerXYZ, array<float>^ rotationABCD);
		void AddCapsule(float halfHeightX, float radius, array<float>^ centerXYZ, array<float>^ rotationABCD);
		void AddMesh(array<float>^ vertices, array<int>^ faces);
		void AddConvexShape(array<float>^ planes, array<float>^ upperLimit, array<float>^ lowerLimit);

		int TimeStamp;
	internal:		
		    //Plane properties
			array<float>^ Planes;
			int NumPlanes;
			//Sphere properties
			int NumSpheres;
			List<float>^ SphereCenters;
			List<float>^ SphereRadii;
			//Box properties
			int NumBoxes;
			List<float>^ BoxHalfHeights;
			List<float>^ BoxCenters;
			List<float>^ BoxRotations;
			//Capsule properties
			int NumCapsules;
			List<float>^ CapsuleHalfHeights;
			List<float>^ CapsuleRadii;
			List<float>^ CapsuleCenters;
			List<float>^ CapsuleRotations;
			//Mesh properties
			int NumMeshes;
			List<array<float>^>^ MeshVertices;
			List<array<int>^>^ MeshFaces;
			List<array<float>^>^ MeshLowerBounds;
			List<array<float>^>^ MeshUpperBounds;
			//ConvexShape properties
			int NumConvex;
			List<array<float>^>^ ConvexPlanes;
			List<array<float>^>^ ConvexLowerBounds;
			List<array<float>^>^ ConvexUpperBounds;
	};

	public ref class FlexScene {
	public:
		FlexScene();
		int NumParticles() { return Particles->Count; };

		List<FlexParticle^>^ Particles;		
		List<FlexParticle^>^ GetAllParticles();

		//Particles in general
		void RegisterParticles(array<float>^ positions, array<float>^ velocities, array<float>^ inverseMasses, bool isFluid, bool selfCollision, int groupIndex);

		//Fluids
		void RegisterFluid(array<float>^ positions, array<float>^ velocities, array<float>^ inverseMasses, int groupIndex);
		List<FlexParticle^>^ GetFluidParticles();

		//Rigids
		int NumRigids() { return RigidOffsets->Count-1; };
		void RegisterRigidBody(array<float>^ vertices, array<float>^ vertexNormals, array<float>^ velocity, array<float>^ inverseMasses, float stiffness, int groupIndex);
		List<FlexParticle^>^ GetRigidParticles();
		List<float>^ GetRigidRotations() { return RigidRotations; };
		List<float>^ GetRigidTranslations() { return RigidTranslations; };

		//Springs
		int NumSprings() { return (int)(SpringPairIndices->Count * 0.5); };
		List<int>^ GetSpringPairIndices();
		int RegisterSpringSystem(array<float>^ positions, array<float>^ velocities, array<float>^ inverseMasses, array<int>^ springPairIndices, array<float>^ stiffnesses, array<float>^ defaultLengths, bool selfCollision, array<int>^ anchorIndices, int groupIndex);
		List<FlexParticle^>^ GetSpringParticles();

		//Cloth
		int GetNumCloths() { return NumCloths; };
		void RegisterCloth(array<float>^ positions, array<float>^ velocities, array<float>^ inverseMasses, array<int>^ triangles, array<float>^ triangleNormals, float stretchStiffness, float bendingStiffness, float preTensionFactor, array<int>^ anchorIndices, int groupIndex);
		List<FlexParticle^>^ GetClothParticles();

		//Inflatables
		int GetNumInflatables() { return NumInflatables; };
		void RegisterInflatable(array<float>^ positions, array<float>^ velocities, array<float>^ inverseMasses, array<int>^ triangles, array<float>^ triangleNormals, float stretchStiffness, float bendingStiffness, float preTensionFactor, float restVolume, float overPressure, float constraintScale, array<int>^ anchorIndices, int groupIndex);
		List<FlexParticle^>^ GetInflatableParticles();		
		
		String^ ToString() override;

		int TimeStamp;

		FlexScene^ AppendScene(FlexScene^ newScene);

	internal:
		//reference to flex class
		Flex^ Flex;
		//Fluids
		List<int>^ FluidIndices;
		//Rigids
		List<int>^ RigidIndices;
		List<int>^ RigidOffsets;
		List<float>^ RigidRestPositions;
		List<float>^ RigidRestNormals;
		List<float>^ RigidStiffnesses;
		List<float>^ RigidRotations;
		List<float>^ RigidTranslations;
		List<int>^ SpringIndices;
		List<int>^ SpringPairIndices;
		List<float>^ SpringLengths;
		List<float>^ SpringStiffnesses;
		//Cloth Constraints
		int NumCloths;
		List<int>^ ClothIndices;
		//Dynamic triangles for cloth, inflatables
		List<int>^ DynamicTriangleIndices;
		List<float>^ DynamicTriangleNormals;
		//Inflatables
		int NumInflatables;
		List<int>^ InflatableIndices;
		List<int>^ InflatableStartIndices;
		List<int>^ InflatableNumTriangles;
		List<float>^ InflatableRestVolumes;
		List<float>^ InflatableOverPressures;
		List<float>^ InflatableConstraintScales;
	};

	public ref class FlexParticle {
	public:
		FlexParticle(array<float>^ position, array<float>^ velocity, float inverseMass, bool selfCollision, bool isFluid, int groupIndex, bool isActive);
		FlexParticle(array<float>^ position, array<float>^ velocity, float inverseMass, int phase, bool isActive);
		float PositionX, PositionY, PositionZ, InverseMass, VelocityX, VelocityY, VelocityZ;
		int GroupIndex;
		bool SelfCollision;
		bool IsFluid;
		int Phase;
		bool IsActive = true;
		bool IsValid();
		String^ ToString() override;
	};

	public ref struct FlexSolverOptions {
	public:
		FlexSolverOptions();
		FlexSolverOptions(float dt, int subSteps, int numIterations);
		int SubSteps;
		int NumIterations;
		float dT;
		bool IsValid();
		String^ ToString() override;
		int TimeStamp;
	};

	public ref class FlexUtils {
	//public:
		//static void ExtCreateWeldedMeshIndices(array<float>^ vertices, [Out] array<int>^ %uniqueVerts, [Out] array<int>^ %originalToUniqueMapping, float tolerance);
	};
}