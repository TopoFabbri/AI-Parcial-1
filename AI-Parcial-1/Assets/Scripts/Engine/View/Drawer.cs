using System.Collections.Generic;
using System.Threading.Tasks;
using Model.Tools.Drawing;
using UnityEngine;

namespace Engine.View
{
    public class Drawer
    {
        public struct DrawInfo
        {
            public readonly Material mat;
            public readonly Mesh mesh;
            public Quaternion rotation;
            public Vector3 scale;

            public DrawInfo(Material mat, Mesh mesh, Quaternion rotation, Vector3 scale)
            {
                this.mat = mat;
                this.mesh = mesh;
                this.rotation = rotation;
                this.scale = scale;
            }
        }

        private const int MaxObjsPerDrawCall = 1000;
        private readonly ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 32 };

        private readonly DrawInfo defaultDrawInfo;
        private readonly Dictionary<string, DrawInfo> prefabDrawInfo = new();

        public Drawer(Dictionary<string, GameObject> prefabs, GameObject defaultPrefab)
        {
            foreach (KeyValuePair<string, GameObject> prefab in prefabs)
            {
                Material mat = prefab.Value.GetComponent<MeshRenderer>().sharedMaterial;
                Mesh mesh = prefab.Value.GetComponent<MeshFilter>().sharedMesh;
                Quaternion rotation = prefab.Value.transform.rotation;
                Vector3 scale = prefab.Value.transform.localScale;

                prefabDrawInfo.Add(prefab.Key, new DrawInfo(mat, mesh, rotation, scale));
            }

            Material defMat = defaultPrefab.GetComponent<MeshRenderer>().sharedMaterial;
            Mesh defMesh = defaultPrefab.GetComponent<MeshFilter>().sharedMesh;
            Quaternion defRot = defaultPrefab.transform.rotation;
            Vector3 defScale = defaultPrefab.transform.localScale;

            defaultDrawInfo = new DrawInfo(defMat, defMesh, defRot, defScale);
        }

        public void Draw()
        {
            List<Matrix4x4[]> drawMatrices = new();

            foreach (string name in Drawables.GetDrawableNames())
            {
                drawMatrices.Clear();

                if (!prefabDrawInfo.TryGetValue(name, out DrawInfo drawInfo))
                {
                    Debug.LogWarning("Prefab for " + name + " not found. Using default prefab instead.");
                    drawInfo = defaultDrawInfo;
                }
                
                List<IDrawable> drawables = Drawables.GetDrawablesOfName(name);
                int meshes = drawables.Count;

                for (int i = 0; i < meshes; i += MaxObjsPerDrawCall)
                {
                    drawMatrices.Add(new Matrix4x4[meshes > MaxObjsPerDrawCall ? MaxObjsPerDrawCall : meshes]);
                    meshes -= MaxObjsPerDrawCall;
                }

                ParallelLoopResult result = Parallel.For(0, drawables.Count, parallelOptions, i =>
                {
                    Vector3 position = new(drawables[i].GetPosition().X, drawables[i].GetPosition().Y, drawables[i].GetPosition().Z);
                    
                    drawMatrices[i / MaxObjsPerDrawCall][i % MaxObjsPerDrawCall].SetTRS(position, drawInfo.rotation, drawInfo.scale);
                });

                foreach (Matrix4x4[] matrixArray in drawMatrices)
                {
                    Graphics.DrawMeshInstanced(drawInfo.mesh, 0, drawInfo.mat, matrixArray);
                }
            }
        }
    }
}