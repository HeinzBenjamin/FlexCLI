#include "stdafx.h"
#include "FlexCLI.h"

namespace FlexCLI {

	FlexSolverOptions::FlexSolverOptions() {
		dT = 0.01666667f;
		SubSteps = 3;
		NumIterations = 3;
		TimeStamp = 0;
	}

	FlexSolverOptions::FlexSolverOptions(float dt, int subSteps, int numIterations)
	{
		dT = dt;
		SubSteps = subSteps;
		NumIterations = numIterations;
		TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	bool FlexSolverOptions::IsValid() {
		return dT > 0.0f && SubSteps > 0;
	}

	String^ FlexSolverOptions::ToString() {
		String^ str = gcnew String("FlexScene:");
		str += "\ndt = " + dT.ToString();
		str += "\nSubSteps = " + SubSteps.ToString();
		str += "\nNumIter = " + NumIterations.ToString();
		str += "\n\nTimeStamp = " + TimeStamp.ToString();
		return str;
	}
}