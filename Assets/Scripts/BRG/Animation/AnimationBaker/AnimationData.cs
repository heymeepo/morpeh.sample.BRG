using UnityEngine;

namespace Prototypes.BRG.Animation.TAO.VertexAnimation
{
    [System.Serializable]
    public struct AnimationData
    {
        // The frames in this animation.
        public int frames;
        // The maximum of frames the texture holds.
        public int maxFrames;
        // The index of the related animation texture.
        public int animationMapIndex;
        // The index of the related color textures if/when added.
        public int colorMapIndex;
        // Time of a single frame.
        public float frameTime;
        // Total time of the animation.
        public float duration;
        // Is the animation looping
        public bool isLooping;

        public AnimationData(int frames, int maxFrames, int fps, int positionMapIndex, bool isLooping, int colorMapIndex = -1)
        {
            this.frames = frames;
            this.maxFrames = maxFrames;
            animationMapIndex = positionMapIndex;
            this.colorMapIndex = colorMapIndex;
            frameTime = 1.0f / maxFrames * fps;
            duration = 1.0f / maxFrames * (frames - 1);
            this.isLooping = isLooping;
        }
    }
}
