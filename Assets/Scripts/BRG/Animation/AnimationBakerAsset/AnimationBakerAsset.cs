#if UNITY_EDITOR
using Prototypes.BRG.Animation.TAO.VertexAnimation;
using Scellecs.Morpeh;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Prototypes.BRG.Animation
{
    [CreateAssetMenu(fileName = "AnimationBaker", menuName = "BRG/AnimationBaker")]
    public sealed class AnimationBakerAsset : ScriptableObject
    {
        [SerializeField] private GameObject prefab = default;
        [SerializeField] private Shader shader = default;
        [SerializeField] private Material presetMaterial = default;
        [SerializeField] private RenderSharedData renderDataAsset = default;
        [SerializeField] private AnimationSharedData animationDataAsset = default;
        [SerializeField] private AnimationPairList animations = new AnimationPairList();

        [SerializeField, Range(1, 60)] private int fps = 24;
        [SerializeField] private int textureWidth = 512;
        [SerializeField] private bool applyRootMotion = false;

#if ODIN_INSPECTOR
        [Button]
#endif
        private void Bake()
        {
            if (CheckRequired())
            {
                var target = Instantiate(prefab);
                target.name = prefab.name;

                MeshCombiner.CombineAndConvertGameObject(target);
                var bakedData = BakeAnimations(target);
                var positionMap = CreatePositionMap(bakedData);

                DestroyImmediate(target);
                SaveAssets(bakedData, positionMap);
            }
        }

        private AnimationBaker.BakedData BakeAnimations(GameObject target)
        {
            var clips = new AnimationClip[animations.Count];

            for (int i = 0; i < clips.Length; i++)
            {
                clips[i] = animations[i].clip;
            }

            return AnimationBaker.Bake(target, clips, applyRootMotion, fps, textureWidth);
        }

        private Texture2DArray CreatePositionMap(AnimationBaker.BakedData bakedData)
        {
            return Texture2DArrayUtils.CreateTextureArray(bakedData.positionMaps.ToArray(), false, true, TextureWrapMode.Repeat, FilterMode.Point, 1, string.Format("{0} PositionMap", prefab.name), true);
        }

        private void SaveAssets(AnimationBaker.BakedData bakedData, Texture2DArray positionMap)
        {
            GenerateRenderData(bakedData, positionMap);
            GenerateAnimationData(bakedData);
        }

        private void GenerateRenderData(AnimationBaker.BakedData bakedData, Texture2DArray positionMap)
        {
            NamingConventionUtils.PositionMapInfo info = bakedData.GetPositionMap.name.GetTextureInfo();
            var material = AnimationMaterial.Create(string.Format("{0} Material", prefab.name), shader, positionMap, info.maxFrames, presetMaterial);

            bakedData.mesh.Optimize();
            bakedData.mesh.UploadMeshData(true);

            AssetDatabaseUtils.RemoveChildAssets(renderDataAsset);
            AssetDatabase.AddObjectToAsset(material, renderDataAsset);
            AssetDatabase.AddObjectToAsset(bakedData.mesh, renderDataAsset);
            AssetDatabase.AddObjectToAsset(positionMap, renderDataAsset);

            renderDataAsset.sharedMesh = bakedData.mesh;
            renderDataAsset.sharedMaterial = material;

            SaveAsset(renderDataAsset);
        }

        private void GenerateAnimationData(AnimationBaker.BakedData bakedData)
        {
            animationDataAsset.animations = new AnimationData[bakedData.positionMaps.Count];
            animationDataAsset.components = new IComponent[bakedData.positionMaps.Count];

            List<NamingConventionUtils.PositionMapInfo> info = new List<NamingConventionUtils.PositionMapInfo>();

            foreach (var tex in bakedData.positionMaps)
            {
                info.Add(tex.name.GetTextureInfo());
            }

            for (int i = 0; i < info.Count; i++)
            {
                AnimationData newData = new AnimationData(info[i].frames, info[i].maxFrames, info[i].fps, i, animations[i].clip.isLooping, -1);
                IComponent animStateComponent = animations[i].animationStateComponent;

                var componentType = animStateComponent.GetType();
                var fields = componentType.GetFields();

                if (fields.Length == 1 && fields[0].FieldType == typeof(int))
                {
                    fields[0].SetValue(animStateComponent, i);
                }
                else
                {
                    Debug.LogError($"Generation failed. Component {componentType.FullName} has invalid format. It must have exactly one int field");
                    return;
                }

                animationDataAsset.animations[i] = newData;
                animationDataAsset.components[i] = animStateComponent;
            }

            SaveAsset(animationDataAsset);
        }

        private void SaveAsset(ScriptableObject asset)
        {
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private bool CheckRequired()
        {
            bool result = true;

            if (prefab == null)
            {
                Debug.LogError($"Animation baker {name} : prefab is not assigned");
                result = false;
            }
            if (shader == null)
            {
                Debug.LogError($"Animation baker {name} : shader is not assigned");
                result = false;
            }
            if (renderDataAsset == null)
            {
                Debug.LogError($"Animation baker {name} : renderData asset is not assigned");
                result = false;
            }
            if (animationDataAsset == null)
            {
                Debug.LogError($"Animation baker {name} : animationData asset is not assigned");
                result = false;
            }
            if (animations.Count == 0)
            {
                Debug.LogError($"Animation baker {name} : animations are not assigned");
                result = false;
            }

            return result;
        }

    }
}
#endif
