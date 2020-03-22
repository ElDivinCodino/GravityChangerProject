using UnityEngine;

public class MoveCamera : MonoBehaviour {

    public Transform head;

    float offset = 0.4f;

    void Update() {
        transform.position = head.position + transform.forward * offset;
        transform.rotation = head.rotation;
    }
}