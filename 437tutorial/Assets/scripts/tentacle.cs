using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tentacle : MonoBehaviour
{
    public bool preserveLength;
    public int length;
    public LineRenderer lineRend;
    public Vector3[] segmentPoses;
    private Vector3[] segmentV;
    public Transform targetDir;
    public float targetDist;
    public float smoothSpeed = 5f;
    private void Start()
    {
        lineRend.positionCount = length;
        segmentPoses = new Vector3[length];
        segmentV = new Vector3[length];
    }
    private void Update()
    {
        // set start of line to the position of the target
        segmentPoses[0] = targetDir.position;

        // set the rest of the segments to follow the previous segment
        for (int i = 1; i < segmentPoses.Length; i++)
        {
            if (preserveLength)
            {
                Vector3 targetPos = segmentPoses[i - 1] + (segmentPoses[i] - segmentPoses[i - 1]).normalized * targetDist;
                segmentPoses[i] = Vector3.SmoothDamp(
                    segmentPoses[i],
                    targetPos,
                    ref segmentV[i],
                    smoothSpeed);
            }
            else
            {
                segmentPoses[i] = Vector3.SmoothDamp(
                    segmentPoses[i],
                    segmentPoses[i - 1] + targetDir.right * targetDist,
                    ref segmentV[i],
                    smoothSpeed);
            }
        }
        // send data to the line renderer
        lineRend.SetPositions(segmentPoses);
    }
}
