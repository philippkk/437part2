using UnityEditor;
using UnityEngine;

public class FaskIKFabric : MonoBehaviour
{
    public int ChainLength = 2;

    public Transform Target;
    public Transform Pole;

    [Header("Solver Parameters")]
    public int Iterations = 10;

    public float Delta = 0.001f;


    [Range(0, 1)]
    public float SnapBackStrength = 1f;


    protected float[] BonesLength;
    protected float CompleteLength;
    protected Transform[] Bones;
    protected Vector3[] Positions;
    protected Vector3[] StartDirectionSucc;
    protected Quaternion[] StartRotationBone;
    protected Quaternion StartRotationTarget;
    protected Transform Root;


    void Awake()
    {
        Init();
    }

    void Init()
    {
        Bones = new Transform[ChainLength + 1];
        Positions = new Vector3[ChainLength + 1];
        BonesLength = new float[ChainLength];
        StartDirectionSucc = new Vector3[ChainLength + 1];
        StartRotationBone = new Quaternion[ChainLength + 1];

        //find root
        Root = transform;
        for (var i = 0; i <= ChainLength; i++)
        {
            if (Root == null)
                throw new UnityException("The chain value is longer than the ancestor chain!");
            Root = Root.parent;
        }

        //init target
        if (Target == null)
        {
            Target = new GameObject(gameObject.name + " Target").transform;
            SetPositionRootSpace(Target, GetPositionRootSpace(transform));
        }
        StartRotationTarget = GetRotationRootSpace(Target);


        //init data
        var current = transform;
        CompleteLength = 0;
        for (var i = Bones.Length - 1; i >= 0; i--)
        {
            Bones[i] = current;
            StartRotationBone[i] = GetRotationRootSpace(current);

            if (i == Bones.Length - 1)
            {
                //leaf
                StartDirectionSucc[i] = GetPositionRootSpace(Target) - GetPositionRootSpace(current);
            }
            else
            {
                //mid bone
                StartDirectionSucc[i] = GetPositionRootSpace(Bones[i + 1]) - GetPositionRootSpace(current);
                BonesLength[i] = StartDirectionSucc[i].magnitude;
                CompleteLength += BonesLength[i];
            }

            current = current.parent;
        }



    }

    // Update is called once per frame
    void LateUpdate()
    {
        ResolveIK();
    }

    private void ResolveIK()
    {
        if (Target == null)
            return;

        if (BonesLength.Length != ChainLength)
            Init();


        //get position
        for (int i = 0; i < Bones.Length; i++)
            Positions[i] = GetPositionRootSpace(Bones[i]);

        var targetPosition = GetPositionRootSpace(Target);
        var targetRotation = GetRotationRootSpace(Target);

        //1st is possible to reach?
        if ((targetPosition - GetPositionRootSpace(Bones[0])).sqrMagnitude >= CompleteLength * CompleteLength)
        {
            //just strech it
            var direction = (targetPosition - Positions[0]).normalized;
            //set everything after root
            for (int i = 1; i < Positions.Length; i++)
                Positions[i] = Positions[i - 1] + direction * BonesLength[i - 1];
        }
        else
        {
            for (int i = 0; i < Positions.Length - 1; i++)
                Positions[i + 1] = Vector3.Lerp(Positions[i + 1], Positions[i] + StartDirectionSucc[i], SnapBackStrength);

            for (int iteration = 0; iteration < Iterations; iteration++)
            {
                for (int i = Positions.Length - 1; i > 0; i--)
                {
                    if (i == Positions.Length - 1)
                        Positions[i] = targetPosition;
                    else
                        Positions[i] = Positions[i + 1] + (Positions[i] - Positions[i + 1]).normalized * BonesLength[i]; //set in line on distance
                }

                //forward
                for (int i = 1; i < Positions.Length; i++)
                    Positions[i] = Positions[i - 1] + (Positions[i] - Positions[i - 1]).normalized * BonesLength[i - 1];

                if ((Positions[Positions.Length - 1] - targetPosition).sqrMagnitude < Delta * Delta)
                    break;
            }
        }

        //move towards pole
        if (Pole != null)
        {
            var polePosition = GetPositionRootSpace(Pole);
            for (int i = 1; i < Positions.Length - 1; i++)
            {
                var plane = new Plane(Positions[i + 1] - Positions[i - 1], Positions[i - 1]);
                var projectedPole = plane.ClosestPointOnPlane(polePosition);
                var projectedBone = plane.ClosestPointOnPlane(Positions[i]);
                var angle = Vector3.SignedAngle(projectedBone - Positions[i - 1], projectedPole - Positions[i - 1], plane.normal);
                Positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (Positions[i] - Positions[i - 1]) + Positions[i - 1];
            }
        }

        //set position & rotation
        for (int i = 0; i < Positions.Length; i++)
        {
            if (i == Positions.Length - 1)
                SetRotationRootSpace(Bones[i], Quaternion.Inverse(targetRotation) * StartRotationTarget * Quaternion.Inverse(StartRotationBone[i]));
            else
                SetRotationRootSpace(Bones[i], Quaternion.FromToRotation(StartDirectionSucc[i], Positions[i + 1] - Positions[i]) * Quaternion.Inverse(StartRotationBone[i]));
            SetPositionRootSpace(Bones[i], Positions[i]);
        }
    }

    private Vector3 GetPositionRootSpace(Transform current)
    {
        if (Root == null)
            return current.position;
        else
            return Quaternion.Inverse(Root.rotation) * (current.position - Root.position);
    }

    private void SetPositionRootSpace(Transform current, Vector3 position)
    {
        if (Root == null)
            current.position = position;
        else
            current.position = Root.rotation * position + Root.position;
    }

    private Quaternion GetRotationRootSpace(Transform current)
    {
        if (Root == null)
            return current.rotation;
        else
            return Quaternion.Inverse(current.rotation) * Root.rotation;
    }

    private void SetRotationRootSpace(Transform current, Quaternion rotation)
    {
        if (Root == null)
            current.rotation = rotation;
        else
            current.rotation = Root.rotation * rotation;
    }

    void OnDrawGizmos()
    {
        var current = this.transform;
        for (int i = 0; i < ChainLength && current != null && current.parent != null; i++)
        {
            var scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
            Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
            Handles.color = Color.green;
            Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
            current = current.parent;
        }
    }

}
