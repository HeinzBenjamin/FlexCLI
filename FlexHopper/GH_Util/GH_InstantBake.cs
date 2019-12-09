using System;
using System.Collections.Generic;
using System.Drawing;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino.Display;
using Rhino.Geometry;
using FlexHopper.Properties;
namespace FlexHopper.GH_Util
{
    public class GH_InstantBake : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_InstantBake class.
        /// </summary>
        public GH_InstantBake()
          : base("Instant Bake", "Bake",
              "Bakes a objects and deletes them again when updated, to account for movement. Use this to render your animation during runtime.",
              "Flex", "Util")
        {
        }

        List<Guid> ids = new List<Guid> {};
        Rhino.RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
        int counter = 0;

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            
            pManager.AddGeometryParameter("Objects", "Objects", "Objects to bake", GH_ParamAccess.list);
            pManager.AddTextParameter("Layer Name", "Layer", "", GH_ParamAccess.item, "Default");
            pManager.AddGenericParameter("Material Name", "Mat", "Specify render material by its name in your Rhino material table.", GH_ParamAccess.item);
            pManager.AddGenericParameter("Attributes (Optional)", "Att", "Add custom Rhino.DocObjects.ObjectAttributes - object. Either through scripting or by using Horster (or similar). If set this overrides the 'Layer Name' and 'Material Name' input", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Clear Layer", "ClearL", "<CAUTION!> This deletes all objects on the specified layer without further warning!", GH_ParamAccess.item, false);
            pManager[0].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[0].DataMapping = GH_DataMapping.Flatten;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("GUID", "id", "GUIDs of baked objects", GH_ParamAccess.list);
        }



        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<IGH_GeometricGoo> objs = new List<IGH_GeometricGoo>();
            DA.GetDataList(0, objs);

            string layerName = "Default";
            object material = null;
            bool clearL = false;

            DA.GetData(1, ref layerName);
            DA.GetData(2, ref material);

            /*Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();
            att.LayerIndex = doc.Layers.Find(layerName, true);
            att.MaterialIndex = doc.Materials.Find(matName, true);*/

            Rhino.DocObjects.ObjectAttributes att = new Rhino.DocObjects.ObjectAttributes();


            //Set material
            if (material != null)
            {
                string matName = "";
                DisplayMaterial mat = new DisplayMaterial();
                Color col = new Color();
                int isName = -1;
                int materialIndex = -1;
                try { matName = (material as GH_String).Value; isName = 1; }
                catch
                {
                    try
                    {
                        mat = (material as GH_Material).Value;
                        isName = 0;
                    }
                    catch
                    {
                        try { col = (material as GH_Colour).Value; mat = new DisplayMaterial(col); isName = 0; }
                        catch { AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Can't identify material object. Please supply render material name, GH material or color."); }
                    }
                }


                if (isName == 1)
                {
                    materialIndex = doc.Materials.Find(matName, true);
                    att.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;
                    if (materialIndex < 0)
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Couldn't find material by name. If you're sure the material exists in the Rhino Material list, try adding it to one object manually. After that it should work.");
                    att.MaterialIndex = materialIndex;
                }

                else
                {
                    materialIndex = doc.Materials.Add();
                    if (materialIndex > -1)
                    {
                        Rhino.DocObjects.Material m = doc.Materials[materialIndex];
                        m.Name = matName;
                        m.AmbientColor = mat.Ambient;
                        m.DiffuseColor = mat.Diffuse;
                        m.EmissionColor = mat.Emission;
                        //m.ReflectionColor = no equivalent
                        m.SpecularColor = mat.Specular;
                        m.Shine = mat.Shine;
                        m.Transparency = mat.Transparency;
                        //m.TransparentColor = no equivalent
                        m.CommitChanges();

                        att.MaterialSource = Rhino.DocObjects.ObjectMaterialSource.MaterialFromObject;
                        att.MaterialIndex = materialIndex;
                    }
                    else
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Couldn't add material. Try cleaning up your materials."); //This never happened to me.
                }
            }


            DA.GetData(3, ref att);
            DA.GetData(4, ref clearL);

            //Delete objects by GUID
            doc.Objects.Delete(ids, true);


            for (int i = 0; i < objs.Count; i++)
            {
                GeometryBase gb;
                Point3d pt;
                Brep b;
                if (ids.Count <= i) ids.Add(Guid.NewGuid());

                att.ObjectId = ids[i];
                if (objs[i].CastTo<Point3d>(out pt))
                    doc.Objects.AddPoint(pt, att);
                else if (objs[i].CastTo<Brep>(out b))
                    doc.Objects.AddBrep(b, att);
                else if (objs[i].CastTo<GeometryBase>(out gb))
                    doc.Objects.Add(gb, att);               

                else
                    throw new Exception("Object nr. " + i + " is not bakeable:\n" + objs[i].ToString() + ".\nIf it's a box or a curve, try turning it into a Brep by wiring it through a Brep container component before feeding it to Instant Bake.");
            }

            if (clearL)
            {
                foreach (Rhino.DocObjects.RhinoObject o in doc.Objects.FindByLayer(layerName))
                    doc.Objects.Delete(o, true);
            }

            if (counter >= 10)
            {
                Rhino.RhinoDoc.ActiveDoc.ClearUndoRecords(true);
                counter = 0;
            }


            DA.SetDataList(0, ids);
            counter++;
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.instant_bake;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("e92927d9-cefe-4990-bc71-d25acf366180"); }
        }
    }
}