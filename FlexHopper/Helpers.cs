using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace FlexHopper
{
    class Fluid
    {
        public float[] Positions { get; private set; }
        public float[] Velocities { get; private set; }
        public float[] InvMasses { get; private set; }
        public int GroupIndex { get; private set; }

        public Fluid(float[] positions, float[] velocities, float[] invMasses, int groupIndex)
        {
            Positions = positions;
            Velocities = velocities;
            InvMasses = invMasses;
            GroupIndex = groupIndex;
        }

        public override string ToString()
        {
            string str = "Fluid System:";
            str += "\nParticle Count: " + Positions.Length / 3;
            str += "\nGroup Index: " + GroupIndex;
            return str + "\n";
        }
    }

    class RigidBody
    {
        public float[] Vertices;
        public float[] Velocity;
        public float[] VertexNormals;
        public float[] InvMasses;
        public float Stiffness;
        public int GroupIndex;
        public float[] MassCenter;
        public Mesh Mesh;

        public RigidBody(float[] vertices, float[] velocity, float[] vertexNormals, float[] invMasses, float stiffness, int groupIndex)
        {
            Vertices = vertices;
            Velocity = velocity;
            VertexNormals = vertexNormals;
            InvMasses = invMasses;
            Stiffness = stiffness;
            GroupIndex = groupIndex;

            MassCenter = new float[3] { 0.0f, 0.0f, 0.0f };
            for(int i = 0; i < vertices.Length / 3; i++)
            {
                MassCenter[0] += vertices[3 * i];
                MassCenter[1] += vertices[3 * i + 1];
                MassCenter[2] += vertices[3 * i + 2];
            }

            MassCenter[0] /= (vertices.Length / 3);
            MassCenter[1] /= (vertices.Length / 3);
            MassCenter[2] /= (vertices.Length / 3);
        }

        public bool HasMesh()
        {
            return Mesh != null && Mesh.IsValid;
        }

        public override string ToString()
        {
            string str = "Rigid Body:";
            str+= "\nVertex Count: " + (Vertices.Length / 3);
            str += "\nTotal Mass: " + (Vertices.Length / 3 * (1.0 / InvMasses[0]));
            str += "\nStiffness: " + Stiffness;
            str += "\nGroup Index: " + GroupIndex;
            return str + "\n";
        }
    }

    class SoftBody
    {
        public float[] Vertices;
        public int[] Triangles;
        public float[] Velocities;
        public float[] InvMasses;
        public float[] SoftParams;
        public int[] AnchorIndices;
        public int GroupIndex;
        public Mesh Mesh;

        public SoftBody(float[] vertices, float[] velocities, float[] invMass, int[] triangles, float[] softParams, int[] anchorIndices, int groupIndex)
        {
            Vertices = vertices;
            Triangles = triangles;
            Velocities = velocities;
            InvMasses = invMass;
            SoftParams = softParams;
            AnchorIndices = anchorIndices;
            GroupIndex = groupIndex;
        }

        public bool HasMesh()
        {
            return Mesh != null && Mesh.IsValid;
        }
    }

    class SpringSystem
    {
        public float[] Positions;
        public float[] Velocities;
        public float[] InvMasses;
        public int[] SpringPairIndices;
        public int SpringOffset;
        public float[] TargetLengths;
        public float[] InitialLengths;
        public float[] Stiffnesses;
        public bool SelfCollision;
        public int[] AnchorIndices;
        public int GroupIndex;
        public Mesh Mesh;
        public bool IsSheetMesh;

        public SpringSystem(float[] positions, float[] velocities, float[] invMasses, int[] springPairIndices, float[] targetLengths, float[] stiffnesses, bool selfCollision, int[] anchorIndices, int groupIndex)
        {
            Positions = positions;
            Velocities = velocities;
            InvMasses = invMasses;
            SpringPairIndices = springPairIndices;
            TargetLengths = targetLengths;
            Stiffnesses = stiffnesses;
            SelfCollision = selfCollision;
            AnchorIndices = anchorIndices;
            GroupIndex = groupIndex;
            SpringOffset = 0;
            IsSheetMesh = false;
        }

        public bool HasMesh()
        {
            return Mesh != null && Mesh.IsValid;
        }

        public override string ToString()
        {
            string str = "Spring System:";
            int numSprings = SpringPairIndices.Length / 2;
            str += "\nNumSprings = " + numSprings;
            str += "\nNumParticles = " + Positions.Length / 3;
            float avSprLength = 0.0f;
            foreach (float d in InitialLengths)
                avSprLength += d / numSprings;
            str += "\nMean Spring Length = " + avSprLength;
            float avTarSprLength = 0.0f;
            foreach (float d in TargetLengths)
                avTarSprLength += d / numSprings;
            str += "\nMean Target Spring Length = " + avTarSprLength;
            float totalMass = 0.0f;
            foreach (float im in InvMasses)
                totalMass += 1.0f / im;
            str += "\nTotal mass = " + totalMass;
            str += "\nSelf Collision = " + SelfCollision.ToString();            
            str += "\nAnchor Indices = ";
            foreach (int a in AnchorIndices)
                str += a + ", ";
            str += "\nIsSheetMesh = " + IsSheetMesh;
            str += "\nGroup Index = " + GroupIndex;
            return str + "\n";
        }
    }

    class Cloth
    {
        public float[] Positions;
        public float[] Velocities;
        public float[] InvMasses;
        public int[] Triangles;
        public float[] TriangleNormals;
        public float StretchStiffness;
        public float BendingStiffness;
        public float PreTensionFactor;
        public int[] AnchorIndices;
        public int GroupIndex;
        public int SpringOffset;
        public Mesh Mesh;

        public Cloth(float[] positions, float[] velocities, float[] invMasses, int[] triangles, float[] triangleNormals, float stretchStiffness, float bendingStiffness, float preTensionFactor, int[] anchorIndices, int groupIndex)
        {
            Positions = positions;
            Velocities = velocities;
            InvMasses = invMasses;
            Triangles = triangles;
            TriangleNormals = triangleNormals;
            StretchStiffness = stretchStiffness;
            BendingStiffness = bendingStiffness;
            PreTensionFactor = preTensionFactor;
            AnchorIndices = anchorIndices;
            GroupIndex = groupIndex;
            SpringOffset = 0;
        }

        public override string ToString()
        {
            string str = "Cloth:";
            int numSprings = Mesh.TopologyEdges.Count;
            str += "\nNumSprings = " + numSprings;
            str += "\nNumParticles = " + Positions.Length / 3;
            float avSpringLength = 0.0f;
            for (int i = 0; i < numSprings; i++)
                avSpringLength += (float)Mesh.TopologyEdges.EdgeLine(i).Length / numSprings;
            str += "\nMean Spring Length = " + avSpringLength;
            float totalMass = 0.0f;
            foreach (float im in InvMasses)
                totalMass += 1.0f / im;
            str += "\nTotal mass = " + totalMass;
            str += "\nAnchor Indices = ";
            foreach (int a in AnchorIndices)
                str += a + ", ";
            str += "\nGroup Index = " + GroupIndex;
            return str;
        }
    }

    class Inflatable
    {
        public float[] Positions;
        public float[] Velocities;
        public float[] InvMasses;
        public int[] Triangles;
        public float[] TriangleNormals;
        public float StretchStiffness;
        public float BendingStiffness;
        public float PreTensionFactor;
        public int[] AnchorIndices;
        public int GroupIndex;
        public float RestVolume;
        public float OverPressure;
        public float ConstraintScale;
        public int SpringOffset;
        public Mesh Mesh;
        private float MeshVolume;
        

        public Inflatable(float[] positions, float[] velocities, float[] invMasses, int[] triangles, float[] triangleNormals, float stretchStiffness, float bendingStiffness, float preTensionFactor, float restVolume, float overPressure, float constraintScale, int[] anchorIndices, int groupIndex)
        {
            Positions = positions;
            Velocities = velocities;
            InvMasses = invMasses;
            Triangles = triangles;
            TriangleNormals = triangleNormals;
            StretchStiffness = stretchStiffness;
            BendingStiffness = bendingStiffness;
            PreTensionFactor = preTensionFactor;
            RestVolume = restVolume;
            OverPressure = overPressure;
            ConstraintScale = constraintScale;
            AnchorIndices = anchorIndices;
            GroupIndex = groupIndex;
            SpringOffset = 0;
        }

        public bool HasMesh()
        {
            return Mesh != null && Mesh.IsValid && Mesh.IsClosed;
        }

        public float GetMeshVolume()
        {
            if (HasMesh())
            {
                if (float.IsNaN(MeshVolume) || MeshVolume == 0.0f)
                {
                    VolumeMassProperties properties = VolumeMassProperties.Compute(Mesh);
                    MeshVolume = (float)properties.Volume;

                }
                return MeshVolume;
            }
            else
                throw new Exception("Inflatable: Invalid mesh!");
        }

        public override string ToString()
        {
            string str = "Inflatable:";
            str += "\nNumParticles = " + Positions.Length / 3;
            int numSprings = Mesh.TopologyEdges.Count;
            str += "\nNumSprings = " + numSprings;            
            float avSpringLength = 0.0f;
            for (int i = 0; i < numSprings; i++)
                avSpringLength += (float)Mesh.TopologyEdges.EdgeLine(i).Length / numSprings;
            str += "\nMean Spring Length = " + avSpringLength;
            if(Mesh != null && Mesh.IsValid && Mesh.IsClosed)
            {
                str += "\nMeshVolume = " + GetMeshVolume();
            }
            str += "\nRestVolume = " + RestVolume;
            float totalMass = 0.0f;
            foreach (float im in InvMasses)
                totalMass += 1.0f / im;
            str += "\nTotal mass = " + totalMass;
            str += "\nAnchor Indices = ";
            foreach (int a in AnchorIndices)
                str += a + ", ";
            str += "\nGroup Index = " + GroupIndex;
            return str;
        }
    }

    class ConstraintSystem
    {
        public int[] AnchorIndices;
        public int[] SpringPairIndices;
        public float[] SpringTargetLengths;
        public float[] SpringStiffnesses;
        public int[] TriangleIndices;
        public float[] TriangleNormals;
        public int[] ShapeMatchingIndices;
        public float ShapeStiffness;

        public ConstraintSystem()
        {
            AnchorIndices = new int[0];
            ShapeMatchingIndices = new int[0];
            ShapeStiffness = 0.0f;
            SpringPairIndices = new int[0];
            SpringTargetLengths = new float[0];
            SpringStiffnesses = new float[0];
            TriangleIndices = new int[0];
            TriangleNormals = new float[0];
        }

        public ConstraintSystem(int[] anchorIndices)
        {
            AnchorIndices = anchorIndices;
            ShapeMatchingIndices = new int[0];
            ShapeStiffness = 0.0f;
            SpringPairIndices = new int[0];
            SpringTargetLengths = new float[0];
            SpringStiffnesses = new float[0];
            TriangleIndices = new int[0];
            TriangleNormals = new float[0];
        }

        public ConstraintSystem(int[] shapeMatchingIndices, float shapeStiffness)
        {
            AnchorIndices = new int[0];
            ShapeMatchingIndices = shapeMatchingIndices;
            ShapeStiffness = shapeStiffness;
            SpringPairIndices = new int[0];
            SpringTargetLengths = new float[0];
            SpringStiffnesses = new float[0];
            TriangleIndices = new int[0];
            TriangleNormals = new float[0];
            ShapeMatchingIndices = shapeMatchingIndices;
            ShapeStiffness = shapeStiffness;
        }

        public ConstraintSystem(int[] springPairIndices, float[] springTargetLengths, float[] springStiffnesses)
        {
            if (springPairIndices.Length / 2 != springTargetLengths.Length || springPairIndices.Length / 2 != springStiffnesses.Length)
                throw new Exception("Spring constraint input is invalid!");
            AnchorIndices = new int[0];
            ShapeMatchingIndices = new int[0];
            ShapeStiffness = 0.0f;
            SpringPairIndices = springPairIndices;
            SpringTargetLengths = springTargetLengths;
            SpringStiffnesses = springStiffnesses;
            TriangleIndices = new int[0];
            TriangleNormals = new float[0];
        }

        public ConstraintSystem(int[] triangleIndices, float[] triangleNormals = null)
        {
            AnchorIndices = new int[0];
            ShapeMatchingIndices = new int[0];
            ShapeStiffness = 0.0f;
            SpringPairIndices = new int[0];
            SpringTargetLengths = new float[0];
            SpringStiffnesses = new float[0];
            TriangleIndices = triangleIndices;
            if (triangleNormals == null || triangleNormals.Length != triangleIndices.Length)
            {
                triangleNormals = new float[triangleIndices.Length];
                for (int i = 0; i < triangleNormals.Length; i++)
                    triangleNormals[i] = 0.0f;
            }
            TriangleNormals = triangleNormals;
        }

        public override string ToString()
        {
            string str = "Constraint System";
            str += "\nAnchorIndices = {";
            foreach (int i in AnchorIndices)
                str += " " + i + ",";
            str += "}";
            str += "\nShapeMatching = {";
            foreach (int i in ShapeMatchingIndices)
                str += " " + i + ",";
            str += "}";
            str += "\nSpringPairs = {";
            for(int i = 0; i < SpringPairIndices.Length / 2; i++)
                str += " " + SpringPairIndices[2 * i] + " - " + SpringPairIndices[2 * i + 1] + ",";
            str += "}";
            str += "\nTriangles = {";
            for (int i = 0; i < TriangleIndices.Length / 3; i++)
                str += " " + TriangleIndices[3 * i] + " - " + TriangleIndices[3 * i + 1] + " - " + TriangleIndices[3 * i + 2] + ",";
            str += "}";

            return str;
        }
    }

    class Util
    {
        public static float SquareDistance(Point3d pointA, Point3d pointB)
        {
            float[] pA = new float[3] { (float)pointA.X, (float)pointA.Y, (float)pointA.Z };
            float[] pB = new float[3] { (float)pointB.X, (float)pointB.Y, (float)pointB.Z };
            return (pA[0] - pB[0]) * (pA[0] - pB[0]) + (pA[1] - pB[1]) * (pA[1] - pB[1]) + (pA[2] - pB[2]) * (pA[2] - pB[2]);
        }

        public static int[] AnchorIndicesFromIGH_Goo(List<IGH_Goo> anchorGoo, Point3d[] vertices, float anchorThreshold)
        {
            List<int> anchorIndices = new List<int>();

            foreach (IGH_Goo ao in anchorGoo)
            {
                string aS = "";
                int aI = -1;
                Point3d aP;
                if (ao.CastTo<Point3d>(out aP))
                {
                    for (int j = 0; j < vertices.Length; j++)
                    {
                        if (Util.SquareDistance(vertices[j], aP) < anchorThreshold * anchorThreshold)
                        {
                            anchorIndices.Add(j);
                            break;
                        }
                    }
                }
                else if (ao.CastTo<int>(out aI))
                    anchorIndices.Add(aI);
                else if (ao.CastTo<string>(out aS))
                    anchorIndices.Add(int.Parse(aS));
            }

            return anchorIndices.ToArray();
        }
    }
}