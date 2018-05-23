using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

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
            pManager.AddIntegerParameter("Triangle Indices", "Ind", "Indices of particles that are connected by a triangle and affected by wind, lift and drag. Supply a tree where each branch has three indices.", GH_ParamAccess.tree);
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
            GH_Structure<GH_Integer> ind = new GH_Structure<GH_Integer>();
            //List<Vector3d> nor = new List<Vector3d>();            

            DA.GetDataTree(0, out ind);
            //DA.GetDataList(1, nor);

            

            List<int> indices = new List<int>();
            foreach (List<GH_Integer> i in ind.Branches)
            {
                indices.Add(i[0].Value);
                indices.Add(i[1].Value);
                indices.Add(i[2].Value);
            }

            /*List<float> normals = new List<float>();
            foreach(Vector3d v in nor)
            {
                normals.Add((float)v.X);
                normals.Add((float)v.Y);
                normals.Add((float)v.Z);
            }*/

            //DA.SetData(0, new ConstraintSystem(indices.ToArray(), normals.ToArray()));
            DA.SetData(0, new ConstraintSystem(indices.ToArray(), true));
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