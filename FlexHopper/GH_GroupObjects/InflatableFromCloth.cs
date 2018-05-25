using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino.Geometry;
using FlexHopper.Properties;

namespace FlexHopper.GH_GroupObjects
{
    public class InflatableFromCloth : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the InflatableFromCloth class.
        /// </summary>
        public InflatableFromCloth()
          : base("Inflatable From Cloth", "Inflatable",
              "Inflatable from Flex Cloth Object",
              "Flex", "Composition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Cloths", "Cloth", "One or more cloth objects.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rest Volume", "RVolume", "Volume at which the inflatable would come to a rest.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Over Pressure", "Pressure", "Factor to the rest volume applied to inflatable.", GH_ParamAccess.list);
            pManager.AddNumberParameter("Contraint Scale", "Constraint", "Similar to a stiffness value", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Self Collision", "SelfColl", "Turn self collision on or off.", GH_ParamAccess.list, new List<bool> { false });
            pManager.AddIntegerParameter("Group Index", "GInd", "Index to identify this fluid group later on. Make sure no index is more than once in your entire flex simulation.", GH_ParamAccess.list);
            pManager[4].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Inflatables", "Inflatables", "Connect to FlexScene", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Cloth> cloths = new List<Cloth>();
            List<double> restVolumes = new List<double>();
            List<double> overPressures = new List<double>();
            List<double> constraintScales = new List<double>();
            List<int> groupIndices = new List<int>();
            List<bool> selfColl = new List<bool>();

            DA.GetDataList(0, cloths);
            DA.GetDataList(1, restVolumes);
            DA.GetDataList(2, overPressures);
            DA.GetDataList(3, constraintScales);
            DA.GetDataList(4, selfColl);
            DA.GetDataList(4, groupIndices);

            List<Inflatable> inflatables = new List<Inflatable>();

            if (cloths.Count != restVolumes.Count || cloths.Count != overPressures.Count || cloths.Count != constraintScales.Count || cloths.Count != groupIndices.Count)
                throw new Exception("Input lists don't match!");

            for (int i = 0; i < cloths.Count; i++)
            {
                Inflatable infla = new Inflatable(cloths[i].Positions, cloths[i].Velocities, cloths[i].InvMasses, cloths[i].Triangles, cloths[i].TriangleNormals, cloths[i].StretchStiffness, cloths[i].BendingStiffness, cloths[i].PreTensionFactor, (float)restVolumes[i], (float)overPressures[i], (float)constraintScales[i], cloths[i].AnchorIndices, selfColl[i], groupIndices[i]);
                infla.Mesh = cloths[i].Mesh;
                inflatables.Add(infla);
            }

            DA.SetDataList(0, inflatables);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.inflabales;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("01889498-e27d-4178-9296-212732807200"); }
        }
    }
}