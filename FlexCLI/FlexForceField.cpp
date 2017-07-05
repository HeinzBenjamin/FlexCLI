#include "stdafx.h"
#include "FlexCLI.h"

namespace FlexCLI {
	FlexForceField::FlexForceField(array<float>^ position, float radius, float strength, bool linearFallOff, int mode) {
		Position = position;
		LinearFallOff = linearFallOff;
		Radius = radius;
		Strength = strength;
		Mode = mode;
		TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	String^ FlexForceField::ToString() {
		String^ str = gcnew String("FlexScene:");
		str += "\nPosition = " + Position[0] + ", " + Position[1] + ", " + Position[2];
		str += "\nTimeStamp = " + TimeStamp;
		return str;
	}
}