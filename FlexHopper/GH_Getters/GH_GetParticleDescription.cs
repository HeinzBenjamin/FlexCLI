using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using FlexCLI;

namespace FlexHopper.GH_Getters
{
    public class GH_GetParticleDescription : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_GetParticleDescription class.
        /// </summary>
        public GH_GetParticleDescription()
          : base("Get Particle Description", "Particles",
              "<WARNING: This will slow down simulation significantly>\nRetrieve information about every single particle in the simulation.",
              "Flex", "Decomposition")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Flex Object", "Flex", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("n", "n", "You can chose to only retrieve particle information every nth solver iteration. This significantly speeds up internal simulation at the cost of less smooth appearance.", GH_ParamAccess.item, 1);
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {

            ToolStripMenuItem item1 = Menu_AppendItem(menu, "As tree by group index", item1_Clicked, true, asTree);
            item1.ToolTipText = "If set the particle info will be structured in tree branches according to the particles group indices.";
        }

        private void item1_Clicked(object sender, EventArgs e)
        {
            RecordUndoEvent("asTree");
            asTree = !asTree;
            ExpireSolution(true);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Particle Description", "Info", "", GH_ParamAccess.list);
        }

        int n = 1;
        int counter = 0;
        bool asTree = true;

        GH_Structure<GH_String> strings = new GH_Structure<GH_String>();

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            counter++;
            DA.GetData(1, ref n);
            n = Math.Max(1, n);

            if (counter % n == 0)
            {
                Flex flex = null;


                DA.GetData(0, ref flex);

                if (flex != null)
                {
                    List<FlexParticle> part = flex.Scene.GetAllParticles();

                    strings = new GH_Structure<GH_String>();

                    for(int i = 0; i < part.Count; i++)
                    {
                        string desc = "";
                        GH_Path path = new GH_Path(0);
                        if (asTree)
                            path = new GH_Path(part[i].GroupIndex);

                        desc = part[i].ToString();
                        desc = desc.Insert(8, " " + i.ToString());
                        strings.Append(new GH_String(desc), path);

                    }
                }
            }
            DA.SetDataTree(0, strings);
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
            get { return new Guid("9339df0b-2dfb-4b72-92f0-45e32262f5a5"); }
        }
    }
}