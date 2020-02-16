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
            stackChips.SetVertices(GetEdgeLoopChipVertex());
            SortVerteces(ref stackChips);
            DebugMesh(stackChips);
            CreateObject(stackChips);
           
        }
        private List<Vector3> GetEdgeLoopChipVertex()
        {
            List<Vector3> v3 = new List<Vector3>();
            float zBig = float.MinValue;
            float zOffset = 0.01f;
            foreach (Vector3 v in chip.vertices)
            {
                if (zBig < v.z) { zBig = v.z; }
            }
            foreach (Vector3 v in chip.vertices)
            {
                if (v.z > zBig - zOffset && v.z < zBig+zOffset)
                {
                    v3.Add(v);
                }
            }
            return v3;

        }
        
        void CreateObject(Mesh stackMesh)
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
        void SortVerteces(ref Mesh mesh)
        {
            List<Vector3> v3 = new List<Vector3>();
            List<Vector3> vv3 = new List<Vector3>();
            foreach (Vector3 v in mesh.vertices)
            {
                v3.Add(v);
            }
            vv3 = v3.OrderBy(v => Vector3.SignedAngle(v, Vector3.up, Vector3.forward)).ToList();
            mesh.SetVertices(vv3);

        }
       
    }

}