using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;

using FlexCLI;

namespace FlexHopper
{
    public class GH_Engine : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_Engine class.
        /// </summary>
        public GH_Engine()
          : base("Flex Engine", "Flex",
              "Main component",
              "Flex", "Engine")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("FlexParams", "Params", "Simulation Parameters", GH_ParamAccess.item);
            pManager.AddGenericParameter("FlexCollisionGeometry", "Colliders", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("FlexScene", "Scene", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("FlexSolverOptions", "Options", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Lock Mode", "Lock", "If true, the engine won't consider input updates during runtime. If you want to emit scene objects during simulation or check for updates in params, collision geometry or solver options, this must be true.", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Reset", "Reset", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Go", "Go", "", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Flex Object", "Flex", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Information", "Info", "Information about solver:\n1. Iteration nr.\n2. Total time [ms]\n3. Time of last tick [ms]\n4. Average time per tick [ms]\n5. Time percentage consumed only by solver update [%]\n6. Time percentage consumed by everything other than solver update [%]", GH_ParamAccess.list);
        }


        Flex flex = null;
        int counter = 0;
        Stopwatch sw = new Stopwatch();
        long totalTimeMs = 0;
        long totalUpdateTimeMs = 0;      //total time only consumed by this very engine component
        List<string> outInfo = new List<string>();

        //time stamps
        bool lockMode = false;
        int optionsTimeStamp = 0;
        int paramsTimeStamp = 0;
        List<int> sceneTimeStamps = new List<int>();
        int geomTimeStamp = 0;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //CONTINUE HERE!!!!
            FlexParams param = new FlexParams();
            FlexCollisionGeometry geom = new FlexCollisionGeometry();
            List<FlexScene> scenes = new List<FlexScene>();
            FlexSolverOptions options = new FlexSolverOptions();
            bool reset = false;
            
            bool go = false;

            DA.GetData(4, ref lockMode);
            DA.GetData(5, ref reset);
            DA.GetData(6, ref go);            

            if (reset)
            {
                //reset everything related to time tracking
                counter = 0;
                totalTimeMs = 0;
                totalUpdateTimeMs = 0;
                sw.Stop();
                sw.Reset();

                outInfo = new List<string>();

                //retrieve relevant data
                DA.GetData(0, ref param);
                DA.GetData(1, ref geom);
                DA.GetDataList(2, scenes);
                DA.GetData(3, ref options);

                /*for (int i = 0; i < scenes.Count; i++)
                {
                    GH_Scene ghScene = (Params.Input[2].Sources[i].Attributes.Parent as GH_ComponentAttributes).Owner as GH_Scene;
                    ghScene.ExpireSolution(true);
                }*/

                sceneTimeStamps = new List<int>();

                //destroy old Flex instance
                if(flex != null)
                    flex.Destroy();

                //Create new instance and assign everything
                flex = new Flex();
                
                flex.SetParams(param);
                flex.SetCollisionGeometry(geom);
                FlexScene scene = new FlexScene();
                foreach (FlexScene s in scenes)
                {
                    scene.AppendScene(s);
                    sceneTimeStamps.Add(s.TimeStamp);
                }
                flex.SetScene(scene);
                flex.SetSolverOptions(options);
            }
            else if (go && flex != null && flex.IsReady())
            {
                //Add timing info
                outInfo = new List<string>();
                counter++;
                outInfo.Add(counter.ToString());
                long currentTickTimeMs = sw.ElapsedMilliseconds;
                totalTimeMs += currentTickTimeMs;                
                outInfo.Add(totalTimeMs.ToString());
                outInfo.Add(currentTickTimeMs.ToString());
                double avTotalTickTime = ((double)totalTimeMs / (double)counter);
                outInfo.Add(avTotalTickTime.ToString());

                if (!lockMode)
                {
                    DA.GetData(0, ref param);
                    if (param.TimeStamp != paramsTimeStamp)
                        flex.SetParams(param);

                    DA.GetData(1, ref geom);
                    if (geom.TimeStamp != geomTimeStamp)
                        flex.SetCollisionGeometry(geom);

                    DA.GetDataList(2, scenes);
                    
                    for (int i = sceneTimeStamps.Count; i < scenes.Count; i++)
                        sceneTimeStamps.Add(scenes[i].TimeStamp);
                    for(int i = 0; i < scenes.Count;i++)
                        if (scenes[i].TimeStamp != sceneTimeStamps[i])
                        {
                            flex.SetScene(flex.Scene.AppendScene(scenes[i]));
                            sceneTimeStamps[i] = scenes[i].TimeStamp;                            
                        }
                    

                    DA.GetData(3, ref options);
                    if (options.TimeStamp != optionsTimeStamp)
                        flex.SetSolverOptions(options);
                }

                //Actually Update the solver
                sw.Restart();
                flex.UpdateSolver();                

                //Add more timing info
                long postUpdateTick = sw.ElapsedMilliseconds;
                totalUpdateTimeMs += postUpdateTick;
                double ratUpdateTime = (((double)totalUpdateTimeMs / (double)counter) / avTotalTickTime) * 100.0;
                outInfo.Add(ratUpdateTime.ToString());
                outInfo.Add((100.0 - ratUpdateTime).ToString());
            }

            if(go)
                ExpireSolution(true);

            if(flex != null)
                DA.SetData(0, flex);
            DA.SetDataList(1, outInfo);
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
            get { return new Guid("{ce49fae6-905e-4485-8453-aa89e7c58bd5}"); }
        }
    }
}