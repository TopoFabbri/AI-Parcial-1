using Model.Game.Graph;
using UnityEngine;

namespace Engine.Controller.Input
{
    public class CursorController : MonoBehaviour
    {
        private Coordinate currentCoordinate;
        private Vector3 position;
        private Camera cam;

        private void Start()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            
        }
    }
}