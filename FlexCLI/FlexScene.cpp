#include "stdafx.h"
#include "FlexCLI.h"

namespace FlexCLI {

	FlexScene::FlexScene() {
		//general
		Particles = gcnew List<FlexParticle^>();
		//fluids
		FluidIndices = gcnew List<int>();
		//rigids
		RigidIndices = gcnew List<int>();
		RigidOffsets = gcnew List<int>();
		RigidOffsets->Add(0);
		RigidRestPositions = gcnew List<float>();
		RigidRestNormals = gcnew List<float>();
		RigidStiffnesses = gcnew List<float>();
		RigidRotations = gcnew List<float>();
		RigidTranslations = gcnew List<float>();
		//springs
		SpringPairIndices = gcnew List<int>();
		SpringIndices = gcnew List<int>();
		SpringLengths = gcnew List<float>();
		SpringStiffnesses = gcnew List<float>();
		//cloth
		NumCloths = 0;
		ClothIndices = gcnew List<int>();
		//dynamic triangles
		DynamicTriangleIndices = gcnew List<int>();
		DynamicTriangleNormals = gcnew List<float>();
		//Inflatables
		NumInflatables = 0;
		InflatableIndices = gcnew List<int>();
		InflatableStartIndices = gcnew List<int>();
		InflatableNumTriangles = gcnew List<int>();
		InflatableRestVolumes = gcnew List<float>();
		InflatableOverPressures = gcnew List<float>();
		InflatableConstraintScales = gcnew List<float>();

		TimeStamp = TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	void FlexScene::RegisterAsset(NvFlexExtAsset* asset, int groupIndex) {
		
		if (asset->numParticles == 0)
			return;

		int oldNumParticles = NumParticles();
		List<FlexParticle^>^ parts = gcnew List<FlexParticle^>();

		for (int i = 0; i < asset->numParticles; i++) {
			array<float>^ pos = gcnew array<float>{asset->particles[4 * i], asset->particles[4 * i + 1], asset->particles[4 * i + 2]};
			array<float>^ vel = gcnew array<float>{0.0f, 0.0f, 0.0f};
			float im = asset->particles[4 * i + 3];
			parts->Add(gcnew FlexParticle(pos, vel, im, NvFlexMakePhase(groupIndex, 0), true));
		}
		Particles->AddRange(parts);


		for (int i = 0; i < asset->numSprings; i++) {
			SpringPairIndices->Add(asset->springIndices[2 * i] + oldNumParticles);
			SpringPairIndices->Add(asset->springIndices[2 * i + 1] + oldNumParticles);
			SpringStiffnesses->Add(asset->springCoefficients[i]);
			SpringLengths->Add(asset->springRestLengths[i]);
			SpringIndices->Add(i + oldNumParticles);
		}

		array<int>^ shapeIndices = gcnew array<int>(asset->numShapeIndices);
		for (int i = 0; i < asset->numShapeIndices; i++) {
			shapeIndices[i] = asset->shapeIndices[i];
			RigidIndices->Add(asset->shapeIndices[i] + oldNumParticles);
		}
		array<int>^ shapeOffsets = gcnew array<int>(asset->numShapes+1);
		shapeOffsets[0] = 0;
		int oldOffsetPosition = RigidOffsets[RigidOffsets->Count - 1];
		array<float>^ shapeCoefficients = gcnew array<float>(asset->numShapes);
		for (int i = 0; i < asset->numShapes; i++) {
			shapeOffsets[i+1] = asset->shapeOffsets[i];
			RigidOffsets->Add(asset->shapeOffsets[i] + oldOffsetPosition);
			RigidStiffnesses->Add(asset->shapeCoefficients[i]);
			RigidTranslations->Add(0.0f);
			RigidTranslations->Add(0.0f);
			RigidTranslations->Add(0.0f);
			RigidRotations->Add(0.0f);
			RigidRotations->Add(0.0f);
			RigidRotations->Add(0.0f);
			RigidRotations->Add(0.0f);
			shapeCoefficients[i] = asset->shapeCoefficients[i];
			for (int j = shapeOffsets[i]; j < shapeOffsets[i + 1]; j++)
			{
				RigidRestPositions->Add(parts[shapeIndices[j]]->PositionX - asset->shapeCenters[3 * i]);
				RigidRestPositions->Add(parts[shapeIndices[j]]->PositionY - asset->shapeCenters[3 * i + 1]);
				RigidRestPositions->Add(parts[shapeIndices[j]]->PositionZ - asset->shapeCenters[3 * i + 2]);
				RigidRestNormals->Add(0.0f);
				RigidRestNormals->Add(0.0f);
				RigidRestNormals->Add(0.0f);
				RigidRestNormals->Add(-0.5f);
			}
		}


		TimeStamp = TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;

	}

	void FlexScene::RegisterParticles(array<float>^ positions, array<float>^ velocities, array<float>^ inverseMasses, bool isFluid, bool selfCollision, int groupIndex) {
		if (positions->Length % 3 != 0 || velocities->Length % 3 != 0 || positions->Length != velocities->Length || positions->Length / 3 != inverseMasses->Length)
			throw gcnew Exception("FlexScene::RegisterParticles(...) Invalid input!");

		int currentNumParticles = positions->Length / 3;

		for (int i = 0; i < currentNumParticles; i++) {
			//Add each particle to the scene global Particles list
			array<float>^ pos = gcnew array<float>(3) { positions[i * 3], positions[i * 3 + 1], positions[i * 3 + 2] };
			array<float>^ vel = gcnew array<float>(3) { velocities[i * 3], velocities[i * 3 + 1], velocities[i * 3 + 2] };
			Particles->Add(gcnew FlexParticle(pos, vel, inverseMasses[i], selfCollision, isFluid, groupIndex, true));
		}

		TimeStamp = TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	///<summary>
	///Specify one group of fluid particles
	///</summary>
	///<param name = 'positions'>Flat array of particle positions [x, y, z]. Must be of length 3 * nr. of vertices</param>
	///<param name = 'velocities'>Flat array of particle velocities [x, y, z]. Should be of same length as 'vertices'.</param>
	///<param name = 'inverseMass'>Supply inverse mass per particle. Alternatively supply array of length one, value will be assigned to every vertex particle.</param>
	///<param name = 'groupIndex'>A uniquely used index between 0 and 2^24. All particles in this group will be identified by the group index in the future.</param>
	void FlexScene::RegisterFluid(array<float>^ positions, array<float>^ velocities, array<float>^ inverseMasses, int groupIndex) {
#pragma region check everything
		for each (FlexParticle^ p in Particles)
			if (p->GroupIndex == groupIndex)
				throw gcnew Exception("Fluid: Group index " + groupIndex + " already in use!");
		if (positions->Length % 3 != 0 || velocities->Length % 3 != 0 || positions->Length != velocities->Length)
			throw gcnew Exception("FlexScene::RegisterFluid(...) Invalid input!");
#pragma endregion

		int currentNumParticles = positions->Length / 3;

		for (int i = 0; i < currentNumParticles; i++) {
			//Add each particle to the scene global Particles list
			array<float>^ pos = gcnew array<float>(3) { positions[i * 3], positions[i * 3 + 1], positions[i * 3 + 2] };
			array<float>^ vel = gcnew array<float>(3) { velocities[i * 3], velocities[i * 3 + 1], velocities[i * 3 + 2] };
			FluidIndices->Add(NumParticles());
			Particles->Add(gcnew FlexParticle(pos, vel, inverseMasses[i], NvFlexMakePhase(groupIndex, eNvFlexPhaseSelfCollide | eNvFlexPhaseFluid), true));
		}

		TimeStamp = TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	///<summary>
	///Specify one rigid body
	///</summary>
	///<param name = 'vertices'>Flat array of vertex positions. Must be of length 3 * nr. of vertices</param>
	///<param name = 'vertexNormals'>Flat array of normal vectors in vertices. Should be of same length as 'vertices'.</param>
	///<param name = 'velocity'>Intial velocity acting on the rigid body.</param>
	///<param name = 'inverseMass'>Supply inverse mass per particle. Alternatively supply array of length one, value will be assigned to every vertex particle.</param>
	///<param name = 'stiffness'>Stiffness between 0.0 and 1.0</param>
	///<param name = 'groupIndex'>A uniquely used index between 0 and 2^24. All particles in this object will be identified by the group index in the future.</param>
	void FlexScene::RegisterRigidBody(array<float>^ vertices, array<float>^ vertexNormals, array<float>^ velocity, array<float>^ inverseMasses, float stiffness, int groupIndex) {
#pragma region check everything
		for each (FlexParticle^ p in Particles)
			if (p->GroupIndex == groupIndex)
				throw gcnew Exception("Rigid Body: Group index " + groupIndex + " already in use!");
		if (vertices->Length % 3 != 0 || vertexNormals->Length % 3 != 0 || velocity->Length != 3 || stiffness < 0.0f || stiffness > 1.0f)
			throw gcnew Exception("FlexScene::RegisterRigidBody(...) Invalid input!");

		if (inverseMasses->Length == 1)
		{
			float im = inverseMasses[0];
			inverseMasses = gcnew array<float>(vertices->Length / 3);
			for (int i = 0; i < vertices->Length / 3; i++)
				inverseMasses[i] = im;
		}

		for each(float i in inverseMasses)
			if (i < 0.0f)
				throw gcnew Exception("FlexScene::RegisterRigidBody(...) Invalid input! Inverse mass must be >= 0.0.");
#pragma endregion

		int currentNumParticles = vertices->Length / 3;

		array<float>^ vel = gcnew array<float>(3) { velocity[0], velocity[1], velocity[2] };
		float3 massCenter = float3(0.0f, 0.0f, 0.0f);

		for (int i = 0; i < currentNumParticles; i++) {
			//Add each particle to the scene global Particles list
			array<float>^ pos = gcnew array<float>(3) { vertices[i * 3], vertices[i * 3 + 1], vertices[i * 3 + 2] };

			RigidIndices->Add(NumParticles());
			Particles->Add(gcnew FlexParticle(pos, vel, inverseMasses[i], NvFlexMakePhase(groupIndex, 0), true));
			massCenter.x += vertices[i * 3];
			massCenter.y += vertices[i * 3 + 1];
			massCenter.z += vertices[i * 3 + 2];
		}
		massCenter.x /= (float)currentNumParticles;
		massCenter.y /= (float)currentNumParticles;
		massCenter.z /= (float)currentNumParticles;

		for (int i = 0; i < currentNumParticles; i++) {
			RigidRestPositions->Add(vertices[i * 3] - massCenter.x);
			RigidRestPositions->Add(vertices[i * 3 + 1] - massCenter.y);
			RigidRestPositions->Add(vertices[i * 3 + 2] - massCenter.z);
			RigidRestNormals->Add(vertexNormals[i * 3]);
			RigidRestNormals->Add(vertexNormals[i * 3 + 1]);
			RigidRestNormals->Add(vertexNormals[i * 3 + 2]);
			RigidRestNormals->Add(-0.5f);
		}

		RigidOffsets->Add(RigidOffsets[RigidOffsets->Count - 1] + currentNumParticles);

		RigidStiffnesses->Add(stiffness);

		RigidRotations->Add(0.0f);
		RigidRotations->Add(0.0f);
		RigidRotations->Add(0.0f);
		RigidRotations->Add(0.0f);

		RigidTranslations->Add(0.0f);
		RigidTranslations->Add(0.0f);
		RigidTranslations->Add(0.0f);

		TimeStamp = TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	void FlexScene::RegisterSoftBody(array<float>^ vertices, array<int>^ triangles, float particleSpacing, float volumeSampling, float surfaceSampling, float clusterSpacing, float clusterRadius, float clusterStiffness, float linkRadius, float linkStiffness, float globalStiffness, array<int>^ anchorIndices, int groupIndex) {
		#pragma region check everthing
		for each (FlexParticle^ p in Particles)
			if (p->GroupIndex == groupIndex)
				throw gcnew Exception("Soft Body: Group index " + groupIndex + " already in use!");
		if (vertices->Length == 0 || vertices->Length % 3 != 0 || triangles->Length % 3 != 0)
			throw gcnew Exception("FlexScene::RegisterSoftBody(...) Invalid input!");
		#pragma endregion

		std::vector<float> vt(vertices->Length);
		for (int i = 0; i < vertices->Length; i++) vt[i] = vertices[i];

		std::vector<int> tr(triangles->Length);
		for (int i = 0; i < triangles->Length; i++) tr[i] = triangles[i];

		NvFlexExtAsset* asset = NvFlexExtCreateSoftFromMesh(&vt[0], vertices->Length / 3, &tr[0], triangles->Length, particleSpacing, volumeSampling, surfaceSampling, clusterSpacing, clusterRadius, clusterStiffness, linkRadius, linkStiffness, globalStiffness);

		RegisterAsset(asset, groupIndex);

		NvFlexExtDestroyAsset(asset);
	}

	///<summary>
	///Specify one spring system by spring pair indices
	///</summary>
	///<returns>The offset in spring indices resulting from previously registered spring systems. Use this to redraw the spring lines correctly later on.</returns>
	int FlexScene::RegisterSpringSystem(array<float>^ positions, array<float>^ velocities, array<float>^ inverseMasses, array<int>^ springPairIndices, array<float>^ stiffnesses, array<float>^ defaultLengths, bool selfCollision, array<int>^ anchorIndices, int groupIndex) {
#pragma region check everything
		for each (FlexParticle^ p in Particles)
			if (p->GroupIndex == groupIndex)
				throw gcnew Exception("Spring System: Group index " + groupIndex + " already in use!");
		if (positions->Length % 3 != 0 || velocities->Length % 3 != 0 || positions->Length != velocities->Length || springPairIndices->Length % 2 != 0)
			throw gcnew Exception("FlexScene::RegisterFluid(...) Invalid input!");
#pragma endregion

		int maxIndex = 0;
		for (int i = 0; i < springPairIndices->Length / 2; i++) {
			int ind0 = springPairIndices[2 * i] + NumParticles();
			int ind1 = springPairIndices[2 * i + 1] + NumParticles();
			if (ind0 > maxIndex)
				maxIndex = ind0;
			if (ind1 > maxIndex)
				maxIndex = ind1;
			SpringPairIndices->Add(ind0);
			SpringPairIndices->Add(ind1);
			SpringStiffnesses->Add(stiffnesses[i]);
			SpringLengths->Add(defaultLengths[i]);
		}

		int currentNumParticles = positions->Length / 3;

		if (maxIndex - NumParticles() >= currentNumParticles)
			throw gcnew Exception("FlexCLI: void FlexScene::RegisterSpringSystem(...) ---> At least one spring index is too high.");

		int toReturn = SpringIndices->Count;

		int anchorCounter = 0;
		anchorIndices->Sort(anchorIndices);
		for (int i = 0; i < currentNumParticles; i++) {
			//Add each particle to the scene global Particles list
			array<float>^ pos = gcnew array<float>(3) { positions[i * 3], positions[i * 3 + 1], positions[i * 3 + 2] };
			array<float>^ vel = gcnew array<float>(3) { velocities[i * 3], velocities[i * 3 + 1], velocities[i * 3 + 2] };
			float iM = inverseMasses[i];
			if (anchorIndices->Length > anchorCounter && anchorIndices[anchorCounter] == i) {
				iM = 0.0f;
				anchorCounter++;
			}
			SpringIndices->Add(NumParticles());
			Particles->Add(gcnew FlexParticle(pos, vel, iM, selfCollision, false, groupIndex, true));


			//NumParticles++;
		}
		TimeStamp = TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
		return toReturn;
	}

	///<summary>
	///Specify one cloth
	///</summary>
	void FlexScene::RegisterCloth(array<float>^ positions, array<float>^ velocities, array<float>^ inverseMasses, array<int>^ triangles, array<float>^ triangleNormals, float stretchStiffness, float bendingStiffness, float preTensionFactor, array<int>^ anchorIndices, int groupIndex) {
#pragma region check everything
		if (triangles->Length % 3 != 0 || positions->Length % 3 != 0 || velocities->Length % 3 != 0 || inverseMasses->Length * 3 != positions->Length || positions->Length != velocities->Length || stretchStiffness < 0.0f || bendingStiffness < 0.0f)
			throw gcnew Exception("void FlexScene::RegisterCloth(...) ---> Invalid input!");
		for each (FlexParticle^ p in Particles)
			if (p->GroupIndex == groupIndex)
				throw gcnew Exception("Cloth: Group index " + groupIndex + " already in use!");
#pragma endregion

		List<int>^ springPairIndices = gcnew List<int>();
		List<float>^ lengths = gcnew List<float>();
		List<float>^ stretchStiffnesses = gcnew List<float>();

		for (int i = 0; i < triangles->Length / 3; i++)
		{
			//assign triangle indices
			DynamicTriangleIndices->Add(triangles[i * 3] + NumParticles());
			DynamicTriangleIndices->Add(triangles[i * 3 + 1] + NumParticles());
			DynamicTriangleIndices->Add(triangles[i * 3 + 2] + NumParticles());


			//only use unique lines and points
			int alreadyExistsAs = -1;
			int candStart = triangles[3 * i];
			int candEnd = triangles[3 * i + 1];
			for (int j = 0; j < springPairIndices->Count / 2; j++) {
				if ((candStart == springPairIndices[j * 2] && candEnd == springPairIndices[j * 2 + 1]) ||
					(candStart == springPairIndices[j * 2 + 1] && candEnd == springPairIndices[j * 2])) {
					alreadyExistsAs = j;
					break;
				}
			}
			if (alreadyExistsAs == -1) {
				springPairIndices->Add(candStart);
				springPairIndices->Add(candEnd);
				stretchStiffnesses->Add(stretchStiffness);
				float newLength = Math::Sqrt(
					(positions[candStart * 3] - positions[candEnd * 3]) * (positions[candStart * 3] - positions[candEnd * 3]) +
					(positions[candStart * 3 + 1] - positions[candEnd * 3 + 1]) * (positions[candStart * 3 + 1] - positions[candEnd * 3 + 1]) +
					(positions[candStart * 3 + 2] - positions[candEnd * 3 + 2]) * (positions[candStart * 3 + 2] - positions[candEnd * 3 + 2])
				);
				lengths->Add(preTensionFactor * newLength);
			}

			alreadyExistsAs = -1;
			candStart = triangles[3 * i + 1];
			candEnd = triangles[3 * i + 2];
			for (int j = 0; j < springPairIndices->Count / 2; j++) {
				if ((candStart == springPairIndices[j * 2] && candEnd == springPairIndices[j * 2 + 1]) ||
					(candStart == springPairIndices[j * 2 + 1] && candEnd == springPairIndices[j * 2])) {
					alreadyExistsAs = j;
					break;
				}
			}
			if (alreadyExistsAs == -1) {
				springPairIndices->Add(candStart);
				springPairIndices->Add(candEnd);
				stretchStiffnesses->Add(stretchStiffness);
				float newLength = Math::Sqrt(
					(positions[candStart * 3] - positions[candEnd * 3]) * (positions[candStart * 3] - positions[candEnd * 3]) +
					(positions[candStart * 3 + 1] - positions[candEnd * 3 + 1]) * (positions[candStart * 3 + 1] - positions[candEnd * 3 + 1]) +
					(positions[candStart * 3 + 2] - positions[candEnd * 3 + 2]) * (positions[candStart * 3 + 2] - positions[candEnd * 3 + 2])
				);
				lengths->Add(preTensionFactor * newLength);
			}

			alreadyExistsAs = -1;
			candStart = triangles[3 * i + 2];
			candEnd = triangles[3 * i];
			for (int j = 0; j < springPairIndices->Count / 2; j++) {
				if ((candStart == springPairIndices[j * 2] && candEnd == springPairIndices[j * 2 + 1]) ||
					(candStart == springPairIndices[j * 2 + 1] && candEnd == springPairIndices[j * 2])) {
					alreadyExistsAs = j;
					break;
				}
			}
			if (alreadyExistsAs == -1) {
				springPairIndices->Add(candStart);
				springPairIndices->Add(candEnd);
				stretchStiffnesses->Add(stretchStiffness);
				float newLength = Math::Sqrt(
					(positions[candStart * 3] - positions[candEnd * 3]) * (positions[candStart * 3] - positions[candEnd * 3]) +
					(positions[candStart * 3 + 1] - positions[candEnd * 3 + 1]) * (positions[candStart * 3 + 1] - positions[candEnd * 3 + 1]) +
					(positions[candStart * 3 + 2] - positions[candEnd * 3 + 2]) * (positions[candStart * 3 + 2] - positions[candEnd * 3 + 2])
				);
				lengths->Add(preTensionFactor * newLength);
			}
		}

		List<int>^ bendingSprings = gcnew List<int>();
		if (bendingStiffness > 0.0f) {
			for (int i = 0; i < springPairIndices->Count / 2; i++) {
				int startIndex = springPairIndices[2 * i];
				int endIndex = springPairIndices[2 * i + 1];

				List<int>^ neighborsOfStart = gcnew List<int>();
				List<int>^ neighborsOfEnd = gcnew List<int>();
				for (int j = 0; j < springPairIndices->Count / 2; j++) {
					if (springPairIndices[2 * j] == startIndex && springPairIndices[2 * j + 1] != endIndex)
						neighborsOfStart->Add(springPairIndices[2 * j + 1]);
					else if (springPairIndices[2 * j + 1] == startIndex && springPairIndices[2 * j] != endIndex)
						neighborsOfStart->Add(springPairIndices[2 * j]);

					if (springPairIndices[2 * j] == endIndex && springPairIndices[2 * j + 1] != startIndex)
						neighborsOfEnd->Add(springPairIndices[2 * j + 1]);
					else if (springPairIndices[2 * j + 1] == endIndex && springPairIndices[2 * j] != startIndex)
						neighborsOfEnd->Add(springPairIndices[2 * j]);
				}


				List<int>^ commonNeighbors = gcnew List<int>();
				for (int j = 0; j < neighborsOfStart->Count; j++) {
					for (int k = 0; k < neighborsOfEnd->Count; k++) {
						if (neighborsOfStart[j] == neighborsOfEnd[k])
							commonNeighbors->Add(neighborsOfStart[j]);
					}
				}

				if (commonNeighbors->Count == 2) {
					bendingSprings->AddRange(commonNeighbors);
					stretchStiffnesses->Add(bendingStiffness);
					float newLength = Math::Sqrt(
						(positions[commonNeighbors[0] * 3] - positions[commonNeighbors[1] * 3]) * (positions[commonNeighbors[0] * 3] - positions[commonNeighbors[1] * 3]) +
						(positions[commonNeighbors[0] * 3 + 1] - positions[commonNeighbors[1] * 3 + 1]) * (positions[commonNeighbors[0] * 3 + 1] - positions[commonNeighbors[1] * 3 + 1]) +
						(positions[commonNeighbors[0] * 3 + 2] - positions[commonNeighbors[1] * 3 + 2]) * (positions[commonNeighbors[0] * 3 + 2] - positions[commonNeighbors[1] * 3 + 2])
					);
					lengths->Add(preTensionFactor * newLength);
				}
			}

			springPairIndices->AddRange(bendingSprings);
		}

		//Assign springs
		int oldNumParticles = RegisterSpringSystem(positions, velocities, inverseMasses, springPairIndices->ToArray(), stretchStiffnesses->ToArray(), lengths->ToArray(), true, anchorIndices, groupIndex);
		ClothIndices->AddRange(SpringIndices->GetRange(oldNumParticles, positions->Length / 3));
		SpringIndices->RemoveRange(oldNumParticles, positions->Length / 3);

		if (triangles->Length == triangleNormals->Length)
			DynamicTriangleNormals->AddRange(triangleNormals);

		NumCloths++;
		TimeStamp = TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	void FlexScene::RegisterInflatable(array<float>^ positions, array<float>^ velocities, array<float>^ inverseMasses, array<int>^ triangles, array<float>^ triangleNormals, float stretchStiffness, float bendingStiffness, float preTensionFactor, float restVolume, float overPressure, float constraintScale, array<int>^ anchorIndices, int groupIndex) {
#pragma region check everything
		if (triangles->Length % 3 != 0 || positions->Length % 3 != 0 || velocities->Length % 3 != 0 || inverseMasses->Length * 3 != positions->Length || positions->Length != velocities->Length || triangleNormals->Length != triangles->Length || restVolume < 0.0f || constraintScale < 0.0f)
			throw gcnew Exception("void FlexScene::RegisterInflatable(...) ---> Invalid input!");
		for each (FlexParticle^ p in Particles)
			if (p->GroupIndex == groupIndex)
				throw gcnew Exception("Inflatable: Group index " + groupIndex + "already in use!");
#pragma endregion

		InflatableStartIndices->Add(NumParticles());
		int oldNumParticles = ClothIndices->Count;
		RegisterCloth(positions, velocities, inverseMasses, triangles, triangleNormals, stretchStiffness, bendingStiffness, preTensionFactor, anchorIndices, groupIndex);
		InflatableIndices->AddRange(ClothIndices->GetRange(oldNumParticles, positions->Length / 3));
		ClothIndices->RemoveRange(oldNumParticles, positions->Length / 3);

		InflatableConstraintScales->Add(constraintScale);
		InflatableOverPressures->Add(overPressure);
		InflatableRestVolumes->Add(restVolume);

		InflatableNumTriangles->Add(triangles->Length / 3);

		NumInflatables++;
		TimeStamp = TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	void FlexScene::RegisterCustomConstraints(array<int>^ anchorIndices, array<int>^ shapeMatchingIndices, float shapeStiffness, array<int>^ springPairIndices, array<float>^ springStiffnesses, array<float>^ springDefaultLengths, array<int>^ triangleIndices, array<float>^ triangleNormals) {
	#pragma region check everything
		if (triangleIndices->Length % 3 != 0 || springPairIndices->Length % 2 != 0 || springPairIndices->Length / 2 != springStiffnesses->Length || springPairIndices->Length / 2 != springDefaultLengths->Length)
			throw gcnew Exception("void FlexScene::RegisterCustomConstraints(...) ---> Invalid input!");
	#pragma endregion

		for (int i = 0; i < anchorIndices->Length; i++)
			Particles[anchorIndices[i]]->InverseMass = 0.0f;
		
		if (shapeMatchingIndices->Length > 0) {
			RigidIndices->AddRange(shapeMatchingIndices);
			float3 massCenter = float3( 0.0f,0.0f,0.0f );
			for (int i = 0; i < shapeMatchingIndices->Length; i++) {
				massCenter.x += Particles[shapeMatchingIndices[i]]->PositionX;
				massCenter.y += Particles[shapeMatchingIndices[i]]->PositionY;
				massCenter.z += Particles[shapeMatchingIndices[i]]->PositionZ;				
			}
			massCenter.x /= shapeMatchingIndices->Length;
			massCenter.y /= shapeMatchingIndices->Length;
			massCenter.z /= shapeMatchingIndices->Length;

			for (int i = 0; i < shapeMatchingIndices->Length; i++) {
				RigidRestNormals->Add(0.0f);
				RigidRestNormals->Add(0.0f);
				RigidRestNormals->Add(0.0f);
				RigidRestNormals->Add(-0.5f);
				RigidRestPositions->Add(Particles[shapeMatchingIndices[i]]->PositionX - massCenter.x);
				RigidRestPositions->Add(Particles[shapeMatchingIndices[i]]->PositionY - massCenter.y);
				RigidRestPositions->Add(Particles[shapeMatchingIndices[i]]->PositionZ - massCenter.z);
				RigidRotations->Add(0.0f);
				RigidRotations->Add(0.0f);
				RigidRotations->Add(0.0f);
				RigidRotations->Add(0.0f);
				RigidTranslations->Add(0.0f);
				RigidTranslations->Add(0.0f);
				RigidTranslations->Add(0.0f);
			}

			RigidOffsets->Add(shapeMatchingIndices->Length + RigidOffsets[RigidOffsets->Count - 1]);
			RigidStiffnesses->Add(shapeStiffness);
		}

		if (springPairIndices->Length > 0) {
			SpringPairIndices->AddRange(springPairIndices);
			SpringStiffnesses->AddRange(springStiffnesses);


			for (int i = 0; i < springDefaultLengths->Length; i++) {
				if (springDefaultLengths[i] >= 0.0f)
					SpringLengths->Add(springDefaultLengths[i]);
				else {
					float distX = Particles[springPairIndices[2 * i]]->PositionX - Particles[springPairIndices[2 * i + 1]]->PositionX;
					float distY = Particles[springPairIndices[2 * i]]->PositionY - Particles[springPairIndices[2 * i + 1]]->PositionY;
					float distZ = Particles[springPairIndices[2 * i]]->PositionZ - Particles[springPairIndices[2 * i + 1]]->PositionZ;

					float distance = Math::Sqrt(distX * distX + distY * distY + distZ * distZ);

					SpringLengths->Add(distance * springDefaultLengths[i] * -1.0);
				}
			}

			for (int i = 0; i < springPairIndices->Length; i++) {
				for each(int j in SpringIndices) {
					if (j == springPairIndices[i])
						goto breakLoop;
				}
				SpringIndices->Add(springPairIndices[i]);
			breakLoop:
				int doNothing = 0;
			}
		}

		if (triangleIndices->Length > 0) {
			DynamicTriangleIndices->AddRange(triangleIndices);
			DynamicTriangleNormals->AddRange(triangleNormals);
		}

		TimeStamp = TimeStamp = System::DateTime::Now.Minute * 60000 + System::DateTime::Now.Second * 1000 + System::DateTime::Now.Millisecond;
	}

	List<FlexParticle^>^ FlexScene::GetAllParticles() {
		return Particles;
	}

	List<FlexParticle^>^ FlexScene::GetFluidParticles() {
		List<FlexParticle^>^ particles = gcnew List<FlexParticle^>();

		for (int i = 0; i < FluidIndices->Count; i++)
			particles->Add(Particles[FluidIndices[i]]);

		return particles;
	}

	List<FlexParticle^>^ FlexScene::GetRigidParticles() {
		List<FlexParticle^>^ particles = gcnew List<FlexParticle^>();

		for (int numRigid = 0; numRigid < NumRigids(); numRigid++) {
			for (int j = RigidOffsets[numRigid]; j < RigidOffsets[numRigid + 1]; j++) {
				FlexParticle^ part = Particles[RigidIndices[j]];

				particles->Add(part);
			}
		}

		return particles;
	}

	List<FlexParticle^>^ FlexScene::GetSpringParticles() {
		List<FlexParticle^>^ particles = gcnew List<FlexParticle^>();

		for (int i = 0; i < SpringIndices->Count; i++)
			particles->Add(Particles[SpringIndices[i]]);

		return particles;
	}

	List<FlexParticle^>^ FlexScene::GetClothParticles() {
		List<FlexParticle^>^ particles = gcnew List<FlexParticle^>();

		for (int i = 0; i < ClothIndices->Count; i++)
			particles->Add(Particles[ClothIndices[i]]);

		return particles;
	}

	List<FlexParticle^>^ FlexScene::GetInflatableParticles() {
		List<FlexParticle^>^ particles = gcnew List<FlexParticle^>();

		for (int i = 0; i < InflatableIndices->Count; i++)
			particles->Add(Particles[InflatableIndices[i]]);

		return particles;
	}

	List<int>^ FlexScene::GetSpringPairIndices() {
		return SpringPairIndices;
	}

	FlexScene^ FlexScene::AppendScene(FlexScene^ newScene) {

		//general particle stuff
		int oldNumParticles = this->NumParticles();
		this->Particles->AddRange(newScene->Particles);

		//fluids
		for each(int fi in newScene->FluidIndices)
			this->FluidIndices->Add(fi + oldNumParticles);

		//rigids
		int oldRigidOffset = this->RigidOffsets[this->RigidOffsets->Count - 1];
		for each(int ri in newScene->RigidIndices)
			this->RigidIndices->Add(ri + oldNumParticles); {}
		for each(int ro in newScene->RigidOffsets)
			if (ro != 0 || oldRigidOffset == 0 && !(ro == 0 && oldRigidOffset == 0))
				this->RigidOffsets->Add(ro + oldRigidOffset);
		this->RigidRotations->AddRange(newScene->RigidRotations);
		this->RigidTranslations->AddRange(newScene->RigidTranslations);
		this->RigidRestNormals->AddRange(newScene->RigidRestNormals);
		this->RigidRestPositions->AddRange(newScene->RigidRestPositions);
		this->RigidStiffnesses->AddRange(newScene->RigidStiffnesses);

		//springs
		for each(int si in newScene->SpringIndices)
			this->SpringIndices->Add(si + oldNumParticles);
		for each(int spi in newScene->SpringPairIndices)
			this->SpringPairIndices->Add(spi + oldNumParticles);
		this->SpringLengths->AddRange(newScene->SpringLengths);
		this->SpringStiffnesses->AddRange(newScene->SpringStiffnesses);

		//cloth
		for each(int ci in newScene->ClothIndices)
			this->ClothIndices->Add(ci + oldNumParticles);

		for each(int di in newScene->DynamicTriangleIndices)
			this->DynamicTriangleIndices->Add(di + oldNumParticles);
		this->DynamicTriangleNormals->AddRange(newScene->DynamicTriangleNormals);

		//inflatables
		this->InflatableConstraintScales->AddRange(newScene->InflatableConstraintScales);
		this->InflatableOverPressures->AddRange(newScene->InflatableOverPressures);
		this->InflatableRestVolumes->AddRange(newScene->InflatableRestVolumes);
		for each(int ii in newScene->InflatableIndices)
			this->InflatableIndices->Add(ii + oldNumParticles);
		this->InflatableNumTriangles->AddRange(newScene->InflatableNumTriangles);
		for each(int is in newScene->InflatableStartIndices)
			this->InflatableStartIndices->Add(is + oldNumParticles);


		this->TimeStamp = newScene->TimeStamp;
		return this;
	};

	bool FlexScene::IsValid() {
		return Particles->Count > 0;
	}

	String^ FlexScene::ToString() {
		String^ str = gcnew String("FlexScene:");
		str += "\nNumParticles = " + NumParticles();
		str += "\nconsisting of:";
		str += "\nNumFluidParticles = " + FluidIndices->Count;
		str += "\nNumRigidParticles = " + RigidIndices->Count;
		str += "\nNumRigidBodies = " + (RigidOffsets->Count - 1);
		str += "\nNumSpringParticles = " + SpringIndices->Count;
		str += "\nNumSprings = " + (SpringPairIndices->Count / 2);
		str += "\nNumClothParticles = " + ClothIndices->Count;
		str += "\nNumDynamicTriangles = " + DynamicTriangleIndices->Count / 3;
		str += "\nNumInflatables = " + NumInflatables;
		str += "\n\nTimeStamp = " + TimeStamp.ToString();
		return str;
	}
}

