using UnityEngine;
using UnityEngine.XR;

public class XRInputTracker : MonoBehaviour
{
    public XRNode Node;

    void Update()
    {
        transform.localPosition = InputTracking.GetLocalPosition(Node);
        transform.localRotation = InputTracking.GetLocalRotation(Node);
    }
}
