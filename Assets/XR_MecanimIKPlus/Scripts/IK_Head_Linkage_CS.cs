using System.Collections;
using UnityEngine;

namespace MecanimIKPlus
{
	
	public class IK_Head_Linkage_CS : MonoBehaviour
	{

		public Transform eyeTransform;
		public Transform headTransform;
		public Transform neckTransform;
		public bool switchAxisXZ = true;

		void LateUpdate ()
		{
			Rotate ();
		}

		void Rotate ()
		{
			Vector3 headAng = headTransform.eulerAngles;
			Vector3 neckAng = neckTransform.eulerAngles;
			float ang = Mathf.DeltaAngle (360.0f, eyeTransform.eulerAngles.z);
			if (switchAxisXZ) {
				headAng.x = ang;
				neckAng.x = ang * 0.5f;
			} else {
				headAng.z = ang;
				neckAng.z = ang * 0.5f;
			}
			headTransform.eulerAngles = headAng;
			neckTransform.eulerAngles = neckAng;
		}

	}

}
