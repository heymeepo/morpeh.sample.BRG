using System.Runtime.CompilerServices;

namespace Prototypes.BRG
{
    public enum BatchMaterialPropertyFormat
    {
        Float = 0,
        Float2 = 1,
        Float3 = 2,
        Float4 = 3,
        Float2x4 = 4,
        Float3x4 = 5,
        Float4x4 = 6,
    }

    public static class BatchMaterialPropertyFormatExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSizeFormat(this BatchMaterialPropertyFormat format)
        {
            if (format == BatchMaterialPropertyFormat.Float)
            {
                return sizeof(float);
            }
            else if (format == BatchMaterialPropertyFormat.Float2)
            {
                return sizeof(float) * 2;
            }
            else if (format == BatchMaterialPropertyFormat.Float3)
            {
                return sizeof(float) * 3;
            }
            else if (format == BatchMaterialPropertyFormat.Float4)
            {
                return sizeof(float) * 4;
            }
            else if (format == BatchMaterialPropertyFormat.Float2x4)
            {
                return sizeof(float) * 2 * 4;
            }
            else if (format == BatchMaterialPropertyFormat.Float3x4)
            {
                return sizeof(float) * 3 * 4;
            }
            else
            {
                return sizeof(float) * 4 * 4;
            }
        }
    }
}
