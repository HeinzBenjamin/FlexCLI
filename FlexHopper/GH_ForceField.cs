using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FlexCLI;
using FlexHopper.Properties;

namespace FlexHopper
{
    public class GH_ForceField : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_ForceField class.
        /// </summary>
        public GH_ForceField()
          : base("Flex Force Field", "Force Field",
              "",
              "Flex", "Setup")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Positions", "Pos", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Radius", "Radius", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Strength", "Strength", "", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Linear Fall Off", "Linear", "", GH_ParamAccess.list);
            pManager.AddIntegerParameter("Mode", "Mode", "0: Constant Force, 1: Impulse, 2: Velocity Change", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Force Fields", "Fields", "Force fields to be passed to the Flex engine", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<FlexForceField> forceFields = new List<FlexForceField>();

            List<Point3d> pts = new List<Point3d>();
            List<double> radii = new List<double>();
            List<double> strengths = new List<double>();
            List<bool> lineFO = new List<bool>();
            List<int> modes = new List<int>();

            DA.GetDataList(0, pts);
            DA.GetDataList(1, radii);
            DA.GetDataList(2, strengths);
            DA.GetDataList(3, lineFO);
            DA.GetDataList(4, modes);

            for(int i = 0; i < pts.Count; i++)
            {
                FlexForceField ff = new FlexForceField(
                    new float[3] {
                        (float)pts[i].X,
                        (float)pts[i].Y,
                        (float)pts[i].Z,},
                    (float)radii[i],
                    (float)strengths[i], lineFO[i], modes[i]);

                forceFields.Add(ff);
            }

            DA.SetDataList(0, forceFields);

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.forceField;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("418762f8-115c-497c-ab06-25c2e94a855d"); }
        }
    }
}