using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MouseTeleporter : MonoBehaviour, IMotionModel {

    void IMotionModel.SetWaypoints(List<Vector3> w) {}

    void IMotionModel.MoveOrder(Vector3 v) {
        transform.position = v;
    }
}
