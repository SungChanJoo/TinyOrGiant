using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportationTrajectoryRenderer : MonoBehaviour
{
    [Range(0, 200)] public int segmentCount = 50;

    public Gradient defaultColor;
    public Gradient highlightColor;

    public Transform secondaryControlPoint;

    TeleportationManagerExt teleportManager;

    LineRenderer lineRenderer;
    TeleportationAnchor teleportAnchor;
    IEnumerator currentRendering;
    bool isHighlighting = false;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        teleportAnchor = GetComponent<TeleportationAnchor>();
    }

    private IEnumerator Start()
    {
        // Find teleportManager
        while (teleportManager == null)
        {
            teleportManager = FindObjectOfType<TeleportationManagerExt>();
            yield return null;
        }
    }

    public void OnTeleporting()
    {
        HideTrajectory();

        if (teleportManager == null) teleportManager = FindObjectOfType<TeleportationManagerExt>();
        teleportManager.ChangePivotPoint(teleportAnchor);
    }

    public void OnHoverEntered()
    {
        currentRendering = RenderTrajectory();
        StartCoroutine(currentRendering);
    }

    public void OnHoverExited()
    {
        HideTrajectory();
    }
    
    public void OnSelectEntered()
    {
        isHighlighting = true;
    }

    public void OnSelectExited()
    {
        isHighlighting = false;
    }

    private void HideTrajectory()
    {
        if (currentRendering != null)
        {
            lineRenderer.positionCount = 0;
            StopCoroutine(currentRendering);
        }
        currentRendering = null;
    }

    private IEnumerator RenderTrajectory()
    {
        yield return null;

        while (true)
        {
            DrawTrajectory(isHighlighting);
            yield return null;
        }
    }

    /// <summary>
    /// Draw bezier's curve with 4 control points
    /// </summary>
    /// <param name="isHighlighting"></param>
    private void DrawTrajectory(bool isHighlighting)
    {
        if (teleportManager.currentAnchor == null) return;

        // Set control points (p1/p2/p3/p4)
        Vector3 p1 = teleportManager.currentAnchor.transform.position;
        Vector3 p2 = teleportManager.GetSecondaryAnchorPosition();
        Vector3 p3 = secondaryControlPoint.position;
        Vector3 p4 = transform.position;

        // Draw bezier's curve
        lineRenderer.positionCount = segmentCount + 1;

        lineRenderer.SetPosition(0, GetDrawPoint(p1, p2, p3, p4, 0));

        for (int i = 1; i < segmentCount + 1; i++)
        {
            float value = i / (float)segmentCount;
            var point = GetDrawPoint(p1, p2, p3, p4, value);
            lineRenderer.SetPosition(i, point);
        }
        lineRenderer.colorGradient = isHighlighting ? highlightColor : defaultColor;
    }

    private Vector3 GetDrawPoint(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, float value)
    {
        // Start points (A/B/C)
        Vector3 A = Vector3.Lerp(p1, p2, value);
        Vector3 B = Vector3.Lerp(p2, p3, value);
        Vector3 C = Vector3.Lerp(p3, p4, value);

        // Sub points (E/F)
        Vector3 E = Vector3.Lerp(A, B, value);
        Vector3 F = Vector3.Lerp(B, C, value);

        // Draw point (G)
        Vector3 G = Vector3.Lerp(E, F, value);

        return G;
    }
}
