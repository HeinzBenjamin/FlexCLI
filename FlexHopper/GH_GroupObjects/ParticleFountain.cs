using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FlexHopper.Properties;

namespace FlexHopper.GH_GroupObjects
{
    public class ParticleFountain : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PointFountain class.
        /// </summary>
        public ParticleFountain()
          : base("Particle Fountain", "Fountain",
              "Connect a timer to me and make sure to not be in Lock Mode (Flex Engine component).",
              "Flex", "Composition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Origin Plane", "Plane", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Particle Count", "Count", "Particles generated per tick. Avoid multiples of seven when'Rand' input is false.", GH_ParamAccess.item, 20);
            pManager.AddNumberParameter("Diameter", "Dia", "Diameter of the fountain cross section. When using fluids or self colliding particles make sure this is large enough for the repelling particles to fit in the fountain cross section!", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Random Direction Range", "RDir", "Range of randomness in initial particle directions {0.0 to Pi}. If randomness is involved, connect a timer to this component.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Velocity", "Vel", "", GH_ParamAccess.item, 1.0);
            pManager.AddBooleanParameter("Random Positions", "Rand", "Determine whether initial particle positions are distributed randomly or evenly (through sunflower pattern). If randomness is involved, connect a timer to this component.", GH_ParamAccess.item, true);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "Connct this to the position input of fluid or particles.", GH_ParamAccess.list);
            pManager.AddVectorParameter("Velocities", "Vels", "Connct this to the position input of fluid or particles.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Plane oPlane = Plane.Unset;
            int count = 0;
            double angle = 0.0;
            double dia = 1.0;
            double vel = 0.0;
            bool rand = true;

            DA.GetData(0, ref oPlane);
            DA.GetData(1, ref count);
            DA.GetData(2, ref dia);
            DA.GetData(3, ref angle);
            DA.GetData(4, ref vel);
            DA.GetData(5, ref rand);

            List<Point3d> pts = new List<Point3d>();
            List<Vector3d> vels = new List<Vector3d>();

            for(int i = 0; i < count; i++)
            {
                if(rand)
                    pts.Add(new Point3d(oPlane.PointAt((rnd.NextDouble() - 0.5) * dia, (rnd.NextDouble() - 0.5) * dia)));
                else
                {
                    double ratio = ((double)i / (double)count) * (137.508 / Math.PI);
                    double u = Math.Pow(ratio, 0.5) * Math.Cos(ratio);
                    u = u * dia * 0.5 / (2 * Math.PI);
                    double v = Math.Pow(ratio, 0.5) * Math.Sin(ratio);
                    v = v * dia * 0.5 / (2 * Math.PI);
                    pts.Add(oPlane.PointAt(u, v));
                }

                Vector3d vv = oPlane.ZAxis;
                vv.Unitize();
                
                vv *= vel;
                Vector3d rotA = oPlane.XAxis;
                rotA.Rotate(rnd.NextDouble() * Math.PI * 2, oPlane.ZAxis);
                vv.Rotate(((rnd.NextDouble() - 0.5) * 2) * angle, rotA);
                vels.Add(vv);
            }

            DA.SetDataList(0, pts);
            DA.SetDataList(1, vels);

        }

        Random rnd = new Random();

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.fountain;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("0857ba1d-faf1-477d-8331-f4679fee318e"); }
        }
    }
}