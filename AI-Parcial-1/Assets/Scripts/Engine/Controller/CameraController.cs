using Model.Game.Graph;
using Model.Tools.Pathfinder.Graph;
using Model.Tools.Pathfinder.Node;
using UnityEngine;

namespace Engine.Controller
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float height = 10f;
        [SerializeField] private float sizeMultiplier;
        [SerializeField] private Camera cam;

        private const float PerspectiveMultiplier = 5f;

        public void PositionCamera(IGraph<Node<Coordinate>, Coordinate> graph)
        {
            transform.position = new Vector3((graph.GetSize().X - 1) * graph.GetNodeDistance() / 2f, height, (graph.GetSize().Y - 1) * graph.GetNodeDistance() / 2f);
            transform.LookAt(transform.position + Vector3.down);
            
            if (cam.orthographic)
                cam.orthographicSize = Mathf.Max(graph.GetSize().X, graph.GetSize().Y) * graph.GetNodeDistance() / 2f * sizeMultiplier;
            else
                cam.fieldOfView = Mathf.Max(graph.GetSize().X, graph.GetSize().Y) * graph.GetNodeDistance() * sizeMultiplier * PerspectiveMultiplier;
        }
    }
}