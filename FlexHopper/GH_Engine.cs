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

using FlexHopper.Properties;

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
            pManager.AddGenericParameter("Flex Params", "Params", "Simulation Parameters", GH_ParamAccess.item);
            pManager.AddGenericParameter("Flex Collision Geometry", "Colliders", "Geometry to collide against", GH_ParamAccess.item);
            pManager.AddGenericParameter("Flex Force Fields", "Fields", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Flex Scene", "Scene", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Global Constraints", "Constraints", "Add additional custom constraints. The indices supplied in these constraints refer to all particles from all scenes combined to allow for constraints involving particles from multiple scenes. These constraints supplement earlier constraint inputs.", GH_ParamAccess.list);
            pManager.AddGenericParameter("Flex Solver Options", "Options", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "Reset", "", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("Go", "Go", "", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[3].DataMapping = GH_DataMapping.Flatten;
            pManager[4].DataMapping = GH_DataMapping.Flatten;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Flex Object", "Flex", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Information", "Info", "Information about solver:\n1. Iteration nr.\n2. Total time [ms]\n3. Time of last tick (without internal solver) [ms]\n4. Average time per tick [ms] (without internal solver)\n5. Time of last tick [ms] (internal solver only)\n6. Average time per tick [ms] (internal solver only)", GH_ParamAccess.list);
        }


        Flex flex = null;
        int counter = 0;
        Stopwatch sw = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();
        long totalTimeMs = 0;
        long totalUpdateTimeMs = 0;      //total time only consumed by this very engine component
        List<string> outInfo = new List<string>();

        //time stamps
        bool lockMode = false;
        int optionsTimeStamp = 0;
        int paramsTimeStamp = 0;
        List<int> sceneTimeStamps = new List<int>();
        List<int> constraintTimeStamps = new List<int>();
        List<int> forceFieldTimeStamps = new List<int>();
        int geomTimeStamp = 0;
        Task<int> UpdateTask;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //CONTINUE HERE!!!!
            UpdateTask = new Task<int>(() => Update());


            FlexParams param = new FlexParams();
            FlexCollisionGeometry geom = new FlexCollisionGeometry();
            List<FlexForceField> forceFields = new List<FlexForceField>();
            List<FlexScene> scenes = new List<FlexScene>();
            List<ConstraintSystem> constraints = new List<ConstraintSystem>();
            FlexSolverOptions options = new FlexSolverOptions();
            bool reset = false;
            
            bool go = false;

            DA.GetData(6, ref reset);
            DA.GetData(7, ref go);            

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
                DA.GetDataList(2, forceFields);
                DA.GetDataList(3, scenes);
                DA.GetDataList(4, constraints);
                DA.GetData(5, ref options);

                /*for (int i = 0; i < scenes.Count; i++)
                {
                    GH_Scene ghScene = (Params.Input[2].Sources[i].Attributes.Parent as GH_ComponentAttributes).Owner as GH_Scene;
                    ghScene.ExpireSolution(true);
                }*/

                sceneTimeStamps = new List<int>();
                forceFieldTimeStamps = new List<int>();

                //destroy old Flex instance
                if(flex != null)
                    flex.Destroy();

                //Create new instance and assign everything
                flex = new Flex();
                
                flex.SetParams(param);
                flex.SetCollisionGeometry(geom);
                flex.SetForceFields(forceFields);
                foreach (FlexForceField f in forceFields)
                    forceFieldTimeStamps.Add(f.TimeStamp);
                FlexScene scene = new FlexScene();
                foreach (FlexScene s in scenes)
                {
                    scene.AppendScene(s);
                    sceneTimeStamps.Add(s.TimeStamp);
                }
                foreach(ConstraintSystem c in constraints)
                {
                    scene.RegisterCustomConstraints(c.AnchorIndices, c.ShapeMatchingIndices, c.ShapeStiffness, c.SpringPairIndices, c.SpringStiffnesses, c.SpringTargetLengths, c.TriangleIndices, c.TriangleNormals);
                    constraintTimeStamps.Add(c.TimeStamp);
                }
                flex.SetScene(scene);
                flex.SetSolverOptions(options);

            }
            else if (go && flex != null && flex.IsReady())
            {
                DA.GetData(5, ref options);
                if (options.TimeStamp != optionsTimeStamp)
                    flex.SetSolverOptions(options);

                if (options.SceneMode == 0 || options.SceneMode == 1)
                {
                    //update params if timestamp expired
                    DA.GetData(0, ref param);
                    if (param.TimeStamp != paramsTimeStamp)
                    {
                        flex.SetParams(param);
                        paramsTimeStamp = param.TimeStamp;
                    }                        

                    //update geom if timestamp expired
                    if (DA.GetData(1, ref geom))
                    {
                        if (geom.TimeStamp != geomTimeStamp)
                        {
                            flex.SetCollisionGeometry(geom);
                            geomTimeStamp = geom.TimeStamp;
                        }
                    }
                    else if (geom != null)
                        flex.SetCollisionGeometry(new FlexCollisionGeometry());

                    //update forcefields where timestamp expired
                    DA.GetDataList(2, forceFields);
                    bool needsUpdate = false;
                    for (int i = forceFieldTimeStamps.Count; i < forceFields.Count; i++)
                    {
                        forceFieldTimeStamps.Add(forceFields[i].TimeStamp);
                        needsUpdate = true;
                    }
                    for (int i = 0; i < forceFields.Count; i++)
                        if (forceFields[i].TimeStamp != forceFieldTimeStamps[i])
                        {
                            needsUpdate = true;
                            forceFieldTimeStamps[i] = forceFields[i].TimeStamp;
                        }
                    if (needsUpdate)
                        flex.SetForceFields(forceFields);

                    //update scenes where timestamp expired
                    DA.GetDataList(3, scenes);                    
                    for (int i = sceneTimeStamps.Count; i < scenes.Count; i++)
                        sceneTimeStamps.Add(scenes[i].TimeStamp);
                    for(int i = 0; i < scenes.Count;i++)
                        if (scenes[i].TimeStamp != sceneTimeStamps[i])
                        {
                            if(options.SceneMode == 0)
                                flex.SetScene(flex.Scene.AlterScene(scenes[i], false));
                            else
                                flex.SetScene(flex.Scene.AppendScene(scenes[i]));                            
                            sceneTimeStamps[i] = scenes[i].TimeStamp;                            
                        }

                    DA.GetDataList(4, constraints);
                    for (int i = constraintTimeStamps.Count; i < constraints.Count; i++)
                        constraintTimeStamps.Add(constraints[i].TimeStamp);
                    for (int i = 0; i < constraints.Count; i++)
                    {
                        ConstraintSystem c = constraints[i];
                        if (c.TimeStamp != constraintTimeStamps[i])
                        {
                            if (!flex.Scene.RegisterCustomConstraints(c.AnchorIndices, c.ShapeMatchingIndices, c.ShapeStiffness, c.SpringPairIndices, c.SpringStiffnesses, c.SpringTargetLengths, c.TriangleIndices, c.TriangleNormals))
                                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Custom constraint indices exceeded particle count. No constraints applied!");
                            flex.SetScene(flex.Scene);
                            constraintTimeStamps[i] = constraints[i].TimeStamp;
                        }
                    }


                    
                }

                //Add timing info
                outInfo = new List<string>();
                counter++;
                outInfo.Add(counter.ToString());
                long currentTickTimeMs = sw.ElapsedMilliseconds;
                sw.Restart();
                totalTimeMs += currentTickTimeMs;
                outInfo.Add(totalTimeMs.ToString());
                outInfo.Add(currentTickTimeMs.ToString());
                float avTotalTickTime = ((float)totalTimeMs / (float)counter);
                outInfo.Add(avTotalTickTime.ToString());

                //start update
                UpdateTask.Start();

                //Add solver timing info
                int tickTimeSolver = UpdateTask.Result;
                totalUpdateTimeMs += tickTimeSolver;
                float ratUpdateTime = ((float)totalUpdateTimeMs / (float)counter);
                outInfo.Add(tickTimeSolver.ToString());
                outInfo.Add(ratUpdateTime.ToString());

                                
                
            }

            if (go && options.FixedTotalIterations < 1)
                ExpireSolution(true);

            else if (flex != null && UpdateTask.Status == TaskStatus.Running)
                UpdateTask.Dispose();

            if(flex != null)
                DA.SetData(0, flex);
            DA.SetDataList(1, outInfo);
        }
        

        int Update()
        {
            sw2.Restart();
            flex.UpdateSolver();
            return (int)sw2.ElapsedMilliseconds;
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.engine;
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