#include "stdafx.h"
#include "FlexCLI.h"

namespace FlexCLI {

	FlexParams::FlexParams() {
#pragma region set dedault values
		NumIterations = 3;
		GravityX = 0.0f;
		GravityY = 0.0f;
		GravityZ = -9.81f;
		WindX = 0.0f;
		WindY = 0.0f;
		WindZ = 0.0f;
		Radius = 0.15f;
		Viscosity = 0.0f;
		DynamicFriction = 0.0f;
		StaticFriction = 0.0f;
		ParticleFriction = 0.0f; // scale friction between particles by default
		FreeSurfaceDrag = 0.0f;
		Drag = 0.0f;
		Lift = 0.0f;
		FluidRestDistance = 0.1f;
		SolidRestDistance = 0.15f;
		AnisotropyScale = 0.0f;
		AnisotropyMin = 0.1f;
		AnisotropyMax = 0.2f;
		Smoothing = 0.0f;
		Dissipation = 0.0f;
		Damping = 0.0f;
		ParticleCollisionMargin = 0.5f;
		ShapeCollisionMargin = 0.5f;
		CollisionDistance = 0.075f;
		PlasticThreshold = 0.0f;
		PlasticCreep = 0.0f;
		Fluid = true;
		SleepThreshold = 0.0f;
		ShockPropagation = 0.0f;
		Restitution = 0.0f;
		MaxSpeed = FLT_MAX;
		MaxAcceleration = 100.0f;	// approximately 10x gravity
		RelaxationMode = eNvFlexRelaxationLocal;
		RelaxationFactor = 1.0f;
		SolidPressure = 1.0f;
		Adhesion = 0.0f;
		Cohesion = 0.025f;
		SurfaceTension = 0.0f;
		VorticityConfinement = 0.0f;
		Buoyancy = 1.0f;
		DiffuseThreshold = FLT_MAX;
		DiffuseBuoyancy = 0.0f;
		DiffuseDrag = 0.0f;
		DiffuseBallistic = 1;
		DiffuseSortAxisX = 0.0f;
		DiffuseSortAxisY = 0.0f;
		DiffuseSortAxisZ = 0.0f;
		DiffuseLifetime = 0.0f;
		NumPlanes = 0;
#pragma endregion

		TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}
	bool FlexParams::IsValid() {
		//TODOOOO!!!!
		return Radius > 0.0f;
	};

	String^ FlexParams::ToString() {
		return "FlexParams";
	}
}