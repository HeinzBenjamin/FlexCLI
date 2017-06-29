using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;

using Rhino.Geometry;

namespace FlexHopper.GH_GroupObjects
{
    public class FluidFromPts : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FluidFromPts class.
        /// </summary>
        public FluidFromPts()
          : base("Fluid From Points", "Fluid",
              "",
              "Flex", "Groups")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Positions", "Pos", "Point cloud of fluid particles", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Velocities", "Vel", "Initial velocities per particle. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Masses", "Mass", "Masses per particle. If one value is supplied it's applied to all particles equally.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Group Index", "GInd", "Index to identify this fluid group later on. Make sure no index is more than once in your entire flex simulation.", GH_ParamAccess.tree);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Fluid Groups", "Fluids", "Connect to FlexScene", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Point> pts = new GH_Structure<GH_Point>();
            GH_Structure<GH_Vector> vel = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Number> masses = new GH_Structure<GH_Number>();
            GH_Structure<GH_Integer> groupIndices = new GH_Structure<GH_Integer>();

            DA.GetDataTree(0, out pts);
            DA.GetDataTree(1, out vel);
            DA.GetDataTree(2, out masses);
            DA.GetDataTree(3, out groupIndices);

            if (pts.Branches.Count != vel.Branches.Count || pts.Branches.Count != masses.Branches.Count || pts.Branches.Count != groupIndices.Branches.Count)
                throw new Exception("Tree structures don't match! Make sure everything is clean. The branch count should be the same for all inputs. GroupIndex input must hold exactly one value per branch; masses and velocities should either hold one value per branch or as many values per branch as there are particles in that branch.");

            List<Fluid> fluids = new List<Fluid>();

            for (int i = 0; i < pts.Branches.Count; i++)
            {
                List<GH_Point> lPt = pts.Branches[i];

                float[] Positions = new float[pts.Branches[i].Count * 3];
                float[] Velocities = new float[pts.Branches[i].Count * 3];
                float[] InvMasses = new float[pts.Branches[i].Count];
                int GroupIndex = groupIndices.Branches[i][0].Value;

                //Fill up masses in case its shorter than positions
                for (int j = masses.Branches[i].Count; j < pts.Branches[i].Count; j++)
                    masses.Branches[i].Add(masses.Branches[i][0]);

                for (int j = vel.Branches[i].Count; j < pts.Branches[i].Count; j++)
                    vel.Branches[i].Add(vel.Branches[i][0]);

                for (int j = 0; j < pts.Branches[i].Count; j++)
                {
                    Positions[j * 3] = (float)pts.Branches[i][j].Value.X;
                    Positions[j * 3 + 1] = (float)pts.Branches[i][j].Value.Y;
                    Positions[j * 3 + 2] = (float)pts.Branches[i][j].Value.Z;

                    Velocities[j * 3] = (float)vel.Branches[i][j].Value.X;
                    Velocities[j * 3 + 1] = (float)vel.Branches[i][j].Value.Y;
                    Velocities[j * 3 + 2] = (float)vel.Branches[i][j].Value.Z;

                    InvMasses[j] = (float)(1.0f / masses.Branches[i][j].Value);
                }

                fluids.Add(new Fluid(Positions, Velocities, InvMasses, groupIndices.Branches[i][0].Value));
            }

            DA.SetDataList(0, fluids);

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
            get { return new Guid("{7da59058-7766-44c2-b959-5ba4e3f897d3}"); }
        }
    }
}