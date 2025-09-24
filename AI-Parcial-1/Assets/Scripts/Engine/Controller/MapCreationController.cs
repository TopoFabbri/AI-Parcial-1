using System;
using System.Globalization;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Engine.Controller
{
    public class MapCreationController : MonoBehaviour
    {
        [Required, SerializeField] private Slider nodeDistanceSlider;
        [Required, SerializeField] private Slider mineQtySlider;
        
        [SerializeField] private TextMeshProUGUI nodeDistanceText;
        [SerializeField] private TextMeshProUGUI mineQtyText;

        private void Start()
        {
            nodeDistanceSlider.value = MapCreationData.NodeDistance;
            mineQtySlider.value = MapCreationData.MineQty;
        }

        public void OnNodeDistanceChanged(float value)
        {
            if (nodeDistanceText) 
                nodeDistanceText.text = MathF.Round(value, 1).ToString(CultureInfo.InvariantCulture);
            
            MapCreationData.NodeDistance = value;
        }
        
        public void OnMineQtyChanged(float value)
        {
            value = Mathf.Round(value);
            
            if (mineQtyText) 
                mineQtyText.text = value.ToString(CultureInfo.InvariantCulture);
            
            MapCreationData.MineQty = (int)value;
        }

        public void OnCreate()
        {
            SceneManager.LoadScene("SampleScene");
        }

        public void OnEnd()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }
    }

    public static class MapCreationData
    {
        public static float NodeDistance { get; set; } = 1f;
        public static int MineQty { get; set; } = 10;
    }
}
