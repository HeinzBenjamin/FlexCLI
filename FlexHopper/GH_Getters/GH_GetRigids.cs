using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;

using FlexCLI;
using FlexHopper.Properties;

namespace FlexHopper.GH_Getters
{
    public class GH_GetRigids : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the Rigids class.
        /// </summary>
        public GH_GetRigids()
          : base("Get Rigids", "Rigids",
              "Get rigid bodies from engine object",
              "Flex", "Decomposition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Flex Object", "Flex", "", GH_ParamAccess.item);
            pManager.AddIntegerParameter("n", "n", "You can chose to only draw rigids every nth solver iteration. This significantly speeds up internal simulation at the cost of less smooth appearance.", GH_ParamAccess.item, 1);            
            pManager.AddBooleanParameter("Deformable", "Def", "If off, meshes are displayed as if they were 100% stiff. this is faster but neglects all kinds of deformation.", GH_ParamAccess.item, true);
            pManager.AddBooleanParameter("Draw Particles", "DrawP", "Optionally not drawing particles can speed up the simulation.", GH_ParamAccess.item, true);
            pManager.AddGenericParameter("Rigids", "Rigids", "Optionally connect the original 'Rigid from Mesh' component to draw meshes.", GH_ParamAccess.list);
            pManager[4].Optional = true;

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

        GH_Structure<GH_Point> pts = new GH_Structure<GH_Point>();
        GH_Structure<GH_Vector> vel = new GH_Structure<GH_Vector>();
        GH_Structure<GH_Mesh> msh = new GH_Structure<GH_Mesh>();

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
                List<FlexParticle> part = new List<FlexParticle>();

                Flex flex = null;
                bool deformable = true;
                bool drawPts = true;
                List<RigidBody> rigids = new List<RigidBody>();

                DA.GetData(0, ref flex);
                DA.GetData(2, ref deformable);
                DA.GetData(3, ref drawPts);
                DA.GetDataList(4, rigids);

                if (flex != null)
                {
                    pts = new GH_Structure<GH_Point>();
                    vel = new GH_Structure<GH_Vector>();
                    if (drawPts)
                    {                        
                        part = flex.Scene.GetRigidParticles();

                        foreach (FlexParticle fp in part)
                        {
                            GH_Path p = new GH_Path(fp.GroupIndex);
                            pts.Append(new GH_Point(new Point3d(fp.PositionX, fp.PositionY, fp.PositionZ)), p);
                            vel.Append(new GH_Vector(new Vector3d(fp.VelocityX, fp.VelocityY, fp.VelocityZ)), p);
                        }
                    }

                    if (rigids.Count != 0)
                    {
                        msh = new GH_Structure<GH_Mesh>();

                        if (deformable && part.Count == 0)
                            part = flex.Scene.GetRigidParticles();
                        int rb_Index = 0;
                        
                        foreach (RigidBody r in rigids)
                        {
                            if (r.HasMesh())
                            {
                                GH_Mesh m = new GH_Mesh(r.Mesh.DuplicateMesh());
                                if (deformable)
                                {                                    
                                    for(int i = 0; i < m.Value.Vertices.Count; i++)
                                    {
                                        m.Value.Vertices[i] = new Point3f(part[rb_Index].PositionX, part[rb_Index].PositionY, part[rb_Index].PositionZ);

                                        rb_Index++;
                                    }                                    
                                }
                                else
                                {
                                    List<float> rotations = flex.Scene.GetRigidRotations();
                                    List<float> translations = flex.Scene.GetRigidTranslations();
                                    m.Value.Translate(translations[3 * rb_Index + 0] - r.MassCenter[0], translations[3 * rb_Index + 1] - r.MassCenter[1], translations[3 * rb_Index + 2] - r.MassCenter[2]);

                                    double halfAngle = Math.Acos(rotations[4 * rb_Index + 3]);
                                    double X = rotations[4 * rb_Index] / Math.Sin(halfAngle);
                                    double Y = rotations[4 * rb_Index + 1] / Math.Sin(halfAngle);
                                    double Z = rotations[4 * rb_Index + 2] / Math.Sin(halfAngle);
                                    double angle = 2.0 * halfAngle;

                                    m.Value.Rotate(angle, new Vector3d(X, Y, Z), new Point3d(translations[3 * rb_Index], translations[3 * rb_Index + 1], translations[3 * rb_Index + 2]));
                                    rb_Index++;
                                }
                                msh.Append(m, new GH_Path(r.GroupIndex));
                                
                            }
                        }
                            
                    }
                    else
                        msh = new GH_Structure<GH_Mesh>();
                    
                }
            }

            DA.SetDataTree(0, pts);
            DA.SetDataTree(1, vel);
            DA.SetDataTree(2, msh);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.getRigids;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{9bc5dc33-fa1b-408e-a667-ed98203855e2}"); }
        }
    }
}