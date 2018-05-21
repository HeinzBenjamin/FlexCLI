using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Display;
using Rhino.Geometry;

using FlexCLI;
using FlexHopper.Properties;

namespace FlexHopper.GH_Getters
{
    public class GH_GetRigidTransformations : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Rigids class.
        /// </summary>
        public GH_GetRigidTransformations()
          : base("Get Rigid Tranformations", "Transform",
              "Get the transformation matrices of all rigid bodies in the simulation. This includes shape matching constraints in soft bodies.",
              "Flex", "Decomposition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Flex Object", "Flex", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddVectorParameter("Translations", "T", "", GH_ParamAccess.list);
            pManager.AddVectorParameter("Rotation vectors", "Rv", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Rotation angles", "Ra", "", GH_ParamAccess.list);
            pManager.AddTransformParameter("Transformations", "X", "Transformation matrices.", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Flex flex = null;
            DA.GetData(0, ref flex);

            List<Vector3d> translations = new List<Vector3d>();
            List<Vector3d> rotVec = new List<Vector3d>();
            List<double> rotAng = new List<double>();
            List<Transform> transformations = new List<Transform>();
            

            if (flex != null)
            {
                List<float> rot = flex.Scene.GetRigidRotations();
                List<float> trans = flex.Scene.GetRigidTranslations();
                List<float> massCenters = flex.Scene.GetShapeMassCenters();

                for(int i = 0; i < massCenters.Count / 3; i++)
                {
                    translations.Add(new Vector3d(trans[i * 3] - massCenters[i * 3], trans[i * 3 + 1] - massCenters[i * 3 + 1], trans[i * 3 + 2] - massCenters[i * 3 + 2]));

                    Vector3d r = new Vector3d(0.0, 0.0, 1.0);
                    double halfAngle = Math.Acos(rot[i * 4 + 3]);
                    double X = rot[i * 4] / Math.Sin(halfAngle);
                    double Y = rot[i * 4 + 1] / Math.Sin(halfAngle);
                    double Z = rot[i * 4 + 2] / Math.Sin(halfAngle);
                    if (HasValue(X) && HasValue(Y) && HasValue(Z))
                        r = new Vector3d(X, Y, Z);
                    double angle = 2.0 * halfAngle;

                    Transform a = Transform.Translation(translations[i]);
                    Transform b = Transform.Rotation(angle, r, new Point3d(massCenters[i * 3], massCenters[i * 3 + 1], massCenters[i * 3 + 2]));

                    Transform t = a * b;

                    rotVec.Add(r);
                    rotAng.Add(angle);
                    transformations.Add(t);
                }
            }

            DA.SetDataList(0, translations);
            DA.SetDataList(1, rotVec);
            DA.SetDataList(2, rotAng);
            DA.SetDataList(3, transformations);
        }
        
        public static bool HasValue(double value)
        {
            return !Double.IsNaN(value) && !Double.IsInfinity(value);
        }
                         
        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.getRigidTransformations;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{DB1C5304-1515-4013-8EA5-ECCCC040F1C7}"); }
        }
    }
}