using System;
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
            pManager.AddMeshParameter("Meshes", "Mesh", "", GH_ParamAccess.item);
            pManager.AddVectorParameter("Velocities", "Vel", "Initial velocities per mesh. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.list, new Vector3d(0.0, 0.0, 0.0));
            pManager.AddNumberParameter("Mass", "Mass", "Masses per mesh particle. Supply as many values as you have meshes. The values are applied to EACH particle in the respective mesh. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.list, 1.0);
            pManager.AddNumberParameter("Soft Params", "Params", "Supply the following parameters as a number list:\n0 Particle spacing\n1 Volume sampling (set to zero if mesh is not closed)\n2 Surface sampling (good for ensure details in intricate meshes and for open meshes)\n3 Cluster spacing (should be at least particle spacing)\n4 Cluster radius(should be larger than cluster sampling, otherwise there's no overlap)\n5 Cluster stiffness\n6 Link radius\n7 Link stiffness\n8 Global stiffness", GH_ParamAccess.list);
            pManager.AddGenericParameter("Anchors", "Anchors", "Index numbers or (x,y,z)-points.", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Group Index", "GInd", "Index to identify each soft body later on. Each soft body has to have its own unique group index!If you supply multiple meshes you have two different options for the GInd input:\n1) Supply one integer index for each mesh\n2) Supply one integer index for the first mesh, the others are numbered upwards(if you have more object components in your scene, you'll have to ensure yourself that each GInd is globally unique to the engine)", GH_ParamAccess.list, new List<int> { 0 });
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Soft Bodies", "Softs", "Connect to FlexScene", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            float anchorThreshold = 0.01f;

            Mesh msh = new Mesh();
            List<Vector3d> vels = new List<Vector3d>();
            List<double> masses = new List<double>();
            List<double> softParams = new List<double>();
            List<IGH_Goo> anchors = new List<IGH_Goo>();
            int groupIndex = 0;

            DA.GetData(0, ref msh);
            DA.GetDataList(1, vels);
            DA.GetDataList(2, masses);
            DA.GetDataList(3, softParams);
            DA.GetDataList(4, anchors);
            DA.GetData(5, ref groupIndex);

            Mesh mesh = new Mesh();
            mesh.Vertices.AddVertices(msh.Vertices);
            mesh.Faces.AddFaces(msh.Faces);

            float[] vertices = new float[mesh.Vertices.Count * 3];
            for(int i = 0; i < mesh.Vertices.Count; i++)
            {
                vertices[3 * i] = mesh.Vertices[i].X;
                vertices[3 * i + 1] = mesh.Vertices[i].Y;
                vertices[3 * i + 2] = mesh.Vertices[i].Z;
            }

            float[] velocities = new float[mesh.Vertices.Count * 3];
            for(int i = 0; i < mesh.Vertices.Count; i++)
            {
                if (vels.Count == 1)
                {
                    velocities[3 * i] = (float)vels[0].X;
                    velocities[3 * i + 1] = (float)vels[0].Y;
                    velocities[3 * i + 2] = (float)vels[0].Z;
                }
                else
                {
                    velocities[3 * i] = (float)vels[i].X;
                    velocities[3 * i + 1] = (float)vels[i].Y;
                    velocities[3 * i + 2] = (float)vels[i].Z;
                }
                    
            }

            float[] invMasses = new float[mesh.Vertices.Count];
            for(int i = 0; i < mesh.Vertices.Count; i++)
            {
                if (masses.Count == 1)
                    invMasses[i] = 1.0f / (float)masses[0];
                else
                    invMasses[i] = 1.0f / (float)masses[i];
            }

            int[] triangles = new int[mesh.Faces.Count * 3];
            for(int i = 0; i < mesh.Faces.Count; i++)
            {
                triangles[3 * i] = mesh.Faces[i].A;
                triangles[3 * i + 1] = mesh.Faces[i].B;
                triangles[3 * i + 2] = mesh.Faces[i].C;
            }

            float[] softParameters = new float[softParams.Count];
            for (int i = 0; i < softParams.Count; i++)
                softParameters[i] = (float)softParams[i];

            int[] anchorIndices = Util.AnchorIndicesFromIGH_Goo(anchors, mesh.Vertices.ToPoint3dArray(), anchorThreshold);
            


            SoftBody softBody = new SoftBody(vertices, velocities, invMasses, triangles, softParameters, anchorIndices, groupIndex);
            softBody.Mesh = mesh;

            DA.SetData(0, softBody);
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