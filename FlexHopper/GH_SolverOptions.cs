using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FlexCLI;
using FlexHopper.Properties;

namespace FlexHopper
{
    public class GH_SolverOptions : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_SolverOptions class.
        /// </summary>
        public GH_SolverOptions()
          : base("Flex Solver Options", "Opts",
              "",
              "Flex", "Engine")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Time step", "dt", "Time step per engine iteration", GH_ParamAccess.item, 0.0166666666667);
            pManager.AddIntegerParameter("Sub Steps", "SubSteps", "Number of sub-steps in each time step. Collision detection is performed per sub-step. Therefore many sub-steps are slow but more reliable.", GH_ParamAccess.item, 3);
            pManager.AddIntegerParameter("NumIterations", "NumIt", "Number of iterations to be performed per sub step. A higher value ensures accurate computation at the cost of speed.", GH_ParamAccess.item, 3);
            pManager.AddIntegerParameter("Scene Mode", "sMode", "Define how FlexHopper reacts to changes in your scenes:\n0 - Update existing scene: stiffnesses, anchor positions, and inflation pressures are updated. No new particles can be added to the scene.\n1 - Append scene: New particles are added whenever scene constructors are updated (good for fountains or whenever you want to add new particles)\n2 - Lock Mode: No changes in scenes are considered at all. This is the fastest mode, but doesn't allow much interaction during runtime.", GH_ParamAccess.item, 0);
            pManager.AddIntegerParameter("Fixed Number of Iteration", "fIter", "When positive, the solver will perform the supplied number of calculation cycles, before outputting. Useful, when you don't need to see the system converge, but want only one output after n iterations. Also faster, than normal mode. CAUTION: This might take a while to compute.", GH_ParamAccess.item, -1);
            pManager.AddIntegerParameter("Memory Requirements", "memQ", "Flex needs to reserve memory on GPU and RAM for your simulation. By telling the engine up front how detailed your simulation will be, you can avoid using excessive amounts of memory, or request more memory for big scenes. Normally default vals should be fine. Supply a list containing:\n[0] max nr. of particles (default 131072)\n[1] max nr. of neighbors per particle (default: 96)\n[2] max nr. of collision body entries (default: 65536)\n[3] max nr. of mesh vertices in collision meshes (default: 65536)\n[4] max nr. of mesh faces in collision meshes (default: 65536)\n[5] max nr. of mesh faces in convex meshes (default: 65536)\n[6] max nr. of rigid bodies (default: 65536)\n[7] max nr. of springs (default: 196608)\n[8] max nr. of cloth triangles (default: 131072)\nIMPORTANT NOTE: For input nr. [2] max nr. of collision body entries: This is not the number of collision objects but the number of memory entries the engine needs to make per collision objects. Planes require 0 entries, spheres require 1 entry, boxes require 3 entries, meshes and convex meshes require 4 entries.\nSo if you have a scene of 5 planes, 1000 spheres, 100 boxes and 10 meshes, you should set this value to 5*0 + 1000*1 + 100*3 + 10*4", GH_ParamAccess.list, defaultMemq);
            pManager.AddNumberParameter("Stability Scale", "stabS", "Due to some instability issues, particle systems of very large x,y,z values (e.g. when working in millimeter scale), sometimes drift sideways without any reason. Stability scale helps resolving this issue. It simply scales the x,y,z-values of your entire scene before it enters the engine and scales them back again afterwards. If objects in your scene start to drift and you have very large or very small x,y,z values, apply this scaling factor, so you have x,y,z values in the approximate range of -10 to 10. Parameter values like radius or collision margins are scaled automatically too, so you don't have to adjust them yourself.", GH_ParamAccess.item, 1.0);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Solver Options", "Options", "Solver options object to be passed into the engine.", GH_ParamAccess.item);
        }

        List<int> defaultMemq = new List<int> { 131072, 96, 65536, 65536, 65536, 65536, 65536, 196608, 131972 };

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double dt = 0.0166667;
            int sS = 3;
            int nI = 3;
            int sM = 0;
            int fI = -1;
            double stabS = 1.0;
            var memq = new List<int>();

            DA.GetData(0, ref dt);
            DA.GetData(1, ref sS);
            DA.GetData(2, ref nI);
            DA.GetData(3, ref sM);
            DA.GetData(4, ref fI);
            DA.GetDataList(5, memq);
            DA.GetData(6, ref stabS);

            if (dt == 0.0 || sS == 0)
                throw new Exception("Neither dt nor SubSteps can be zero!");

            if(memq.Count == 0 || memq.Count != 9)
            {
                if(memq.Count > 0)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Memq input is not valid. Must contain nine integers. Defaults were used.");
                memq = defaultMemq;
            }

            DA.SetData(0, new FlexSolverOptions((float)dt, sS, nI, sM, fI, memq.ToArray(), (float)Math.Max(stabS, 0.0001)));
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.opts;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{7a8cd854-71e8-453f-81e5-d2f37d53c7c8}"); }
        }
    }
}