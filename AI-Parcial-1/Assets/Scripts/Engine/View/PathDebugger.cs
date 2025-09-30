using System;
using System.Collections.Generic;
using System.Linq;
using Model.Game.Graph;
using Model.Game.World.Agents;
using Model.Tools.Drawing;
using UnityEngine;

namespace Engine.View
{
    public class PathDebugger : MonoBehaviour
    {
        [SerializeField] private EngineManager engineManager;
        [SerializeField] private Color minerColour;
        [SerializeField] private Color caravanColour;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private Vector3 offset = Vector3.up;
        
        private readonly Dictionary<Miner, LineRenderer> minerPathRenderers = new();
        private readonly Dictionary<Caravan, LineRenderer> caravanPathRenderers = new();

        private void Update()
        {
            RenderMinerPaths();
            RenderCaravanPaths();
        }

        private void RenderCaravanPaths()
        {
            List<ILocalizable> localizableCaravans = Localizables.GetLocalizablesOfName("Caravan").ToList();
            List<Caravan> currentCaravans = localizableCaravans.Cast<Caravan>().ToList();

            // Remove renderers for miners that no longer exist
            List<Caravan> caravansToRemove = caravanPathRenderers.Keys.Except(currentCaravans).ToList();
            foreach (Caravan caravan in caravansToRemove)
            {
                Destroy(caravanPathRenderers[caravan].gameObject);
                caravanPathRenderers.Remove(caravan);
            }

            foreach (Caravan caravan in currentCaravans)
            {
                if (!caravanPathRenderers.TryGetValue(caravan, out LineRenderer lineRenderer))
                {
                    GameObject lineObject = new("CaravanPath" + caravan.Id);
                    lineObject.transform.SetParent(transform);
                    lineRenderer = lineObject.AddComponent<LineRenderer>();
                    lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Simple default material
                    lineRenderer.startWidth = lineWidth;
                    lineRenderer.endWidth = lineWidth;
                    lineRenderer.startColor = caravanColour;
                    lineRenderer.endColor = caravanColour;
                    lineRenderer.useWorldSpace = true;
                    caravanPathRenderers.Add(caravan, lineRenderer);
                }

                List<Vector3> pathPoints = BuildWrappedPath(caravan.Path);
                
                lineRenderer.positionCount = pathPoints.Count;
                if (pathPoints.Count > 0)
                    lineRenderer.SetPositions(pathPoints.ToArray());
            }
        }

        private void RenderMinerPaths()
        {
            List<ILocalizable> localizableMiners = Localizables.GetLocalizablesOfName("Miner").ToList();
            List<Miner> currentMiners = localizableMiners.Cast<Miner>().ToList();

            // Remove renderers for miners that no longer exist
            List<Miner> minersToRemove = minerPathRenderers.Keys.Except(currentMiners).ToList();
            foreach (Miner miner in minersToRemove)
            {
                Destroy(minerPathRenderers[miner].gameObject);
                minerPathRenderers.Remove(miner);
            }

            foreach (Miner miner in currentMiners)
            {
                if (!minerPathRenderers.TryGetValue(miner, out LineRenderer lineRenderer))
                {
                    GameObject lineObject = new("MinerPath" + miner.Id);
                    lineObject.transform.SetParent(transform);
                    lineRenderer = lineObject.AddComponent<LineRenderer>();
                    lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // Simple default material
                    lineRenderer.startWidth = lineWidth;
                    lineRenderer.endWidth = lineWidth;
                    lineRenderer.startColor = minerColour;
                    lineRenderer.endColor = minerColour;
                    lineRenderer.useWorldSpace = true;
                    minerPathRenderers.Add(miner, lineRenderer);
                }

                List<Vector3> pathPoints = BuildWrappedPath(miner.Path);
                lineRenderer.positionCount = pathPoints.Count;
                if (pathPoints.Count > 0)
                    lineRenderer.SetPositions(pathPoints.ToArray());
            }
        }

        private List<Vector3> BuildWrappedPath(List<System.Numerics.Vector3> rawPath)
        {
            List<Vector3> result = new();
            if (rawPath == null || rawPath.Count == 0)
                return result;

            bool circ = engineManager != null && engineManager.Graph != null && engineManager.Graph.IsCircumnavigable();
            float worldWidth = 0f, worldHeight = 0f;

            if (circ)
            {
                Coordinate size = engineManager.Graph.GetSize();
                worldWidth = size.X * engineManager.Graph.GetNodeDistance();
                worldHeight = size.Y * engineManager.Graph.GetNodeDistance();
            }

            // Start with first point as baseline (in world space)
            System.Numerics.Vector3 prevRaw = rawPath[0];
            System.Numerics.Vector3 accum = prevRaw;
            result.Add(new Vector3(accum.X + offset.x, accum.Y + offset.y, accum.Z + offset.z));

            for (int i = 1; i < rawPath.Count; i++)
            {
                System.Numerics.Vector3 current = rawPath[i];
                System.Numerics.Vector3 delta = current - prevRaw;

                if (circ)
                {
                    if (worldWidth > 0f)
                    {
                        if (MathF.Abs(delta.X) > worldWidth / 2f)
                            delta.X -= MathF.Sign(delta.X) * worldWidth;
                    }
                    if (worldHeight > 0f)
                    {
                        if (MathF.Abs(delta.Z) > worldHeight / 2f)
                            delta.Z -= MathF.Sign(delta.Z) * worldHeight;
                    }
                }

                accum += delta;
                result.Add(new Vector3(accum.X + offset.x, accum.Y + offset.y, accum.Z + offset.z));
                prevRaw = current;
            }

            return result;
        }
    }
}