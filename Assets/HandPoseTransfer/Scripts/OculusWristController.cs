// Copyright (c) Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace HandPoseTransfer.OculusQuest
{
    public class OculusWristController : MonoBehaviour
    {
        public Transform _LeftHandTrackingReference;
        public Transform _RightHandTrackingReference;
        public OVRHand _LeftOVRHand;
        public OVRHand _RightOVRHand;

        [Header("Local transform for Controller")]
        [SerializeField] Vector3 _LeftWristPositionForController = new Vector3(0.0f, -0.03f, -0.1f);
        [SerializeField] Vector3 _LeftWristAnglesForController = new Vector3(-90, 90, 0);
        [SerializeField] Vector3 _RightWristPositionForController = new Vector3(0.0f, -0.03f, -0.1f);
        [SerializeField] Vector3 _RightWristAnglesForController = new Vector3(-90, -90, 0);

        [Header("Local transform for Hand Tracking")]
        [SerializeField] Vector3 _LeftWristPositionForHandTracking = new Vector3(0, 0, 0);
        [SerializeField] Vector3 _LeftWristAnglesForHandTracking = new Vector3(0, 0, 180);
        [SerializeField] Vector3 _RightWristPositionForHandTracking = new Vector3(0, 0, 0);
        [SerializeField] Vector3 _RightWristAnglesForHandTracking = new Vector3(180, 0, 180);

        void Update()
        {
            if (_LeftOVRHand.IsTracked)
            {
                _LeftHandTrackingReference.localPosition = _LeftWristPositionForHandTracking;
                _LeftHandTrackingReference.localEulerAngles = _LeftWristAnglesForHandTracking;
            }
            else
            {
                _LeftHandTrackingReference.localPosition = _LeftWristPositionForController;
                _LeftHandTrackingReference.localEulerAngles = _LeftWristAnglesForController;
            }

            if (_RightOVRHand.IsTracked)
            {
                _RightHandTrackingReference.localPosition = _RightWristPositionForHandTracking;
                _RightHandTrackingReference.localEulerAngles = _RightWristAnglesForHandTracking;
            }
            else
            {
                _RightHandTrackingReference.localPosition = _RightWristPositionForController;
                _RightHandTrackingReference.localEulerAngles = _RightWristAnglesForController;
            }
        }
    }
}
