using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FlexHopper.Properties;

namespace FlexHopper.GH_GroupObjects
{
    public class ConstraintSprings : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SpringConstraints class.
        /// </summary>
        public ConstraintSprings()
          : base("Constraints: Springs", "Springs",
              "Add spring constraints by particle indices.",
              "Flex", "Composition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Spring Pair Indices", "SPI", "Flat list of spring pair indices. Must be of even length", GH_ParamAccess.list);
            pManager.AddNumberParameter("Target Spring Length", "Length", "Leave empty, if current particle distance should be the target length. If you enter a negative value, its absolute value will be interpreted as a length factor. If one value is supplied it will be supplied to all springs", GH_ParamAccess.list);
            pManager.AddNumberParameter("Spring Stiffness", "Stiff", "Between 0.0 and 1.0. If one value is supplied it will be supplied to all springs. Default: 0.95", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Spring Constraint", "Constraint", "Connect to FlexScene", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<int> spi = new List<int>();
            List<double> lengths = new List<double>();
            List<double> stiffnesses = new List<double>();

            DA.GetDataList(0, spi);
            DA.GetDataList(1, lengths);
            DA.GetDataList(2, stiffnesses);

            if (spi.Count % 2 != 0)
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "SPI must be of even length!");

            List<float> l = new List<float>();
            List<float> s = new List<float>();

            for (int i = 0; i < spi.Count / 2; i++)
            {
                if (lengths.Count == 0)
                    l.Add(-1.0f);
                else if (lengths.Count > i)
                    l.Add((float)lengths[i]);
                else
                    l.Add((float)lengths[0]);

                if (stiffnesses.Count == 0)
                    s.Add(0.95f);
                else if (stiffnesses.Count > i)
                    s.Add((float)stiffnesses[i]);
                else
                    s.Add((float)stiffnesses[0]);
            }


            DA.SetData(0, new ConstraintSystem(spi.ToArray(), l.ToArray(), s.ToArray()));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.spring_constraints;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7e7b35f7-75b5-40d2-ace3-33ecd1fdec11"); }
        }
    }
}