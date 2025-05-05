using UnityEngine;

public class InverseKinematics : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Transform pole;

    [SerializeField] Transform firstBone;
    [SerializeField] Vector3 firstBoneEulerAngleOffset;
    [SerializeField] Transform secondBone;
    [SerializeField] Vector3 secondBoneEulerAngleOffset;
    [SerializeField] Transform thirdBone;
    [SerializeField] Vector3 thirdBoneEulerAngleOffset;
    [SerializeField] bool alignThirdBoneWithTargetRotation = true;

    void OnEnable()
    {
        if (
            firstBone == null ||
            secondBone == null ||
            thirdBone == null ||
            pole == null ||
            target == null
        )
        {
            Debug.LogError("IK bones not initialized", this);
            enabled = false;
            return;
        }
    }

    void LateUpdate()
    {
        Vector3 towardPole = pole.position - firstBone.position;
        Vector3 towardTarget = target.position - firstBone.position;

        float rootBoneLength = Vector3.Distance(firstBone.position, secondBone.position);
        float secondBoneLength = Vector3.Distance(secondBone.position, thirdBone.position);
        float totalChainLength = rootBoneLength + secondBoneLength;

        // Align root with target
        firstBone.rotation = Quaternion.LookRotation(towardTarget, towardPole);
        firstBone.localRotation *= Quaternion.Euler(firstBoneEulerAngleOffset);

        Vector3 towardSecondBone = secondBone.position - firstBone.position;

        var targetDistance = Vector3.Distance(firstBone.position, target.position);

        targetDistance = Mathf.Min(targetDistance, totalChainLength * 0.9999f);

        var adjacent =
            (
                (rootBoneLength * rootBoneLength) +
                (targetDistance * targetDistance) -
                (secondBoneLength * secondBoneLength)
            ) / (2 * targetDistance * rootBoneLength);
        var angle = Mathf.Acos(adjacent) * Mathf.Rad2Deg;

        Vector3 cross = Vector3.Cross(towardPole, towardSecondBone);

        if (!float.IsNaN(angle))
        {
            firstBone.RotateAround(firstBone.position, cross, -angle);
        }

        var secondBoneTargetRotation = Quaternion.LookRotation(target.position - secondBone.position, cross);
        secondBoneTargetRotation *= Quaternion.Euler(secondBoneEulerAngleOffset);
        secondBone.rotation = secondBoneTargetRotation;

        if (alignThirdBoneWithTargetRotation)
        {
            thirdBone.rotation = target.rotation;
            thirdBone.localRotation *= Quaternion.Euler(thirdBoneEulerAngleOffset);
        }
    }
}
