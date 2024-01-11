using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CannonController : NetworkBehaviour
{
    public Transform rotater;
    public Transform cannon;

    private float startAngle;

    public GameObject target = null;

    private IEnumerator Start()
    {
        startAngle = cannon.eulerAngles.x;

        // Find VRHead game object
        while (target == null)
        {
            target = GameObject.FindGameObjectWithTag("PhysicsHead");
            yield return null;
        }

        //Invoke(nameof(ActivateCannon), 2f);
        //Invoke(nameof(DeactivateCannon), 10f);
    }

    public void ActivateCannon()
    {
        // Start Look Target
        if (_currentLookTarget != null)
        {
            StopCoroutine(_currentLookTarget);
            _currentLookTarget = null;
        }
        _currentLookTarget = LookTarget();
        StartCoroutine(_currentLookTarget);

        // Start Cannon Up
        if (_currentCannonUp != null)
        {
            StopCoroutine(_currentCannonUp);
            _currentCannonUp = null;
        }
        _currentCannonUp = CannonUp();
        StartCoroutine(_currentCannonUp);
    }

    public void DeactivateCannon()
    {
        // Stop Look Target
        if (_currentLookTarget != null)
        {
            StopCoroutine(_currentLookTarget);
            _currentLookTarget = null;
        }

        // Start Cannon Down
        if (_currentCannonDown != null)
        {
            StopCoroutine(_currentCannonDown);
            _currentCannonDown = null;
        }
        _currentCannonDown = CannonDown();
        StartCoroutine(_currentCannonDown);
    }

    [Header("Rotater Look Target")]
    public float rotaterLookTargetDuration = 2f;
    public AnimationCurve rotaterLookTargetOverTime;

    private IEnumerator _currentLookTarget = null;
    private IEnumerator LookTarget()
    {
        float elapsedTime = 0f;
        while (elapsedTime < rotaterLookTargetDuration)
        {
            yield return null;

            elapsedTime += Time.deltaTime;

            var currentForward = rotater.forward.normalized;
            var targetGroundPos = new Vector3(target.transform.position.x, rotater.position.y, target.transform.position.z);
            var targetForward = (targetGroundPos - rotater.transform.position).normalized;

            var progress = rotaterLookTargetOverTime.Evaluate(elapsedTime / rotaterLookTargetDuration);
            var angleDiff = Vector3.Angle(currentForward, targetForward);
            var angleToRotate = Mathf.Lerp(0, rotater.eulerAngles.y + angleDiff, progress);

            rotater.eulerAngles = new Vector3(rotater.eulerAngles.x, angleToRotate, rotater.eulerAngles.z);
        }

        while (true)
        {
            yield return null;

            var targetGroundPos = new Vector3(target.transform.position.x, rotater.position.y, target.transform.position.z);
            rotater.LookAt(targetGroundPos);
        }
    }

    [Header("Cannon Up")]
    public float cannonUpDuration = 2f;
    public float cannonUpMaxAngle = -40f;
    public AnimationCurve cannonUpOverTime;

    private IEnumerator _currentCannonUp = null;
    private IEnumerator CannonUp()
    {
        // Stop Cannon Down
        if (_currentCannonDown != null)
        {
            StopCoroutine(_currentCannonDown);
            _currentCannonDown = null;
        }

        float startValue = startAngle;
        float targetValue = cannonUpMaxAngle;

        float elpasedTime = 0f;
        while (elpasedTime < cannonUpDuration)
        {
            yield return null;

            elpasedTime += Time.deltaTime;

            var progress = cannonUpOverTime.Evaluate(elpasedTime / cannonUpDuration);
            var angleToRotate = Mathf.Lerp(startValue, targetValue, progress);
            cannon.eulerAngles = new Vector3(angleToRotate, cannon.eulerAngles.y, cannon.eulerAngles.z);
        }
    }

    [Header("Cannon Down")]
    public float cannonDownDuration = .5f;
    public AnimationCurve cannonDownOverTime;

    private IEnumerator _currentCannonDown = null;
    private IEnumerator CannonDown()
    {
        // Stop Cannon Up
        if (_currentCannonUp != null)
        {
            StopCoroutine(_currentCannonUp);
            _currentCannonUp = null;
        }

        float startValue = cannon.eulerAngles.x - 360;
        float targetValue = startAngle;

        float elpasedTime = 0f;
        while (elpasedTime < cannonDownDuration)
        {
            yield return null;

            elpasedTime += Time.deltaTime;

            var progress = cannonDownOverTime.Evaluate(elpasedTime / cannonDownDuration);
            var angleToRotate = Mathf.Lerp(startValue, targetValue, progress);
            cannon.eulerAngles = new Vector3(angleToRotate, cannon.eulerAngles.y, cannon.eulerAngles.z);
        }
    }
}
