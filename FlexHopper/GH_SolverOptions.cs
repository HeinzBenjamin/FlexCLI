using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FlexCLI;
using FlexHopper.Properties;

namespace FlexHopper
{
    public class GH_SolverOptions : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_SolverOptions class.
        /// </summary>
        public GH_SolverOptions()
          : base("Flex Sover Options", "Opts",
              "",
              "Flex", "Engine")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Time step", "dt", "Time step per engine iteration", GH_ParamAccess.item, 0.0166666666667);
            pManager.AddIntegerParameter("Sub Steps", "SubSteps", "Number of sub-steps between each iteration. Collision detection is performed per sub-step. Therefore many sub-steps are slow but more reliable.", GH_ParamAccess.item, 3);
            pManager.AddIntegerParameter("NumIterations", "NumIt", "Number of iterations to be performed per sub step.", GH_ParamAccess.item, 3);
            //TODO pManager.AddBooleanParameter("Enable Timers","Timers","Tool for monitoring per kernel solver timing. Slows down solver a LOT. Only use for monitoring")
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Solver Options", "Options", "Solver options object to be passed into the engine.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double dt = 0.0166667;
            int sS = 3;
            int nI = 3;

            DA.GetData(0, ref dt);
            DA.GetData(1, ref sS);
            DA.GetData(2, ref nI);

            

            if (dt == 0.0 || sS == 0)
                throw new Exception("Neither dt nor SubSteps can be zero!");

            DA.SetData(0, new FlexSolverOptions((float)dt, sS, nI));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.opts;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{7a8cd854-71e8-453f-81e5-d2f37d53c7c8}"); }
        }
    }
}