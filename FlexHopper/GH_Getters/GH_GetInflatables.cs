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
    public class GH_GetInflatables : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_GetSpringSystems class.
        /// </summary>
        public GH_GetInflatables()
          : base("Get Inflatables", "Inflatables",
              "",
              "Flex", "Decomposition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Flex Object", "Flex", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("n", "n", "You can chose to only draw inflatables every nth solver iteration. This significantly speeds up internal simulation at the cost of less smooth appearance.", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Draw Particles", "drawP", "Not drawing particles speeds up the simulation.", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Inflatable", "Inflatable", "Optionally connect the original 'Inflatable' component to draw the respective mesh.", GH_ParamAccess.list);
            pManager[3].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("Points", "Pts", "", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Vector", "Vec", "", GH_ParamAccess.tree);
            pManager.AddMeshParameter("Meshes", "Msh", "", GH_ParamAccess.tree);
        }

        int n = 1;
        int counter = 0;

        GH_Structure<GH_Point> pts = new GH_Structure<GH_Point>();
        GH_Structure<GH_Vector> vel = new GH_Structure<GH_Vector>();
        GH_Structure<GH_Mesh> msh = new GH_Structure<GH_Mesh>();

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            counter++;
            DA.GetData(1, ref n);
            n = Math.Max(1, n);

            if (counter % n == 0)
            {
                Flex flex = null;
                bool drawP = true;
                List<Inflatable> inflatables = new List<Inflatable>();

                DA.GetData(0, ref flex);
                DA.GetData(2, ref drawP);
                DA.GetDataList(3, inflatables);

                if (flex != null)
                {
                    List<FlexParticle> part = flex.Scene.GetInflatableParticles();
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

                    if (inflatables.Count > 0)
                    {
                        msh = new GH_Structure<GH_Mesh>();
                        foreach (Inflatable inf in inflatables)
                        {
                            List<FlexParticle> meshParts = new List<FlexParticle>();
                            foreach (FlexParticle fp in part)
                                if (fp.GroupIndex == inf.GroupIndex)
                                    meshParts.Add(fp);

                            GH_Path p = new GH_Path(inf.GroupIndex);
                            for (int i = 0; i < inf.Mesh.Vertices.Count; i++)
                            {
                                inf.Mesh.Vertices[i] = new Point3f(meshParts[i + inf.SpringOffset].PositionX, meshParts[i + inf.SpringOffset].PositionY, meshParts[i + inf.SpringOffset].PositionZ);
                            }
                            msh.Append(new GH_Mesh(inf.Mesh), p);
                        }
                    }
                }
            }

            DA.SetDataTree(0, pts);
            DA.SetDataTree(1, vel);
            DA.SetDataTree(2, msh);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.getInflatable;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("db48f9f6-eb9b-495c-9e8a-562be5e74fbf"); }
        }
    }
}