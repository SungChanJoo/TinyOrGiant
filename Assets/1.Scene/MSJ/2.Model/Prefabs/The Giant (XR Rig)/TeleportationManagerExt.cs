using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportationManagerExt : TeleportationManager
{
    public Dictionary<TeleportationAnchor, int> anchorToIndex = new Dictionary<TeleportationAnchor, int>();

    [Header("Extension")]
    public int currentAnchorIndex = -1;
    public TeleportationAnchor currentAnchor;

    private void Awake()
    {
        var anchors = FindObjectsOfType<TeleportationAnchor>();

        for (int i = 0; i < anchors.Length; i++)
        {
            anchorToIndex.Add(anchors[i], i);
        }
    }

    public void ChangePivotPoint(TeleportationAnchor anchor)
    {
        currentAnchorIndex = anchorToIndex[anchor];
        currentAnchor = anchor;
    }

    public Vector3 GetSecondaryAnchorPosition()
    {
        return currentAnchor.gameObject.GetComponent<TeleportationTrajectoryRenderer>().secondaryControlPoint.position;
    }
}
