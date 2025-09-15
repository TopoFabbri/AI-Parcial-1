using Model.Game.Events;
using Model.Tools.EventSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Engine.Controller.Input
{
    public class InputManager : MonoBehaviour
    {
        [SerializeField] private EngineManager engineManager;
        
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

        public void OnCreateMiner()
        {
            EventSystem.Raise<RequestedMinerCreationEvent>(engineManager.MinerSpeed, engineManager.MinerMineSpeed);
        }

        public void CreateCaravan()
        {
            EventSystem.Raise<RequestedCaravanCreationEvent>(engineManager.CaravanSpeed, engineManager.CaravanCapacity);
        }

        public void RaiseAlarm()
        {
            EventSystem.Raise<RaiseAlarmEvent>();
        }
    }
}