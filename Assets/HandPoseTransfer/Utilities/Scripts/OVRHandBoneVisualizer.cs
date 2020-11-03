// Copyright (c) Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace HandPoseTransfer.OculusQuest
{
    public class OVRHandBoneVisualizer : MonoBehaviour
    {
        public bool IsAvailable { get; private set; }

        [SerializeField] OVRSkeleton _OVRHandSkeleton;
        [SerializeField] GameObject _XYZAxisPrefab;
        [SerializeField] float _AxisObjectScale = 0.2f;

        Dictionary<OVRSkeleton.BoneId, Transform> _HandBoneTransforms = new Dictionary<OVRSkeleton.BoneId, Transform>();
        Dictionary<OVRSkeleton.BoneId, Transform> _BoneVisualizerTransforms = new Dictionary<OVRSkeleton.BoneId, Transform>();

        List<OVRSkeleton.BoneId> _HandBoneIdList = new List<OVRSkeleton.BoneId>()
        {
            OVRSkeleton.BoneId.Hand_WristRoot,
            OVRSkeleton.BoneId.Hand_Thumb0,
            OVRSkeleton.BoneId.Hand_Thumb1,
            OVRSkeleton.BoneId.Hand_Thumb2,
            OVRSkeleton.BoneId.Hand_Thumb3,
            OVRSkeleton.BoneId.Hand_Index1,
            OVRSkeleton.BoneId.Hand_Index2,
            OVRSkeleton.BoneId.Hand_Index3,
            OVRSkeleton.BoneId.Hand_Middle1,
            OVRSkeleton.BoneId.Hand_Middle2,
            OVRSkeleton.BoneId.Hand_Middle3,
            OVRSkeleton.BoneId.Hand_Ring1,
            OVRSkeleton.BoneId.Hand_Ring2,
            OVRSkeleton.BoneId.Hand_Ring3,
            OVRSkeleton.BoneId.Hand_Pinky0,
            OVRSkeleton.BoneId.Hand_Pinky1,
            OVRSkeleton.BoneId.Hand_Pinky2,
            OVRSkeleton.BoneId.Hand_Pinky3,
        };

        async void Start()
        {
            Debug.LogWarning("OVRHandSkeleton.IsInitialized (Before WaitUntil): " + _OVRHandSkeleton.IsInitialized);

            await UniTask.WaitUntil(() => _OVRHandSkeleton.IsInitialized);

            Debug.LogWarning("OVRHandSkeleton.IsInitialized (After WaitUntil): " + _OVRHandSkeleton.IsInitialized);

            foreach (OVRSkeleton.BoneId boneId in _HandBoneIdList)
            {
                Transform boneTransform = _OVRHandSkeleton.Bones[(int)boneId].Transform;
                if (boneTransform != null)
                {
                    _HandBoneTransforms.Add(boneId, boneTransform);
                }
            }

            foreach (OVRSkeleton.BoneId boneId in _HandBoneTransforms.Keys)
            {
                GameObject go = GameObject.Instantiate(_XYZAxisPrefab, Vector3.zero, Quaternion.identity, this.transform);
                go.name = boneId.ToString();
                go.transform.localScale = new Vector3(_AxisObjectScale, _AxisObjectScale, _AxisObjectScale);
                go.transform.position = _HandBoneTransforms[boneId].position;
                go.transform.rotation = _HandBoneTransforms[boneId].rotation;
                _BoneVisualizerTransforms[boneId] = go.transform;
            }

            IsAvailable = true;
        }

        void Update()
        {
            foreach (OVRSkeleton.BoneId boneId in _HandBoneTransforms.Keys)
            {
                _BoneVisualizerTransforms[boneId].transform.position = _HandBoneTransforms[boneId].position;
                _BoneVisualizerTransforms[boneId].transform.rotation = _HandBoneTransforms[boneId].rotation;
            }
        }
    }
}
