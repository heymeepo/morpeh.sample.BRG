using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Prototypes.BRG
{
    [CreateAssetMenu(fileName = "RenderData", menuName = "BRG/RenderData")]
    public sealed class RenderSharedData : ScriptableObject
    {
        public Mesh sharedMesh;
        public Material sharedMaterial;
        public BatchMaterialPropertiesList materialProperties = new BatchMaterialPropertiesList();

        [NonSerialized] private DrawKey drawKey;
        [NonSerialized] private bool initialized;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetDrawKey(out DrawKey drawKey)
        {
            drawKey = this.drawKey;
            return initialized;
        }

        public void Initialize(DrawKey drawKey)
        {
            if (Application.isPlaying && initialized == false)
            {
                this.drawKey = drawKey;
                initialized = true;
            }
        }
    }
}
