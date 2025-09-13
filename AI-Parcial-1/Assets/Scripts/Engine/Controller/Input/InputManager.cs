using UnityEngine;
using UnityEngine.SceneManagement;

namespace Engine.Controller.Input
{
    public class InputManager : MonoBehaviour
    {
        public void RestartGame()
        {
            string activeSceneName = SceneManager.GetActiveScene().name;
            
            SceneManager.LoadScene("IntermediateScene");
            SceneManager.LoadScene(activeSceneName);
        }
        
        public void EndGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }
}