using System;
using UnityEngine.Rendering;

namespace Prototypes.BRG
{
    public struct DrawKey : IEquatable<DrawKey>, IComparable<DrawKey>
    {
        public BatchMeshID meshID;
        public BatchMaterialID materialID;
        public uint submeshIndex;

        public override int GetHashCode() => HashCode.Combine(meshID, submeshIndex, materialID);

        public int CompareTo(DrawKey other)
        {
            int cmpMaterial = materialID.CompareTo(other.materialID);
            int cmpMesh = meshID.CompareTo(other.meshID);
            int cmpSubmesh = submeshIndex.CompareTo(other.submeshIndex);

            if (cmpMaterial != 0)
            {
                return cmpMaterial;
            }

            if (cmpMesh != 0)
            {
                return cmpMesh;
            }

            return cmpSubmesh;
        }

        public bool Equals(DrawKey other) => CompareTo(other) == 0;
    }
}
