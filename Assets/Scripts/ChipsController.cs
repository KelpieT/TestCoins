using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ChipsController
{

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

        private void Start()
        {
            LoadAllResources();
            GenerateStack(1);
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
            List<Vector3> totalVertex = new List<Vector3>();
            for (int i = 0; i < countChips + 1; i++)
            {
                foreach (Vector3 v in edgeLoop)
                {
                    totalVertex.Add(v + Vector3.forward * 0.3f * i);
                }

            }

            stackChips.SetVertices(totalVertex);
            DebugMesh(stackChips);
            SetTrisToEdge(ref stackChips);
            CreateObject(stackChips);

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
        private void SetTrisToEdge(ref Mesh mesh)
        {
            // ______________________________________________
            //TEST!!!
            // ______________________________________________



            mesh.triangles = CreateTrianglesInEdge(0, 0, mesh.vertexCount).ToArray();

        }
        private List<int> CreateTrianglesInEdge(int firstVert, int FirstTotalVert, int LastTotalVert)
        {
            List<int> currentTwotriangles = new List<int>();
            if (firstVert < LastTotalVert / 2)
            {
                int p1 = firstVert;
                int p2 = LastTotalVert / 2 + firstVert;
                int p3 = LastTotalVert / 2 + firstVert + 1;
                int p4 = firstVert + 1;
                if (firstVert == LastTotalVert / 2 - 1)
                {
                    //connect edge
                    p3 = LastTotalVert / 2;
                    p4 = FirstTotalVert;
                }
                currentTwotriangles.Add(p1);
                currentTwotriangles.Add(p2);
                currentTwotriangles.Add(p3);
                currentTwotriangles.Add(p4);
                currentTwotriangles.Add(p1);
                currentTwotriangles.Add(p3);
                firstVert += 1;
                currentTwotriangles.AddRange(CreateTrianglesInEdge(firstVert, FirstTotalVert, LastTotalVert));
            }
            return currentTwotriangles;
        }



    }

}