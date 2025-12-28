using System.Collections.Generic;
using UnityEngine;

public class TrajectoryPredictor : MonoBehaviour
{
    [Header("Line Renderer")]
    public LineRenderer trajectoryLine;
    public float lineWidth = 0.1f;
    public Color lineColor = new Color(0f, 0f, 0f, 0.5f); // Black with 50% opacity

    [Header("Prediction Settings")]
    public int maxPoints = 100; // Maximum points in trajectory
    public float pointSpacing = 0.1f; // Distance between points
    public float maxDistance = 50f; // Maximum prediction distance
    public LayerMask collisionLayers; // Layers to collide with

    [Header("Ball Settings")]
    public float ballRadius = 0.25f; // Radius of the ball for collision detection
    public float ballSpeed = 10f; // Speed of the ball (should match PlayerCharacter.ballSpeed)
    
    private PlayerCharacter playerCharacter;

    private void Start()
    {
        // Auto-find PlayerCharacter to get ballSpeed
        if (playerCharacter == null)
        {
            playerCharacter = GetComponent<PlayerCharacter>();
            if (playerCharacter == null)
            {
                playerCharacter = FindObjectOfType<PlayerCharacter>();
            }
        }

        // Create LineRenderer if not assigned
        if (trajectoryLine == null)
        {
            GameObject lineObj = new GameObject("TrajectoryLine");
            lineObj.transform.SetParent(transform);
            trajectoryLine = lineObj.AddComponent<LineRenderer>();
        }

        // Setup LineRenderer
        trajectoryLine.material = CreateDotLineMaterial();
        trajectoryLine.startWidth = lineWidth;
        trajectoryLine.endWidth = lineWidth;
        trajectoryLine.startColor = lineColor;
        trajectoryLine.endColor = lineColor;
        trajectoryLine.useWorldSpace = true;
        trajectoryLine.positionCount = 0;
        trajectoryLine.enabled = false;

        // Set texture mode for dot line effect
        trajectoryLine.textureMode = LineTextureMode.Tile;
    }

    public void SetBallSpeed(float speed)
    {
        ballSpeed = speed;
    }

    private Material CreateDotLineMaterial()
    {
        // Create a material with dot pattern for dotted line
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = lineColor;
        
        // Create a dot texture pattern
        Texture2D dotTexture = new Texture2D(8, 1);
        Color[] colors = new Color[8];

        for (int i = 0; i < 8; i++)
        {
            colors[i] = (i % 2 == 0) ? Color.white : Color.clear;
        }
        dotTexture.SetPixels(colors);
        dotTexture.wrapMode = TextureWrapMode.Repeat;
        dotTexture.filterMode = FilterMode.Point;
        dotTexture.Apply();
        mat.mainTexture = dotTexture;
        mat.mainTextureScale = new Vector2(20f, 1f); // Adjust spacing for visible dots
        
        return mat;
    }

    public void ShowTrajectory(Vector2 startPos, Vector2 direction)
    {
        if (trajectoryLine == null) return;

        List<Vector3> points = CalculateTrajectory(startPos, direction);
        
        if (points.Count > 0)
        {
            trajectoryLine.positionCount = points.Count;
            trajectoryLine.SetPositions(points.ToArray());
            trajectoryLine.enabled = true;
        }
        else
        {
            trajectoryLine.enabled = false;
        }
    }

    public void HideTrajectory()
    {
        if (trajectoryLine != null)
        {
            trajectoryLine.enabled = false;
        }
    }

    private List<Vector3> CalculateTrajectory(Vector2 startPos, Vector2 direction)
    {
        List<Vector3> points = new List<Vector3>();
        points.Add(startPos);

        Vector2 currentPos = startPos;
        Vector2 currentVelocity = direction.normalized * ballSpeed;
        float totalDistance = 0f;
        bool hasBounced = false; // Only predict one bounce
        float stepSize = pointSpacing; // Step size for checking collisions

        while (!hasBounced && totalDistance < maxDistance && points.Count < maxPoints)
        {
            // Check for non-trigger collisions (walls, solid enemies)
            // CircleCast ignores triggers by default, but we need to explicitly check
            RaycastHit2D hit = Physics2D.CircleCast(
                currentPos,
                ballRadius,
                currentVelocity.normalized,
                maxDistance - totalDistance,
                collisionLayers
            );

            // Find the first non-trigger collision
            float distanceToSolidCollision = float.MaxValue;
            RaycastHit2D solidHit = new RaycastHit2D();
            
            if (hit.collider != null && !hit.collider.isTrigger)
            {
                distanceToSolidCollision = hit.distance;
                solidHit = hit;
            }
            else
            {
                // No solid collision found, check if there are any solid objects further along
                // by doing multiple casts to skip triggers
                float searchDistance = 0f;
                float maxSearchDistance = maxDistance - totalDistance;
                
                while (searchDistance < maxSearchDistance)
                {
                    Vector2 searchPos = currentPos + currentVelocity.normalized * searchDistance;
                    RaycastHit2D searchHit = Physics2D.CircleCast(
                        searchPos,
                        ballRadius,
                        currentVelocity.normalized,
                        maxSearchDistance - searchDistance,
                        collisionLayers
                    );
                    
                    if (searchHit.collider != null && !searchHit.collider.isTrigger)
                    {
                        distanceToSolidCollision = searchDistance + searchHit.distance;
                        solidHit = searchHit;
                        break;
                    }
                    
                    // If we hit a trigger, skip past it
                    if (searchHit.collider != null && searchHit.collider.isTrigger)
                    {
                        searchDistance += searchHit.distance + ballRadius * 2f;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (distanceToSolidCollision < float.MaxValue && solidHit.collider != null)
            {
                // Hit a solid object (non-trigger), will bounce
                float distanceToHit = distanceToSolidCollision;
                int segments = Mathf.Max(1, Mathf.CeilToInt(distanceToHit / pointSpacing));
                
                // Add points along the path to the collision (passing through any triggers)
                for (int i = 1; i <= segments; i++)
                {
                    float t = (float)i / segments;
                    Vector2 point = currentPos + currentVelocity.normalized * (distanceToHit * t);
                    points.Add(point);
                    
                    if (points.Count >= maxPoints)
                    {
                        return points;
                    }
                }

                // Update position to collision point (move slightly away from surface to avoid getting stuck)
                currentPos = solidHit.point + solidHit.normal * (ballRadius + 0.01f);
                totalDistance += distanceToHit;

                // Calculate reflection (bounce) - ensure direction is correct
                Vector2 normal = solidHit.normal;
                currentVelocity = Vector2.Reflect(currentVelocity, normal);
                
                // Ensure velocity maintains the same speed
                currentVelocity = currentVelocity.normalized * ballSpeed;

                hasBounced = true; // Only one bounce
            }
            else
            {
                // No solid collision, add points along the remaining path (passing through triggers)
                float remainingDistance = maxDistance - totalDistance;
                int segments = Mathf.Max(1, Mathf.CeilToInt(remainingDistance / pointSpacing));
                
                for (int i = 1; i <= segments; i++)
                {
                    float t = (float)i / segments;
                    Vector2 point = currentPos + currentVelocity.normalized * (remainingDistance * t);
                    points.Add(point);
                    
                    if (points.Count >= maxPoints)
                    {
                        break;
                    }
                }
                
                break;
            }
        }

        // If we bounced, add points for the post-bounce trajectory
        if (hasBounced)
        {
            float remainingDistance = maxDistance - totalDistance;
            int segments = Mathf.Max(1, Mathf.CeilToInt(remainingDistance / pointSpacing));
            
            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                Vector2 point = currentPos + currentVelocity.normalized * (remainingDistance * t);
                points.Add(point);
                
                if (points.Count >= maxPoints)
                {
                    break;
                }
            }
        }

        return points;
    }
}

