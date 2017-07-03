using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FlexCLI;
using FlexHopper.Properties;

namespace FlexHopper
{
    public class GH_ParamsFromFile : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GH_ParamsFromFile()
          : base("FlexParameters from .xml file", "Params",
              "Set environmental parameters for your simulation. Link a .xml file (INFO: Auto update doesn't work yet, so if you change you .xml file you'll have to manually recompute the component.",
              "Flex", "Setup")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("File path", "Path", "Path to .xml file", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FlexParams", "Params", "FlexParams object to be passed into the engine.", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>

        //private System.Delegate dUpdate(string folder, string filter);

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = "";
            FlexParams param = new FlexParams();

            DA.GetData(0, ref path);

            XmlDocument doc = new XmlDocument();
            string folder = "";
            if (!path.Contains("/") && !path.Contains(@"\"))
            {
                folder = this.OnPingDocument().FilePath;
                path = folder.Substring(0, folder.LastIndexOf(@"\") + 1) + path;
            }

            doc.Load(path);


            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                #region get param
                if (node.Name == "Adhesion")
                    param.Adhesion = float.Parse(node.InnerText);

                else if (node.Name == "AnisotropyMax")
                    param.AnisotropyMax = float.Parse(node.InnerText);

                else if (node.Name == "AnisotropyMin")
                    param.AnisotropyMin = float.Parse(node.InnerText);

                else if (node.Name == "AnisotropyScale")
                    param.AnisotropyScale = float.Parse(node.InnerText);

                else if (node.Name == "Buoyancy")
                    param.Buoyancy = float.Parse(node.InnerText);

                else if (node.Name == "Cohesion")
                    param.Cohesion = float.Parse(node.InnerText);

                else if (node.Name == "CollisionDistance")
                    param.CollisionDistance = float.Parse(node.InnerText);

                else if (node.Name == "Damping")
                    param.Damping = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseBallistic")
                    param.DiffuseBallistic = int.Parse(node.InnerText);

                else if (node.Name == "DiffuseBuoyancy")
                    param.DiffuseBuoyancy = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseDrag")
                    param.DiffuseDrag = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseLifetime")
                    param.DiffuseLifetime = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseSortAxisX")
                    param.DiffuseSortAxisX = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseSortAxisY")
                    param.DiffuseSortAxisY = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseSortAxisZ")
                    param.DiffuseSortAxisZ = float.Parse(node.InnerText);

                else if (node.Name == "DiffuseThreshold")
                    param.DiffuseThreshold = float.Parse(node.InnerText);

                else if (node.Name == "Dissipation")
                    param.Dissipation = float.Parse(node.InnerText);

                else if (node.Name == "Drag")
                    param.Drag = float.Parse(node.InnerText);

                else if (node.Name == "DynamicFriction")
                    param.DynamicFriction = float.Parse(node.InnerText);

                else if (node.Name == "Fluid")
                    param.Fluid = bool.Parse(node.InnerText);

                else if (node.Name == "FluidRestDistance")
                    param.FluidRestDistance = float.Parse(node.InnerText);

                else if (node.Name == "FreeSurfaceDrag")
                    param.FreeSurfaceDrag = float.Parse(node.InnerText);

                else if (node.Name == "GravityX")
                    param.GravityX = float.Parse(node.InnerText);

                else if (node.Name == "GravityY")
                    param.GravityY = float.Parse(node.InnerText);

                else if (node.Name == "GravityZ")
                    param.GravityZ = float.Parse(node.InnerText);

                else if (node.Name == "Lift")
                    param.Lift = float.Parse(node.InnerText);

                else if (node.Name == "MaxAcceleration")
                    param.MaxAcceleration = float.Parse(node.InnerText);

                else if (node.Name == "MaxSpeed")
                    param.MaxSpeed = float.Parse(node.InnerText);

                else if (node.Name == "NumIterations")
                    param.NumIterations = int.Parse(node.InnerText);

                else if (node.Name == "NumPlanes")
                    param.NumPlanes = int.Parse(node.InnerText);

                else if (node.Name == "ParticleCollisionMargin")
                    param.ParticleCollisionMargin = float.Parse(node.InnerText);

                else if (node.Name == "ParticleFriction")
                    param.ParticleFriction = float.Parse(node.InnerText);

                else if (node.Name == "PlasticCreep")
                    param.PlasticCreep = float.Parse(node.InnerText);

                else if (node.Name == "PlasticThreshold")
                    param.PlasticThreshold = float.Parse(node.InnerText);

                else if (node.Name == "Radius")
                    param.Radius = float.Parse(node.InnerText);

                else if (node.Name == "RelaxationFactor")
                    param.RelaxationFactor = float.Parse(node.InnerText);

                else if (node.Name == "RelaxationMode")
                    param.RelaxationMode = int.Parse(node.InnerText);

                else if (node.Name == "Restitution")
                    param.Restitution = float.Parse(node.InnerText);

                else if (node.Name == "ShapeCollisionMargin")
                    param.ShapeCollisionMargin = float.Parse(node.InnerText);

                else if (node.Name == "ShockPropagation")
                    param.ShockPropagation = float.Parse(node.InnerText);

                else if (node.Name == "SleepThreshold")
                    param.SleepThreshold = float.Parse(node.InnerText);

                else if (node.Name == "Smoothing")
                    param.Smoothing = float.Parse(node.InnerText);

                else if (node.Name == "SolidPressure")
                    param.SolidPressure = float.Parse(node.InnerText);

                else if (node.Name == "SolidRestDistance")
                    param.SolidRestDistance = float.Parse(node.InnerText);

                else if (node.Name == "StaticFriction")
                    param.StaticFriction = float.Parse(node.InnerText);

                else if (node.Name == "SurfaceTension")
                    param.SurfaceTension = float.Parse(node.InnerText);

                else if (node.Name == "Viscosity")
                    param.Viscosity = float.Parse(node.InnerText);

                else if (node.Name == "VorticityConfinement")
                    param.VorticityConfinement = float.Parse(node.InnerText);

                else if (node.Name == "WindX")
                    param.WindX = float.Parse(node.InnerText);

                else if (node.Name == "WindY")
                    param.WindY = float.Parse(node.InnerText);

                else if (node.Name == "WindZ")
                    param.WindZ = float.Parse(node.InnerText);

                else
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Param couldn't be identified: " + node.Name);

                #endregion
            }

            DA.SetData(0, param);

        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.params1;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{fa2568e6-1f24-4613-8d95-1cad2a3137b1}"); }
        }
    }
}
