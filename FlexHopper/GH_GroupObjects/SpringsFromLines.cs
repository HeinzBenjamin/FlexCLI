using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

using Rhino;
using Rhino.Geometry;

using FlexHopper.Properties;

namespace FlexHopper.GH_GroupObjects
{
    public class SpringsFromLines : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SpringsFromLines class.
        /// </summary>
        public SpringsFromLines()
          : base("Spring System From Lines / Meshes", "Springs",
              "",
              "Flex", "Composition")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Springs", "Springs", "Connect lines, polylines, curves or meshes to make a spring systems from. Each branch should either contain a single mesh or a set of lines / polylines / curves.", GH_ParamAccess.tree);
            pManager.AddVectorParameter("Velocities", "Vel", "Initial velocities per particle. If one value is supplied it's applied to all particles equally. Velocities per particle are only supported for mesh inputs.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Masses", "Mass", "Masses per particle. If one value is supplied it's applied to all particles equally. masses per particle are only supported for mesh inputs.", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Target Spring Length", "Length", "Leave empty, if current line length should be the target length. If you enter a negative value, its absolute value will interpreted as a length factor.", GH_ParamAccess.tree);            
            pManager.AddNumberParameter("Spring Stiffness", "Stiff", "Between 0.0 and 1.0", GH_ParamAccess.tree);
            pManager.AddNumberParameter("Additional Sheet Stiffening", "Sheet", "Optionally supply a stiffness factor value that is applied to the +1-neighborhood of each vertex. This is currently only supported for mesh inputs.", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Self Collision", "SelfColl", "Determine whether particles of one spring group collide with themselves", GH_ParamAccess.list, true);            
            pManager.AddGenericParameter("Anchors", "Anchors", "Index numbers or (x,y,z)-points.", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Group Index", "GInd", "Index to identify this fluid group later on. Make sure no index is more than once in your entire flex simulation.", GH_ParamAccess.list);
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Spring Groups", "Springs", "Connect to FlexScene", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double anchorThreshold = 0.01;
            double pointDuplicateThreshold = 0.01;
            bool isSheetMesh = false;
            

            GH_Structure<IGH_Goo> springTree = new GH_Structure<IGH_Goo>();
            GH_Structure<GH_Vector> velTree = new GH_Structure<GH_Vector>();
            GH_Structure<GH_Number> massTree = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> lengthTree = new GH_Structure<GH_Number>();
            GH_Structure<GH_Number> stiffnessTree = new GH_Structure<GH_Number>();
            List<double> sheetStiffeningList = new List<double>();
            GH_Structure<IGH_Goo> anchorTree = new GH_Structure<IGH_Goo>();
            List<bool> selfCollisionList = new List<bool>();
            List<int> groupIndexList = new List<int>();

            List<SpringSystem> springSystems = new List<SpringSystem>();

            DA.GetDataTree(0, out springTree);
            DA.GetDataTree(1, out velTree);
            DA.GetDataTree(2, out massTree);
            DA.GetDataTree(3, out lengthTree);
            DA.GetDataTree(4, out stiffnessTree);
            DA.GetDataList(5, sheetStiffeningList);
            DA.GetDataList(6, selfCollisionList);
            DA.GetDataTree(7, out anchorTree);
            DA.GetDataList(8, groupIndexList);

            #region simplify trees and if(branch.Count == 1) make sure everything sits in path {0}
            if (!springTree.IsEmpty) springTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (!velTree.IsEmpty) velTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (!massTree.IsEmpty) massTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (!lengthTree.IsEmpty) lengthTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (!stiffnessTree.IsEmpty) stiffnessTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);
            if (!anchorTree.IsEmpty) anchorTree.Simplify(GH_SimplificationMode.CollapseAllOverlaps);

            if (springTree.Branches.Count != groupIndexList.Count || springTree.Branches.Count != selfCollisionList.Count)
                throw new Exception("Line tree doesn't fit either groupIndices count or selfCollision count!");

            if (springTree.Branches.Count == 1)
            {
                GH_Structure<IGH_Goo> lT = new GH_Structure<IGH_Goo>();
                lT.AppendRange(springTree.Branches[0], new GH_Path(0));
                springTree = lT;
            }
            if (velTree.Branches.Count == 1)
            {
                GH_Structure<GH_Vector> vT = new GH_Structure<GH_Vector>();
                vT.AppendRange(velTree.Branches[0], new GH_Path(0));
                velTree = vT;
            }
            if (massTree.Branches.Count == 1)
            {
                GH_Structure<GH_Number> mT = new GH_Structure<GH_Number>();
                mT.AppendRange(massTree.Branches[0], new GH_Path(0));
                massTree = mT;
            }
            if (lengthTree.Branches.Count == 1)
            {
                GH_Structure<GH_Number> leT = new GH_Structure<GH_Number>();
                leT.AppendRange(lengthTree.Branches[0], new GH_Path(0));
                lengthTree = leT;
            }
            if (stiffnessTree.Branches.Count == 1)
            {
                GH_Structure<GH_Number> sT = new GH_Structure<GH_Number>();
                sT.AppendRange(stiffnessTree.Branches[0], new GH_Path(0));
                stiffnessTree = sT;
            }
            if (anchorTree.Branches.Count == 1)
            {
                GH_Structure<IGH_Goo> aT = new GH_Structure<IGH_Goo>();
                aT.AppendRange(anchorTree.Branches[0], new GH_Path(0));
                anchorTree = aT;
            }
            #endregion

            for (int branchIndex = 0; branchIndex < springTree.Branches.Count; branchIndex++)
            {
                List<float> positions = new List<float>();
                List<float> velocities = new List<float>();
                List<float> invMasses = new List<float>();
                List<int> springPairIndices = new List<int>();
                List<float> targetLengths = new List<float>();
                List<float> stiffnesses = new List<float>();
                List<int> anchorIndices = new List<int>();
                List<float> initialLengths = new List<float>(); //just for info

                GH_Path path = new GH_Path(branchIndex);

                List<Line> lines = new List<Line>();
                Curve c;
                Line l;
                Mesh mesh = new Mesh();
                foreach (IGH_Goo springObject in springTree.get_Branch(path))
                {
                    if (springObject.CastTo<Mesh>(out mesh))
                        break;
                    else if (springObject.CastTo<Curve>(out c))
                        if (c.IsPolyline()) {
                            Polyline pl;
                            c.TryGetPolyline(out pl);
                            for (int i = 0; i < pl.SegmentCount; i++)
                                lines.Add(pl.SegmentAt(i));
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Remark, "Polyline in branch " + branchIndex + " was split into its segments!");
                        }
                    else
                        lines.Add(new Line(c.PointAtStart, c.PointAtEnd));
                    else if (springObject.CastTo<Line>(out l))
                        lines.Add(l);
                }

                #region isMesh
                if (mesh != null && mesh.IsValid)
                {
                    mesh.Vertices.CombineIdentical(true, true);
                    mesh.Weld(Math.PI);
                    mesh.UnifyNormals();

                    Rhino.Geometry.Collections.MeshTopologyVertexList mv = mesh.TopologyVertices;
                    Rhino.Geometry.Collections.MeshTopologyEdgeList me = mesh.TopologyEdges;
                    
                    //Add everything related to particles
                    for(int i = 0; i < mv.Count; i++)
                    {
                        //add position
                        positions.Add(mv[i].X);
                        positions.Add(mv[i].Y);
                        positions.Add(mv[i].Z);

                        //add velocity
                        Vector3d vel = new Vector3d(0.0, 0.0, 0.0);
                        if (velTree.PathExists(path))
                        {
                            if (velTree.get_Branch(path).Count > i)
                                vel = velTree.get_DataItem(path, i).Value;
                            else
                                vel = velTree.get_DataItem(path, 0).Value;
                        }
                        velocities.Add((float)vel.X);
                        velocities.Add((float)vel.Y);
                        velocities.Add((float)vel.Z);

                        //add inverse mass
                        float invMass = 1.0f;
                        if (massTree.PathExists(path))
                        {
                            if (massTree.get_Branch(path).Count > i)
                                invMass = 1.0f / (float)massTree.get_DataItem(path, i).Value;
                            else
                                invMass = 1.0f / (float)massTree.get_DataItem(path, 0).Value;
                        }
                        invMasses.Add(invMass);
                    }

                    //Add everything related to spring lines
                    for(int i = 0; i < me.Count; i++)
                    {
                        springPairIndices.Add(me.GetTopologyVertices(i).I);
                        springPairIndices.Add(me.GetTopologyVertices(i).J);                        

                        //add length
                        float length = (float)me.EdgeLine(i).Length;
                        initialLengths.Add(length);
                        if (lengthTree.PathExists(path))
                        {
                            float temp = 0.0f;
                            if (lengthTree.get_Branch(path).Count > i)
                                temp = (float)lengthTree.get_DataItem(path, i).Value;
                            else
                                temp = (float)lengthTree.get_DataItem(path, 0).Value;

                            if (temp < 0.0)
                                length *= -temp;
                            else
                                length = temp;
                        }
                        targetLengths.Add(length);

                        //add stiffness
                        float stiffness = 1.0f;
                        if (stiffnessTree.PathExists(path))
                        {
                            if (stiffnessTree.get_Branch(path).Count > i)
                                stiffness = (float)stiffnessTree.get_DataItem(path, i).Value;
                            else
                                stiffness = (float)stiffnessTree.get_DataItem(path, 0).Value;
                        }
                        stiffnesses.Add(stiffness);


                        List<Line> f = new List<Line>();
                        if (sheetStiffeningList.Count > branchIndex && sheetStiffeningList[branchIndex] > 0.0)
                        {
                            isSheetMesh = true;
                            int[] adjFaceInd = me.GetConnectedFaces(i);
                            if (adjFaceInd.Length == 2)
                            {
                                f.Add(me.EdgeLine(i));
                                MeshFace faceA = mesh.Faces[adjFaceInd[0]];
                                MeshFace faceB = mesh.Faces[adjFaceInd[1]];
                                if (faceA.IsTriangle && faceB.IsTriangle)
                                {
                                    List<int> allInds = new List<int> { faceA.A, faceA.B, faceA.C, faceB.A, faceB.B, faceB.C };
                                    int[] uniques = new int[6] { 0, 0, 0, 0, 0, 0 };
                                    for (int h = 0; h < 6; h++)
                                        for (int g = 0; g < 6; g++)
                                            if (allInds[h] == allInds[g]) uniques[h]++;
                                    for (int h = 0; h < 6; h++)
                                        if (uniques[h] == 1)
                                        {
                                            springPairIndices.Add(mv.TopologyVertexIndex(allInds[h]));
                                            stiffnesses.Add((float)(stiffness * sheetStiffeningList[branchIndex]));
                                        }
                                    float le = (float)mv[springPairIndices[springPairIndices.Count - 2]].DistanceTo(mv[springPairIndices[springPairIndices.Count - 1]]);
                                    targetLengths.Add(le);
                                    initialLengths.Add(le);
                                    f.Add(new Line(mesh.Vertices[mv.TopologyVertexIndex(springPairIndices[springPairIndices.Count - 1])], mesh.Vertices[mv.TopologyVertexIndex(springPairIndices[springPairIndices.Count - 2])]));
                                }

                            }
                        }
                    }
                }
                #endregion

                #region isLines
                else if (lines.Count != 0)
                {
                    List<Line> cleanLineList = new List<Line>();
                    double ptDuplThrSquared = pointDuplicateThreshold * pointDuplicateThreshold;

                    #region clean up line list
                    List<Line> lHist = new List<Line>();
                    for (int j = 0; j < lines.Count; j++)
                    {
                        //Clean list from duplicate lines
                        Line lCand = lines[j];
                        Point3d ptCandA = lCand.From;
                        Point3d ptCandB = lCand.To;
                        bool lineExistsAlready = false;
                        foreach (Line lh in lHist)
                        {
                            Line tempL = new Line(lCand.From, lCand.To);
                            if ((Util.SquareDistance(tempL.From, lh.From) < ptDuplThrSquared && Util.SquareDistance(tempL.To, lh.To) < ptDuplThrSquared) ||
                                (Util.SquareDistance(tempL.From, lh.To) < ptDuplThrSquared && Util.SquareDistance(tempL.To, lh.From) < ptDuplThrSquared))
                                lineExistsAlready = true;
                        }

                        //Clean list from too short lines
                        if (!(Util.SquareDistance(ptCandA, ptCandB) < ptDuplThrSquared || lineExistsAlready))
                        {
                            lHist.Add(lCand);
                            cleanLineList.Add(lCand);
                        }
                        else
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Spring nr. " + j + " in branch " + branchIndex + " is either invalid (too short) or appeared for the second time. It is ignored.");
                    }
                    #endregion


                    //get velocity and mass for this branch (no mass / velo per particle allowed)
                    List<float> branchDefaultVelocity = new List<float>() { 0.0f, 0.0f, 0.0f };
                    if (velTree.PathExists(path))
                        branchDefaultVelocity = new List<float> { (float)velTree.get_DataItem(path, 0).Value.X, (float)velTree.get_DataItem(path, 0).Value.Y, (float)velTree.get_DataItem(path, 0).Value.Z };

                    float branchDefaultInvMass = 1.0f;
                    if (massTree.PathExists(path))
                        branchDefaultInvMass = 1.0f / (float)massTree.get_DataItem(path, 0).Value;

                    //find unique line start indices
                    List<int> springStartIndices = new List<int>();
                    int advance = 0;
                    for (int item = 0; item < cleanLineList.Count; item++)
                    {
                        Point3d ptCand = cleanLineList[item].From;
                        int alreadyExistsAs = -1;
                        for (int k = 0; k < positions.Count / 3; k++)
                        {

                            //simple squared distance
                            if (Util.SquareDistance(new Point3d(positions[k * 3], positions[k * 3 + 1], positions[k * 3 + 2]), ptCand) < ptDuplThrSquared)
                            {
                                alreadyExistsAs = k;
                                springStartIndices.Add(alreadyExistsAs);
                                break;
                            }
                        }
                        if (alreadyExistsAs == -1)
                        {
                            positions.Add((float)ptCand.X);
                            positions.Add((float)ptCand.Y);
                            positions.Add((float)ptCand.Z);

                            velocities.AddRange(branchDefaultVelocity);
                            invMasses.Add(branchDefaultInvMass);

                            springStartIndices.Add(advance);
                            advance++;
                        }
                    }

                    //find unique line end indices
                    List<int> springEndIndices = new List<int>();
                    for (int item = 0; item < cleanLineList.Count; item++)
                    {
                        Point3d ptCand = cleanLineList[item].To;
                        int alreadyExistsAs = -1;
                        for (int k = 0; k < positions.Count / 3; k++)
                        {
                            if (Util.SquareDistance(new Point3d(positions[3 * k], positions[3 * k + 1], positions[3 * k + 2]), ptCand) < ptDuplThrSquared)
                            {
                                alreadyExistsAs = k;
                                springEndIndices.Add(alreadyExistsAs);
                                break;
                            }
                        }
                        if (alreadyExistsAs == -1)
                        {
                            positions.Add((float)ptCand.X);
                            positions.Add((float)ptCand.Y);
                            positions.Add((float)ptCand.Z);

                            velocities.AddRange(branchDefaultVelocity);
                            invMasses.Add(branchDefaultInvMass);

                            springEndIndices.Add(advance);
                            advance++;
                        }
                    }

                    //weave spring start indices and spring end indices together                 
                    for (int w = 0; w < springStartIndices.Count; w++)
                    {
                        springPairIndices.Add(springStartIndices[w]);
                        springPairIndices.Add(springEndIndices[w]);
                    }

                    //Add everything spring line related...
                    for(int i = 0; i < cleanLineList.Count; i++)
                    {
                        //add length
                        float length = (float)cleanLineList[i].Length;
                        initialLengths.Add(length);
                        if (lengthTree.PathExists(path))
                        {
                            float temp = 0.0f;
                            if (lengthTree.get_Branch(path).Count > i)
                                temp = (float)lengthTree.get_DataItem(path, i).Value;
                            else
                                temp = (float)lengthTree.get_DataItem(path, 0).Value;

                            if (temp < 0.0)
                                length *= -temp;
                            else
                                length = temp;
                        }
                        targetLengths.Add(length);

                        //add stiffness
                        float stiffness = 1.0f;
                        if (stiffnessTree.PathExists(path))
                        {
                            if (stiffnessTree.get_Branch(path).Count > i)
                                stiffness = (float)stiffnessTree.get_DataItem(path, i).Value;
                            else
                                stiffness = (float)stiffnessTree.get_DataItem(path, 0).Value;
                        }
                        stiffnesses.Add(stiffness);
                    }
                }
                #endregion
                else
                    throw new Exception("No valid spring input found in branch " + branchIndex);


                //Add anchors
                if (anchorTree.PathExists(path))
                    foreach (IGH_Goo anchorObj in anchorTree.get_Branch(path))
                    {
                        string ass = "";
                        int ai = 0;
                        Point3d ap = new Point3d(0.0, 0.0, 0.0);
                        if (anchorObj.CastTo<string>(out ass))
                            anchorIndices.Add(int.Parse(ass));
                        else if (anchorObj.CastTo<int>(out ai))
                            anchorIndices.Add(ai);
                        else if (anchorObj.CastTo<Point3d>(out ap))
                            for (int i = 0; i < positions.Count / 3; i++)
                            {
                                if ((anchorThreshold * anchorThreshold) > Math.Pow((positions[3 * i] - ap.X), 2) + Math.Pow((positions[3 * i + 1] - ap.Y), 2) + Math.Pow((positions[3 * i + 2] - ap.Z), 2))
                                    anchorIndices.Add(i);
                            }
                    }

                SpringSystem ss = new SpringSystem(positions.ToArray(), velocities.ToArray(),invMasses.ToArray(),springPairIndices.ToArray(),targetLengths.ToArray(),stiffnesses.ToArray(),selfCollisionList[branchIndex],anchorIndices.ToArray(),groupIndexList[branchIndex]);
                ss.Mesh = mesh;
                ss.IsSheetMesh = isSheetMesh;
                ss.InitialLengths = initialLengths.ToArray();
                springSystems.Add(ss);
            }
            DA.SetDataList(0, springSystems);


        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Resources.springs;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("{661c3d1c-5713-44fe-913a-2d561e4ccf94}"); }
        }

        
    }
}