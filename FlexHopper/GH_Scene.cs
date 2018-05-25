using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FlexCLI;
using FlexHopper.Properties;

namespace FlexHopper
{
    public class GH_Scene : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_Scene class.
        /// </summary>
        public GH_Scene()
          : base("Flex Scene", "Scene",
              "Create a scene object containing all moving geometry.",
              "Flex", "Setup")
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
            pManager.AddGenericParameter("Soft Bodies", "Softs", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Spring Systems", "Springs", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Cloth", "Cloths", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Inflatables", "Inflatables", "", GH_ParamAccess.list);
            pManager.AddGenericParameter("Local Constraints", "Constraints", "Add additional custom constraints. The indices supplied in these constraints refer to all particles combined in this specific scene. These constraints supplement earlier constraint inputs.", GH_ParamAccess.list);
            for(int i = 0; i < 8; i++)
            {
                pManager[i].Optional = true;
                pManager[i].DataMapping = GH_DataMapping.Flatten;
            }
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
            List<SoftBody> softs = new List<SoftBody>();
            List<SpringSystem> springs = new List<SpringSystem>();
            List<Cloth> cloths = new List<Cloth>();
            List<Inflatable> inflatables = new List<Inflatable>();
            List<ConstraintSystem> constraints = new List<ConstraintSystem>();

            DA.GetDataList(0, parts);
            DA.GetDataList(1, fluids);
            DA.GetDataList(2, rigids);
            DA.GetDataList(3, softs);
            DA.GetDataList(4, springs);
            DA.GetDataList(5, cloths);
            DA.GetDataList(6, inflatables);
            DA.GetDataList(7, constraints);


            foreach (FlexParticle p in parts)
                scene.RegisterParticles(new float[3] { p.PositionX, p.PositionY, p.PositionZ }, new float[3] { p.VelocityX, p.VelocityY, p.VelocityZ }, new float[1] { p.InverseMass }, p.IsFluid, p.SelfCollision, p.GroupIndex);

            foreach (Fluid f in fluids)
                scene.RegisterFluid(f.Positions, f.Velocities, f.InvMasses, f.GroupIndex);

            foreach(RigidBody r in rigids)
                scene.RegisterRigidBody(r.Vertices, r.VertexNormals, r.Velocity, r.InvMasses, r.Stiffness, r.GroupIndex);

            foreach (SoftBody s in softs)
            {
                if (s.Asset != 0)
                    scene.RegisterAsset(s.Asset, s.Velocity, s.InvMass, s.GroupIndex, true);
            }
            foreach (SpringSystem s in springs)
                s.SpringOffset = scene.RegisterSpringSystem(s.Positions, s.Velocities, s.InvMasses, s.SpringPairIndices, s.Stiffnesses, s.TargetLengths, s.SelfCollision, s.AnchorIndices, s.GroupIndex);

            foreach (Cloth c in cloths)
                scene.RegisterCloth(c.Positions, c.Velocities, c.InvMasses, c.Triangles, c.TriangleNormals, c.StretchStiffness, c.BendingStiffness, c.PreTensionFactor, c.AnchorIndices, c.SelfCollision, c.GroupIndex);

            foreach (Inflatable inf in inflatables)
                scene.RegisterInflatable(inf.Positions, inf.Velocities, inf.InvMasses, inf.Triangles, inf.TriangleNormals, inf.StretchStiffness, inf.BendingStiffness, inf.PreTensionFactor, inf.RestVolume, inf.OverPressure, inf.ConstraintScale, inf.AnchorIndices, inf.SelfCollision, inf.GroupIndex);

            foreach (ConstraintSystem c in constraints)
                scene.RegisterCustomConstraints(c.AnchorIndices, c.ShapeMatchingIndices, c.ShapeStiffness, c.SpringPairIndices, c.SpringStiffnesses, c.SpringTargetLengths, c.TriangleIndices, c.TriangleNormals);

            DA.SetData(0, scene);
        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.scene;
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