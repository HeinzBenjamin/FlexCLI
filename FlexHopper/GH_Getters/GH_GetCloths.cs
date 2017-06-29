using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using FlexCLI;

namespace FlexHopper.GH_Getters
{
    public class GH_GetCloths : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_GetSpringSystems class.
        /// </summary>
        public GH_GetCloths()
          : base("Get Cloths", "Cloths",
              "",
              "Flex", "Getters")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Flex Object", "Flex", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("n", "n", "You can chose to only draw cloths every nth solver iteration. This significantly speeds up internal simulation at the cost of less smooth appearance.", GH_ParamAccess.item, 1);
            pManager.AddBooleanParameter("Draw Particles", "drawP", "Not drawing particles speeds up the simulation.", GH_ParamAccess.item, false);
            pManager.AddGenericParameter("Cloth object", "Cloth", "Optionally connect the original 'Cloth' component to draw the respective mesh.", GH_ParamAccess.list);
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
                List<Cloth> cloths = new List<Cloth>();

                DA.GetData(0, ref flex);
                DA.GetData(2, ref drawP);
                DA.GetDataList(3, cloths);

                if (flex != null)
                {
                    List<FlexParticle> part = flex.Scene.GetClothParticles();
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

                    if (cloths.Count > 0)
                    {
                        msh = new GH_Structure<GH_Mesh>();
                        foreach (Cloth c in cloths)
                        {
                            List<FlexParticle> meshParts = new List<FlexParticle>();
                            foreach (FlexParticle fp in part)
                                if (fp.GroupIndex == c.GroupIndex)
                                    meshParts.Add(fp);

                            GH_Path p = new GH_Path(c.GroupIndex);
                            for (int i = 0; i < c.Mesh.Vertices.Count; i++)
                            {
                                
                                c.Mesh.Vertices[i] = new Point3f(meshParts[i + c.SpringOffset].PositionX, meshParts[i + c.SpringOffset].PositionY, meshParts[i + c.SpringOffset].PositionZ);
                            }
                            msh.Append(new GH_Mesh(c.Mesh), p);
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
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("cde9f74d-9b5d-41a1-88fe-2d1b9a814007"); }
        }
    }
}