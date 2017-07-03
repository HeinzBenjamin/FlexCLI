using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using FlexCLI;

namespace FlexHopper.GH_Getters
{
    public class GH_GetAllSprings : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_GetAllParticles class.
        /// </summary>
        public GH_GetAllSprings()
          : base("Get All Springs", "AllSprings",
              "Get all springs from engine object",
              "Flex", "Decomposition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Flex Object", "Flex", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("n", "n", "You can chose to only draw fluids every nth solver iteration. This significantly speeds up internal simulation at the cost of less smooth appearance.", GH_ParamAccess.item, 1);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Springs", "Springs", "", GH_ParamAccess.tree);
        }

        int n = 1;
        int counter = 0;
        GH_Structure<GH_Line> lineTree = new GH_Structure<GH_Line>();

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            counter++;
            DA.GetData(1, ref n);
            n = Math.Max(1, n);

            if (counter % n == 0)
            {
                Flex flex = null;


                DA.GetData(0, ref flex);

                if (flex != null)
                {
                    List<FlexParticle> part = flex.Scene.GetAllParticles();
                    List<int> springsPairIndices = flex.Scene.GetSpringPairIndices();

                    lineTree = new GH_Structure<GH_Line>();

                    for(int i = 0; i < springsPairIndices.Count / 2; i++)
                    {
                        lineTree.Append(
                            new GH_Line(
                            new Line(
                            new Point3d(part[springsPairIndices[2 * i]].PositionX, part[springsPairIndices[2 * i]].PositionY, part[springsPairIndices[2 * i]].PositionZ),
                            new Point3d(part[springsPairIndices[2 * i + 1]].PositionX, part[springsPairIndices[2 * i + 1]].PositionY, part[springsPairIndices[2 * i + 1]].PositionZ))),
                            new GH_Path(part[springsPairIndices[2 * i]].GroupIndex));
                    }
                }
            }

            DA.SetDataTree(0, lineTree);
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
            get { return new Guid("81932a5a-a282-4d68-9f7d-3486a9113b08"); }
        }
    }
}