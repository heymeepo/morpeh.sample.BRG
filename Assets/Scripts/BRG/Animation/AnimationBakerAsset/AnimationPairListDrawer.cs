#if UNITY_EDITOR && ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using UnityEngine;

namespace Prototypes.BRG.Animation
{
    public class AnimationPairListDrawer : OdinValueDrawer<AnimationPairList>
    {
        protected override void DrawPropertyLayout(GUIContent label) => Property.Children[0].Draw(label);
    }
}
#endif
