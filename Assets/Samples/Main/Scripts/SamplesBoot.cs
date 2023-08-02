using UnityEngine;

namespace Prototypes.SamplesBRG
{
    public class SamplesBoot : MonoBehaviour
    {
        [SerializeField] private MainMenuUI menuUI;
        [SerializeField] private SceneLoader sceneLoader;

        private static readonly string PRIMITIVES_SAMPLE_SCENE_NAME = "PrimitivesSample";
        private static readonly string ANIMATION_SAMPLE_SCENE_NAME = "AnimationSample";

        private void Awake()
        {
            Application.targetFrameRate = 1024;
            InitializeMenu();
        }

        private void InitializeMenu()
        {
            menuUI.PrimitivesButtonOnClickAddListener(() => sceneLoader.LoadSceneAsync(PRIMITIVES_SAMPLE_SCENE_NAME));
            menuUI.AnimationButtonOnClickAddListener(() => sceneLoader.LoadSceneAsync(ANIMATION_SAMPLE_SCENE_NAME));
        }
    }
}
