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
    public class GH_GetSofts : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Rigids class.
        /// </summary>
        public GH_GetSofts()
          : base("Get Soft Bodies", "Softs",
              "Get soft bodies from engine object",
              "Flex", "Decomposition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Flex Object", "Flex", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Softs", "Softs", "(Optionally) connect the original 'Soft from Mesh' component to retrieve shape information. Otherwise only particles of soft bodies will be displayed.", GH_ParamAccess.list);
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Velocity", "Vel", "", GH_ParamAccess.tree);
            pManager.AddLineParameter("Spring Lines", "S", "", GH_ParamAccess.tree);
            pManager.AddBoxParameter("Shape Matching Boxes", "B", "", GH_ParamAccess.tree);
        }

        

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<List<FlexParticle>> part = new List<List<FlexParticle>>();

            Flex flex = null;
            List<SoftBody> softs = new List<SoftBody>();

            DA.GetData(0, ref flex);

            GH_Structure<GH_Point> pts = new GH_Structure<GH_Point>();
            GH_Structure<GH_Vector> vel = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Line> lnTree = new GH_Structure<GH_Line>();
            GH_Structure<GH_Box> boxTree = new GH_Structure<GH_Box>();

            if (flex != null)
            {
                part = flex.Scene.GetSoftParticles();
                

                for(int i = 0; i < part.Count; i++)
                {
                    GH_Path gp = new GH_Path(i);

                    for(int j = 0; j < part[i].Count; j++)
                    {
                        pts.Append(new GH_Point(new Point3d(part[i][j].PositionX, part[i][j].PositionY, part[i][j].PositionZ)), gp);
                        vel.Append(new GH_Vector(new Vector3d(part[i][j].VelocityX, part[i][j].VelocityY, part[i][j].VelocityZ)), gp);
                    }
                }

                DA.GetDataList(1, softs);

                if (softs.Count != part.Count)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Number of supplied soft bodies doesn't match internal info. Please make sure to supply ALL soft bodies connected to the engine in the correct order");

                int rotateCounter = flex.Scene.NumRigidBodies() * 4;
                List<float> rotation = flex.Scene.GetRigidRotations();

                for (int i = 0; i < softs.Count; i++)
                {
                    GH_Path gp = new GH_Path(i);
                    List<GH_Point> thisBranchPts = (List<GH_Point>)pts.get_Branch(gp);
                    foreach (List<GH_Integer> spi in softs[i].SpringIndices.Branches)
                        lnTree.Append(new GH_Line(new Line(thisBranchPts[spi[0].Value].Value, thisBranchPts[spi[1].Value].Value)), gp);

                    foreach (List<GH_Integer> smc in softs[i].ShapeIndices.Branches)
                    {
                        List<Point3d> shapePoints = new List<Point3d>();
                        for (int k = 0; k < smc.Count; k++)
                            shapePoints.Add(pts.Branches[i][smc[k].Value].Value);

                        Plane refPlane = Plane.WorldXY;
                        float a = rotation[rotateCounter];
                        float b = rotation[rotateCounter + 1];
                        float c = rotation[rotateCounter + 2];
                        float d = rotation[rotateCounter + 3];

                        double halfAngle = Math.Acos(d);
                        double X = a / Math.Sin(halfAngle);
                        double Y = b / Math.Sin(halfAngle);
                        double Z = c / Math.Sin(halfAngle);
                        double angle = 2.0 * halfAngle;

                        refPlane.Rotate(angle, new Vector3d(X, Y, Z));

                        boxTree.Append(new GH_Box(new Box(refPlane, shapePoints)), gp);

                        rotateCounter += 4;
                    }
                }

                
                
            }

            DA.SetDataTree(0, pts);
            DA.SetDataTree(1, vel);
            DA.SetDataTree(2, lnTree);
            DA.SetDataTree(3, boxTree);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.getSoftBodies;
            }
        }


        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{E41D6979-9364-44BF-85CC-F1E2F0677E12}"); }
        }
    }
}