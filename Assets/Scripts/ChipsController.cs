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
        public List<MyVertex> edge;
        public List<MyVertex> face;

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
            RotateListMyVertex(ref edge);
            RotateListMyVertex(ref face);

        }

        public MyChip(List<MyVertex> edge, List<MyVertex> face, float rotation, Color color)
        {
            this.face = face;
            this.edge = edge;
            this.rotation = rotation;
            this.color = color;

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

        private List<int[]> triangles = new List<int[]>();

        //---------------------------------------------

        //Temp variables
        //---------------------------------------------
        public float MinRotation;
        public float MaxRotation;
        public Color[] colors;
        //---------------------------------------------
        private int[] trianglesEdge;
        private int[] trianglesFace;

        private void Start()
        {
            LoadAllResources();
            WorkWithMyVertex();
            newgenerateStack(1);
        }
        private void LoadAllResources()
        {
            chip = Resources.Load<Mesh>(pathToMeshChip);
            material = Resources.Load<Material>(pathToMeshMaterial);
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
            foreach (Vector3 v in mesh.normals)
            {
                Debug.Log(v.ToString());
            }

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

            CreateAllTris();

            sortMyVerteces();
        }

        private void CreateAllTris()
        {
            int[] tris = chip.triangles;
            for (int i = 0; i < tris.Length / 3; i++)
            {
                int[] triangle = new int[3];
                for (int j = 0; j < 3; j++)
                {
                    triangle[j] = tris[i * 3 + j];
                }
                triangles.Add(triangle);
            }
        }

        private void sortMyVerteces()
        {

            float zOffset = 0.1f;
            List<int[]> tempTrisFace = new List<int[]>();
            List<int> numbersInTrisFace = new List<int>();
            List<int[]> tempTrisEdge = new List<int[]>();
            List<int> numbersInTrisEdge = new List<int>();
            for (int i = 0; i < MyVertices.Count; i++)
            {

                if (MyVertices[i].normal.z > zOffset)
                {
                    MyVerticesFace.Add(MyVertices[i]);
                    tempTrisFace.AddRange(SearchTris(i));
                    numbersInTrisFace.Add(i);
                }
                else if (MyVertices[i].normal.z > -zOffset)
                {
                    MyVerticesEdge.Add(MyVertices[i]);
                    tempTrisEdge.AddRange(SearchTris(i));
                    numbersInTrisEdge.Add(i);
                }
            }
            trianglesFace = replacedTris(tempTrisFace, numbersInTrisFace);
            trianglesEdge = replacedTris(tempTrisEdge, numbersInTrisEdge);

        }
        List<int[]> SearchTris(int vertexNumber)
        {
            List<int[]> temp = new List<int[]>();
            foreach (int[] tr in triangles)
            {
                bool isFound = false;
                foreach (int i in tr)
                {
                    if (i == vertexNumber)
                    {
                        isFound = true;
                        break;
                    }
                }
                if (isFound)
                {
                    temp.Add(tr);
                }
            }
            return temp;
        }
        int[] replacedTris(List<int[]> tempTris, List<int> numbers)
        {
            int[] totalTris = new int[tempTris.Count * 3];
            for (int i = 0; i < tempTris.Count; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    totalTris[i * 3 + j] = tempTris[i][j];
                }
            }
            for (int i = 0; i < numbers.Count; i++)
            {
                for (int j = 0; j < totalTris.Length; j++)
                {
                    if (numbers[i] == totalTris[j])
                    {
                        totalTris[j] = i;
                    }
                }
            }
            return totalTris;
        }



        void newgenerateStack(int countChips)
        {
            List<int> trianglesEdge = new List<int>();
            List<int> trianglesFace = new List<int>();
            for (int i = 0; i < countChips; i++)
            {
                MyChip myChip = new MyChip(MyVerticesEdge, null, Random.Range(MinRotation, MaxRotation), colors[Random.Range(0, colors.Length)]);

                MyVertices.AddRange(myChip.edge);
                if (i == countChips - 1)
                {
                    myChip.face = MyVerticesFace;
                    myChip.RotateAll();
                    MyVertices.AddRange(myChip.face);
                }

                //test
                stackChips = new Mesh();
                List<Vector3> pos = new List<Vector3>();
                pos.AddRange(GetPositionFromMyVertices(MyVerticesFace));

                stackChips.vertices = pos.ToArray();

                List<int> tris = new List<int>();
                stackChips.triangles = this.trianglesFace;

                List<Vector2> uv = new List<Vector2>();
                uv.AddRange(GetUvFromMyVertices(MyVerticesFace));
                stackChips.uv = uv.ToArray();

                List<Vector3> norm = new List<Vector3>();
                norm.AddRange(GetNormalFromMyVertices(MyVerticesFace));
                stackChips.normals = norm.ToArray();

                CreateObject(stackChips);
                DebugMesh(stackChips);
            }


        }


    }
}

