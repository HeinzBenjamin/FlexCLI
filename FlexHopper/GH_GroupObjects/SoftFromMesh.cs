﻿using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using FlexHopper.Properties;

using Rhino.Geometry;

namespace FlexHopper.GH_GroupObjects
{
    public class SoftFromMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SoftFromMesh class.
        /// </summary>
        public SoftFromMesh()
          : base("Soft From Mesh", "Soft",
              "<CAUTION> Soft params heavily influence computation time when adding soft body to a scene.",
              "Flex", "Composition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Soft Body Mesh", "Mesh", "Make sure the meshes are clean and all normals are pointing outward.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "Vel", "Initial velocities per mesh. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.list, new Vector3d(0.0, 0.0, 0.0));
            pManager.AddNumberParameter("Mass", "Mass", "Masses of mesh particles. Supply one value per mesh. The values are applied to EACH particle in the respective mesh. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.list, new List<double> { 1.0 });
            pManager.AddNumberParameter("Soft Params", "Params", "Supply the following parameters as a number list:\n0 Particle spacing\n1 Volume sampling (set to zero if mesh is not closed)\n2 Surface sampling (good for ensure details in intricate meshes and for open meshes)\n3 Cluster spacing (should be at least particle spacing)\n4 Cluster radius(should be larger than cluster sampling, otherwise there's no overlap)\n5 Cluster stiffness\n6 Link radius\n7 Link stiffness\n8 Global stiffness", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Group Index", "GInd", "Index to identify each soft body later on. Each soft body has to have its own unique group index!If you supply multiple meshes you have two different options for the GInd input:\n1) Supply one integer index for each mesh\n2) Supply one integer index for the first mesh, the others are numbered upwards(if you have more object components in your scene, you'll have to ensure yourself that each GInd is globally unique to the engine)", GH_ParamAccess.list, new List<int> { 0 });
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Soft Bodies", "Softs", "Connect to Flex Scene", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            float anchorThreshold = 0.01f;

            List<Mesh> meshes = new List<Mesh>();
            List<Vector3d> velocities = new List<Vector3d>();
            List<double> masses = new List<double>() { 1.0 };
            List<double> softParams = new List<double>();
            List<int> groupIndices = new List<int>();

            DA.GetDataList(0, meshes);
            DA.GetDataList(1, velocities);
            DA.GetDataList(2, masses);
            DA.GetDataList(3, softParams);
            DA.GetDataList(4, groupIndices);

            List<SoftBody> softBodies = new List<SoftBody>();

            for(int i = 0; i < meshes.Count; i++)
            {
                Mesh mesh = new Mesh();                
                //make new super shallow copy, as user referenced meshes are crazy heavy smh

                mesh.Vertices.AddVertices(meshes[i].Vertices);
                mesh.Faces.AddFaces(meshes[i].Faces);

                mesh.UnifyNormals();
                mesh.Normals.ComputeNormals();
                mesh.Normals.UnitizeNormals();

                float[] vertices = new float[mesh.Vertices.Count * 3];
                float[] velocity = new float[3];
                float invMass = 1.0f;

                for (int j = 0; j < mesh.Vertices.Count; j++)
                {
                    //set vertices
                    vertices[3 * j] = mesh.Vertices[j].X;
                    vertices[3 * j + 1] = mesh.Vertices[j].Y;
                    vertices[3 * j + 2] = mesh.Vertices[j].Z;
                }

                //set velocities
                if (velocities.Count == 1)
                {
                    velocity[0] = (float)velocities[0].X;
                    velocity[1] = (float)velocities[0].Y;
                    velocity[2] = (float)velocities[0].Z;
                }

                else if (velocities.Count > i)
                {
                    velocity[0] = (float)velocities[i].X;
                    velocity[1] = (float)velocities[i].Y;
                    velocity[2] = (float)velocities[i].Z;
                }

                //set masses
                if (masses.Count == 1)
                    invMass = 1.0f / (float)masses[0];
                else if (masses.Count > i)
                    invMass = 1.0f / (float)masses[i];

                int[] triangles = new int[mesh.Faces.Count * 3];
                for (int j = 0; j < mesh.Faces.Count; j++)
                {
                    triangles[3 * j] = mesh.Faces[j].A;
                    triangles[3 * j + 1] = mesh.Faces[j].B;
                    triangles[3 * j + 2] = mesh.Faces[j].C;
                }

                float[] softParameters = new float[softParams.Count];
                for (int j = 0; j < softParams.Count; j++)
                    softParameters[j] = (float)softParams[j];

                int groupIndex = i;
                if (groupIndices.Count == 1)
                    groupIndex += groupIndices[0];
                else if (groupIndices.Count > i)
                    groupIndex = groupIndices[i];

                SoftBody softBody = new SoftBody(vertices, velocity, invMass, triangles, softParameters, groupIndex);
                softBody.Mesh = mesh;
                softBodies.Add(softBody);
            }
            DA.SetDataList(0, softBodies);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.softBody;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("d01490b5-6201-4596-8ae4-86ed2b06a910"); }
        }
    }
}