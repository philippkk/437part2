using System.Collections;
using UnityEngine;

public class legSteppa : MonoBehaviour
{
    [SerializeField] Transform homeTransform;
    [SerializeField] float wantStepAtDistance;
    [SerializeField] float moveDuration;
    [SerializeField] float stepOvershootFraction;

    public bool Moving;

    public void TryMove()
    {
        if (Moving) return;

        float distFromHome = Vector3.Distance(transform.position, homeTransform.position);

        if (distFromHome > wantStepAtDistance)
        {
            StartCoroutine(Move());
        }
    }

    IEnumerator Move()
    {
        Moving = true;

        Vector3 startPoint = transform.position;
        Quaternion startRot = transform.rotation;

        Quaternion endRot = homeTransform.rotation;

        Vector3 towardHome = homeTransform.position - transform.position;
        float overshootDistance = wantStepAtDistance * stepOvershootFraction;
        Vector3 overshootVector = towardHome * overshootDistance;
        overshootVector = Vector3.ProjectOnPlane(overshootVector, Vector3.up);

        // Apply the overshoot
        Vector3 endPoint = homeTransform.position + overshootVector;

        Vector3 centerPoint = (startPoint + endPoint) / 2;
        centerPoint += homeTransform.up * Vector3.Distance(startPoint, endPoint) / 2f;

        float timeElapsed = 0;
        do
        {
            timeElapsed += Time.deltaTime;
            float normalizedTime = timeElapsed / moveDuration;

            normalizedTime = Easing.EaseInOutCubic(normalizedTime);

            // step arc logic
            transform.position =
                Vector3.Lerp(
                    Vector3.Lerp(startPoint, centerPoint, normalizedTime),
                    Vector3.Lerp(centerPoint, endPoint, normalizedTime),
                    normalizedTime
                );

            transform.rotation = Quaternion.Slerp(startRot, endRot, normalizedTime);

            yield return null;
        }
        while (timeElapsed < moveDuration);

        Moving = false;
    }
}