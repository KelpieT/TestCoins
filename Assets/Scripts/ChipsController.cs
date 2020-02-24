using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ChipsController
{

    public struct MyVertex
    {
        public Vector3 position;
        public Vector2 uv;
        public Vector3 normal;
        public MyVertex(Vector3 position)
        {
            this.position = position;
            uv = Vector2.zero;
            normal = Vector3.forward;
        }
    }
    public struct MyChip
    {
        // public List<MyVertex> bottom;
        public List<MyVertex> edge;
        public List<MyVertex> face;
        public List<int> trianglesEdge;
        public List<int> trianglesFace;
        public float rotation;
        public Color color;
        private void RotateListMyVertex(ref List<MyVertex> listMyVertex)
        {
            if (listMyVertex != null)
            {
                for (int i = 0; i < listMyVertex.Count; i++)
                {
                    MyVertex mv = listMyVertex[i];
                    mv.position = Quaternion.Euler(0, rotation, 0) * listMyVertex[i].position;
                    listMyVertex[i] = mv;
                }
            }
        }
        public void RotateAll()
        {
            // RotateListMyVertex(ref bottom);
            RotateListMyVertex(ref edge);
            RotateListMyVertex(ref face);

        }

        public MyChip( List<MyVertex> top, List<MyVertex> topFace, float rotation, Color color, List<int> trianglesEdge, List<int> trianglesFace)
        {
            // this.bottom = bottom;
            this.face = topFace;
            this.edge = top;
            this.rotation = rotation;
            this.color = color;
            this.trianglesFace = trianglesFace;
            this.trianglesEdge = trianglesEdge;
            RotateAll();
        }


    }
    public class ChipsController : MonoBehaviour
    {
        //Resourses
        //---------------------------------------------
        private Mesh chip;
        private string pathToMeshChip = "Models/Chip";
        private Material material;
        private string pathToMeshMaterial = "Models/ChipMaterial";
        //---------------------------------------------

        //Mesh
        //---------------------------------------------
        private Mesh stackChips;
        private int countChipsInStack;
        private int CountChipsInStack { get => countChipsInStack; set => countChipsInStack = value >= 0 ? value : 0; }
        private string nameMesh = "StackMesh";
        //---------------------------------------------

        //ComponentsMesh
        //---------------------------------------------
        private List<MyVertex> MyVertices = new List<MyVertex>();
        private List<MyVertex> MyVerticesFace = new List<MyVertex>();
        private List<MyVertex> MyVerticesEdge = new List<MyVertex>();



        //---------------------------------------------

        //Temp variables
        //---------------------------------------------
        public float MinRotation;
        public float MaxRotation;
        public Color[] colors;
        //---------------------------------------------
        List<int> trianglesEdge1 = new List<int>();
        List<int> trianglesFace1 = new List<int>();

        private void Start()
        {
            LoadAllResources();
            // GenerateStack(0);
            WorkWithMyVertex();
            newgenerateStack(1);
        }
        private void LoadAllResources()
        {
            chip = Resources.Load<Mesh>(pathToMeshChip);
            material = Resources.Load<Material>(pathToMeshMaterial);
        }
        private void GenerateStack(int countChips)
        {
            stackChips = new Mesh();
            CountChipsInStack = countChips;

            // List<Vector3> edgeLoop = GetEdgeLoopChipVertex();
            List<MyVertex> totalVertex = new List<MyVertex>();
            List<int> tris = new List<int>();
            // for (int i = 0; i < countChips + 1; i++)
            // {
            //     // foreach (MyVertex mv in MyVerticesBotomEdge)
            //     // {
            //     //     totalVertex.Add(new MyVertex(mv.position + Vector3.forward * 0.3f * i));
            //     // }
            //     if (i > 0)
            //     {
            //         // tris.AddRange(CreateTrianglesInEdge(0, (edgeLoop.Count) * (i - 1), edgeLoop.Count));
            //     }
            //     if (i == countChips)
            //     {
            //         //CreateFaceTris
            //         List<int> notCon = new List<int>();
            //         for (int j = 0; j < MyVerticesFace.Count; j++)
            //         {
            //             notCon.Add(j + MyVerticesFace.Count * (i + 1));
            //         }
            //         notCon.Add(notCon[0]);//circle surface
            //         tris.AddRange(CreateTrianglesInFace(notCon));
            //     }
            // }

            stackChips.SetVertices(GetPositionFromMyVertices(totalVertex));
            // DebugMesh(stackChips);
            // debugUv();
            WorkWithMyVertex();
            stackChips.triangles = tris.ToArray();
            CreateObject(stackChips);

        }

        // private List<Vector3> GetEdgeLoopChipVertex()
        // {
        //     List<Vector3> v3 = new List<Vector3>();
        //     List<Vector3> temp = new List<Vector3>();
        //     chip.GetVertices(temp);
        //     float zBig = float.MinValue;
        //     float zOffset = 0.01f;
        //     foreach (Vector3 v in temp)
        //     {
        //         if (zBig < v.z) { zBig = v.z; }
        //     }
        //     for (int i = 0; i < chip.vertices.Length; i++)
        //     {

        //         Vector3 v = chip.vertices[i];
        //         if (v.z > zBig - zOffset && v.z < zBig + zOffset)
        //         {

        //             v3.Add(v);
        //         }
        //     }
        //     v3 = DeleteDuplicateVerteces(v3);
        //     //SortVerteces(ref v3);
        //     return v3;

        // }
        private void CreateObject(Mesh stackMesh)
        {
            GameObject stack = new GameObject(nameMesh);
            MeshFilter meshFilter = stack.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = stack.AddComponent<MeshRenderer>();

            stackMesh.name = nameMesh;
            meshFilter.mesh = stackMesh;
            meshRenderer.material = material;
        }
        private void SplitStack()
        {

        }
        private void DebugMesh(Mesh mesh)
        {
            for (int i = 0; i < mesh.vertices.Length - 1; i++)
            {
                Vector3 s = mesh.vertices[i];
                Vector3 e = mesh.vertices[i + 1];
                Debug.DrawLine(s, e, new Color(1, 1, 1, 1), 60f);

            }
            foreach (Vector3 v in mesh.normals)
            {
                Debug.Log(v.ToString());
            }
            
        }
        private List<MyVertex> DeleteDuplicateVerteces(List<MyVertex> currentList)
        {
            List<MyVertex> tempList = new List<MyVertex>();
            for (int i = 0; i < currentList.Count; i++)
            {
                MyVertex currentVector = currentList[i];
                bool foundInList = false;
                foreach (MyVertex v in tempList)
                {
                    if (v.position == currentVector.position)
                    {
                        foundInList = true;
                        break;
                    }
                }
                if (!foundInList)
                {
                    tempList.Add(currentVector);
                }
            }
            return tempList;
        }
        private void SortVerteces(ref List<MyVertex> currentList)
        {
            List<MyVertex> tempList = new List<MyVertex>();
            foreach (MyVertex mv in currentList)
            {
                tempList.Add(mv);
            }
            currentList = new List<MyVertex>();
            currentList = tempList.OrderBy(v => Vector3.SignedAngle(v.position, Vector3.up, Vector3.forward)).ToList();
        }

        private List<int> CreateTrianglesInEdge(int firstVert, int startIndex, int countVertexEdge)
        {
            List<int> currentTwotriangles = new List<int>();
            if (firstVert < (countVertexEdge - 1))
            {
                int p1 = firstVert;
                int p2 = countVertexEdge + firstVert;
                int p3 = countVertexEdge + firstVert + 1;
                int p4 = firstVert + 1;
                // if (firstVert == countVertexEdge - 2)
                // {
                //     //connect edge
                //     p3 = countVertexEdge;
                //     p4 = 0;
                // }
                currentTwotriangles.Add(p1 + startIndex);
                currentTwotriangles.Add(p2 + startIndex);
                currentTwotriangles.Add(p3 + startIndex);
                currentTwotriangles.Add(p4 + startIndex);
                currentTwotriangles.Add(p1 + startIndex);
                currentTwotriangles.Add(p3 + startIndex);
                firstVert += 1;
                currentTwotriangles.AddRange(CreateTrianglesInEdge(firstVert, startIndex, countVertexEdge));
            }
            return currentTwotriangles;
        }
        private List<int> CreateTrianglesInFace(List<int> notCon)
        {
            List<int> currentTriangle = new List<int>();
            List<int> temp = notCon;
            for (int i = 0; i < notCon.Count - 2; i++)
            {

                currentTriangle.Add(notCon[i + 1]);
                currentTriangle.Add(notCon[i]);
                currentTriangle.Add(notCon[i + 2]);
                temp.Remove(notCon[i + 1]);

            }
            if (notCon.Count > 3)
            {
                currentTriangle.AddRange(CreateTrianglesInFace(temp));
            }
            return currentTriangle;
        }

        private List<Vector3> GetPositionFromMyVertices(List<MyVertex> listMyVertex)
        {
            List<Vector3> position = new List<Vector3>();
            foreach (MyVertex mv in listMyVertex)
            {
                position.Add(mv.position);
            }
            return position;
        }
        private List<Vector2> GetUvFromMyVertices(List<MyVertex> listMyVertex)
        {
            List<Vector2> uv = new List<Vector2>();
            foreach (MyVertex mv in listMyVertex)
            {
                uv.Add(mv.uv);
            }
            return uv;
        }
        private List<Vector3> GetNormalFromMyVertices(List<MyVertex> listMyVertex)
        {
            List<Vector3> normal = new List<Vector3>();
            foreach (MyVertex mv in listMyVertex)
            {
                normal.Add(mv.normal);
            }
            return normal;
        }

        private void WorkWithMyVertex()
        {
            List<Vector3> chipPos = new List<Vector3>();
            chip.GetVertices(chipPos);

            List<Vector2> chipUV = new List<Vector2>();
            chip.GetUVs(0, chipUV);

            List<Vector3> chipNormals = new List<Vector3>();
            chip.GetNormals(chipNormals);

            for (int i = 0; i < chipPos.Count; i++)
            {
                MyVertex myVertex = new MyVertex(chipPos[i]);
                myVertex.uv = chipUV[i];
                myVertex.normal = chipNormals[i];
                MyVertices.Add(myVertex);
            }

            sortMyVerteces();
        }
        private void sortMyVerteces()
        {
           
            float zOffset = 0.01f;
            
            for (int i = 0; i < MyVertices.Count;i++)
            {
                
                    if (MyVertices[i].normal.z > zOffset)
                    {
                        MyVerticesFace.Add(MyVertices[i]);
                    }
                    else if(MyVertices[i].normal.z > - zOffset)
                    {
                        MyVerticesEdge.Add(MyVertices[i]);
                    }
               
                    
                
            }
            SortVerteces(ref MyVerticesFace);
            SortVerteces(ref MyVerticesEdge);
            // MyVerticesEdge = DeleteDuplicateVerteces(MyVerticesEdge);
            // SortVerteces(ref MyVerticesBotomEdge);
            // MyVerticesBotomEdge = DeleteDuplicateVerteces(MyVerticesBotomEdge);

        }
        void newgenerateStack(int countChips)
        {
            List<int> trianglesEdge = new List<int>();
            List<int> trianglesFace = new List<int>();
            for (int i = 0; i < countChips; i++)
            {
                MyChip myChip = new MyChip(
                    // MyVerticesBotomEdge,
                    MyVerticesEdge,
                    null,
                    Random.Range(MinRotation, MaxRotation),
                    colors[Random.Range(0, colors.Length)],
                    trianglesEdge,
                    trianglesFace
                );


                MyVertices.AddRange(myChip.edge);
                // MyVertices.AddRange(myChip.bottom);


                if (i == countChips - 1)
                {
                    myChip.face = MyVerticesFace;
                    myChip.RotateAll();
                    MyVertices.AddRange(myChip.face);
                }

                // myChip.trianglesEdge = CreateTrianglesInEdge(0, 0, myChip.bottom.Count -1 );
                // List<int> notCon = new List<int>();
                // for (int j = 0; j <  myChip.topFace.Count; j++)
                // {
                //     notCon.Add(j + myChip.topFace.Count * (i+1));
                // }
                // notCon.Add(notCon[0]);//circle surface
                // myChip.trianglesFace = CreateTrianglesInFace(notCon);

                //test
                stackChips = new Mesh();
                List<Vector3> pos = new List<Vector3>();
                // pos.AddRange(GetPositionFromMyVertices(myChip.bottom));
                pos.AddRange(GetPositionFromMyVertices(myChip.edge));
                //pos.AddRange(GetPositionFromMyVertices(myChip.topFace));

                stackChips.vertices = pos.ToArray();
                // stackChips.SetVertices(GetPositionFromMyVertices(totalVertex));

                List<int> tris = new List<int>();
                tris.AddRange(myChip.trianglesEdge);
                // tris.AddRange(myChip.trianglesFace);
                stackChips.triangles = tris.ToArray();

                List<Vector2> uv = new List<Vector2>();
                // uv.AddRange(GetUvFromMyVertices(myChip.bottom));
                uv.AddRange(GetUvFromMyVertices(myChip.edge));
                //  uv.AddRange(GetUvFromMyVertices(myChip.topFace));
                stackChips.uv = uv.ToArray();

                List<Vector3> norm = new List<Vector3>();
                // norm.AddRange(GetNormalFromMyVertices(myChip.bottom));
                norm.AddRange(GetNormalFromMyVertices(myChip.edge));
                // norm.AddRange(GetNormalFromMyVertices(myChip.topFace));
                stackChips.normals = norm.ToArray();

                CreateObject(stackChips);
                DebugMesh(chip);
            }


        }
       

    }
}

