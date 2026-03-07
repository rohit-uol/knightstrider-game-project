using UnityEngine;
using UnityEngine.SceneManagement;

namespace TheMasterPath
{
    public class MenuButtons : MonoBehaviour
    {
        [SerializeField]
        GameObject credits;

        public void OnPlay()
        {
            SceneManager.LoadScene("Level1");
        }

        public void OnCredits()
        {
            credits.SetActive(!credits.activeSelf);
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
