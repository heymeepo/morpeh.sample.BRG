using UnityEngine;
using UnityEngine.SceneManagement;

namespace Prototypes.SamplesBRG
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private KeyCode backToMenuKey;

        private static readonly string MAIN_MENU_SCENE_NAME = "MainMenu";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Input.GetKeyDown(backToMenuKey))
            {
                SceneManager.LoadScene(MAIN_MENU_SCENE_NAME, LoadSceneMode.Single);
            }
        }

        public void LoadSceneAsync(string name) => SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
    }
}
