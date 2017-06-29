#include "stdafx.h"
#include "FlexCLI.h"

namespace FlexCLI {

	FlexParticle::FlexParticle(array<float>^ position, array<float>^ velocity, float inverseMass, bool selfCollision, bool isFluid, int groupIndex, bool isActive) {
		if (position->Length != 3 || velocity->Length != 3 || inverseMass < 0.0f || groupIndex < 0)
			throw gcnew Exception("Invalid particle!\n" + ToString());

		PositionX = position[0];
		PositionY = position[1];
		PositionZ = position[2];
		VelocityX = velocity[0];
		VelocityY = velocity[1];
		VelocityZ = velocity[2];
		Phase = NvFlexMakePhase(groupIndex, eNvFlexPhaseFluid * isFluid | eNvFlexPhaseSelfCollide * selfCollision);
		IsFluid = isFluid;
		SelfCollision = selfCollision;
		GroupIndex = groupIndex;
		InverseMass = inverseMass;
		IsActive = isActive;
	}

	FlexParticle::FlexParticle(array<float>^ position, array<float>^ velocity, float inverseMass, int phase, bool isActive)
	{
		if (position->Length != 3 || velocity->Length != 3 || inverseMass < 0.0f || phase < 0)
			throw gcnew Exception("Invalid particle!\n" + ToString());
		else {
			PositionX = position[0];
			PositionY = position[1];
			PositionZ = position[2];
			VelocityX = velocity[0];
			VelocityY = velocity[1];
			VelocityZ = velocity[2];
			int groupIndex = 0;
			bool selfCollision = false;
			bool isFluid = false;
			Flex::DecomposePhase(phase, groupIndex, selfCollision, isFluid);
			Phase = phase;
			GroupIndex = groupIndex;
			SelfCollision = selfCollision;
			IsFluid = isFluid;
			InverseMass = inverseMass;
			IsActive = isActive;
		}
	}

	bool FlexParticle::IsValid() {
		return InverseMass >= 0.0f && GroupIndex >= 0 && Phase >= 0;
	}

	String^ FlexParticle::ToString() {
		String^ str = "Particle";
		try {
			str += "\nPosition =  {" + PositionX + ", " + PositionY + ", " + PositionZ + "}";
			str += "\nVelocity =  {" + VelocityX + ", " + VelocityY + ", " + VelocityZ + "}";
			str += "\nInverse mass = " + InverseMass;
			str += "\nIs active= " + IsActive;
			int gi = 0;
			bool sc = false;
			bool isf = false;
			Flex::DecomposePhase(Phase, gi, sc, isf);
			str += "\nGroup index = " + gi;
			str += "\nIs Fluid = " + isf;
			str += "\nSelf Collision = " + sc;
			str += "\nPhase= " + Phase;
		}
		catch (Exception^ e) { str += "FlexParticle <can't retrieve at least one property>:\n" + e->Message; }
		return str;
	}
}