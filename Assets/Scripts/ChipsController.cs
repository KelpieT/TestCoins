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
        public Color color;
        public MyVertex(Vector3 position)
        {
            this.position = position;
            uv = Vector2.zero;
            normal = Vector3.forward;
            color = Color.white;
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
                    mv.position = Quaternion.Euler(0, 0, rotation) * listMyVertex[i].position;
                    listMyVertex[i] = mv;
                }
            }
        }
        public void RotateAll()
        {
            RotateListMyVertex(ref edge);
            RotateListMyVertex(ref face);

        }
        public void ColorAll()
        {
            if (edge != null)
            {
                for (int i = 0; i < edge.Count; i++)
                {
                    MyVertex mv = edge[i];
                    mv.color = color;
                    edge[i] = mv;
                }
            }
            if (face != null)
            {
                for (int i = 0; i < face.Count; i++)
                {
                    MyVertex mv = face[i];
                    mv.color = color;
                    face[i] = mv;
                }
            }
        }
        public void AddVector3ToPos(Vector3 addV3)
        {
            if (edge != null)
            {
                for (int i = 0; i < edge.Count; i++)
                {
                    MyVertex mv = edge[i];
                    mv.position = mv.position + addV3;
                    edge[i] = mv;
                }
            }
            if (face != null)
            {
                for (int i = 0; i < face.Count; i++)
                {
                    MyVertex mv = face[i];
                    mv.position = mv.position + addV3;
                    face[i] = mv;
                }
            }
        }
        public MyChip(List<MyVertex> edge, List<MyVertex> face, float rotation, Color color)
        {
            this.face = face;
            this.edge = edge;
            this.rotation = rotation;
            this.color = color;
            for (int i = 0; i < edge.Count; i++)
            {
                MyVertex mv = edge[i];
                mv.color = color;
                edge[i] = mv;
            }
            // RotateAll();
            ColorAll();
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
        [SerializeField]
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
        private int[] totTris = new int[0];

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
            CreateMyVertices();
            CreateAllTris();
            GenerateStack(CountChipsInStack);
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
        private List<Color> GetColorFromMyVertices(List<MyVertex> listMyVertex)
        {
            List<Color> colors = new List<Color>();
            foreach (MyVertex mv in listMyVertex)
            {
                colors.Add(mv.color);
            }
            return colors;
        }

        private void CreateMyVertices()
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

        }

        private void CreateAllTris()
        {
            AssignAllTrisFromMesh();
            CreateTrisForParts();
        }

        private void AssignAllTrisFromMesh()
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

        private void CreateTrisForParts()
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
            tempTrisFace = RemoveDuplicateTris(tempTrisFace);
            tempTrisEdge = RemoveDuplicateTris(tempTrisEdge);

            trianglesFace = ReplacedTris(tempTrisFace, numbersInTrisFace);
            trianglesEdge = ReplacedTris(tempTrisEdge, numbersInTrisEdge);

        }
        List<int[]> RemoveDuplicateTris(List<int[]> listTris)
        {
            //List<int[]> temp = new List<int[]>();

            int i = 0;
            int count = listTris.Count;
            while (i < count)
            {

                List<int> listFound = new List<int>();
                for (int j = i + 1; j < listTris.Count; j++)
                {
                    if (listTris[i][0] == listTris[j][0] && listTris[i][1] == listTris[j][1] && listTris[i][2] == listTris[j][2])
                    {

                        listFound.Add(j);

                    }
                }
                if (listFound.Count > 0)
                {
                    int k = 0;

                    while (k < listFound.Count)
                    {
                        listTris.RemoveAt(listFound[k] - k);
                        k++;
                    }
                }
                i++;
            }
            return listTris;
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
        int[] ReplacedTris(List<int[]> tempTris, List<int> numbers)
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

        void GenerateStack(int countChips)
        {
            List<Vector3> pos = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector3> norm = new List<Vector3>();
            List<Color> colors = new List<Color>();
            int countVertexInMesh = 0;
            for (int i = 0; i < countChips; i++)
            {

                MyChip myChip = new MyChip(MyVerticesEdge, MyVerticesFace, Random.Range(MinRotation, MaxRotation), this.colors[Random.Range(0, this.colors.Length)]);
                AddToTotalTriangles(trianglesEdge, countVertexInMesh);
                countVertexInMesh += myChip.edge.Count;
                myChip.AddVector3ToPos(Vector3.forward * i);
                pos.AddRange(GetPositionFromMyVertices(myChip.edge));
                uv.AddRange(GetUvFromMyVertices(myChip.edge));
                norm.AddRange(GetNormalFromMyVertices(myChip.edge));
                colors.AddRange(GetColorFromMyVertices(myChip.edge));


                if (i == countChips - 1)
                {
                    //MyChip myChip = new MyChip(MyVerticesEdge, MyVerticesFace, Random.Range(MinRotation, MaxRotation), this.colors[Random.Range(0, this.colors.Length)]);
                    AddToTotalTriangles(trianglesFace, countVertexInMesh);
                    countVertexInMesh += myChip.face.Count;
                    myChip.AddVector3ToPos(Vector3.forward * (i));
                    pos.AddRange(GetPositionFromMyVertices(myChip.face));
                    uv.AddRange(GetUvFromMyVertices(myChip.face));
                    norm.AddRange(GetNormalFromMyVertices(myChip.face));
                    colors.AddRange(GetColorFromMyVertices(myChip.face));

                    // myChip.RotateAll();
                    // myChip.ColorAll();
                }
                myChip.RotateAll();
                myChip.ColorAll();
                if (i == countChips - 1)
                {

                }


            }
            stackChips = new Mesh();
            stackChips.vertices = pos.ToArray();

            stackChips.uv = uv.ToArray();
            stackChips.normals = norm.ToArray();
            stackChips.colors = colors.ToArray();
            stackChips.triangles = totTris;
            CreateObject(stackChips);
            DebugMesh(stackChips);

        }
        void AddToTotalTriangles(int[] addTris, int countVert)
        {
            int lastNumber = countVert;

            List<int> temp = new List<int>();
            temp.AddRange(totTris);
            for (int i = 0; i < addTris.Length; i++)
            {
                int tr = addTris[i] + lastNumber;
                addTris[i] = tr;
            }
            temp.AddRange(addTris);
            totTris = temp.ToArray();
        }

    }
}

