using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using FlexHopper.Properties;

namespace FlexHopper.GH_Util
{
    public class GH_TelepathyOut : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TelepathyOut class.
        /// </summary>
        public GH_TelepathyOut()
          : base("Telepathy Output", "rename_me",
              "Telepathy let's you make data from one component reappear anywhere else in your script. Even as an input to that component. So you can easily form script loops to reuse data as an input for itself. You can couple specific telepathy input and output components by giving them the same nickname. This component outputs the data received in the corresponding (i.e. equally nicknamed) input component.",
              "Flex", "Util")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "data", "", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            DA.SetDataTree(0, telepathy);
        }

        public GH_Structure<IGH_Goo> telepathy = null;

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resources.telepathy_out;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("abc93759-eb85-4bd0-80be-ceb94ff15959"); }
        }
    }
}