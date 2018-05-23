using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FlexCLI;
using FlexHopper.Properties;
using System.Windows.Forms;

namespace FlexHopper
{
    public class GH_ParamsFromFile : GH_Component
    {

        string path = "";
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public GH_ParamsFromFile()
          : base("Flex Parameters from .xml file", "Params",
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
            pManager[0].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FlexParams", "Params", "FlexParams object to be passed into the engine.", GH_ParamAccess.item);
        }

        bool isDefaultFile = false;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //string path = "";
            FlexParams param = new FlexParams();

            if (!isDefaultFile)
                DA.GetData(0, ref path);
            else
                isDefaultFile = false;

            XmlDocument doc = new XmlDocument();
            string folder = "";
            if (path == "")
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Input parameter 'path' failed to collect data.");
                return;
            }

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


        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem mm = Menu_AppendItem(menu, "Make default file", item1_Clicked);
            mm.ToolTipText = "Lets you dave a flex parameter file the default values to your drive, so you can just make changes to the default values.";
        }

        private void item1_Clicked(object sender, EventArgs e)
        {
            SaveFileDialog savefile = new SaveFileDialog();
            // set a default file name
            savefile.FileName = "defaultFlexParams.xml";
            GH_Document doc = this.OnPingDocument();
            //savefile.InitialDirectory = doc.FilePath.Substring(0, doc.FilePath.LastIndexOf(@"\"));
            savefile.InitialDirectory = ".";
            // set filters - this can be done in properties as well
            savefile.Filter = ".xml files (*.xml)|*.xml|All files (*.*)|*.*";

            if (savefile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string s = "<?xml version=\"1.0\"?><params><GravityX>0.0</GravityX><GravityY>0.0</GravityY><GravityZ>-9.81</GravityZ><WindX>0.0</WindX><WindY>0.0</WindY><WindZ>0.0</WindZ><Radius>0.15</Radius><Viscosity>0.0</Viscosity><DynamicFriction>0.0</DynamicFriction><StaticFriction>0.0</StaticFriction><ParticleFriction>0.0</ParticleFriction><FreeSurfaceDrag>0.0</FreeSurfaceDrag><Drag>0.0</Drag><Lift>0.0</Lift><FluidRestDistance>0.1</FluidRestDistance><SolidRestDistance>0.15</SolidRestDistance><Dissipation>0.0</Dissipation><Damping>0.0</Damping><ParticleCollisionMargin>0.075</ParticleCollisionMargin><ShapeCollisionMargin>0.075</ShapeCollisionMargin><CollisionDistance>0.075</CollisionDistance><PlasticThreshold>0.0</PlasticThreshold><PlasticCreep>0.0</PlasticCreep><Fluid>true</Fluid><SleepThreshold>0.0</SleepThreshold><ShockPropagation>0.0</ShockPropagation><Restitution>0.0</Restitution><MaxSpeed>3.402823466e+38</MaxSpeed><MaxAcceleration>100.0</MaxAcceleration><RelaxationMode>1</RelaxationMode><RelaxationFactor>1.0</RelaxationFactor><SolidPressure>1.0</SolidPressure><Adhesion>0.0</Adhesion><Cohesion>0.025</Cohesion><SurfaceTension>0.0</SurfaceTension><Buoyancy>1.0</Buoyancy></params>";
                    XmlDocument xdoc = new XmlDocument();
                    xdoc.LoadXml(s);
                    
                    Stream stream = System.IO.File.Open(savefile.FileName, System.IO.FileMode.Create);
                    xdoc.Save(stream);
                    path = savefile.FileName;
                    stream.Close();
                    isDefaultFile = true;
                    ExpireSolution(true);
                }
                catch (Exception ex)
                {
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "couldn't save param file:\n" + ex.ToString());
                }
            }
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
