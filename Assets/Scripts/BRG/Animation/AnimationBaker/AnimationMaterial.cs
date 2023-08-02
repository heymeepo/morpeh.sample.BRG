#if UNITY_EDITOR
using UnityEditor.Presets;
using UnityEngine;

namespace Prototypes.BRG.Animation.TAO.VertexAnimation
{
    public static class AnimationMaterial
    {
        public static Material Create(string name, Shader shader, Texture2DArray positionMap, int maxFrames, Material presetMaterial)
        {
            Material material = new Material(shader);

            if (presetMaterial != null)
            {
                var preset = new Preset(presetMaterial);

                if (preset.CanBeAppliedTo(material))
                {
                    preset.ApplyTo(material);
                }

                Object.DestroyImmediate(preset);
            }

            material.Update(name, shader, positionMap, maxFrames);
            return material;
        }

        public static void Update(this Material material, string name, Shader shader, Texture2DArray positionMap, int maxFrames)
        {
            material.name = name;

            if (material.shader != shader)
            {
                material.shader = shader;
            }

            material.SetTexture("_PositionMap", positionMap);
            material.SetInt("_MaxFrames", maxFrames);
        }
    }
}
#endif
