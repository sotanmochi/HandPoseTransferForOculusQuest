// Copyright (c) Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace HandPoseTransfer.OculusQuest
{
    [RequireComponent(typeof(OculusWristController))]
    public class OculusHandPoseTransfer : MonoBehaviour
    {
        public bool IsAvailable { get; private set; }

        [SerializeField] OculusWristController _WristController;
        [SerializeField] OVRHand _LeftOVRHand;
        [SerializeField] OVRHand _RightOVRHand;

        [SerializeField] GameObject _TargetHumanoidAvatar;
        public GameObject TargetHumanoidAvatar
        {
            get { return _TargetHumanoidAvatar; }
            set { _TargetHumanoidAvatar = value; SetTargetPoseHandler(); }
        }

        public Vector3 BodyPositionCorrection = new Vector3(0.0f, -0.1f, 0.0f);

        [SerializeField] bool _VisualizeBones;
        [SerializeField] GameObject _XYZAxisPrefab;
        [SerializeField] float _AxisObjectScale = 0.2f;

        HumanPoseHandler _SrcPoseHandler;
        HumanPoseHandler _TargetPoseHandler;
        HumanPose _SourcePose;
        HumanPose _TargetPose;

        Transform _TargetLeftHandWrist;
        Transform _TargetRightHandWrist;

        async void Start()
        {
            _LeftHandOVRSkelton = _LeftOVRHand.GetComponent<OVRSkeleton>();
            _RightHandOVRSkelton = _RightOVRHand.GetComponent<OVRSkeleton>();

#if UNITY_EDITOR
            _LeftHandOVRSkelton.ShouldUpdateBonePoses = true;
            _RightHandOVRSkelton.ShouldUpdateBonePoses = true;
#endif
            await UniTask.WaitUntil(() => (_LeftHandOVRSkelton.IsInitialized));
            await UniTask.WaitUntil(() => (_RightHandOVRSkelton.IsInitialized));

            Initialize();
            SetTargetPoseHandler();

            if (_WristController == null)
            {
                _WristController = GetComponent<OculusWristController>();
            }
            _WristController._LeftOVRHand = _LeftOVRHand;
            _WristController._RightOVRHand = _RightOVRHand;

            IsAvailable = true;
        }

        void LateUpdate()
        {
            if (IsAvailable)
            {
                // Update hand pose
                if (_LeftOVRHand.IsTracked)
                {
                    UpdateHandSkeletonBones(true);
                }
                if (_RightOVRHand.IsTracked)
                {            
                    UpdateHandSkeletonBones(false);
                }

                if (_TargetPoseHandler != null)
                {
                    // Get current human poses
                    _SrcPoseHandler.GetHumanPose(ref _SourcePose);
                    _TargetPoseHandler.GetHumanPose(ref _TargetPose);

                    // Update avatar hand pose
                    for (int i = 55; i < 95; i++)
                    {
                        _TargetPose.muscles[i] = _SourcePose.muscles[i];
                    }

                    // Update avatar human pose
                    _TargetPose.bodyPosition = _TargetPose.bodyPosition + BodyPositionCorrection;
                    _TargetPoseHandler.SetHumanPose(ref _TargetPose);
                }

                // Update wrist pose
                if (_TargetHumanoidAvatar != null && _WristController != null)
                {
                    _TargetLeftHandWrist.position = _WristController._LeftHandTrackingReference.position;
                    _TargetLeftHandWrist.rotation = _WristController._LeftHandTrackingReference.rotation;
                    _TargetRightHandWrist.position = _WristController._RightHandTrackingReference.position;
                    _TargetRightHandWrist.rotation = _WristController._RightHandTrackingReference.rotation;
                }
            }
        }

        void SetTargetPoseHandler()
        {
            if (_TargetHumanoidAvatar != null)
            {
                var targetAnimator = _TargetHumanoidAvatar.GetComponent<Animator>();
                _TargetPoseHandler = new HumanPoseHandler(targetAnimator.avatar, _TargetHumanoidAvatar.transform);
                _TargetLeftHandWrist = targetAnimator.GetBoneTransform(HumanBodyBones.LeftHand);
                _TargetRightHandWrist = targetAnimator.GetBoneTransform(HumanBodyBones.RightHand);
            }
        }

        OVRSkeleton _LeftHandOVRSkelton;
        OVRSkeleton _RightHandOVRSkelton;
        IDictionary<HumanBodyBones, Transform> _Skeleton;

        void Initialize()
        {
            var skeletonBuilder = new SkeletonBuilder(this.transform);
            skeletonBuilder.AddBasicSkeleton(new SkeletonBuilderParams());

            var leftHandRotation = Quaternion.Euler(0, 180, 180);
            skeletonBuilder.UpdateRotation(HumanBodyBones.LeftHand, leftHandRotation);

            var rightHandRotation = Quaternion.Euler(0, 0, 0);
            skeletonBuilder.UpdateRotation(HumanBodyBones.RightHand, rightHandRotation);

            AddHandSkeletonBones(skeletonBuilder, true); // Left

            AddHandSkeletonBones(skeletonBuilder, false); // Right

            _Skeleton = skeletonBuilder.Skeleton;

            var avatar = AvatarBuilder.BuildHumanAvatar(this.gameObject, skeletonBuilder.GetHumanDescription());
            _SrcPoseHandler = new HumanPoseHandler(avatar, this.transform);

            if (_VisualizeBones && _XYZAxisPrefab != null)
            {
                foreach (var bone in skeletonBuilder.Skeleton.Values)
                {
                    GameObject axis = GameObject.Instantiate(_XYZAxisPrefab, Vector3.zero, Quaternion.identity, bone);
                    axis.transform.localScale = new Vector3(_AxisObjectScale, _AxisObjectScale, _AxisObjectScale);
                    axis.transform.localPosition = Vector3.zero;
                    axis.transform.localRotation = Quaternion.identity;
                }
            }
        }

        void AddHandSkeletonBones(SkeletonBuilder skeletonBuilder, bool isLeftHand)
        {
            OVRSkeleton skeleton = isLeftHand ? _LeftHandOVRSkelton : _RightHandOVRSkelton;

            Dictionary<HumanBodyBones, OVRSkeleton.BoneId> handBoneIdMap;
            handBoneIdMap = isLeftHand ? _LeftHandBoneIdMap : _RightHandBoneIdMap;

            foreach (HumanBodyBones boneKey in handBoneIdMap.Keys)
            {
                OVRSkeleton.BoneId ovrBoneId = handBoneIdMap[boneKey];
                if ((int)ovrBoneId < skeleton.Bones.Count)
                {
                    Transform skeletonBone = skeleton.BindPoses[(int)ovrBoneId].Transform;
                    Vector3 localPosition = skeletonBone.localPosition;
                    Quaternion localRotation = skeletonBone.localRotation;

                    // Local pose composition
                    if ((ovrBoneId ==  OVRSkeleton.BoneId.Hand_Thumb2) || (ovrBoneId ==  OVRSkeleton.BoneId.Hand_Pinky1))
                    {
                        short parentBoneIndex = skeleton.Bones[(int)ovrBoneId].ParentBoneIndex;
                        Transform parentBone = skeleton.Bones[parentBoneIndex].Transform;
                        localPosition = parentBone.localPosition + parentBone.localRotation * localPosition;
                        localRotation = parentBone.localRotation * localRotation;
                    }

                    skeletonBuilder.Add(boneKey, _HandBoneParent[boneKey], localPosition, localRotation);
                }
            }
        }

        void UpdateHandSkeletonBones(bool isLeftHand)
        {
            OVRSkeleton skeleton = isLeftHand ? _LeftHandOVRSkelton : _RightHandOVRSkelton;

            Dictionary<HumanBodyBones, OVRSkeleton.BoneId> handBoneIdMap;
            handBoneIdMap = isLeftHand ? _LeftHandBoneIdMap : _RightHandBoneIdMap;

            foreach (HumanBodyBones boneKey in handBoneIdMap.Keys)
            {
                OVRSkeleton.BoneId ovrBoneId = handBoneIdMap[boneKey];
                if ((int)ovrBoneId < skeleton.Bones.Count)
                {
                    Transform skeletonBone = skeleton.Bones[(int)ovrBoneId].Transform;
                    Quaternion localRotation = skeletonBone.localRotation;

                    // Local pose composition
                    if ((ovrBoneId ==  OVRSkeleton.BoneId.Hand_Thumb2) || (ovrBoneId ==  OVRSkeleton.BoneId.Hand_Pinky1))
                    {
                        short parentBoneIndex = skeleton.Bones[(int)ovrBoneId].ParentBoneIndex;
                        Transform parentBone = skeleton.Bones[parentBoneIndex].Transform;
                        localRotation = parentBone.localRotation * localRotation;
                    }

                    _Skeleton[boneKey].localRotation = localRotation;
                }
            }
        }

        static readonly Dictionary<HumanBodyBones, OVRSkeleton.BoneId> _LeftHandBoneIdMap = new Dictionary<HumanBodyBones, OVRSkeleton.BoneId>()
        {
            {HumanBodyBones.LeftThumbProximal,      OVRSkeleton.BoneId.Hand_Thumb0},
            {HumanBodyBones.LeftThumbIntermediate,  OVRSkeleton.BoneId.Hand_Thumb2},
            {HumanBodyBones.LeftThumbDistal,        OVRSkeleton.BoneId.Hand_Thumb3},
            {HumanBodyBones.LeftIndexProximal,      OVRSkeleton.BoneId.Hand_Index1},
            {HumanBodyBones.LeftIndexIntermediate,  OVRSkeleton.BoneId.Hand_Index2},
            {HumanBodyBones.LeftIndexDistal,        OVRSkeleton.BoneId.Hand_Index3},
            {HumanBodyBones.LeftMiddleProximal,     OVRSkeleton.BoneId.Hand_Middle1},
            {HumanBodyBones.LeftMiddleIntermediate, OVRSkeleton.BoneId.Hand_Middle2},
            {HumanBodyBones.LeftMiddleDistal,       OVRSkeleton.BoneId.Hand_Middle3},
            {HumanBodyBones.LeftRingProximal,       OVRSkeleton.BoneId.Hand_Ring1},
            {HumanBodyBones.LeftRingIntermediate,   OVRSkeleton.BoneId.Hand_Ring2},
            {HumanBodyBones.LeftRingDistal,         OVRSkeleton.BoneId.Hand_Ring3}, 
            {HumanBodyBones.LeftLittleProximal,     OVRSkeleton.BoneId.Hand_Pinky1},
            {HumanBodyBones.LeftLittleIntermediate, OVRSkeleton.BoneId.Hand_Pinky2},
            {HumanBodyBones.LeftLittleDistal,       OVRSkeleton.BoneId.Hand_Pinky3},
        };

        static readonly Dictionary<HumanBodyBones, OVRSkeleton.BoneId> _RightHandBoneIdMap = new Dictionary<HumanBodyBones, OVRSkeleton.BoneId>()
        {
            {HumanBodyBones.RightThumbProximal,      OVRSkeleton.BoneId.Hand_Thumb0},
            {HumanBodyBones.RightThumbIntermediate,  OVRSkeleton.BoneId.Hand_Thumb2},
            {HumanBodyBones.RightThumbDistal,        OVRSkeleton.BoneId.Hand_Thumb3},
            {HumanBodyBones.RightIndexProximal,      OVRSkeleton.BoneId.Hand_Index1},
            {HumanBodyBones.RightIndexIntermediate,  OVRSkeleton.BoneId.Hand_Index2},
            {HumanBodyBones.RightIndexDistal,        OVRSkeleton.BoneId.Hand_Index3},
            {HumanBodyBones.RightMiddleProximal,     OVRSkeleton.BoneId.Hand_Middle1},
            {HumanBodyBones.RightMiddleIntermediate, OVRSkeleton.BoneId.Hand_Middle2},
            {HumanBodyBones.RightMiddleDistal,       OVRSkeleton.BoneId.Hand_Middle3},
            {HumanBodyBones.RightRingProximal,       OVRSkeleton.BoneId.Hand_Ring1},
            {HumanBodyBones.RightRingIntermediate,   OVRSkeleton.BoneId.Hand_Ring2},
            {HumanBodyBones.RightRingDistal,         OVRSkeleton.BoneId.Hand_Ring3}, 
            {HumanBodyBones.RightLittleProximal,     OVRSkeleton.BoneId.Hand_Pinky1},
            {HumanBodyBones.RightLittleIntermediate, OVRSkeleton.BoneId.Hand_Pinky2},
            {HumanBodyBones.RightLittleDistal,       OVRSkeleton.BoneId.Hand_Pinky3},
        };

        static readonly Dictionary<HumanBodyBones, HumanBodyBones> _HandBoneParent = new Dictionary<HumanBodyBones, HumanBodyBones>()
        {
            // Left hand
            {HumanBodyBones.LeftThumbProximal, HumanBodyBones.LeftHand},
            {HumanBodyBones.LeftThumbIntermediate, HumanBodyBones.LeftThumbProximal},
            {HumanBodyBones.LeftThumbDistal, HumanBodyBones.LeftThumbIntermediate},
            {HumanBodyBones.LeftIndexProximal, HumanBodyBones.LeftHand},
            {HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexProximal},
            {HumanBodyBones.LeftIndexDistal, HumanBodyBones.LeftIndexIntermediate},
            {HumanBodyBones.LeftMiddleProximal, HumanBodyBones.LeftHand},
            {HumanBodyBones.LeftMiddleIntermediate, HumanBodyBones.LeftMiddleProximal},
            {HumanBodyBones.LeftMiddleDistal, HumanBodyBones.LeftMiddleIntermediate},
            {HumanBodyBones.LeftRingProximal, HumanBodyBones.LeftHand},
            {HumanBodyBones.LeftRingIntermediate, HumanBodyBones.LeftRingProximal},
            {HumanBodyBones.LeftRingDistal, HumanBodyBones.LeftRingIntermediate},
            {HumanBodyBones.LeftLittleProximal, HumanBodyBones.LeftHand},
            {HumanBodyBones.LeftLittleIntermediate, HumanBodyBones.LeftLittleProximal},
            {HumanBodyBones.LeftLittleDistal, HumanBodyBones.LeftLittleIntermediate},
            // Right hand
            {HumanBodyBones.RightThumbProximal, HumanBodyBones.RightHand},
            {HumanBodyBones.RightThumbIntermediate, HumanBodyBones.RightThumbProximal},
            {HumanBodyBones.RightThumbDistal, HumanBodyBones.RightThumbIntermediate},
            {HumanBodyBones.RightIndexProximal, HumanBodyBones.RightHand},
            {HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexProximal},
            {HumanBodyBones.RightIndexDistal, HumanBodyBones.RightIndexIntermediate},
            {HumanBodyBones.RightMiddleProximal, HumanBodyBones.RightHand},
            {HumanBodyBones.RightMiddleIntermediate, HumanBodyBones.RightMiddleProximal},
            {HumanBodyBones.RightMiddleDistal, HumanBodyBones.RightMiddleIntermediate},
            {HumanBodyBones.RightRingProximal, HumanBodyBones.RightHand},
            {HumanBodyBones.RightRingIntermediate, HumanBodyBones.RightRingProximal},
            {HumanBodyBones.RightRingDistal, HumanBodyBones.RightRingIntermediate},
            {HumanBodyBones.RightLittleProximal, HumanBodyBones.RightHand},
            {HumanBodyBones.RightLittleIntermediate, HumanBodyBones.RightLittleProximal},
            {HumanBodyBones.RightLittleDistal, HumanBodyBones.RightLittleIntermediate},
        };
    }
}
