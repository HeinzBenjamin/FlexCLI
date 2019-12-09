using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using FlexHopper.Properties;

namespace FlexHopper.GH_Util
{
    public class GH_TelepathyIn_Attributes : GH_ComponentAttributes
    {
        //GH_TelepathyIn Owner;
        public GH_TelepathyIn_Attributes(GH_TelepathyIn component) : base(component) { }
        
        protected override void Render(GH_Canvas canvas, Graphics graphics, GH_CanvasChannel channel)
        {
            
            var owner = ((GH_TelepathyIn)Owner);
            if (channel == GH_CanvasChannel.Wires && !owner.hideWire)
            {
                
                if(owner.OutComponents != null && owner.OutComponents.Count > 0)
                {
                    var hash = MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(owner.NickName));
                    var col = System.Drawing.Color.FromArgb(200, hash[0] / 2, hash[1] / 2, hash[2] / 2);

                    //var inputPt = Owner.Params.Input[0].Attributes.InputGrip;
                    var inputY = Owner.Attributes.Bounds.Y + 0.5f * Owner.Attributes.Bounds.Height;
                    var inputX = Owner.Attributes.Bounds.Right;
                    var inputPt = new PointF(inputX, inputY);
                    foreach(IGH_DocumentObject o in owner.OutComponents)
                    {
                        var c = (GH_Component)o;
                        var outputX = c.Attributes.Bounds.X + 0.5f * c.Attributes.Bounds.Width;
                        var outputY = c.Attributes.Bounds.Y + 0.5f * c.Attributes.Bounds.Height;
                        var outputPt = new PointF(outputX, outputY);
                        var pen = new Pen(col, 2);
                        pen.DashStyle = DashStyle.DashDotDot;
                        //graphics.DrawLine(pen, inputPt.X, inputPt.Y, outputPt.X, outputPt.Y);
                        //graphics.DrawCurve(pen, new PointF[2] { inputPt, outputPt }, 1.0f);
                        var pt2 = inputPt;
                        pt2.X += (outputPt.X - inputPt.X) * 0.25f;
                        pt2.Y += (outputPt.Y - inputPt.Y) * 0.15f;
                        //graphics.DrawBezier(pen, inputPt, pt2, pt3, outputPt);
                        graphics.DrawCurve(pen, new PointF[3] { inputPt, pt2, outputPt }, 0.35f);
                        pen.Dispose();
                    }
                }
            }

            base.Render(canvas, graphics, channel);
        }
        
        
    }
    public class GH_TelepathyIn : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the TelepathyIn class.
        /// </summary>
        public GH_TelepathyIn()
          : base("Telepathy Input", "rename_me",
              "Telepathy let's you make data from one component reappear anywhere else in your script. Even as an input to that component. So you can easily form script loops to reuse data as an input for itself. You can couple specific telepathy input and output components by giving them the same nickname. Connect the output of a component that you want to reuse somewhere else in the script here.",
              "Flex", "Util")
        {
            base.m_attributes = new GH_TelepathyIn_Attributes(this);
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Data", "data", "", GH_ParamAccess.tree);
            pManager.AddBooleanParameter("Stop", "stop", "Interrupt data transmission", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            
        }
        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            ToolStripMenuItem item1 = Menu_AppendItem(menu, "Deep copy", (object sender, EventArgs e) => { deepDuplicate = !deepDuplicate; }, true, deepDuplicate);
            item1.ToolTipText = "If ticked all data is copied to the telepathy output component. Otherwise it is passed by reference to the original object which meaning that there is only one object. Changes to this object are then effective in all components at once.";
            ToolStripMenuItem item2 = Menu_AppendItem(menu, "Hide wires", (object sender, EventArgs e) => { hideWire = !hideWire; }, true, hideWire);

        }

        public bool deepDuplicate = true;
        public bool hideWire = false;
        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<IGH_Goo> telepathy = null;
            bool stop = false;
            DA.GetDataTree(0, out telepathy);
            DA.GetData(1, ref stop);
            if (stop)
                return;

            OutComponents = FindTelepathyOut(NickName);
            foreach (GH_TelepathyOut c in OutComponents)
            {
                if (deepDuplicate)
                    c.telepathy = telepathy.Duplicate();
                else
                    c.telepathy = telepathy;
                c.ExpireSolution(true);
            }
        }

        public List<IGH_DocumentObject> OutComponents = null;
        List<IGH_DocumentObject> FindTelepathyOut(string id = "")
        {
            List<IGH_DocumentObject> all = new List<IGH_DocumentObject>();
            var doc = OnPingDocument();
            foreach (var o in doc.Objects)
            {
                GH_TelepathyOut f = new GH_TelepathyOut();
                if (o.ComponentGuid == f.ComponentGuid && (((GH_TelepathyOut)o).NickName == id))
                {
                    all.Add(o);

                }
            }
            return all;

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
                return Resources.telepathy_in;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5cadca85-28b9-4ded-8b2c-9c5f84bdc7bd"); }
        }
    }
}