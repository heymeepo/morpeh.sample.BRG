namespace Prototypes.BRG
{
    public readonly struct MetadataValueBPB
    {
        public readonly int id;
        public readonly int size;
        public readonly int propertyBlockOffset;

        public MetadataValueBPB(int id, int size, int propertyBlockOffset)
        {
            this.id = id;
            this.size = size;
            this.propertyBlockOffset = propertyBlockOffset;
        }
    }
}
