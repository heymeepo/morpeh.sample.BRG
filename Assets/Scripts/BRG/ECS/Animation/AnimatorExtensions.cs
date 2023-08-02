using System.Runtime.CompilerServices;

namespace Prototypes.BRG.ECS.Animation
{
    public static class AnimatorExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetAnimation(ref this AnimatorComponent animator, int index, float speed)
        {
            animator.newIndex = index;
            animator.speed = speed;
        }

        public static void SetAnimationOverrided(ref this AnimatorComponent animator, int index, float speed)
        {
            animator.newIndex = index;
            animator.currentIndex = animator.newIndex;
            animator.time = 0f;
            animator.speed = speed;
            animator.isDone = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetSpeed(ref this AnimatorComponent animator, float speed)
        {
            animator.speed = speed;
        }
    }
}
