using System;

namespace Prototypes.BRG.Animation
{
    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
    public sealed class AnimationStateAttribute : Attribute
    {
        public string EditorName { get; }

        public AnimationStateAttribute(string editorName)
        {
            EditorName = editorName;
        }
    }
}
