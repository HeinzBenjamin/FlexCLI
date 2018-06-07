using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;
using FlexHopper.Properties;

namespace FlexHopper.GH_GroupObjects
{
    public class ClothFromMesh : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ClothfromMesh class.
        /// </summary>
        public ClothFromMesh()
          : base("Cloth From Mesh", "Cloth",
              "",
              "Flex", "Composition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddMeshParameter("Cloth Meshes", "Mesh", "Connect one or multiple meshes to make cloth.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "Vel", "Initial velocities. Either supply one vector per mesh vertex or one vector per mesh. In any case it has to be a tree structure matching the mesh count.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Masses", "Mass", "Either supply one value per mesh vertex or one value per mesh (to be applied on each vertx). In any case it has to be a tree structure matching the mesh count.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Stretch Stiffness", "Stretch", "Between 0.0 and 1.0. One value per mesh", GH_ParamAccess.list, new List<double> { 1.0});
            pManager.AddNumberParameter("Bending Stiffness", "Bend", "Between 0.0 and 1.0. One value per mesh", GH_ParamAccess.list, new List<double> { 0.0 });
            pManager.AddBooleanParameter("Self Collision", "SelfColl", "Turn self collision of cloth particles among one another on or off. (default: false)", GH_ParamAccess.list, new List<bool> { false });
            pManager.AddNumberParameter("Pre Tension", "Tension", "Optional pre tension factor.", GH_ParamAccess.list, new List<double> { 1.0 });
            pManager.AddGenericParameter("Anchors", "Anchors", "As vertex index integers or (x,y,z)-points.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Group Index", "GInd", "Index to identify this fluid group later on. Make sure no index is more than once in your entire flex simulation.", GH_ParamAccess.list, new List<int> { 0 });
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Cloths", "Cloths", "Connect to FlexScene", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double anchorThreshold = 0.01;

            List<Mesh> meshes = new List<Mesh>();
            GH_Structure<GH_Vector> velTree = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Number> massTree = new GH_Structure<GH_Number>();
            List<double> stretchStiffness = new List<double>();
            List<double> bendStiffness = new List<double>();
            List<double> preTension = new List<double>();
            GH_Structure<IGH_Goo> anchorTree = new GH_Structure<IGH_Goo>();
            List<int> groupIndexList = new List<int> { 0 };
            List<bool> selfCollision = new List<bool>();


            List<Cloth> cloths = new List<Cloth>();

            DA.GetDataList(0, meshes);
            DA.GetDataTree(1, out velTree);
            DA.GetDataTree(2, out massTree);
            DA.GetDataList(3, stretchStiffness);
            DA.GetDataList(4, bendStiffness);
            DA.GetDataList(5, selfCollision);
            DA.GetDataList(6, preTension);
            DA.GetDataTree(7, out anchorTree);
            DA.GetDataList(8, groupIndexList);

            #region simplify trees and if(branch.Count == 1) make sure everything sits in path {0}
            if (!velTree.IsEmpty) velTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (!massTree.IsEmpty) massTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (!anchorTree.IsEmpty) anchorTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (velTree.Branches.Count == 1)
            {
                GH_Structure<GH_Vector> vT = new GH_Structure<GH_Vector>();
                vT.AppendRange(velTree.Branches[0], new GH_Path(0));
                velTree = vT;
            }
            if (massTree.Branches.Count == 1)
            {
                GH_Structure<GH_Number> mT = new GH_Structure<GH_Number>();
                mT.AppendRange(massTree.Branches[0], new GH_Path(0));
                massTree = mT;
            }
            if (anchorTree.Branches.Count == 1)
            {
                GH_Structure<IGH_Goo> aT = new GH_Structure<IGH_Goo>();
                aT.AppendRange(anchorTree.Branches[0], new GH_Path(0));
                anchorTree = aT;
            }
            #endregion

            for (int i = 0; i < meshes.Count; i++)
            {
                Mesh mesh = new Mesh();
                mesh.Vertices.AddVertices(meshes[i].Vertices);
                mesh.Faces.AddFaces(meshes[i].Faces);

                GH_Path path = new GH_Path(i);
                List<float> positions = new List<float>();
                List<float> velocities = new List<float>();
                List<float> invMasses = new List<float>();

                for(int j = 0; j < mesh.Vertices.Count; j++)
                {
                    positions.Add((float)mesh.Vertices[j].X);
                    positions.Add((float)mesh.Vertices[j].Y);
                    positions.Add((float)mesh.Vertices[j].Z);

                    Vector3d vel = new Vector3d(0.0, 0.0, 0.0);
                    if (velTree.PathExists(path))
                    {
                        if (velTree.get_Branch(path).Count == 1)
                            vel = velTree.get_DataItem(path, 0).Value;
                        else if (velTree.get_Branch(path).Count > j)
                            vel = velTree.get_DataItem(path, j).Value;
                    }
                    velocities.Add((float)vel.X);
                    velocities.Add((float)vel.Y);
                    velocities.Add((float)vel.Z);

                    float invMass = 1.0f;
                    if (massTree.PathExists(path))
                    {
                        if (massTree.get_Branch(path).Count == 1)
                            invMass = 1.0f / (float)massTree.get_DataItem(path, 0).Value;
                        else if (massTree.get_Branch(path).Count > j)
                            invMass = 1.0f / (float)massTree.get_DataItem(path, j).Value;
                    }
                    invMasses.Add(invMass);

                }

                int[] triangles = new int[mesh.Faces.Count * 3];
                float[] triangleNormals = new float[mesh.Faces.Count * 3];
                mesh.UnifyNormals();
                mesh.FaceNormals.ComputeFaceNormals();

                for(int j = 0; j < triangles.Length / 3; j++)
                {
                    triangles[3 * j] = mesh.Faces[j].A;
                    triangles[3 * j + 1] = mesh.Faces[j].B;
                    triangles[3 * j + 2] = mesh.Faces[j].C;
                    triangleNormals[3 * j] = mesh.FaceNormals[j].X;
                    triangleNormals[3 * j + 1] = mesh.FaceNormals[j].Y;
                    triangleNormals[3 * j + 2] = mesh.FaceNormals[j].Z;
                }

                List<int> anchorIndices = new List<int>();
                if (anchorTree.PathExists(path))
                {
                    foreach(IGH_Goo ao in anchorTree.get_Branch(path))
                    {
                        string aS = "";
                        int aI = -1;
                        Point3d aP;
                        if (ao.CastTo<Point3d>(out aP))
                        {
                            for(int j = 0; j < positions.Count / 3; j++)
                            {
                               if(Util.SquareDistance(new Point3d(positions[3 * j],positions[3 * j + 1],positions[3 * j + 2]),aP) < anchorThreshold*anchorThreshold)
                                {
                                    anchorIndices.Add(j);
                                    break;
                                }
                            }
                        }
                        else if (ao.CastTo<int>(out aI))
                            anchorIndices.Add(aI);
                        else if (ao.CastTo<string>(out aS))
                            anchorIndices.Add(int.Parse(aS));
                    }
                }

                float preTens = (float)preTension[0]; ;
                if (preTension.Count > i)
                    preTens = (float)preTension[i];

                float sStiffness = (float)stretchStiffness[0];
                if (stretchStiffness.Count > i)
                    sStiffness = (float)stretchStiffness[i];

                float bStiffness = (float)bendStiffness[0];
                if (bendStiffness.Count > i)
                    bStiffness = (float)bendStiffness[i];


                bool sc = false;
                if (selfCollision.Count > 0 & selfCollision.Count < i)
                    sc = selfCollision[0];
                else if(selfCollision.Count >= i)
                    sc = selfCollision[i];

                Cloth cloth = new Cloth(positions.ToArray(), velocities.ToArray(), invMasses.ToArray(), triangles, triangleNormals, sStiffness, bStiffness, preTens, anchorIndices.ToArray(), selfCollision[i], groupIndexList[i]);
                cloth.Mesh = mesh;
                cloths.Add(cloth);
            }

            DA.SetDataList(0, cloths);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.cloth;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7960c036-fdf4-4f32-90d9-bd1eb4e93d13"); }
        }
    }
}