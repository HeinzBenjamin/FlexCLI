using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using FlexCLI;
using FlexHopper.Properties;

namespace FlexHopper.GH_Getters
{
    public class GH_GetSpringSystems : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_GetSpringSystems class.
        /// </summary>
        public GH_GetSpringSystems()
          : base("Get Spring Systems", "Springs",
              "Get all particles that are parts of springs from engine object",
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
            pManager.AddBooleanParameter("Draw Particles", "drawP", "", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Spring System", "Springs", "Optionally connect the original 'Spring Systems' component to draw the respective geometry.", GH_ParamAccess.list);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Vector", "Vec", "", GH_ParamAccess.tree);
            pManager.AddLineParameter("Lines", "Lns", "", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Meshes", "Msh", "", GH_ParamAccess.tree);
        }

        int n = 1;
        int counter = 0;

        GH_Structure<GH_Point> pts = new GH_Structure<GH_Point>();
        GH_Structure<GH_Vector> vel = new GH_Structure<GH_Vector>();
        GH_Structure<GH_Mesh> msh = new GH_Structure<GH_Mesh>();
        GH_Structure<GH_Line> lns = new GH_Structure<GH_Line>();

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            counter++;
            DA.GetData(1, ref n);
            n = Math.Max(1, n);

            if (counter % n == 0)
            {
                Flex flex = null;
                bool drawP = true;
                List<SpringSystem> springs = new List<SpringSystem>();

                DA.GetData(0, ref flex);
                DA.GetData(2, ref drawP);
                DA.GetDataList(3, springs);

                if (flex != null)
                {
                    List<FlexParticle> part = flex.Scene.GetSpringParticles();
                    if (drawP)
                    {
                        pts = new GH_Structure<GH_Point>();
                        vel = new GH_Structure<GH_Vector>();

                        foreach (FlexParticle fp in part)
                        {
                            GH_Path p = new GH_Path(fp.GroupIndex);
                            pts.Append(new GH_Point(new Point3d(fp.PositionX, fp.PositionY, fp.PositionZ)), p);
                            vel.Append(new GH_Vector(new Vector3d(fp.VelocityX, fp.VelocityY, fp.VelocityZ)), p);
                        }
                    }
                    else
                    {
                        pts = new GH_Structure<GH_Point>();
                        vel = new GH_Structure<GH_Vector>();
                    }

                    if(springs.Count > 0)
                    {
                        lns = new GH_Structure<GH_Line>();
                        msh = new GH_Structure<GH_Mesh>();
                        foreach (SpringSystem ss in springs)
                        {
                            GH_Path p = new GH_Path(ss.GroupIndex);
                            if (ss.HasMesh())
                            {
                                for(int i = 0; i < ss.Mesh.Vertices.Count; i++)
                                {
                                    ss.Mesh.TopologyVertices[i] = new Point3f(part[i + ss.SpringOffset].PositionX, part[i + ss.SpringOffset].PositionY, part[i + ss.SpringOffset].PositionZ);
                                }
                                msh.Append(new GH_Mesh(ss.Mesh), p);
                            }
                            if(!ss.HasMesh() || ss.IsSheetMesh)
                            {
                                                               
                                for (int i = 0; i < ss.SpringPairIndices.Length / 2; i++)
                                {
                                    GH_Line l = new GH_Line(
                                        new Line(
                                            new Point3d(
                                                part[ss.SpringPairIndices[i * 2] + ss.SpringOffset].PositionX,
                                                part[ss.SpringPairIndices[i * 2] + ss.SpringOffset].PositionY,
                                                part[ss.SpringPairIndices[i * 2] + ss.SpringOffset].PositionZ),
                                            new Point3d(
                                                part[ss.SpringPairIndices[i * 2 + 1] + ss.SpringOffset].PositionX,
                                                part[ss.SpringPairIndices[i * 2 + 1] + ss.SpringOffset].PositionY,
                                                part[ss.SpringPairIndices[i * 2 + 1] + ss.SpringOffset].PositionZ)));
                                    lns.Append(l, p);
                                }
                            }
                        }
                    }
                }
            }

            DA.SetDataTree(0, pts);
            DA.SetDataTree(1, vel);
            DA.SetDataTree(2, lns);
            DA.SetDataTree(3, msh);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.getSprings;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{cf170ddb-d93e-47a2-abec-8b1850a4f125}"); }
        }
    }
}