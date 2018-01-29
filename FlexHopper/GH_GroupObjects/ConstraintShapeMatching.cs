using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using FlexHopper.Properties;

namespace FlexHopper.GH_GroupObjects
{
    public class ConstraintShapeMatching : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ConstraintShapeMatching class.
        /// </summary>
        public ConstraintShapeMatching()
          : base("Constraint: Shape Matching Constraint", "Shape Matching",
              "Group particles into shape matching constraints where they remain equidistant relative to each other",
              "Flex", "Composition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Particle Indices", "Ind", "Particle indices to form shape matching constraints", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Stiffness", "Stiffness", "0.0 to 1.0", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Shape Matching Constraint", "Constraint", "Connect to FlexScene", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Integer> siTree = new GH_Structure<GH_Integer>();
            List<double> ss = new List<double>();

            List<ConstraintSystem> constraints = new List<ConstraintSystem>();

            DA.GetDataTree(0, out siTree);
            DA.GetDataList(1, ss);

            for(int i = 0; i < siTree.Branches.Count; i++)
            {
                List<int> ints = new List<int>();
                foreach (GH_Integer inte in siTree.Branches[i])
                    ints.Add(inte.Value);
                if(ints.Count > 1)
                    constraints.Add(new ConstraintSystem(ints.ToArray(), (float)ss[i]));
            }

            DA.SetDataList(0, constraints);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.shapeMatching;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8d1c8749-2afd-4525-ab26-d03879ce7efd"); }
        }
    }
}