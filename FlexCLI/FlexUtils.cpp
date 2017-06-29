#include "stdafx.h"
#include "FlexCLI.h"

namespace FlexCLI {

	/*void FlexUtils::ExtCreateWeldedMeshIndices(array<float>^ vertices, [Out] array<int>^ %uniqueVerts, [Out] array<int>^ %originalToUniqueMapping, float tolerance) {

		float*  v;
		int* outUV;
		int* outMap;

		for (int i = 0; i < vertices->Length; i++)
			v[i] = vertices[i];

		uniqueVerts = gcnew array<int>(NvFlexExtCreateWeldedMeshIndices(v, vertices->Length / 3, outUV, outMap, tolerance));
		originalToUniqueMapping = gcnew array<int>(uniqueVerts->Length);

		for (int i = 0; i < uniqueVerts->Length; i++) {
			uniqueVerts[i] = outUV[i];
			originalToUniqueMapping[i] = outMap[i];
		}
	}*/
}