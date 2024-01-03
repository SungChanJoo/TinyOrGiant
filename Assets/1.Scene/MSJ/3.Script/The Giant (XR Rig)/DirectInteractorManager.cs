using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Mirror;

public class DirectInteractorManager : NetworkBehaviour
{
    GameObject leftHandPresencePhysics;
    GameObject rightHandPresencePhysics;

    public XRDirectInteractor leftHandDirectInteractor;
    public XRDirectInteractor rightHandDirectInteractor;

    public float colliderEnableDelay = .5f;
    Collider[] leftHandColliders;
    Collider[] rightHandColliders;

    private void Start()
    {
        FindHandPresencePhysics();

        leftHandDirectInteractor.selectEntered.AddListener((SelectEnterEventArgs) =>
        {
            DisableLeftHandColliders();
        });
        leftHandDirectInteractor.selectExited.AddListener((SelectExitEventArgs) =>
        {
            Invoke(nameof(EnableLeftHandColliders), colliderEnableDelay);
        });

        rightHandDirectInteractor.selectEntered.AddListener((SelectEnterEventArgs) =>
        {
            DisableRighttHandColliders();
        });
        rightHandDirectInteractor.selectExited.AddListener((SelectExitEventArgs) =>
        {
            Invoke(nameof(EnableRightHandColliders), colliderEnableDelay);
        });
    }

    private void FindHandPresencePhysics()
    {
        StartCoroutine(DelayedFindHandPresencePhysics());
    }

    private IEnumerator DelayedFindHandPresencePhysics()
    {
        yield return new WaitForSeconds(2f);

        leftHandPresencePhysics = GameObject.FindGameObjectWithTag("PhysicsLeftHandPresence");
        rightHandPresencePhysics = GameObject.FindGameObjectWithTag("PhysicsRightHandPresence");

        leftHandColliders = leftHandPresencePhysics.GetComponentsInChildren<Collider>();
        rightHandColliders = rightHandPresencePhysics.GetComponentsInChildren<Collider>();
    }

    private void EnableLeftHandColliders()
    {
        foreach (var handCollider in leftHandColliders)
        {
            handCollider.enabled = true;
        }
    }

    private void DisableLeftHandColliders()
    {
        foreach (var handCollider in leftHandColliders)
        {
            handCollider.enabled = false;
        }
    }

    private void EnableRightHandColliders()
    {
        foreach (var handCollider in rightHandColliders)
        {
            handCollider.enabled = true;
        }
    }

    private void DisableRighttHandColliders()
    {
        foreach (var handCollider in rightHandColliders)
        {
            handCollider.enabled = false;
        }
    }
}
