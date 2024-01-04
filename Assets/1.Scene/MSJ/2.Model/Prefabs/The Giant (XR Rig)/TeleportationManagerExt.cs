using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TeleportationManagerExt : TeleportationManager
{
    public Dictionary<TeleportationAnchor, int> anchorToIndex = new Dictionary<TeleportationAnchor, int>();

    [Header("Extension")]
    public int currentAnchorIndex = -1;
    [field: SerializeField] public TeleportationAnchor CurrentAnchor { get; private set; }
    [field:SerializeField] public XRRayInteractor CurrentTeleportInteractor { get; private set; }

    [Header("Preset")]
    public XRRayInteractor leftHandTeleportInteractor;
    public XRRayInteractor rightHandTeleportInteractor;

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
        CurrentAnchor = anchor;
    }

    public void SetCurrentTeleportInteractor(TeleportationAnchor anchor)
    {
        List<IXRHoverInteractor> interactors = anchor.interactorsHovering;
        foreach (var interactor in interactors)
        {
            var rayInteractor = interactor as XRRayInteractor;
            if (rayInteractor == null) continue;
            
            if (rayInteractor == leftHandTeleportInteractor)
            {
                CurrentTeleportInteractor = leftHandTeleportInteractor;
                rightHandTeleportInteractor.enabled = false;
            }

            if (rayInteractor == rightHandTeleportInteractor)
            {
                CurrentTeleportInteractor = rightHandTeleportInteractor;
                leftHandTeleportInteractor.enabled = false;
            }
        }
    }

    public void ResetCurrentTeleportInteractor()
    {
        CurrentTeleportInteractor = null;

        leftHandTeleportInteractor.enabled = true;
        rightHandTeleportInteractor.enabled = true;
    }

    public Vector3 GetSecondaryAnchorPosition()
    {
        return CurrentAnchor.gameObject.GetComponent<TeleportationTrajectoryRenderer>().secondaryControlPoint.position;
    }
}
