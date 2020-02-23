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
        public int numberInTriangles;
        public MyVertex(Vector3 position)
        {
            this.position = position;
            uv = Vector2.zero;
            normal = Vector3.forward;
            numberInTriangles = -1;
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

        //---------------------------------------------


        private void Start()
        {
            LoadAllResources();
            GenerateStack(0);
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

            List<Vector3> edgeLoop = GetEdgeLoopChipVertex();
            List<MyVertex> totalVertex = new List<MyVertex>();
            List<int> tris = new List<int>();
            for (int i = 0; i < countChips + 1; i++)
            {
                foreach (Vector3 v in edgeLoop)
                {
                    totalVertex.Add(new MyVertex(v + Vector3.forward * 0.3f * i));
                }
                if (i > 0)
                {
                    tris.AddRange(CreateTrianglesInEdge(0, (edgeLoop.Count) * (i - 1), edgeLoop.Count));
                }
                if (i == countChips)
                {
                    List<int> notCon = new List<int>();
                    for (int j = 0; j < edgeLoop.Count; j++)
                    {
                        notCon.Add(j + edgeLoop.Count * i);
                    }
                    notCon.Add(notCon[0]);//circle surface
                    tris.AddRange(CreateTrianglesInFace(notCon));
                }
            }
           
            stackChips.SetVertices(GetPositionFromMyVertices(totalVertex));
            //stackChips.SetUVs(0,uvww);
            //DebugMesh(chip);
            stackChips.triangles = tris.ToArray();
            CreateObject(stackChips);
            // GetUVForFace();

        }

        private List<Vector3> GetEdgeLoopChipVertex()
        {
            List<Vector3> v3 = new List<Vector3>();
            List<Vector3> temp = new List<Vector3>();
            chip.GetVertices(temp);
            float zBig = float.MinValue;
            float zOffset = 0.01f;
            foreach (Vector3 v in temp)
            {
                if (zBig < v.z) { zBig = v.z; }
            }
            for (int i = 0; i < chip.vertices.Length; i++)
            {

                Vector3 v = chip.vertices[i];
                if (v.z > zBig - zOffset && v.z < zBig + zOffset)
                {

                    v3.Add(v);
                }
            }
            v3 = DeleteDuplicateVerteces(v3);
            SortVerteces(ref v3);
            return v3;

        }
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
            foreach (Vector3 v in mesh.vertices)
            {
                Debug.Log(v.ToString());
            }

        }
        private List<Vector3> DeleteDuplicateVerteces(List<Vector3> currentList)
        {
            List<Vector3> tempList = new List<Vector3>();
            for (int i = 0; i < currentList.Count; i++)
            {
                Vector3 currentVector = currentList[i];
                bool foundInList = false;
                foreach (Vector3 v in tempList)
                {
                    if (v == currentVector)
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
        private void SortVerteces(ref List<Vector3> currentList)
        {
            List<Vector3> tempList = new List<Vector3>();
            foreach (Vector3 v in currentList)
            {
                tempList.Add(v);
            }
            currentList = new List<Vector3>();
            currentList = tempList.OrderBy(v => Vector3.SignedAngle(v, Vector3.up, Vector3.forward)).ToList();
        }

        private List<int> CreateTrianglesInEdge(int firstVert, int startIndex, int countVertexEdge)
        {
            List<int> currentTwotriangles = new List<int>();
            if (firstVert < (countVertexEdge))
            {
                int p1 = firstVert;
                int p2 = countVertexEdge + firstVert;
                int p3 = countVertexEdge + firstVert + 1;
                int p4 = firstVert + 1;
                if (firstVert == countVertexEdge - 1)
                {
                    //connect edge
                    p3 = countVertexEdge;
                    p4 = 0;
                }
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

        // void GetUVForFace()
        // {
        //     List<Vector2> nnnn = new List<Vector2>();
        //     chip.GetUVs(0, nnnn);
        //     foreach (Vector2 v in nnnn)
        //     {
        //         Debug.Log(v);
        //     }
        // }
    }

}