using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheMasterPath
{
    public class MenuButtons : MonoBehaviour
    {
        public void OnPlay()
        {
            SceneManager.LoadScene("SampleScene");
        }

        public void OnExit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}
