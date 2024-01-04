using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Mirror;

public class TeleportationTrajectoryRenderer : NetworkBehaviour
{
    [Range(0, 200)] public int segmentCount = 50;

    public Gradient defaultColor;
    public Gradient highlightColor;

    public Transform secondaryControlPoint;

    TeleportationManagerExt teleportManager;

    LineRenderer lineRenderer;
    TeleportationAnchor teleportAnchor;
    IEnumerator currentRendering;

    [SyncVar(hook = nameof(OnHighlightChanged))]
    bool isHighlighting = false;

    public void OnHighlightChanged(bool _, bool newFlag) { }

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
        teleportManager.ChangePivotPoint(teleportAnchor);
    }

    public void OnHoverEntered() => ShowTrajectory();
    public void OnHoverExited() => HideTrajectory();
    public void OnSelectEntered() => isHighlighting = true;
    public void OnSelectExited() => isHighlighting = false;

    private void ShowTrajectory()
    {
        // Check relative Y position of hand position compared with head...

        currentRendering = UpdateTrajectory();
        StartCoroutine(currentRendering);

        teleportManager.SetCurrentTeleportInteractor(teleportAnchor);
    }
    private void HideTrajectory()
    {
        if (currentRendering != null) StopCoroutine(currentRendering);

        currentRendering = null;
        CmdResetLineRenderer();

        teleportManager.ResetCurrentTeleportInteractor();
    }

    [Command(requiresAuthority = false)]
    private void CmdResetLineRenderer()
    {
        RpcResetLineRenderer();
    }

    [ClientRpc]
    public void RpcResetLineRenderer()
    {
        lineRenderer.positionCount = 0;
    }

    private IEnumerator UpdateTrajectory()
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
        if (teleportManager.CurrentAnchor == null) return;

        // Set control points (p1/p2/p3/p4)
        //Vector3 p1 = teleportManager.CurrentAnchor.transform.position;
        //Vector3 p2 = teleportManager.GetSecondaryAnchorPosition();
        Vector3 p1 = teleportManager.CurrentTeleportInteractor.transform.position;
        Vector3 p2 = teleportManager.CurrentTeleportInteractor.transform.position;
        Vector3 p3 = secondaryControlPoint.position;
        Vector3 p4 = transform.position;

        CmdDrawBezierCurve(isHighlighting, p1, p2, p3, p4);
    }

    [Command(requiresAuthority = false)]
    public void CmdDrawBezierCurve(bool isHighlighting, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        RpcDrawBezierCurve(isHighlighting, p1, p2, p3, p4);
    }

    [ClientRpc]
    public void RpcDrawBezierCurve(bool isHighlighting, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        DrawBezierCurve(isHighlighting, p1, p2, p3, p4);
    }

    private void DrawBezierCurve(bool isHighlighting, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
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
