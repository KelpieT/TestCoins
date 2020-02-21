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
            GenerateStack(2);
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
            for (int i = 0; i < countChips; i++)
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
        private List<Vector3> DeleteDuplicateVerteces(List<Vector3> curList)
        {
            List<Vector3> vList = new List<Vector3>();
            for (int i = 0; i < curList.Count; i++)
            {
                Vector3 curV = curList[i];
                bool b = false;
                foreach (Vector3 v in vList)
                {
                    if (v == curV)
                    {
                        b = true;
                        break;
                    }
                }
                if (!b)
                {
                    vList.Add(curV);
                }
            }
            return vList;
        }
        private void SortVerteces(ref List<Vector3> curList)
        {
            List<Vector3> v3 = new List<Vector3>();
            foreach (Vector3 v in curList)
            {
                v3.Add(v);
            }
            curList = new List<Vector3>();
            curList = v3.OrderBy(v => Vector3.SignedAngle(v, Vector3.up, Vector3.forward)).ToList();
        }
        private void SetTrisToEdge(ref Mesh mesh)
        {
            // ______________________________________________
            //TEST!!!
            // ______________________________________________

            int[] totalTris = new int[mesh.vertices.Length * 3];

            for (int i = 0; i < totalTris.Length / 6 - 1; i++)
            {
                totalTris[i * 6] = i;
                totalTris[i * 6 + 1] = mesh.vertices.Length / 2 + i;
                totalTris[i * 6 + 2] = mesh.vertices.Length / 2 + 1 + i;
                totalTris[i * 6 + 3] = i + 1;
                totalTris[i * 6 + 4] = i;
                totalTris[i * 6 + 5] = mesh.vertices.Length / 2 + 1 + i;
                if (i == totalTris.Length / 6 - 2)
                {
                    totalTris[i * 6 + 6] = i + 1;
                    totalTris[i * 6 + 7] = mesh.vertices.Length / 2 + 1 + i;
                    totalTris[i * 6 + 8] = mesh.vertices.Length / 2;
                    totalTris[i * 6 + 9] = 0;
                    totalTris[i * 6 + 10] = i + 1;
                    totalTris[i * 6 + 11] = mesh.vertices.Length / 2;
                }
            }

            mesh.triangles = totalTris;

        }



    }

}