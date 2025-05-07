using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class spiderController : MonoBehaviour
{
    [SerializeField] Transform target;

    [Header("Head Tracking")]
    [SerializeField] Transform headBone;
    [SerializeField] float headMaxTurnAngle = 70f;
    [SerializeField] float headTrackingSpeed = 10f;
    
    [Header("Root Motion")]
    [SerializeField] float turnSpeed = 100f;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float turnAcceleration = 5f;
    [SerializeField] float moveAcceleration = 5f;
    [SerializeField] float minDistToTarget = 4.5f;
    [SerializeField] float maxDistToTarget = 6f;
    [SerializeField] float maxAngToTarget = 25f;

    Vector3 currentVelocity;
    float currentAngularVelocity;

    [Header("Legs")]
    [SerializeField] legSteppa frontLeftLegStepper;
    [SerializeField] legSteppa frontRightLegStepper;
    [SerializeField] legSteppa backLeftLegStepper;
    [SerializeField] legSteppa backRightLegStepper;

    void Awake()
    {
        StartCoroutine(LegUpdateCoroutine());
    }

    void LateUpdate()
    {
        RootMotionUpdate();
        HeadTrackingUpdate();
    }

    void HeadTrackingUpdate()
    {
        Quaternion currentLocalRotation = headBone.localRotation;
        headBone.localRotation = Quaternion.identity;

        Vector3 targetWorldLookDir = target.position - headBone.position;
        Vector3 targetLocalLookDir = headBone.InverseTransformDirection(targetWorldLookDir);

        // Apply angle limit
        targetLocalLookDir = Vector3.RotateTowards(
            Vector3.forward,
            targetLocalLookDir,
            Mathf.Deg2Rad * headMaxTurnAngle,
            0 
        );

        Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);

        // Apply smoothing
        headBone.localRotation = Quaternion.Slerp(
            currentLocalRotation,
            targetLocalRotation,
            1 - Mathf.Exp(-headTrackingSpeed * Time.deltaTime)
        );
    }
    void RootMotionUpdate()
    {
        Vector3 towardTarget = target.position - transform.position;
        Vector3 towardTargetProjected = Vector3.ProjectOnPlane(towardTarget, transform.up);
        float angToTarget = Vector3.SignedAngle(transform.forward, towardTargetProjected, transform.up);

        float targetAngularVelocity = 0;

        if (Mathf.Abs(angToTarget) > maxAngToTarget)
        {
            if (angToTarget > 0)
            {
                targetAngularVelocity = turnSpeed;
            }
            // Invert angular speed if target is to our left
            else
            {
                targetAngularVelocity = -turnSpeed;
            }
        }

        currentAngularVelocity = Mathf.Lerp(
            currentAngularVelocity,
            targetAngularVelocity,
            1 - Mathf.Exp(-turnAcceleration * Time.deltaTime)
        );

        transform.Rotate(0, Time.deltaTime * currentAngularVelocity, 0, Space.World);


        Vector3 targetVelocity = Vector3.zero;

        // dont move if facing away from the target, just rotate in place
        if (Mathf.Abs(angToTarget) < 90)
        {
            float distToTarget = Vector3.Distance(transform.position, target.position);

            // if too far away, approach the target
            if (distToTarget > maxDistToTarget)
            {
                targetVelocity = moveSpeed * towardTargetProjected.normalized;
            }
            // if too close, reverse the direction and move away
            else if (distToTarget < minDistToTarget)
            {
                targetVelocity = moveSpeed * -towardTargetProjected.normalized;
            }
        }

        // same smoothing
        currentVelocity = Vector3.Lerp(
            currentVelocity,
            targetVelocity,
            1 - Mathf.Exp(-moveAcceleration * Time.deltaTime)
        );

        transform.position += currentVelocity * Time.deltaTime;
    }

    IEnumerator LegUpdateCoroutine()
    {
        while (true)
        {
            do
            {
                frontLeftLegStepper.TryMove();
                backRightLegStepper.TryMove();
                yield return null;
            } while (backRightLegStepper.Moving || frontLeftLegStepper.Moving);

            do
            {
                frontRightLegStepper.TryMove();
                backLeftLegStepper.TryMove();
                yield return null;
            } while (backLeftLegStepper.Moving || frontRightLegStepper.Moving);
        }
    }
}