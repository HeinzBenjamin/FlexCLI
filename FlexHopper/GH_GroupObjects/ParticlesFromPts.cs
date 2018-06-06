using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using FlexCLI;
using FlexHopper.Properties;

namespace FlexHopper.GH_GroupObjects
{
    public class ParticlesFromPts : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ParticlesFromPts class.
        /// </summary>
        public ParticlesFromPts()
          : base("Particles From Points", "Particles",
              "",
              "Flex", "Composition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "Point cloud of particles. Group in trees by group index", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Velocities", "Vel", "Initial velocities per particle. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Masses", "Mass", "Masses per particle. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Self Collision", "SC", "If set, particles of the same group will perform collisions with otehr particles of that group. False by default.", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Is Fluid", "IF", "If set, particle will behave as a fluid. False by default.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Group Index", "GInd", "Index to identify this fluid group later on. Make sure no index is more than once in your entire flex simulation. If empty, the respective tree branch index is used.", GH_ParamAccess.tree);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }
    

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Particles", "Particles", "Connect to FlexScene", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Point> ptsTree = new GH_Structure<GH_Point>();
            GH_Structure<GH_Vector> velTree = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Number> massTree = new GH_Structure<GH_Number>();
            GH_Structure<GH_Boolean> scTree = new GH_Structure<GH_Boolean>();
            GH_Structure<GH_Boolean> ifTree = new GH_Structure<GH_Boolean>();
            GH_Structure<GH_Integer> giTree = new GH_Structure<GH_Integer>();

            DA.GetDataTree(0, out ptsTree);
            DA.GetDataTree(1, out velTree);
            DA.GetDataTree(2, out massTree);
            DA.GetDataTree(3, out scTree);
            DA.GetDataTree(4, out ifTree);
            DA.GetDataTree(5, out giTree);

            #region clean up etc
            if (!ptsTree.IsEmpty) ptsTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (!velTree.IsEmpty) ptsTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (!massTree.IsEmpty) ptsTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (!scTree.IsEmpty) ptsTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (!ifTree.IsEmpty) ptsTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (!giTree.IsEmpty) ptsTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);

            if (ptsTree.Branches.Count == 1)
            {
                GH_Structure<GH_Point> pT = new GH_Structure<GH_Point>();
                pT.AppendRange(ptsTree.Branches[0], new GH_Path(0));
                ptsTree = pT;
            }
            if (velTree.Branches.Count == 1)
            {
                GH_Structure<GH_Vector> pT = new GH_Structure<GH_Vector>();
                pT.AppendRange(velTree.Branches[0], new GH_Path(0));
                velTree = pT;
            }
            if (massTree.Branches.Count == 1)
            {
                GH_Structure<GH_Number> mT = new GH_Structure<GH_Number>();
                mT.AppendRange(massTree.Branches[0], new GH_Path(0));
                massTree = mT;
            }
            if (scTree.Branches.Count == 1)
            {
                GH_Structure<GH_Boolean> mT = new GH_Structure<GH_Boolean>();
                mT.AppendRange(scTree.Branches[0], new GH_Path(0));
                scTree = mT;
            }
            if (ifTree.Branches.Count == 1)
            {
                GH_Structure<GH_Boolean> mT = new GH_Structure<GH_Boolean>();
                mT.AppendRange(ifTree.Branches[0], new GH_Path(0));
                ifTree = mT;
            }
            if(giTree.Branches.Count == 1)
            {
                GH_Structure<GH_Integer> mT = new GH_Structure<GH_Integer>();
                mT.AppendRange(giTree.Branches[0], new GH_Path(0));
                giTree = mT;
            }
            #endregion

            List<FlexParticle> parts = new List<FlexParticle>();

            for (int i = 0; i < ptsTree.PathCount; i++)
            {
                GH_Path path = new GH_Path(i);
                
                for(int j = 0; j < ptsTree.get_Branch(path).Count; j++)
                {
                    float[] pos = new float[3] { (float)ptsTree.get_DataItem(path, j).Value.X, (float)ptsTree.get_DataItem(path, j).Value.Y, (float)ptsTree.get_DataItem(path, j).Value.Z };

                    float[] vel = new float[3] { 0.0f, 0.0f, 0.0f };
                    if (velTree.PathExists(path))
                    {
                        if (velTree.get_Branch(path).Count > j)
                            vel = new float[3] { (float)velTree.get_DataItem(path, j).Value.X, (float)velTree.get_DataItem(path, j).Value.Y, (float)velTree.get_DataItem(path, j).Value.Z };
                        else
                            vel = new float[3] { (float)velTree.get_DataItem(path, 0).Value.X, (float)velTree.get_DataItem(path, 0).Value.Y, (float)velTree.get_DataItem(path, 0).Value.Z };
                    }

                    float iM = 1.0f;
                    if (massTree.PathExists(path))
                    {
                        if (massTree.get_Branch(path).Count > j)
                            iM = 1.0f / (float)massTree.get_DataItem(path, j).Value;
                        else
                            iM = 1.0f / (float)massTree.get_DataItem(path, 0).Value;
                    }

                    bool sc = false;
                    if (scTree.PathExists(path))
                    {
                        if (scTree.get_Branch(path).Count > j)
                            sc = scTree.get_DataItem(path, j).Value;
                        else
                            sc = scTree.get_DataItem(path, 0).Value;
                    }

                    bool isf = false;
                    if (ifTree.PathExists(path))
                    {
                        if (ifTree.get_Branch(path).Count > j)
                            isf = ifTree.get_DataItem(path, j).Value;
                        else
                            isf = ifTree.get_DataItem(path, 0).Value;
                    }

                    int gi = i;
                    if (giTree.PathExists(path))
                    {
                        if (giTree.get_Branch(path).Count > j)
                            gi = giTree.get_DataItem(path, j).Value;
                        else
                            gi = giTree.get_DataItem(path, 0).Value;
                    }

                    parts.Add(new FlexParticle(pos, vel, iM, sc, isf, gi, true));
                }
                
            }

            DA.SetDataList(0, parts);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.particles;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("f44c9af3-c35a-42a5-b087-ec2cde0cd700"); }
        }
    }
}