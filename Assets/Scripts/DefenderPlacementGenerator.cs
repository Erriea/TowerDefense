using System.Collections.Generic;
using UnityEngine;

public class DefenderPlacementGenerator : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;

    [SerializeField] private int waypointSampleInterval = 15;
    [SerializeField] private float offsetDistance = 6f;
    [SerializeField] private float towerExclusionRadius = 15f;
    [SerializeField] private float minDistanceFromPath = 5f;
    [SerializeField] private float minSpotSpacing = 5f;
    
    [SerializeField] private GameObject golemPrefab;

    [SerializeField] private GameObject placementMarkerPrefab;
    
    [SerializeField] private float markerHeightOffset = 0.1f;

    private List<Vector2> candidateGridPoints = new List<Vector2>();
    private List<PlacementSpot> placementSpots = new List<PlacementSpot>();
    private List<GameObject> activeMarkers = new List<GameObject>();
    private bool isInPlacementMode = false;
    private GameObject previewMarker;
    private PlacementSpot currentPreviewSpot;

    private class PlacementSpot
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public bool IsOccupied;
    }
    
    public IReadOnlyList<Vector2> CandidateGridPoints => candidateGridPoints;
    
    
    
    public void GenerateCandidateGridPoints()
    {
        candidateGridPoints.Clear();

        List<Vector2> candidates = new List<Vector2>();

        foreach (var path in mapGenerator.Paths)
        {
            for (int i = 0; i < path.Count; i += waypointSampleInterval)
            {
                candidates.Add(GetOffsetGridPoint(path, i, 1f));
                candidates.Add(GetOffsetGridPoint(path, i, -1f));
            }
        }

        Vector2 mapCenter = new Vector2(mapGenerator.Width / 2f, mapGenerator.Height / 2f);

        foreach (var candidate in candidates)
        {
            if (!IsWithinMapBounds(candidate))
                continue;

            if (Vector2.Distance(candidate, mapCenter) < towerExclusionRadius)
                continue;

            if (IsTooCloseToAnyPath(candidate))
                continue;

            if (IsTooCloseToAcceptedSpot(candidate, candidateGridPoints))
                continue;

            candidateGridPoints.Add(candidate);
        }
    }

    public void ResolveWorldPositions()
    {
        placementSpots.Clear();

        foreach (var gridPoint in candidateGridPoints)
        {
            float worldX = gridPoint.x * mapGenerator.CellSize;
            float worldZ = gridPoint.y * mapGenerator.CellSize;
            float worldY = mapGenerator.GetTerrainHeight(worldX, worldZ);

            Vector3 localPos = new Vector3(worldX, worldY, worldZ);
            Vector3 worldPos = mapGenerator.transform.TransformPoint(localPos);

            placementSpots.Add(new PlacementSpot { Position = worldPos, Rotation = mapGenerator.transform.rotation });
        }
    }

    private Vector2 GetOffsetGridPoint(IReadOnlyList<Vector2Int> path, int index, float side)
    {
        int prevIndex = Mathf.Max(index - 1, 0);
        int nextIndex = Mathf.Min(index + 1, path.Count - 1);

        Vector2 direction = ((Vector2)path[nextIndex] - (Vector2)path[prevIndex]).normalized;
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);

        Vector2 pathPoint = path[index];
        return pathPoint + perpendicular * side * offsetDistance;
    }

    private bool IsWithinMapBounds(Vector2 gridPoint)
    {
        return gridPoint.x >= 0 && gridPoint.x < mapGenerator.Width
            && gridPoint.y >= 0 && gridPoint.y < mapGenerator.Height;
    }

    private bool IsTooCloseToAnyPath(Vector2 gridPoint)
    {
        foreach (var path in mapGenerator.Paths)
        {
            foreach (var point in path)
            {
                if (Vector2.Distance(gridPoint, point) < minDistanceFromPath)
                    return true;
            }
        }
        return false;
    }

    private bool IsTooCloseToAcceptedSpot(Vector2 gridPoint, List<Vector2> acceptedGridPoints)
    {
        foreach (var accepted in acceptedGridPoints)
        {
            if (Vector2.Distance(gridPoint, accepted) < minSpotSpacing)
                return true;
        }
        return false;
    }

    public void ShowPlacementMarkers()
    {
        HidePlacementMarkers();

        foreach (var spot in placementSpots)
        {
            if (spot.IsOccupied)
                continue;

            Vector3 markerPosition = spot.Position + Vector3.up * markerHeightOffset;
            GameObject marker = Instantiate(placementMarkerPrefab, markerPosition, spot.Rotation);
            activeMarkers.Add(marker);
        }
    }

    public void HidePlacementMarkers()
    {
        foreach (var marker in activeMarkers)
        {
            Destroy(marker);
        }
        activeMarkers.Clear();
    }

    private void OnDrawGizmos()
    {
        if (placementSpots == null) return;

        Gizmos.color = Color.cyan;

        foreach (var spot in placementSpots)
        {
            Gizmos.DrawSphere(spot.Position, 0.5f);
        }
    }
    
    public void BeginPlacementMode()
    {
        isInPlacementMode = true;
    }

    public void EndPlacementMode()
    {
        isInPlacementMode = false;

        if (previewMarker != null)
        {
            Destroy(previewMarker);
            previewMarker = null;
        }

        currentPreviewSpot = null;
    }

    private void Update()
    {
        if (!isInPlacementMode)
            return;

        UpdatePreview();

        if (Input.GetMouseButtonDown(0) && currentPreviewSpot != null)
        {
            ConfirmPlacement();
        }
    }

    private void UpdatePreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        PlacementSpot nearestSpot = FindNearestUnoccupiedSpot(hit.point);

        if (nearestSpot == null)
            return;

        currentPreviewSpot = nearestSpot;

        if (previewMarker == null)
        {
            previewMarker = Instantiate(placementMarkerPrefab);
        }

        previewMarker.transform.position = nearestSpot.Position + Vector3.up * markerHeightOffset;
        previewMarker.transform.rotation = nearestSpot.Rotation;
    }

    private PlacementSpot FindNearestUnoccupiedSpot(Vector3 worldPoint)
    {
        PlacementSpot nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var spot in placementSpots)
        {
            if (spot.IsOccupied)
                continue;

            float distance = Vector3.Distance(spot.Position, worldPoint);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = spot;
            }
        }

        return nearest;
    }

    private void ConfirmPlacement()
    {
        currentPreviewSpot.IsOccupied = true;
        
        if (previewMarker != null)
        {
            Destroy(previewMarker);
            previewMarker = null;
        }

        Instantiate(golemPrefab, currentPreviewSpot.Position, currentPreviewSpot.Rotation);

        currentPreviewSpot = null;
        isInPlacementMode = false;
    }
}
    
    /*
    public void GeneratePlacementSpots()
    {
        placementSpots.Clear();

        foreach (var path in mapGenerator.Paths)
        {
            for (int i = 0; i < path.Count; i += waypointSampleInterval)
            {
                placementSpots.Add(GetOffsetSpot(path, i, 1f));  //left side path
                placementSpots.Add(GetOffsetSpot(path, i, -1f)); // righ side path
            }
        }
    }

    //figure out which way apths is heading w/ waypoints
    private Vector3 GetOffsetSpot(IReadOnlyList<Vector2Int> path, int index, float side)
    {
        int prevIndex = Mathf.Max(index - 1, 0);
        int nextIndex = Mathf.Min(index + 1, path.Count - 1);
        
        Vector2 direction = ((Vector2)path[nextIndex] - (Vector2)path[prevIndex]).normalized;
        
        Vector2 perpendicular = new Vector2(-direction.y, direction.x);
        
        Vector2 pathPoint = path[index];
        Vector2 offsetPoint = pathPoint + perpendicular * side * offsetDistance;

        float worldX = offsetPoint.x * mapGenerator.CellSize;
        float worldZ = offsetPoint.y * mapGenerator.CellSize;
        float worldY = mapGenerator.GetTerrainHeight(worldX, worldZ);
        
        Vector3 localPos = new Vector3(worldX, worldY, worldZ);
        return mapGenerator.transform.TransformPoint(localPos);
    }
    */
    
 
    

