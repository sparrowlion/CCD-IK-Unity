using System.Collections.Generic;
using UnityEngine;

public class IKChainCCD : MonoBehaviour
{
    [Header("关节数量")]
    public int jointCount = 4;

    [Header("每节长度")]
    public float segmentLength = 1f;

    [Header("迭代次数")]
    public int iterations = 20;

    [Header("根节点位置")]
    public Vector3 rootPosition = new Vector3(0f, 0f, 0f);

    private List<GameObject> joints = new List<GameObject>();
    private List<Vector3> jointPositions = new List<Vector3>();
    private GameObject endEffector;

    void Start()
    {
        BuildChain();
    }

    void BuildChain()
    {
        foreach (var j in joints) Destroy(j);
        joints.Clear();
        jointPositions.Clear();

        Vector3 currentPos = rootPosition;

        for (int i = 0; i < jointCount; i++)
        {
            GameObject joint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            joint.name = "Joint_" + i;
            joint.transform.localScale = Vector3.one * 0.3f;
            joint.transform.position = currentPos;

            joints.Add(joint);
            jointPositions.Add(currentPos);

            currentPos += new Vector3(segmentLength, 0f, 0f);
        }

        endEffector = joints[joints.Count - 1];
        endEffector.transform.localScale = Vector3.one * 0.45f;
        endEffector.AddComponent<DragEndEffector>();
    }

    void LateUpdate()
    {
        if (endEffector == null || joints.Count < 2) return;

        // 同步末端位置
        jointPositions[jointPositions.Count - 1] = endEffector.transform.position;

        CCDSolve(endEffector.transform.position);

        // 把计算结果写回关节
        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].transform.position = jointPositions[i];
        }

        // 让每个关节指向它的下一个关节
        for (int i = 0; i < joints.Count - 1; i++)
        {
            Vector3 dir = jointPositions[i + 1] - jointPositions[i];
            if (dir.sqrMagnitude > 0.0001f)
            {
                joints[i].transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    void CCDSolve(Vector3 targetPos)
    {
        for (int it = 0; it < iterations; it++)
        {
            for (int i = joints.Count - 2; i >= 0; i--)
            {
                Vector3 jointPos = jointPositions[i];
                Vector3 tipPos = jointPositions[jointPositions.Count - 1];
                Vector3 target = targetPos;

                Vector3 dirToTip = tipPos - jointPos;
                Vector3 dirToTarget = target - jointPos;

                float angle = Vector3.SignedAngle(dirToTip, dirToTarget, Vector3.up);
                angle *= Mathf.Deg2Rad;

                Vector3 radius = tipPos - jointPos;
                Vector3 newTip = RotateAround(radius, Vector3.up, angle) + jointPos;

                jointPositions[jointPositions.Count - 1] = newTip;

                for (int k = joints.Count - 2; k > i; k--)
                {
                    Vector3 from = jointPositions[k];
                    Vector3 to = jointPositions[k + 1];
                    float len = segmentLength;
                    Vector3 dir = (to - from).normalized;
                    jointPositions[k] = to - dir * len;
                }
            }
        }
    }

    Vector3 RotateAround(Vector3 v, Vector3 axis, float angle)
    {
        return Quaternion.AngleAxis(angle * Mathf.Rad2Deg, axis) * v;
    }

    void OnDrawGizmos()
    {
        if (jointPositions.Count < 2) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < jointPositions.Count - 1; i++)
        {
            Gizmos.DrawLine(jointPositions[i], jointPositions[i + 1]);
        }

        if (endEffector != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endEffector.transform.position, 0.4f);
        }
    }
}
