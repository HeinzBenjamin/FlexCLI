using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FlexHopper.Properties;

namespace FlexHopper.GH_GroupObjects
{
    public class ConstraintTriangle : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ConstraintTriangle class.
        /// </summary>
        public ConstraintTriangle()
          : base("Constraints: Triangles", "Triangles",
              "Add triangle constraints by particle indices.",
              "Flex", "Composition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Triangle Indices", "Ind", "Indices of particles that are connected by a triangle and affected by wind, lift and drag in the form of a flattened list", GH_ParamAccess.list);
            pManager.AddVectorParameter("Triangle Normals (optional)", "Nor", "Optional vertex normals. Length must be a third of 'Triangle Indices' length.", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Triangle Constraints", "Constraint", "Connect to FlexScene.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> ind = new List<int>();
            List<float> nor = new List<float>();

            DA.GetDataList(0, ind);
            DA.GetDataList(1, nor);

            DA.SetData(0, new ConstraintSystem(ind.ToArray(), nor.ToArray()));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.triangles;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("75613b0d-f95e-4967-a6d1-f54585df1ce8"); }
        }
    }
}