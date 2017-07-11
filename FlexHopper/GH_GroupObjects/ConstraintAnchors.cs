using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FlexHopper.Properties;

namespace FlexHopper.GH_GroupObjects
{
    public class ConstraintAnchors : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_Anchor class.
        /// </summary>
        public ConstraintAnchors()
          : base("Constraints: Anchors", "Anchor",
              "Flex anchor by index",
              "Flex", "Composition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Anchor Index", "Ind", "", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Anchor Constraint", "Constraint", "Connect to FlexScene", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> anchors = new List<int>();
            DA.GetDataList(0, anchors);

            DA.SetData(0, new ConstraintSystem(anchors.ToArray()));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.anchor;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("24d2bdbb-a810-458b-8f63-78e960086396"); }
        }
    }
}