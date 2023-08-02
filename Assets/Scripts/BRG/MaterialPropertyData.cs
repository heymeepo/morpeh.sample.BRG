using System;

namespace Prototypes.BRG
{
    public struct MaterialPropertyData : IEquatable<MaterialPropertyData>
    {
        public int propertyId;
        public BatchMaterialPropertyFormat format;
        public Type associatedComponentType;

        public bool Equals(MaterialPropertyData other) => propertyId.Equals(other.propertyId);
    }
}
