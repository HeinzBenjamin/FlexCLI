using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FlexCLI;

namespace FlexHopper
{
    public class GH_Scene : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_Scene class.
        /// </summary>
        public GH_Scene()
          : base("FlexScene", "Scene",
              "Create Scene",
              "Flex", "Environment")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Particles", "Particles", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Fluids", "Fluids", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Rigid Bodies", "Rigids", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Spring Systems", "Springs", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Cloth", "Cloths", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Inflatables", "Inflatables", "", GH_ParamAccess.list);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("FlexScene", "Scene", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            FlexScene scene = new FlexScene();

            List<FlexParticle> parts = new List<FlexParticle>();
            List<Fluid> fluids = new List<Fluid>();
            List<RigidBody> rigids = new List<RigidBody>();
            List<SpringSystem> springs = new List<SpringSystem>();
            List<Cloth> cloths = new List<Cloth>();
            List<Inflatable> inflatables = new List<Inflatable>();

            DA.GetDataList(0, parts);
            DA.GetDataList(1, fluids);
            DA.GetDataList(2, rigids);
            DA.GetDataList(3, springs);
            DA.GetDataList(4, cloths);
            DA.GetDataList(5, inflatables);


            foreach (FlexParticle p in parts)
                scene.RegisterParticles(new float[3] { p.PositionX, p.PositionY, p.PositionZ }, new float[3] { p.VelocityX, p.VelocityY, p.VelocityZ }, new float[1] { p.InverseMass }, p.IsFluid, p.SelfCollision, p.GroupIndex);
            foreach(Fluid f in fluids)
                scene.RegisterFluid(f.Positions, f.Velocities, f.InvMasses, f.GroupIndex);

            foreach(RigidBody r in rigids)
                scene.RegisterRigidBody(r.Vertices, r.VertexNormals, r.Velocity, r.InvMasses, r.Stiffness, r.GroupIndex);

            foreach (SpringSystem s in springs)
                s.SpringOffset = scene.RegisterSpringSystem(s.Positions, s.Velocities, s.InvMasses, s.SpringPairIndices, s.Stiffnesses, s.TargetLengths, s.SelfCollision, s.AnchorIndices, s.GroupIndex);

            foreach (Cloth c in cloths)
                scene.RegisterCloth(c.Positions, c.Velocities, c.InvMasses, c.Triangles, c.TriangleNormals, c.StretchStiffness, c.BendingStiffness, c.PreTensionFactor, c.AnchorIndices, c.GroupIndex);

            foreach (Inflatable inf in inflatables)
                scene.RegisterInflatable(inf.Positions, inf.Velocities, inf.InvMasses, inf.Triangles, inf.TriangleNormals, inf.StretchStiffness, inf.BendingStiffness, inf.PreTensionFactor, inf.RestVolume, inf.OverPressure, inf.ConstraintScale, inf.AnchorIndices, inf.GroupIndex);

            DA.SetData(0, scene);
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
            get { return new Guid("{f42ad687-357a-476e-842e-240b357d185c}"); }
        }
    }
}