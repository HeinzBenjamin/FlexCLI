using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using FlexCLI;
using FlexHopper.Properties;

namespace FlexHopper
{
    public class GH_CollisionGeometry : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GH_CollisionGeometry class.
        /// </summary>
        public GH_CollisionGeometry()
          : base("FlexCollisionGeometry", "CollGeometry",
              "Specifiy static geometry as colliding objects. Flex supports certain objects (Plane, Sphere and Box) more efficiently. If you want to register such objects as collision geometry, use the respective object input rather than meshes.",
              "Flex", "Setup")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Collision Planes", "Planes", "If you have planes to register, use this input rather than mesh or SDF.", GH_ParamAccess.list);
            pManager.AddSurfaceParameter("Collision Spheres","Spheres", "If you have spheres to register, use this input rather than mesh or SDF.", GH_ParamAccess.list);
            pManager.AddBoxParameter("Collision Boxes", "Boxes", "If you have spheres to register, use this input rather than mesh or SDF.", GH_ParamAccess.list);
            pManager.AddMeshParameter("Collision Meshes", "Meshes", "Make sure the mesh is triangulated, clean and all faces are pointing inwards", GH_ParamAccess.list);
            pManager.AddMeshParameter("Convex Meshes", "CMeshes", "Meshes that are known to be convex are being recognized faster. Add them here. (Currently only supports meshes with up to 64 faces)", GH_ParamAccess.list);
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[0].DataMapping = GH_DataMapping.Flatten;
            pManager[1].DataMapping = GH_DataMapping.Flatten;
            pManager[2].DataMapping = GH_DataMapping.Flatten;
            pManager[3].DataMapping = GH_DataMapping.Flatten;
            pManager[4].DataMapping = GH_DataMapping.Flatten;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Flex Collision Geometry", "Colliders", "", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<Plane> planes = new List<Plane>();
            List<Surface> spheres = new List<Surface>();
            List<Box> boxes = new List<Box>();
            List<Mesh> meshes = new List<Mesh>();
            List<Mesh> cmeshes = new List<Mesh>();

            FlexCollisionGeometry geom = new FlexCollisionGeometry();

            DA.GetDataList(0, planes);
            DA.GetDataList(1, spheres);
            DA.GetDataList(2, boxes);
            DA.GetDataList(3, meshes);
            DA.GetDataList(4, cmeshes);

            foreach (Plane p in planes)
            {
                double[] pe = p.GetPlaneEquation();
                geom.AddPlane((float)pe[0], (float)pe[1], (float)pe[2], (float)pe[3]);
            }

            foreach (Surface s in spheres)
            {
                Sphere sph;
                if (!s.TryGetSphere(out sph))
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "At least one sphere is not a sphere");
                else
                    geom.AddSphere(new float[] { (float)sph.Center.X,
                        (float)sph.Center.Y,
                        (float)sph.Center.Z }, 
                        (float)sph.Radius);
            }

            foreach (Box b in boxes)
            {
                if (!b.IsValid)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid box!");
                else
                {
                    Plane p = Plane.WorldXY;
                    p.Origin = b.Center;
                    Quaternion q = Quaternion.Rotation(Plane.WorldXY, b.Plane);
                    

                    geom.AddBox(new float[] { (float)(b.X.Length * 0.5), (float)(b.Y.Length * 0.5), (float)(b.Z.Length * 0.5) },
                        new float[] { (float)b.Center.X, (float)b.Center.Y, (float)b.Center.Z },
                        new float[] { (float)q.Vector.X, (float)q.Vector.Y, (float)q.Vector.Z, (float)q.Scalar});
                }
            }

            foreach(Mesh m in meshes)
            {
                if (!m.IsValid)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid mesh!");
                else
                {
                    //Flex wants face normals to be pointing inward
                    m.Flip(false, false, true);
                    
                    float[] vertices = new float[m.Vertices.Count * 3];
                    int[] faces = new int[m.Faces.Count * 3];
                    for (int i = 0; i < vertices.Length / 3; i++)
                    {
                        vertices[i * 3] = m.Vertices[i].X;
                        vertices[i * 3 + 1] = m.Vertices[i].Y;
                        vertices[i * 3 + 2] = m.Vertices[i].Z;
                    }
                    for(int i = 0; i < faces.Length / 3; i++)
                    {
                        faces[i * 3] = m.Faces[i].A;
                        faces[i * 3 + 1] = m.Faces[i].B;
                        faces[i * 3 + 2] = m.Faces[i].C;
                    }

                    //add mesh
                    geom.AddMesh(vertices, faces);

                    if (!m.IsClosed)
                    {
                        Mesh mm = m.DuplicateMesh();
                        mm.Flip(true, true, true);
                        vertices = new float[mm.Vertices.Count * 3];
                        faces = new int[mm.Faces.Count * 3];
                        for (int i = 0; i < vertices.Length / 3; i++)
                        {
                            vertices[i * 3] = mm.Vertices[i].X;
                            vertices[i * 3 + 1] = mm.Vertices[i].Y;
                            vertices[i * 3 + 2] = mm.Vertices[i].Z;
                        }
                        for (int i = 0; i < faces.Length / 3; i++)
                        {
                            faces[i * 3] = mm.Faces[i].A;
                            faces[i * 3 + 1] = mm.Faces[i].B;
                            faces[i * 3 + 2] = mm.Faces[i].C;
                        }

                        //add mesh
                        geom.AddMesh(vertices, faces);
                    }
                }
            }
            
            foreach(Mesh m in cmeshes)
            {
                if (!m.IsClosed || !m.IsValid)
                    AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid mesh!");
                else
                {
                    int planeCount = m.Faces.Count;

                    m.FaceNormals.ComputeFaceNormals();

                    float[] cPlanes = new float[planeCount * 4];
                    float[] upperLimit = new float[3] { (float)m.Vertices[0].X, (float)m.Vertices[0].Y, (float)m.Vertices[0].Z };
                    float[] lowerLimit = new float[3] { (float)m.Vertices[0].X, (float)m.Vertices[0].Y, (float)m.Vertices[0].Z };

                    foreach(Point3d v in m.Vertices)
                    {
                        if (v.X > upperLimit[0]) upperLimit[0] = (float)v.X;
                        if (v.Y > upperLimit[1]) upperLimit[1] = (float)v.Y;
                        if (v.Z > upperLimit[2]) upperLimit[2] = (float)v.Z;
                        if (v.X < lowerLimit[0]) lowerLimit[0] = (float)v.X;
                        if (v.Y < lowerLimit[1]) lowerLimit[1] = (float)v.Y;
                        if (v.Z < lowerLimit[2]) lowerLimit[2] = (float)v.Z;
                    }

                    for (int i = 0; i < planeCount; i++)
                    {
                        Point3d faceCenter = new Point3d((m.Vertices[m.Faces[i].A].X + m.Vertices[m.Faces[i].B].X + m.Vertices[m.Faces[i].C].X) / 3.0,
                            (m.Vertices[m.Faces[i].A].Y + m.Vertices[m.Faces[i].B].Y + m.Vertices[m.Faces[i].C].Y) / 3.0,
                            (m.Vertices[m.Faces[i].A].Z + m.Vertices[m.Faces[i].B].Z + m.Vertices[m.Faces[i].C].Z) / 3.0);
                        Plane p = new Plane(faceCenter, m.FaceNormals[i] *-1);

                        double[] ABCD = p.GetPlaneEquation();

                        cPlanes[i * 4] = (float)ABCD[0];
                        cPlanes[i * 4 + 1] = (float)ABCD[1];
                        cPlanes[i * 4 + 2] = (float)ABCD[2];
                        cPlanes[i * 4 + 3] = (float)ABCD[3];
                    }

                    //add convex mesh
                    geom.AddConvexShape(cPlanes, upperLimit, lowerLimit);
                }
            }

            DA.SetData(0, geom);


        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.collgeom;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{7cd22f46-cf2f-4c25-9d1d-0002553e22a9}"); }
        }
    }
}