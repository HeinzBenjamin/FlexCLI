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

#ifndef NV_FLEX_H
#define NV_FLEX_H

#if _WIN32
#define NV_FLEX_API __declspec(dllexport)
#else
#define NV_FLEX_API
#endif

// least 2 significant digits define minor version, eg: 10 -> version 0.10
#define NV_FLEX_VERSION 110


extern "C" {

typedef struct NvFlexLibrary NvFlexLibrary;

typedef struct NvFlexSolver NvFlexSolver;

typedef struct NvFlexBuffer NvFlexBuffer;

enum NvFlexMapFlags
{
    eNvFlexMapWait      = 0,    
    eNvFlexMapDoNotWait = 1,    
    eNvFlexMapDiscard   = 2     
};

enum NvFlexBufferType
{
    eNvFlexBufferHost   = 0,    
    eNvFlexBufferDevice = 1,    
};

enum NvFlexRelaxationMode
{
    eNvFlexRelaxationGlobal = 0,    
    eNvFlexRelaxationLocal  = 1 
};


struct NvFlexParams
{
    int numIterations;                  

    float gravity[3];                   
    float radius;                       
    float solidRestDistance;            
    float fluidRestDistance;            

    // common params
    float dynamicFriction;              
    float staticFriction;               
    float particleFriction;             
    float restitution;                  
    float adhesion;                     
    float sleepThreshold;               
    
    float maxSpeed;                     
    float maxAcceleration;              
    
    float shockPropagation;             
    float dissipation;                  
    float damping;                      

    // cloth params
    float wind[3];                      
    float drag;                         
    float lift;                         

    // fluid params
    bool fluid;                         
    float cohesion;                     
    float surfaceTension;               
    float viscosity;                    
    float vorticityConfinement;         
    float anisotropyScale;              
    float anisotropyMin;                
    float anisotropyMax;                
    float smoothing;                    
    float solidPressure;                
    float freeSurfaceDrag;              
    float buoyancy;                     

    // diffuse params
    float diffuseThreshold;             
    float diffuseBuoyancy;              
    float diffuseDrag;                  
    int diffuseBallistic;               
    float diffuseSortAxis[3];           
    float diffuseLifetime;              

    // rigid params
    float plasticThreshold;             
    float plasticCreep;                 

    // collision params
    float collisionDistance;            
    float particleCollisionMargin;      
    float shapeCollisionMargin;         

    float planes[8][4];                 
    int numPlanes;                      

    NvFlexRelaxationMode relaxationMode;
    float relaxationFactor;             
};

enum NvFlexPhase
{
    eNvFlexPhaseGroupMask           = 0x00ffffff,   

    eNvFlexPhaseSelfCollide         = 1 << 24,      
    eNvFlexPhaseSelfCollideFilter   = 1 << 25,      
    eNvFlexPhaseFluid               = 1 << 26,      
};

NV_FLEX_API inline int NvFlexMakePhase(int group, int flags) { return (group & eNvFlexPhaseGroupMask) | flags; }


struct NvFlexTimers
{
    float predict;              
    float createCellIndices;    
    float sortCellIndices;      
    float createGrid;           
    float reorder;              
    float collideParticles;     
    float collideShapes;        
    float collideTriangles;     
    float collideFields;        
    float calculateDensity;     
    float solveDensities;       
    float solveVelocities;      
    float solveShapes;          
    float solveSprings;         
    float solveContacts;        
    float solveInflatables;     
    float applyDeltas;          
    float calculateAnisotropy;  
    float updateDiffuse;        
    float updateTriangles;      
    float updateNormals;        
    float finalize;             
    float updateBounds;         
    float total;                
};

enum NvFlexErrorSeverity
{
    eNvFlexLogError     =  0,   
    eNvFlexLogInfo      =  1,   
    eNvFlexLogWarning   =  2,   
    eNvFlexLogDebug     =  4,   
    eNvFlexLogAll       = -1,   
};

 
enum NvFlexSolverCallbackStage
{
    eNvFlexStageIterationStart, 
    eNvFlexStageIterationEnd,   
    eNvFlexStageSubstepBegin,   
    eNvFlexStageSubstepEnd,     
    eNvFlexStageUpdateEnd,      
    eNvFlexStageCount,          
};

enum NvFlexComputeType
{
    eNvFlexCUDA,        
    eNvFlexD3D11,       
    eNvFlexD3D12,       
};

struct NvFlexSolverCallbackParams
{
    NvFlexSolver* solver;               
    void* userData;                     

    float* particles;                   
    float* velocities;                  
    int* phases;                        

    int numActive;                      
    
    float dt;                           

    const int* originalToSortedMap;     
    const int* sortedToOriginalMap;     
};

struct NvFlexInitDesc
{
    int deviceIndex;                
    bool enableExtensions;          
    void* renderDevice;             
    void* renderContext;            
    
    NvFlexComputeType computeType;  
};

struct NvFlexSolverCallback
{
    void* userData;
    
    void (*function)(NvFlexSolverCallbackParams params);
};

typedef void (*NvFlexErrorCallback)(NvFlexErrorSeverity type, const char* msg, const char* file, int line);

NV_FLEX_API NvFlexLibrary* NvFlexInit(int version = NV_FLEX_VERSION, NvFlexErrorCallback errorFunc = 0, NvFlexInitDesc * desc = 0);

NV_FLEX_API void NvFlexShutdown(NvFlexLibrary* lib);

NV_FLEX_API int NvFlexGetVersion();

NV_FLEX_API NvFlexSolver* NvFlexCreateSolver(NvFlexLibrary* lib, int maxParticles, int maxDiffuseParticles, int maxNeighborsPerParticle = 96);
NV_FLEX_API void NvFlexDestroySolver(NvFlexSolver* solver);

NV_FLEX_API NvFlexLibrary* NvFlexGetSolverLibrary(NvFlexSolver* solver);

NV_FLEX_API NvFlexSolverCallback NvFlexRegisterSolverCallback(NvFlexSolver* solver, NvFlexSolverCallback function, NvFlexSolverCallbackStage stage);

NV_FLEX_API void NvFlexUpdateSolver(NvFlexSolver* solver, float dt, int substeps, bool enableTimers);

NV_FLEX_API void NvFlexSetParams(NvFlexSolver* solver, const NvFlexParams* params);

NV_FLEX_API void NvFlexGetParams(NvFlexSolver* solver, NvFlexParams* params);

NV_FLEX_API void NvFlexSetActive(NvFlexSolver* solver, NvFlexBuffer* indices, int n);

NV_FLEX_API void NvFlexGetActive(NvFlexSolver* solver, NvFlexBuffer* indices);

NV_FLEX_API int NvFlexGetActiveCount(NvFlexSolver* solver);

NV_FLEX_API void NvFlexSetParticles(NvFlexSolver* solver, NvFlexBuffer* p, int n);

NV_FLEX_API void NvFlexGetParticles(NvFlexSolver* solver, NvFlexBuffer* p, int n);

NV_FLEX_API void NvFlexSetRestParticles(NvFlexSolver* solver, NvFlexBuffer* p, int n);

NV_FLEX_API void NvFlexGetRestParticles(NvFlexSolver* solver, NvFlexBuffer* p, int n);


NV_FLEX_API void NvFlexGetSmoothParticles(NvFlexSolver* solver, NvFlexBuffer*  p, int n);

NV_FLEX_API void NvFlexSetVelocities(NvFlexSolver* solver, NvFlexBuffer*  v, int n);
NV_FLEX_API void NvFlexGetVelocities(NvFlexSolver* solver, NvFlexBuffer*  v, int n);

NV_FLEX_API void NvFlexSetPhases(NvFlexSolver* solver, NvFlexBuffer* phases, int n);
NV_FLEX_API void NvFlexGetPhases(NvFlexSolver* solver, NvFlexBuffer* phases, int n);

NV_FLEX_API void NvFlexSetNormals(NvFlexSolver* solver, NvFlexBuffer* normals, int n);

NV_FLEX_API void NvFlexGetNormals(NvFlexSolver* solver, NvFlexBuffer* normals, int n);


NV_FLEX_API void NvFlexSetSprings(NvFlexSolver* solver, NvFlexBuffer* indices, NvFlexBuffer* restLengths, NvFlexBuffer* stiffness, int numSprings);
NV_FLEX_API void NvFlexGetSprings(NvFlexSolver* solver, NvFlexBuffer* indices, NvFlexBuffer* restLengths, NvFlexBuffer* stiffness, int numSprings);

NV_FLEX_API void NvFlexSetRigids(NvFlexSolver* solver, NvFlexBuffer* offsets, NvFlexBuffer* indices, NvFlexBuffer* restPositions, NvFlexBuffer* restNormals, NvFlexBuffer* stiffness, NvFlexBuffer* rotations, NvFlexBuffer* translations, int numRigids, int numIndices);


NV_FLEX_API void NvFlexGetRigidTransforms(NvFlexSolver* solver, NvFlexBuffer* rotations, NvFlexBuffer* translations);

typedef unsigned int NvFlexTriangleMeshId;

typedef unsigned int NvFlexDistanceFieldId;

typedef unsigned int NvFlexConvexMeshId;

NV_FLEX_API NvFlexTriangleMeshId NvFlexCreateTriangleMesh(NvFlexLibrary* lib);

NV_FLEX_API void NvFlexDestroyTriangleMesh(NvFlexLibrary* lib, NvFlexTriangleMeshId mesh);

NV_FLEX_API void NvFlexUpdateTriangleMesh(NvFlexLibrary* lib, NvFlexTriangleMeshId mesh, NvFlexBuffer* vertices, NvFlexBuffer* indices, int numVertices, int numTriangles, const float* lower, const float* upper);

NV_FLEX_API void NvFlexGetTriangleMeshBounds(NvFlexLibrary* lib, const NvFlexTriangleMeshId mesh, float* lower, float* upper);

NV_FLEX_API NvFlexDistanceFieldId NvFlexCreateDistanceField(NvFlexLibrary* lib);

NV_FLEX_API void NvFlexDestroyDistanceField(NvFlexLibrary* lib, NvFlexDistanceFieldId sdf);

NV_FLEX_API void NvFlexUpdateDistanceField(NvFlexLibrary* lib, NvFlexDistanceFieldId sdf, int dimx, int dimy, int dimz, NvFlexBuffer* field);

NV_FLEX_API NvFlexConvexMeshId NvFlexCreateConvexMesh(NvFlexLibrary* lib);

NV_FLEX_API void NvFlexDestroyConvexMesh(NvFlexLibrary* lib, NvFlexConvexMeshId convex);

NV_FLEX_API void NvFlexUpdateConvexMesh(NvFlexLibrary* lib, NvFlexConvexMeshId convex, NvFlexBuffer* planes, int numPlanes, float* lower, float* upper);

NV_FLEX_API void NvFlexGetConvexMeshBounds(NvFlexLibrary* lib, NvFlexConvexMeshId mesh, float* lower, float* upper);

struct NvFlexSphereGeometry
{
    float radius;
};

struct NvFlexCapsuleGeometry
{
    float radius;
    float halfHeight;
};

struct NvFlexBoxGeometry
{
    float halfExtents[3];
};

struct NvFlexConvexMeshGeometry
{
    float scale[3];
    NvFlexConvexMeshId mesh;
};

struct NvFlexTriangleMeshGeometry
{
    float scale[3];         
    NvFlexTriangleMeshId mesh;  
};

struct NvFlexSDFGeometry
{
    float scale;                 
    NvFlexDistanceFieldId field;     
};

union NvFlexCollisionGeometry
{
    NvFlexSphereGeometry sphere;
    NvFlexCapsuleGeometry capsule;
    NvFlexBoxGeometry box;
    NvFlexConvexMeshGeometry convexMesh;
    NvFlexTriangleMeshGeometry triMesh;
    NvFlexSDFGeometry sdf;
};

enum NvFlexCollisionShapeType
{
    eNvFlexShapeSphere          = 0,        
    eNvFlexShapeCapsule         = 1,        
    eNvFlexShapeBox             = 2,        
    eNvFlexShapeConvexMesh      = 3,        
    eNvFlexShapeTriangleMesh    = 4,        
    eNvFlexShapeSDF             = 5,        
};

enum NvFlexCollisionShapeFlags
{
    eNvFlexShapeFlagTypeMask    = 0x7,      
    eNvFlexShapeFlagDynamic     = 8,        
    eNvFlexShapeFlagTrigger     = 16,       

    eNvFlexShapeFlagReserved    = 0xffffff00
};

NV_FLEX_API inline int NvFlexMakeShapeFlags(NvFlexCollisionShapeType type, bool dynamic) { return type | (dynamic?eNvFlexShapeFlagDynamic:0); }

NV_FLEX_API void NvFlexSetShapes(NvFlexSolver* solver, NvFlexBuffer* geometry, NvFlexBuffer* shapePositions, NvFlexBuffer* shapeRotations, NvFlexBuffer* shapePrevPositions, NvFlexBuffer* shapePrevRotations, NvFlexBuffer* shapeFlags, int numShapes);

NV_FLEX_API void NvFlexSetDynamicTriangles(NvFlexSolver* solver, NvFlexBuffer* indices, NvFlexBuffer* normals, int numTris);
NV_FLEX_API void NvFlexGetDynamicTriangles(NvFlexSolver* solver, NvFlexBuffer* indices, NvFlexBuffer* normals, int numTris);

NV_FLEX_API void NvFlexSetInflatables(NvFlexSolver* solver, NvFlexBuffer* startTris, NvFlexBuffer* numTris, NvFlexBuffer* restVolumes, NvFlexBuffer* overPressures, NvFlexBuffer* constraintScales, int numInflatables);

NV_FLEX_API void NvFlexGetDensities(NvFlexSolver* solver, NvFlexBuffer* densities, int n);

NV_FLEX_API void NvFlexGetAnisotropy(NvFlexSolver* solver, NvFlexBuffer* q1, NvFlexBuffer* q2, NvFlexBuffer* q3);
NV_FLEX_API int NvFlexGetDiffuseParticles(NvFlexSolver* solver, NvFlexBuffer* p, NvFlexBuffer* v, NvFlexBuffer* indices);

NV_FLEX_API void NvFlexSetDiffuseParticles(NvFlexSolver* solver, NvFlexBuffer* p, NvFlexBuffer* v, int n);

NV_FLEX_API void NvFlexGetContacts(NvFlexSolver* solver, NvFlexBuffer* planes, NvFlexBuffer* velocities, NvFlexBuffer* indices, NvFlexBuffer* counts);

NV_FLEX_API void NvFlexGetBounds(NvFlexSolver* solver, NvFlexBuffer* lower, NvFlexBuffer* upper);

NV_FLEX_API float NvFlexGetDeviceLatency(NvFlexSolver* solver);

NV_FLEX_API void NvFlexGetTimers(NvFlexSolver* solver, NvFlexTimers* timers);

struct NvFlexDetailTimer
{ 
    char* name; 
    float time;
};

NV_FLEX_API int NvFlexGetDetailTimers(NvFlexSolver* solver, NvFlexDetailTimer** timers);

NV_FLEX_API NvFlexBuffer* NvFlexAllocBuffer(NvFlexLibrary* lib, int elementCount, int elementByteStride, NvFlexBufferType type);

NV_FLEX_API void NvFlexFreeBuffer(NvFlexBuffer* buf);

NV_FLEX_API void* NvFlexMap(NvFlexBuffer* buffer, int flags);

NV_FLEX_API void NvFlexUnmap(NvFlexBuffer* buffer);

NV_FLEX_API NvFlexBuffer* NvFlexRegisterOGLBuffer(NvFlexLibrary* lib, int buf, int elementCount, int elementByteStride);

NV_FLEX_API void NvFlexUnregisterOGLBuffer(NvFlexBuffer* buf);

NV_FLEX_API NvFlexBuffer* NvFlexRegisterD3DBuffer(NvFlexLibrary* lib, void* buffer, int elementCount, int elementByteStride);

NV_FLEX_API void NvFlexUnregisterD3DBuffer(NvFlexBuffer* buf);

NV_FLEX_API void NvFlexAcquireContext(NvFlexLibrary* lib);

NV_FLEX_API void NvFlexRestoreContext(NvFlexLibrary* lib);

NV_FLEX_API const char* NvFlexGetDeviceName(NvFlexLibrary* lib);

NV_FLEX_API void NvFlexGetDeviceAndContext(NvFlexLibrary* lib, void** device, void** context);
 

NV_FLEX_API void NvFlexFlush(NvFlexLibrary* lib);


NV_FLEX_API void NvFlexSetDebug(NvFlexSolver* solver, bool enable);
NV_FLEX_API void NvFlexGetShapeBVH(NvFlexSolver* solver, void* bvh);
NV_FLEX_API void NvFlexCopySolver(NvFlexSolver* dst, NvFlexSolver* src);


} // extern "C"

#endif // NV_FLEX_H