using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;
using FlexHopper.Properties;

namespace FlexHopper.GH_GroupObjects
{
    public class RigidFromMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the RigidFromMesh class.
        /// </summary>
        public RigidFromMesh()
          : base("Rigid From Mesh", "Rigid",
              "",
              "Flex", "Composition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Rigid Body Mesh", "Mesh", "Make sure the meshes are clean and all normals are pointing outward.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "Vel", "Initial velocities per mesh. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.list, new Vector3d(0.0,0.0,0.0));
            pManager.AddNumberParameter("Mass", "Mass", "Masses of mesh particles. Supply one value per mesh. The values are applied to EACH particle in the respective mesh. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.list, new List<double> { 1.0 });
            pManager.AddNumberParameter("Stiffness", "Sti", "Betwen [0.0] and [1.0]. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.list, new List<double> { 1.0 });
            pManager.AddIntegerParameter("Group Index", "GInd", "Index to identify each rigid body later on. Each rigid body has to have its own unique group index! If you supply multiple meshes you have two different options for the GInd input:\n1) Supply one integer index for each mesh\n2) Supply one integer index for the first mesh, the others are numbered upwards (if you have more object components in your scene, you'll have to ensure yourself that each GInd is globally unique to the engine)", GH_ParamAccess.list, new List<int> { 0 });
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Rigid Bodies", "Rigids", "Connect to Flex Scene", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Mesh> meshes = new List<Mesh>();
            List<Vector3d> vels = new List<Vector3d>();
            List<double> masses = new List<double>();
            List<double> stiffnesses = new List<double>();
            List<int> groupIndices = new List<int>();

            DA.GetDataList(0, meshes);
            DA.GetDataList(1, vels);
            DA.GetDataList(2, masses);
            DA.GetDataList(3, stiffnesses);
            DA.GetDataList(4, groupIndices);

            List<RigidBody> rigids = new List<RigidBody>();

            for(int i = 0; i < meshes.Count; i++)
            {
                Mesh mesh = new Mesh();                
                //make new super shallow copy, as user referenced meshes are crazy heavy smh
                mesh.Vertices.AddVertices(meshes[i].Vertices);
                mesh.Faces.AddFaces(meshes[i].Faces);

                mesh.UnifyNormals();
                mesh.Normals.ComputeNormals();                
                mesh.Normals.UnitizeNormals();
                
                List<float> vertices = new List<float>();
                List<float> normals = new List<float>();
                List<float> invMasses = new List<float>();

                for(int j = 0; j < mesh.Vertices.Count; j++)
                {
                    vertices.Add(mesh.Vertices[j].X);
                    vertices.Add(mesh.Vertices[j].Y);
                    vertices.Add(mesh.Vertices[j].Z);

                    normals.Add(mesh.Normals[j].X);
                    normals.Add(mesh.Normals[j].Y);
                    normals.Add(mesh.Normals[j].Z);

                    double mass = 1.0;
                    if (masses.Count == 1)
                        mass = masses[0];
                    else if (masses.Count > i)
                        mass = masses[i];
                    invMasses.Add((float)(1.0/Math.Max(mass, 0.00000000001)));
                }

                float[] velocity = new float[3] { 0.0f, 0.0f, 0.0f };
                if(vels.Count == 1)
                    velocity = new float[3] {(float)vels[0].X, (float)vels[0].Y, (float)vels[0].Z };

                else if (vels.Count > i)
                    velocity = new float[3] { (float)vels[i].X, (float)vels[i].Y, (float)vels[i].Z };

                float stiffness = 1.0f;
                if (stiffnesses.Count == 1)
                    stiffness = (float)stiffnesses[0];
                else if (stiffnesses.Count > i)
                    stiffness = (float)stiffnesses[i];

                int groupIndex = i;
                if (groupIndices.Count == 1)
                    groupIndex += groupIndices[0];
                else if (groupIndices.Count > i)
                    groupIndex = groupIndices[i];

                RigidBody rb = new RigidBody(vertices.ToArray(), velocity, normals.ToArray(), invMasses.ToArray(), stiffness, groupIndex);

                rb.Mesh = mesh;

                rigids.Add(rb);
            }

            DA.SetDataList(0, rigids);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.rigid;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{89fc0806-11b0-4861-9f25-0a339a3fbbff}"); }
        }
    }
}