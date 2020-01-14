using System.Collections;
using UnityEngine;
using UnityEngine.XR;

namespace MecanimIKPlus
{
	
	public class XR_Hand_Tracking_CS : MonoBehaviour
	{

		public bool isLeft;
		public float offsetAngle = 0.0f;

		Transform thisTransform;
		XRNode node;
		Vector3 targetPos;

		void Start ()
		{
			thisTransform = transform;
			if (isLeft) {
				node = XRNode.LeftHand;
			} else {
				node = XRNode.RightHand;
				offsetAngle = -offsetAngle;
			}
		}

		void Update ()
		{
			thisTransform.localPosition = InputTracking.GetLocalPosition (node);
			thisTransform.localRotation = InputTracking.GetLocalRotation (node) * Quaternion.Euler (0.0f, 0.0f, offsetAngle);
		}
	}

}
