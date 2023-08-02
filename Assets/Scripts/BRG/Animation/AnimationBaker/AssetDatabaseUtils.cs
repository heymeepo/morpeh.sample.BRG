﻿using UnityEditor;
using UnityEngine;

namespace Prototypes.BRG.Animation.TAO.VertexAnimation
{
#if UNITY_EDITOR
    public static class AssetDatabaseUtils
    {
        public static bool HasChildAsset(Object parent, Object child)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(parent));

            foreach (var a in assets)
            {
                if (a == child)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool TryAddChildAsset(Object parent, Object child)
        {
            if (!HasChildAsset(parent, child))
            {
                AssetDatabase.AddObjectToAsset(child, parent);
                return true;
            }

            return false;
        }

        public static void RemoveChildAssets(Object parent)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(parent));

            foreach (var a in assets)
            {
                if (a != parent)
                {
                    AssetDatabase.RemoveObjectFromAsset(a);
                }
            }
        }

        public static bool HasAsset(string path, System.Type type)
        {
            return (AssetDatabase.LoadAssetAtPath(path, type) ? true : false);
        }
    }
#endif
}
