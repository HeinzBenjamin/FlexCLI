// This code contains NVIDIA Confidential Information and is disclosed to you
// under a form of NVIDIA software license agreement provided separately to you.
//
// Notice
// NVIDIA Corporation and its licensors retain all intellectual property and
// proprietary rights in and to this software and related documentation and
// any modifications thereto. Any use, reproduction, disclosure, or
// distribution of this software and related documentation without an express
// license agreement from NVIDIA Corporation is strictly prohibited.
//
// ALL NVIDIA DESIGN SPECIFICATIONS, CODE ARE PROVIDED "AS IS.". NVIDIA MAKES
// NO WARRANTIES, EXPRESSED, IMPLIED, STATUTORY, OR OTHERWISE WITH RESPECT TO
// THE MATERIALS, AND EXPRESSLY DISCLAIMS ALL IMPLIED WARRANTIES OF NONINFRINGEMENT,
// MERCHANTABILITY, AND FITNESS FOR A PARTICULAR PURPOSE.
//
// Information and code furnished is believed to be accurate and reliable.
// However, NVIDIA Corporation assumes no responsibility for the consequences of use of such
// information or for any infringement of patents or other rights of third parties that may
// result from its use. No license is granted by implication or otherwise under any patent
// or patent rights of NVIDIA Corporation. Details are subject to change without notice.
// This code supersedes and replaces all information previously supplied.
// NVIDIA Corporation products are not authorized for use as critical
// components in life support devices or systems without express written approval of
// NVIDIA Corporation.
//
// Copyright (c) 2013-2017 NVIDIA Corporation. All rights reserved.

#ifndef NV_FLEX_EXT_H
#define NV_FLEX_EXT_H

#include "NvFlex.h"

#include <cassert>
#include <cstddef>

// A vector type that wraps a NvFlexBuffer, behaves like a standard vector for POD types (no construction)
// The vector must be mapped using map() before any read/write access to elements or resize operation

template <typename T>
struct NvFlexVector
{
    NvFlexVector(NvFlexLibrary* l, int size=0) : lib(l), buffer(NULL), mappedPtr(NULL), count(0), capacity(0)
    {
        if (size)
        {
            resize(size);

            // resize implicitly maps, unmap initial allocation
            unmap();
        }       
    }
    
    NvFlexVector(NvFlexLibrary* l, const T* ptr, int size) : lib(l), buffer(NULL), mappedPtr(NULL), count(0), capacity(0)
    {
        assign(ptr, size);
        unmap();
    }
    

    ~NvFlexVector() 
    {
        destroy();
    }

    NvFlexLibrary* lib;
    NvFlexBuffer* buffer;

    T* mappedPtr;
    int count;
    int capacity;

    // reinitialize the vector leaving it unmapped
    void init(int size)
    {
        destroy();
        resize(size);
        unmap();
    }

    void destroy()
    {   
        if (mappedPtr)
            NvFlexUnmap(buffer);

        if (buffer)
            NvFlexFreeBuffer(buffer);

        mappedPtr = NULL;
        buffer = NULL;
        capacity = 0;       
        count = 0;
    }

    void map(int flags=eNvFlexMapWait)
    {
        if (!buffer)
            return;

        assert(!mappedPtr);
        mappedPtr = (T*)NvFlexMap(buffer, flags);
    }
    
    void unmap()
    {
        if (!buffer)
            return;

        assert(mappedPtr);

        NvFlexUnmap(buffer);
        mappedPtr = 0;
    }

    const T& operator[](int index) const
    {   
        assert(mappedPtr);
        assert(index < count);

        return mappedPtr[index];
    }

    T& operator[](int index)
    {
        assert(mappedPtr);
        assert(index < count);

        return mappedPtr[index];
    }

    void push_back(const T& t)
    {
        assert(mappedPtr || !buffer);

        reserve(count+1);

        // copy element
        mappedPtr[count++] = t;     
    }

    void assign(const T* srcPtr, int newCount)
    {
        assert(mappedPtr || !buffer);

        resize(newCount);

        memcpy(mappedPtr, srcPtr, newCount*sizeof(T));
    }

    void copyto(T* dest, int count) 
    {
        assert(mappedPtr);

        memcpy(dest, mappedPtr, sizeof(T)*count);
    }

    int size() const { return count; }

    bool empty() const { return size() == 0; }

    const T& back() const
    {
        assert(mappedPtr);
        assert(!empty());

        return mappedPtr[count-1];
    }

    void reserve(int minCapacity)
    {
        if (minCapacity > capacity)
        {
            // growth factor of 1.5
            const int newCapacity = minCapacity*3/2;

            NvFlexBuffer* newBuf = NvFlexAllocBuffer(lib, newCapacity, sizeof(T), eNvFlexBufferHost);

            // copy contents to new buffer          
            void* newPtr = NvFlexMap(newBuf, eNvFlexMapWait);
            memcpy(newPtr, mappedPtr, count*sizeof(T));

            // unmap old buffer, but leave new buffer mapped
            unmap();
            
            if (buffer)
                NvFlexFreeBuffer(buffer);

            // swap
            buffer = newBuf;
            mappedPtr = (T*)newPtr;
            capacity = newCapacity;         
        }
    }

    // resizes mapped buffer and leaves new buffer mapped 
    void resize(int newCount)
    {
        assert(mappedPtr || !buffer);

        reserve(newCount);  

        // resize but do not initialize new entries
        count = newCount;
    }

    void resize(int newCount, const T& val)
    {
        assert(mappedPtr || !buffer);

        const int startInit = count;
        const int endInit = newCount;

        resize(newCount);

        // init any new entries
        for (int i=startInit; i < endInit; ++i)
            mappedPtr[i] = val;
    }
};

extern "C" {

struct NvFlexExtMovingFrame
{
    float position[3];
    float rotation[4];

    float velocity[3];
    float omega[3];

    float acceleration[3];
    float tau[3];

    float delta[4][4];
};

NV_FLEX_API void NvFlexExtMovingFrameInit(NvFlexExtMovingFrame* frame, const float* worldTranslation, const float* worldRotation);

/* Update a frame to a new position, this will automatically update the velocity and acceleration of
 * the frame, which can then be used to calculate inertial forces. This should be called once per-frame
 * with the new position and time-step used when moving the frame.
 *
 * @param[in] frame A pointer to a user-allocated NvFlexExtMovingFrame struct
 * @param[in] worldTranslation A pointer to a vec3 storing the frame's initial translation in world space
 * @param[in] worldRotation A pointer to a quaternion storing the frame's initial rotation in world space
 * @param[in] dt The time that elapsed since the last call to the frame update
 */
NV_FLEX_API void NvFlexExtMovingFrameUpdate(NvFlexExtMovingFrame* frame, const float* worldTranslation, const float* worldRotation, float dt);

/* Teleport particles to the frame's new position and apply the inertial forces
 *
 * @param[in] frame A pointer to a user-allocated NvFlexExtMovingFrame struct
 * @param[in] positions A pointer to an array of particle positions in (x, y, z, 1/m) format
 * @param[in] velocities A pointer to an array of particle velocities in (vx, vy, vz) format
 * @param[in] numParticles The number of particles to update
 * @param[in] linearScale How strongly the translational inertial forces should be applied, 0.0 corresponds to a purely local space simulation removing all inertial forces, 1.0 corresponds to no inertial damping and has no benefit over regular world space simulation
 * @param[in] angularScale How strongly the angular inertial forces should be applied, 0.0 corresponds to a purely local space simulation, 1.0 corresponds to no inertial damping
 * @param[in] dt The time that elapsed since the last call to the frame update, should match the value passed to NvFlexExtMovingFrameUpdate()
 */
NV_FLEX_API void NvFlexExtMovingFrameApply(NvFlexExtMovingFrame* frame, float* positions, float* velocities, int numParticles, float linearScale, float angularScale, float dt);


struct NvFlexExtAsset
{   
    // particles
    float* particles;               
    int numParticles;               
    int maxParticles;               

    // springs
    int* springIndices;             
    float* springCoefficients;      
    float* springRestLengths;       
    int numSprings;                 

    // shapes
    int* shapeIndices;              
    int numShapeIndices;            
    int* shapeOffsets;              
    float* shapeCoefficients;       
    float* shapeCenters;            
    int numShapes;                  

    // faces for cloth
    int* triangleIndices;           
    int numTriangles;               

    // inflatable params
    bool inflatable;                
    float inflatableVolume;         
    float inflatablePressure;       
    float inflatableStiffness;      
};

struct NvFlexExtInstance
{
    int* particleIndices;           
    int numParticles;               
    
    int triangleIndex;              
    int shapeIndex;                 
    int inflatableIndex;            

    float* shapeTranslations;       
    float* shapeRotations;          

    const NvFlexExtAsset* asset;    
    
    void* userData;                 
};

typedef struct NvFlexExtContainer NvFlexExtContainer;

NV_FLEX_API int NvFlexExtCreateWeldedMeshIndices(const float* vertices, int numVertices, int* uniqueVerts, int* originalToUniqueMap, float threshold);

NV_FLEX_API NvFlexExtAsset* NvFlexExtCreateClothFromMesh(const float* particles, int numParticles, const int* indices, int numTriangles, float stretchStiffness, float bendStiffness, float tetherStiffness, float tetherGive, float pressure);

NV_FLEX_API NvFlexExtAsset* NvFlexExtCreateTearingClothFromMesh(const float* particles, int numParticles, int maxParticles, const int* indices, int numTriangles, float stretchStiffness, float bendStiffness, float pressure);

NV_FLEX_API void NvFlexExtDestroyTearingCloth(NvFlexExtAsset* asset);

struct NvFlexExtTearingParticleClone
{
    int srcIndex;   
    int destIndex;
};

struct NvFlexExtTearingMeshEdit
{
    int triIndex;           // index into the triangle indices array to update
    int newParticleIndex;   // new value for the index
};

NV_FLEX_API void NvFlexExtTearClothMesh(NvFlexExtAsset* asset, float maxStrain,  int maxSplits, NvFlexExtTearingParticleClone* particleCopies, int* numParticleCopies, int maxCopies, NvFlexExtTearingMeshEdit* triangleEdits, int* numTriangleEdits, int maxEdits);

NV_FLEX_API NvFlexExtAsset* NvFlexExtCreateRigidFromMesh(const float* vertices, int numVertices, const int* indices, int numTriangleIndices, float radius, float expand);

NV_FLEX_API NvFlexExtAsset* NvFlexExtCreateSoftFromMesh(const float* vertices, int numVertices, const int* indices, int numTriangleIndices, float particleSpacing, float volumeSampling, float surfaceSampling, float clusterSpacing, float clusterRadius, float clusterStiffness, float linkRadius, float linkStiffness, float globalStiffness);

NV_FLEX_API void NvFlexExtDestroyAsset(NvFlexExtAsset* asset);

NV_FLEX_API void NvFlexExtCreateSoftMeshSkinning(const float* vertices, int numVertices, const float* bones, int numBones, float falloff, float maxDistance, float* skinningWeights, int* skinningIndices);

NV_FLEX_API NvFlexExtContainer* NvFlexExtCreateContainer(NvFlexLibrary* lib, NvFlexSolver* solver, int maxParticles);

NV_FLEX_API void NvFlexExtDestroyContainer(NvFlexExtContainer* container);

NV_FLEX_API int  NvFlexExtAllocParticles(NvFlexExtContainer* container, int n, int* indices);

NV_FLEX_API void NvFlexExtFreeParticles(NvFlexExtContainer* container, int n, const int* indices);


NV_FLEX_API int NvFlexExtGetActiveList(NvFlexExtContainer* container, int* indices);


struct NvFlexExtParticleData
{
    float* particles;       
    float* restParticles;   
    float* velocities;      
    int* phases;            
    float* normals;         

    const float* lower;     
    const float* upper;     
};

NV_FLEX_API NvFlexExtParticleData NvFlexExtMapParticleData(NvFlexExtContainer* container);
NV_FLEX_API void NvFlexExtUnmapParticleData(NvFlexExtContainer* container);

struct NvFlexExtTriangleData
{
    int* indices;       
    float* normals;     
};

NV_FLEX_API NvFlexExtTriangleData NvFlexExtMapTriangleData(NvFlexExtContainer* container);

NV_FLEX_API void NvFlexExtUnmapTriangleData(NvFlexExtContainer* container);

struct NvFlexExtShapeData
{
    float* rotations;   
    float* positions;   
    int n;              
};

NV_FLEX_API NvFlexExtShapeData NvFlexExtMapShapeData(NvFlexExtContainer* container);

NV_FLEX_API void NvFlexExtUnmapShapeData(NvFlexExtContainer* container);

NV_FLEX_API NvFlexExtInstance* NvFlexExtCreateInstance(NvFlexExtContainer* container,  NvFlexExtParticleData* particleData, const NvFlexExtAsset* asset, const float* transform, float vx, float vy, float vz, int phase, float invMassScale);

NV_FLEX_API void NvFlexExtDestroyInstance(NvFlexExtContainer* container, const NvFlexExtInstance* instance);

NV_FLEX_API void NvFlexExtNotifyAssetChanged(NvFlexExtContainer* container, const NvFlexExtAsset* asset);

NV_FLEX_API void NvFlexExtTickContainer(NvFlexExtContainer* container, float dt, int numSubsteps, bool enableTimers=false);

NV_FLEX_API void NvFlexExtPushToDevice(NvFlexExtContainer* container);

NV_FLEX_API void NvFlexExtPullFromDevice(NvFlexExtContainer* container);

NV_FLEX_API void NvFlexExtUpdateInstances(NvFlexExtContainer* container);


enum NvFlexExtForceMode
{
    eNvFlexExtModeForce             =      0,

    eNvFlexExtModeImpulse           =      1,

    eNvFlexExtModeVelocityChange    =      2,
};

struct NvFlexExtForceField
{
    float mPosition[3];     
    float mRadius;          
    float mStrength;        
    NvFlexExtForceMode mMode;   
    bool mLinearFalloff;    
};

typedef struct NvFlexExtForceFieldCallback NvFlexExtForceFieldCallback;

NV_FLEX_API NvFlexExtForceFieldCallback* NvFlexExtCreateForceFieldCallback(NvFlexSolver* solver);

NV_FLEX_API void NvFlexExtDestroyForceFieldCallback(NvFlexExtForceFieldCallback* callback);

NV_FLEX_API void NvFlexExtSetForceFields(NvFlexExtForceFieldCallback* callback, const NvFlexExtForceField* forceFields, int numForceFields);



} // extern "C"

#endif // NV_FLEX_EXT_H
