using System;
using System.Collections.Generic;
using System.Windows.Forms;

using Grasshopper.Kernel;
using Rhino.Geometry;
using FlexCLI;

using FlexHopper.Properties;

namespace FlexHopper
{
    public class GH_Params : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_Params class.
        /// </summary>
        public GH_Params()
          : base("FlexParameters from values", "Parameters",
              "Set environmental parameters for your simulation. Specify them here.",
              "Flex", "Setup")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            #region loots of inputs
            //common params
            pManager.AddVectorParameter("Gravity", "Gravity", "Constant acceleration vector applied to al particles.", GH_ParamAccess.item, new Vector3d(0.0, 0.0, -9.81));
            pManager.AddNumberParameter("Radius", "Radius", "The maximum interaction radius for particles.", GH_ParamAccess.item, 0.15);

            //collisions
            pManager.AddNumberParameter("Solid Rest Distance", "SolidRestDistance", "The distance non-fluid particles attempt to maintain from each other, must be in the range (0, radius].", GH_ParamAccess.item, 0.075);
            pManager.AddNumberParameter("Fluid Rest Distance", "FluidRestDistance", "The distance fluid particles are spaced at the rest density, must be in the range (0, radius], for fluids this should generally be 50-70% of Radius, for rigids this can simply be the same as the particle radius.", GH_ParamAccess.item, 0.1);

            pManager.AddNumberParameter("Collision Distance", "CollisionDistance", "Distance particles maintain against shapes, note that for robust collision against triangle meshes this distance should be greater than zero.", GH_ParamAccess.item, 0.075);
            pManager.AddNumberParameter("Particle Collision Margin", "ParticleCollisionMargin", "Increases the radius used during neighbor finding, this is useful if particles are expected to move significantly during a single step to ensure contacts aren't missed on subsequent iterations.", GH_ParamAccess.item, 0.5);
            pManager.AddNumberParameter("Shape Collision Margin", "ShapeCollisionMargin", "Increases the radius used during contact finding against kinematic shapes.", GH_ParamAccess.item, 0.5);

            pManager.AddNumberParameter("Max Speed", "MaxSpeed", "	The magnitude of particle velocity will be clamped to this value at the end of each step.", GH_ParamAccess.item, float.MaxValue);
            pManager.AddNumberParameter("Max Acceleration", "MaxAcceleration", "The magnitude of particle acceleration will be clamped to this value at the end of each step (limits max velocity change per-second), useful to avoid popping due to large interpenetrations.", GH_ParamAccess.item, 1000.0);

            //friction
            pManager.AddNumberParameter("Dynamic Friction", "DynamicFriction", "Coefficient of friction used when colliding against shapes.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Static Friction", "StaticFriction", "Coefficient of static friction used when colliding against shapes.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Particle Friction", "ParticleFriction", "Coefficient of friction used when colliding particles.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Restitution", "Restitution", "Coefficient of restitution used when colliding against shapes, particle collisions are always inelastic.", GH_ParamAccess.item, 0.0);

            //more
            pManager.AddNumberParameter("Adhesion", "Adhesion", "Controls how strongly particles stick to surfaces they hit, default 0.0, range [0.0, +inf].", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Sleep Threshold", "SleepThreshold", "Particles with a velocity magnitude < this threshold will be considered fixed.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Shock Propagation", "ShockPropagation", "Artificially decrease the mass of particles based on height from a fixed reference point, this makes stacks and piles converge faster.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Dissipation", "Dissipation", "	Damps particle velocity based on how many particle contacts it has.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Damping", "Damping", "Viscous drag force, applies a force proportional, and opposite to the particle velocity.", GH_ParamAccess.item, 0.0);

            //fluid params
            pManager.AddBooleanParameter("Fluid", "Fluid", "If true then particles with group index 0 are considered fluid particles and interact using the position based fluids method.", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Viscosity", "Viscosity", "Smoothes particle velocities using XSPH viscosity.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Vorticity Confinement", "VorticityConfinement", "Increases vorticity by applying rotational forces to particles.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Cohesion", "Cohesion", "Control how strongly particles hold each other together, default: 0.025, range [0.0, +inf].", GH_ParamAccess.item, 0.025);
            pManager.AddNumberParameter("Surface Tension", "SurfaceTension", "Controls how strongly particles attempt to minimize surface area, default: 0.0, range: [0.0, +inf].", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Solid Pressure", "SolidPressure", "Add pressure from solid surfaces to particles.", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Free Surface Drag", "FreeSurfaceDrag", "Drag force applied to boundary fluid particles.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Buoyancy", "Buoyancy", "Gravity is scaled by this value for fluid particles.", GH_ParamAccess.item, 1.0);

            //rigid params
            pManager.AddNumberParameter("Plastic Threshold", "PlasticThreshold", "Particles belonging to rigid shapes that move with a position delta magnitude > threshold will be permanently deformed in the rest pose.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Plastic Creep", "PlasticCreep", "Controls the rate at which particles in the rest pose are deformed for particles passing the deformation threshold.", GH_ParamAccess.item, 0.0);

            //cloth inflatable
            pManager.AddVectorParameter("Wind", "Wind", "Constant acceleration applied to particles that belong to cloth and inflatables, drag needs to be > 0 for wind to affect triangles.", GH_ParamAccess.item, new Vector3d(0.0, 0.0, 0.0));
            pManager.AddNumberParameter("Drag", "Drag", "Drag force applied to particles belonging to cloth and inflatables, proportional to velocity^2*area in the negative velocity direction.", GH_ParamAccess.item, 0.0);
            pManager.AddNumberParameter("Lift", "Lift", "Lift force applied to particles belonging to cloth and inflatables, proportional to velocity^2*area in the direction perpendicular to velocity and (if possible), parallel to the plane normal.", GH_ParamAccess.item, 0.0);

            pManager.AddBooleanParameter("Relaxation Mode", "Relaxation Mode", "How the relaxation is applied inside the solver. If false, the relaxation factor is a fixed multiplier on each constraint's position delta. If true, the relaxation factor is a fixed multiplier on each constraint's delta divided by the particle's constraint count, convergence will be slower but more reliable.", GH_ParamAccess.item, true);
            pManager.AddNumberParameter("Relaxation Factor", "RelaxationFactor", "Control the convergence rate of the parallel solver, default: 1, values greater than 1 may lead to instability.", GH_ParamAccess.item, 1.0);

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
            pManager[9].Optional = true;
            pManager[10].Optional = true;
            pManager[11].Optional = true;
            pManager[12].Optional = true;
            pManager[13].Optional = true;
            pManager[14].Optional = true;
            pManager[15].Optional = true;
            pManager[16].Optional = true;
            pManager[17].Optional = true;
            pManager[18].Optional = true;
            pManager[19].Optional = true;
            pManager[20].Optional = true;
            pManager[21].Optional = true;
            pManager[22].Optional = true;
            pManager[23].Optional = true;
            pManager[24].Optional = true;
            pManager[25].Optional = true;
            pManager[26].Optional = true;
            pManager[27].Optional = true;
            pManager[28].Optional = true;
            pManager[29].Optional = true;
            pManager[30].Optional = true;
            pManager[31].Optional = true;
            #endregion
        }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem item1 = Menu_AppendItem(menu, "Save Parameters", save_btn_Clicked);
            item1.ToolTipText = "Save Flex parameters to .xml file. This can later be called with the 'FlexParams from .xml file'-component";
        }

        private void save_btn_Clicked(object sender, EventArgs e)
        {
            //string path = this.OnPingDocument().FilePath.Substring(0, this.OnPingDocument().FilePath.LastIndexOf(@"\") + 1);
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML files (*.xml)|*.xml";
            saveFileDialog.InitialDirectory = ".";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    List<string> paramString = new List<string>();
                    paramString.Add("<?xml version=\"1.0\"?>\n<params>");
                    paramString.Add("<" + nameof(param.Adhesion) + ">" + param.Adhesion + "</" + nameof(param.Adhesion) + ">");                    
                    paramString.Add("<" + nameof(param.Buoyancy) + ">" + param.Buoyancy + "</" + nameof(param.Buoyancy) + ">");
                    paramString.Add("<" + nameof(param.Cohesion) + ">" + param.Cohesion + "</" + nameof(param.Cohesion) + ">");
                    paramString.Add("<" + nameof(param.CollisionDistance) + ">" + param.CollisionDistance + "</" + nameof(param.CollisionDistance) + ">");
                    paramString.Add("<" + nameof(param.Damping) + ">" + param.Damping + "</" + nameof(param.Damping) + ">");
                    paramString.Add("<" + nameof(param.Dissipation) + ">" + param.Dissipation + "</" + nameof(param.Dissipation) + ">");
                    paramString.Add("<" + nameof(param.Drag) + ">" + param.Drag + "</" + nameof(param.Drag) + ">");
                    paramString.Add("<" + nameof(param.DynamicFriction) + ">" + param.DynamicFriction + "</" + nameof(param.DynamicFriction) + ">");
                    paramString.Add("<" + nameof(param.Fluid) + ">" + param.Fluid + "</" + nameof(param.Fluid) + ">");
                    paramString.Add("<" + nameof(param.FluidRestDistance) + ">" + param.FluidRestDistance + "</" + nameof(param.FluidRestDistance) + ">");
                    paramString.Add("<" + nameof(param.FreeSurfaceDrag) + ">" + param.FreeSurfaceDrag + "</" + nameof(param.FreeSurfaceDrag) + ">");
                    paramString.Add("<" + nameof(param.GravityX) + ">" + param.GravityX + "</" + nameof(param.GravityX) + ">");
                    paramString.Add("<" + nameof(param.GravityY) + ">" + param.GravityY + "</" + nameof(param.GravityY) + ">");
                    paramString.Add("<" + nameof(param.GravityZ) + ">" + param.GravityZ + "</" + nameof(param.GravityZ) + ">");
                    paramString.Add("<" + nameof(param.Lift) + ">" + param.Lift + "</" + nameof(param.Lift) + ">");
                    paramString.Add("<" + nameof(param.MaxAcceleration) + ">" + param.MaxAcceleration + "</" + nameof(param.MaxAcceleration) + ">");
                    paramString.Add("<" + nameof(param.MaxSpeed) + ">" + param.MaxSpeed + "</" + nameof(param.MaxSpeed) + ">");
                    paramString.Add("<" + nameof(param.ParticleCollisionMargin) + ">" + param.ParticleCollisionMargin + "</" + nameof(param.ParticleCollisionMargin) + ">");
                    paramString.Add("<" + nameof(param.ParticleFriction) + ">" + param.ParticleFriction + "</" + nameof(param.ParticleFriction) + ">");
                    paramString.Add("<" + nameof(param.PlasticCreep) + ">" + param.PlasticCreep + "</" + nameof(param.PlasticCreep) + ">");
                    paramString.Add("<" + nameof(param.PlasticThreshold) + ">" + param.PlasticThreshold + "</" + nameof(param.PlasticThreshold) + ">");
                    paramString.Add("<" + nameof(param.Radius) + ">" + param.Radius + "</" + nameof(param.Radius) + ">");
                    paramString.Add("<" + nameof(param.RelaxationFactor) + ">" + param.RelaxationFactor + "</" + nameof(param.RelaxationFactor) + ">");
                    paramString.Add("<" + nameof(param.RelaxationMode) + ">" + param.RelaxationMode + "</" + nameof(param.RelaxationMode) + ">");
                    paramString.Add("<" + nameof(param.Restitution) + ">" + param.Restitution + "</" + nameof(param.Restitution) + ">");
                    paramString.Add("<" + nameof(param.ShapeCollisionMargin) + ">" + param.ShapeCollisionMargin + "</" + nameof(param.ShapeCollisionMargin) + ">");
                    paramString.Add("<" + nameof(param.ShockPropagation) + ">" + param.ShockPropagation + "</" + nameof(param.ShockPropagation) + ">");
                    paramString.Add("<" + nameof(param.SleepThreshold) + ">" + param.SleepThreshold + "</" + nameof(param.SleepThreshold) + ">");
                    paramString.Add("<" + nameof(param.SolidPressure) + ">" + param.SolidPressure + "</" + nameof(param.SolidPressure) + ">");
                    paramString.Add("<" + nameof(param.SolidRestDistance) + ">" + param.SolidRestDistance + "</" + nameof(param.SolidRestDistance) + ">");
                    paramString.Add("<" + nameof(param.StaticFriction) + ">" + param.StaticFriction + "</" + nameof(param.StaticFriction) + ">");
                    paramString.Add("<" + nameof(param.SurfaceTension) + ">" + param.SurfaceTension + "</" + nameof(param.SurfaceTension) + ">");
                    paramString.Add("<" + nameof(param.Viscosity) + ">" + param.Viscosity + "</" + nameof(param.Viscosity) + ">");
                    paramString.Add("<" + nameof(param.VorticityConfinement) + ">" + param.VorticityConfinement + "</" + nameof(param.VorticityConfinement) + ">");
                    paramString.Add("<" + nameof(param.WindX) + ">" + param.WindX + "</" + nameof(param.WindX) + ">");
                    paramString.Add("<" + nameof(param.WindY) + ">" + param.WindY + "</" + nameof(param.WindY) + ">");
                    paramString.Add("<" + nameof(param.WindZ) + ">" + param.WindZ + "</" + nameof(param.WindZ) + ">");
                    paramString.Add("</params>");

                    System.IO.Stream stream = System.IO.File.Open(saveFileDialog.FileName, System.IO.FileMode.Create);
                    stream.Close();
                    System.IO.File.WriteAllLines(saveFileDialog.FileName, paramString);
                   
                }
                catch (Exception ex) { AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Couldn't save Flex parameters:\n" + ex.Message); }
            }
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FlexParams", "Params", "FlexParams object to be passed into the engine.", GH_ParamAccess.item);
        }

        FlexParams param = new FlexParams();

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Vector3d gra = new Vector3d(0.0, 0.0, -9.81);
            double rad = 0.15;
            double srd = 0.075;
            double frd = 0.075;
            double cod = 0.0;
            double pcm = 0.0;
            double scm = 0.0;
            double mxs = 0.0;
            double mxa = 0.0;
            double dyf = 0.0;
            double stf = 0.0;
            double paf = 0.0;
            double res = 0.0;
            double adh = 0.0;
            double slt = 0.0;
            double shp = 0.0;
            double dis = 0.0;
            double dam = 0.0;
            bool flu = true;
            double vis = 0.0;
            double vor = 0.0;
            double coh = 0.0;
            double suf = 0.0;
            double sop = 0.0;
            double frs = 0.0;
            double buo = 0.0;
            double plt = 0.0;
            double plc = 0.0;
            Vector3d wind = new Vector3d(0.0, 0.0, 0.0);
            double dra = 0.0;
            double lif = 0.0;
            bool rem = true;
            double rfa = 1.0;

            DA.GetData("Gravity", ref gra);
            DA.GetData("Radius", ref rad);
            DA.GetData("Solid Rest Distance", ref srd);
            DA.GetData("Fluid Rest Distance", ref frd);
            DA.GetData("Collision Distance", ref cod);
            DA.GetData("Particle Collision Margin", ref pcm);
            DA.GetData("Shape Collision Margin", ref scm);
            DA.GetData("Max Speed", ref mxs);
            DA.GetData("Max Acceleration", ref mxa);
            DA.GetData("Dynamic Friction", ref dyf);
            DA.GetData("Static Friction", ref stf);
            DA.GetData("Particle Friction", ref paf);
            DA.GetData("Restitution", ref res);
            DA.GetData("Adhesion", ref adh);
            DA.GetData("Sleep Threshold", ref slt);
            DA.GetData("Shock Propagation", ref shp);
            DA.GetData("Dissipation", ref dis);
            DA.GetData("Damping", ref dam);
            DA.GetData("Fluid", ref flu);
            DA.GetData("Viscosity", ref vis);
            DA.GetData("Vorticity Confinement", ref vor);
            DA.GetData("Cohesion", ref coh);
            DA.GetData("Surface Tension", ref suf);
            DA.GetData("Solid Pressure", ref sop);
            DA.GetData("Free Surface Drag", ref frs);
            DA.GetData("Buoyancy", ref buo);
            DA.GetData("Plastic Threshold", ref plt);
            DA.GetData("Plastic Creep", ref plc);
            DA.GetData("Wind", ref wind);
            DA.GetData("Drag", ref dra);
            DA.GetData("Lift", ref lif);
            DA.GetData("Relaxation Mode", ref rem);
            DA.GetData("Relaxation Factor", ref rfa);

            param.GravityX = (float)gra.X;
            param.GravityY = (float)gra.Y;
            param.GravityZ = (float)gra.Z;
            param.Radius = (float)rad;
            param.SolidRestDistance = (float)srd;
            param.FluidRestDistance = (float)frd;
            param.CollisionDistance = (float)cod;
            param.ParticleCollisionMargin = (float)pcm;
            param.ShapeCollisionMargin = (float)scm;
            param.MaxSpeed = (float)mxs;
            param.MaxAcceleration = (float)mxa;
            param.DynamicFriction = (float)dyf;
            param.StaticFriction = (float)stf;
            param.ParticleFriction = (float)paf;
            param.Restitution = (float)res;
            param.Adhesion = (float)adh;
            param.SleepThreshold = (float)slt;
            param.ShockPropagation = (float)shp;
            param.Dissipation = (float)dis;
            param.Damping = (float)dam;
            param.Fluid = flu;
            param.Viscosity = (float)vis;
            param.VorticityConfinement = (float)vor;
            param.Cohesion = (float)coh;
            param.SurfaceTension = (float)suf;
            param.SolidPressure = (float)sop;
            param.FreeSurfaceDrag = (float)frs;
            param.Buoyancy = (float)buo;
            param.PlasticThreshold = (float)plt;
            param.PlasticCreep = (float)plc;
            param.WindX = (float)wind.X;
            param.WindY = (float)wind.Y;
            param.WindZ = (float)wind.Z;
            param.Drag = (float)dra;
            param.Lift = (float)lif;
            if (!rem)
                param.RelaxationMode = 0;
            else
                param.RelaxationMode = 1;
            param.RelaxationFactor = (float)rfa;

            DA.SetData(0, param);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.params2;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("eae81423-9ae9-422c-a08e-5dabd87e86d8"); }
        }
    }
}