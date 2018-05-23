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
    public class GH_GetCloths : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_GetSpringSystems class.
        /// </summary>
        public GH_GetCloths()
          : base("Get Cloths", "Cloths",
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
            pManager.AddIntegerParameter("n", "n", "You can chose to only draw cloths every nth solver iteration. This significantly speeds up internal simulation at the cost of less smooth appearance. Leave at 0 for preview-only mode.", GH_ParamAccess.item, 0);
            pManager.AddGenericParameter("Cloth object", "Cloth", "Optionally connect the original 'Cloth' component to draw the respective mesh.", GH_ParamAccess.list);
            //pManager.AddGenericParameter("Preview material", "Mat", "Optionally define the preview mesh's display material", GH_ParamAccess.item);
            pManager[2].Optional = true;
            //pManager[3].Optional = true;
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
        DisplayMaterial mat = new DisplayMaterial();

        GH_Structure<GH_Point> pts = new GH_Structure<GH_Point>();
        GH_Structure<GH_Vector> vel = new GH_Structure<GH_Vector>();
        GH_Structure<GH_Mesh> msh = new GH_Structure<GH_Mesh>();
        GH_Structure<GH_Mesh> draw_msh = new GH_Structure<GH_Mesh>();

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            counter++;
            DA.GetData(1, ref n);
            n = Math.Abs(n);

            Flex flex = null;
            List<Cloth> cloths = new List<Cloth>();

            if(!DA.GetData(0, ref flex)) return;
            DA.GetDataList(2, cloths);

            if (flex == null)
                throw new Exception("Invalid flex class");

            List<FlexParticle> part = flex.Scene.GetClothParticles();

            if (n != 0 && counter % n == 0)
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

            if (cloths.Count > 0)
            {
                DA.GetData(3, ref mat);

                draw_msh = new GH_Structure<GH_Mesh>();
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
                    draw_msh.Append(new GH_Mesh(c.Mesh), p);
                }
            }
            

            if (n != 0 && counter % n == 0)
            {
                msh = draw_msh;
                DA.SetDataTree(0, pts);
                DA.SetDataTree(1, vel);
                DA.SetDataTree(2, msh);
            }
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.getCloth;
            }
        }

        public override void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            foreach (GH_Mesh m in draw_msh)
                args.Display.DrawMeshShaded(m.Value, mat);

        }

        bool drawMeshWires = true;

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {

            ToolStripMenuItem item1 = Menu_AppendItem(menu, "Draw Mesh Wires", item1_Clicked, true, drawMeshWires);
        }

        private void item1_Clicked(object sender, EventArgs e)
        {
            RecordUndoEvent("drawMeshWires");
            drawMeshWires = !drawMeshWires;
            ExpireSolution(true);
        }

        public override void DrawViewportWires(IGH_PreviewArgs args)
        {
            if (!drawMeshWires)
                return;
            foreach (GH_Mesh m in draw_msh)
                args.Display.DrawMeshWires(m.Value, mat.Diffuse);
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