using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;

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
              "Flex", "Groups")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Rigid Body Mesh", "Mesh", "Make sure the meshes are clean and all normals are pointing outward.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "Vel", "Initial velocities per mesh. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mass", "Mass", "Masses per mesh particle. Supply as many values as you have meshes. The values are applied to EACH particle in the respective mesh. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Stiffness", "Sti", "Betwen [0.0] and [1.0]. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Group Index", "GInd", "Index to identify each rigid body later on. Make sure no index is more than once in your entire flex simulation.", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Rigid Objects", "Rigids", "Connect to FlexScene", GH_ParamAccess.list);
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
                meshes[i].UnifyNormals();
                meshes[i].Normals.ComputeNormals();                
                meshes[i].Normals.UnitizeNormals();
                
                List<float> vertices = new List<float>();
                List<float> normals = new List<float>();
                List<float> invMasses = new List<float>();

                for(int j = 0; j < meshes[i].Vertices.Count; j++)
                {
                    vertices.Add(meshes[i].Vertices[j].X);
                    vertices.Add(meshes[i].Vertices[j].Y);
                    vertices.Add(meshes[i].Vertices[j].Z);

                    normals.Add(meshes[i].Normals[j].X);
                    normals.Add(meshes[i].Normals[j].Y);
                    normals.Add(meshes[i].Normals[j].Z);

                    double mass = 1.0;
                    if (masses.Count == 1)
                        mass = masses[i];
                    else if (masses.Count > i)
                        mass = masses[i];
                    invMasses.Add((float)(1.0/Math.Max(mass, 0.00000001)));
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

                RigidBody rb = new RigidBody(vertices.ToArray(), velocity, normals.ToArray(), invMasses.ToArray(), stiffness, groupIndices[i]);

                rb.Mesh = meshes[i].DuplicateMesh();

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
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
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