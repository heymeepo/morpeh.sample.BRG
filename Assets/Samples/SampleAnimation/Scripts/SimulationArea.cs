using UnityEngine;
using System;

namespace Prototypes.SamplesBRG.AnimationSample
{
    public sealed class SimulationArea : IDisposable
    {
        private int resolution;
        private Material material;
        private GameObject areaGameObject;

        public SimulationArea(int resolution, Material material)
        {
            this.resolution = resolution;
            this.material = material;

            CreateArea();
        }

        public void Dispose() => UnityEngine.Object.Destroy(areaGameObject);

        private void CreateArea()
        {
            Mesh planeMesh = LoadDefaultPlane();

            areaGameObject = new GameObject("SimulationArea");

            var mf = areaGameObject.AddComponent<MeshFilter>();
            var mr = areaGameObject.AddComponent<MeshRenderer>();

            mf.sharedMesh = planeMesh;
            mr.sharedMaterial = material ? material : LoadDefaultMaterial();

            if (mr.sharedMaterial != null)
            {
                mr.sharedMaterial.mainTextureScale = new Vector2(resolution, resolution);
            }

            areaGameObject.transform.localScale = new Vector3(resolution / 10, 0f, resolution / 10);
        }

        private Mesh LoadDefaultPlane() => Resources.GetBuiltinResource(typeof(Mesh), "New-Plane.fbx") as Mesh;

        private Material LoadDefaultMaterial()
        {
            Shader defaultLitShader = Shader.Find("Universal Render Pipeline/Lit");

            if (defaultLitShader == null)
            {
                Debug.LogError("Failed to find the Default Lit shader!");
                return null;
            }

            return new Material(defaultLitShader);
        }
    }
}
