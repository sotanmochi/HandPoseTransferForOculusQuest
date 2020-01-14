// Copyright (c) Soichiro Sugimoto.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine;

namespace HandPoseTransfer
{
    public class SkeletonBuilderParams
    {
        public float HipsHeight = 0.8f;
        public float HipsLength = 0.2f;
        public float SpineLength = 0.1f;
        public float ChestLength = 0.2f;
        public float NeckLength = 0.1f;
        public float HeadLength = 0.2f;
        public float Shoulder = 0.1f;
        public float UpperArm = 0.3f;
        public float LowerArm = 0.3f;
        public float Hand = 0.1f;
        public float LegDistance = 0.1f;
        public float UpperLeg = 0.3f;
        public float LowerLeg = 0.4f;
        public float Foot = 0.1f;
        public float Toe = 0.1f;
    }

    public class SkeletonBuilder
    {
        Dictionary<HumanBodyBones, Transform> _Skeleton = new Dictionary<HumanBodyBones, Transform>();
        public IDictionary<HumanBodyBones, Transform> Skeleton { get { return _Skeleton; } }

        Transform _Root;

        public SkeletonBuilder(Transform root)
        {
            _Root = root;
        }

        void Add(HumanBodyBones key, Transform parent, Vector3 headPosition, Quaternion headRotation)
        {
            var bone = new GameObject(key.ToString()).transform;
            bone.localPosition = headPosition;
            bone.localRotation = headRotation;
            bone.SetParent(parent, false);
            _Skeleton[key] = bone;
        }

        public void Add(HumanBodyBones key, HumanBodyBones parentKey, Vector3 headPosition, Quaternion headRotation)
        {
            Add(key, _Skeleton[parentKey], headPosition, headRotation);
        }

        public void UpdateRotation(HumanBodyBones key, Quaternion localRotation)
        {
            _Skeleton[key].localRotation = localRotation;
        }

        public void AddBasicSkeleton(SkeletonBuilderParams skeletonParams)
        {
            Add(HumanBodyBones.Hips, _Root, new Vector3(0, skeletonParams.HipsHeight, 0), Quaternion.identity);
            Add(HumanBodyBones.Spine, HumanBodyBones.Hips, new Vector3(0, skeletonParams.HipsLength, 0), Quaternion.identity);
            Add(HumanBodyBones.Chest, HumanBodyBones.Spine, new Vector3(0, skeletonParams.SpineLength, 0), Quaternion.identity);
            Add(HumanBodyBones.Neck, HumanBodyBones.Chest, new Vector3(0, skeletonParams.ChestLength, 0), Quaternion.identity);
            Add(HumanBodyBones.Head, HumanBodyBones.Neck, new Vector3(0, skeletonParams.NeckLength, 0), Quaternion.identity);

            Add(HumanBodyBones.LeftShoulder, HumanBodyBones.Chest,  new Vector3(0, skeletonParams.ChestLength, 0), Quaternion.identity);
            Add(HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftShoulder, new Vector3(-skeletonParams.Shoulder, 0, 0), Quaternion.identity);
            Add(HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm, new Vector3(-skeletonParams.UpperArm, 0, 0), Quaternion.identity);
            Add(HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm, new Vector3(-skeletonParams.LowerArm, 0, 0), Quaternion.identity);

            Add(HumanBodyBones.RightShoulder, HumanBodyBones.Chest,  new Vector3(0, skeletonParams.ChestLength, 0), Quaternion.identity);
            Add(HumanBodyBones.RightUpperArm, HumanBodyBones.RightShoulder, new Vector3(skeletonParams.Shoulder, 0, 0), Quaternion.identity);
            Add(HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm, new Vector3(skeletonParams.UpperArm, 0, 0), Quaternion.identity);
            Add(HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm, new Vector3(skeletonParams.LowerArm, 0, 0), Quaternion.identity);

            Add(HumanBodyBones.LeftUpperLeg, HumanBodyBones.Hips, new Vector3(-skeletonParams.LegDistance, 0, 0), Quaternion.identity);
            Add(HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftUpperLeg, new Vector3(0, -skeletonParams.UpperLeg, 0), Quaternion.identity);
            Add(HumanBodyBones.LeftFoot, HumanBodyBones.LeftLowerLeg, new Vector3(0, -skeletonParams.LowerLeg, 0), Quaternion.identity);
            Add(HumanBodyBones.LeftToes, HumanBodyBones.LeftFoot, new Vector3(0, -skeletonParams.Foot, skeletonParams.Foot), Quaternion.identity);

            Add(HumanBodyBones.RightUpperLeg, HumanBodyBones.Hips, new Vector3(skeletonParams.LegDistance, 0, 0), Quaternion.identity);
            Add(HumanBodyBones.RightLowerLeg, HumanBodyBones.RightUpperLeg, new Vector3(0, -skeletonParams.UpperLeg, 0), Quaternion.identity);
            Add(HumanBodyBones.RightFoot, HumanBodyBones.RightLowerLeg, new Vector3(0, -skeletonParams.LowerLeg, 0), Quaternion.identity);
            Add(HumanBodyBones.RightToes, HumanBodyBones.RightFoot, new Vector3(0, -skeletonParams.Foot, skeletonParams.Foot), Quaternion.identity);
        }

        public HumanDescription GetHumanDescription()
        {
            HumanDescription humanDesc = new HumanDescription();

            humanDesc.human = GetHumanBones().ToArray();
            humanDesc.skeleton = GetSkeletonBones().ToArray();
            humanDesc.armStretch = 0.05f;
            humanDesc.legStretch = 0.05f;
            humanDesc.upperArmTwist = 0.5f;
            humanDesc.lowerArmTwist = 0.5f;
            humanDesc.upperLegTwist = 0.5f;
            humanDesc.lowerLegTwist = 0.5f;
            humanDesc.feetSpacing = 0.0f;
            humanDesc.hasTranslationDoF = false;

            return humanDesc;
        }

        public List<SkeletonBone> GetSkeletonBones()
        {
            List<SkeletonBone> skeletonBones = new List<SkeletonBone>();

            SkeletonBone rootBone = new SkeletonBone();
            rootBone.name = _Root.name;
            rootBone.position = _Root.localPosition;
            rootBone.rotation = _Root.localRotation;
            rootBone.scale = _Root.localScale;

            skeletonBones.Add(rootBone);

            foreach (var boneTransform in _Skeleton.Values)
            {
                SkeletonBone skeletonBone = new SkeletonBone();
                skeletonBone.name = boneTransform.name;
                skeletonBone.position = boneTransform.localPosition;
                skeletonBone.rotation = boneTransform.localRotation;
                skeletonBone.scale = boneTransform.localScale;

                skeletonBones.Add(skeletonBone);
            }

            return skeletonBones;
        }

        public List<HumanBone> GetHumanBones()
        {
            List<HumanBone> humanBones = new List<HumanBone>();

            foreach (var skeleton in _Skeleton)
            {
                HumanBone humanBone = new HumanBone();
                humanBone.humanName = _HumanBoneNameMap[skeleton.Key];
                humanBone.boneName = skeleton.Value.name;
                humanBone.limit.useDefaultValues = true;

                humanBones.Add(humanBone);
            }

            return humanBones;
        }

        static readonly Dictionary<HumanBodyBones, string> _HumanBoneNameMap = new Dictionary<HumanBodyBones, string>()
        {
            // {HumanBodyBones, HumanTrait.BoneName}
            {HumanBodyBones.Hips, "Hips"},
            {HumanBodyBones.LeftUpperLeg, "LeftUpperLeg"},
            {HumanBodyBones.RightUpperLeg, "RightUpperLeg"},
            {HumanBodyBones.LeftLowerLeg, "LeftLowerLeg"},
            {HumanBodyBones.RightLowerLeg, "RightLowerLeg"},
            {HumanBodyBones.LeftFoot, "LeftFoot"},
            {HumanBodyBones.RightFoot, "RightFoot"},
            {HumanBodyBones.Spine, "Spine"},
            {HumanBodyBones.Chest, "Chest"},
            {HumanBodyBones.UpperChest, "UpperChest"},
            {HumanBodyBones.Neck, "Neck"},
            {HumanBodyBones.Head, "Head"},
            {HumanBodyBones.LeftShoulder, "LeftShoulder"},
            {HumanBodyBones.RightShoulder, "RightShoulder"},
            {HumanBodyBones.LeftUpperArm, "LeftUpperArm"},
            {HumanBodyBones.RightUpperArm, "RightUpperArm"},
            {HumanBodyBones.LeftLowerArm, "LeftLowerArm"},
            {HumanBodyBones.RightLowerArm, "RightLowerArm"},
            {HumanBodyBones.LeftHand, "LeftHand"},
            {HumanBodyBones.RightHand, "RightHand"},
            {HumanBodyBones.LeftToes, "LeftToes"},
            {HumanBodyBones.RightToes, "RightToes"},
            {HumanBodyBones.LeftEye, "LeftEye"},
            {HumanBodyBones.RightEye, "RightEye"},
            {HumanBodyBones.Jaw, "Jaw"},
            {HumanBodyBones.LeftThumbProximal, "Left Thumb Proximal"},
            {HumanBodyBones.LeftThumbIntermediate, "Left Thumb Intermediate"},
            {HumanBodyBones.LeftThumbDistal, "Left Thumb Distal"},
            {HumanBodyBones.LeftIndexProximal, "Left Index Proximal"},
            {HumanBodyBones.LeftIndexIntermediate, "Left Index Intermediate"},
            {HumanBodyBones.LeftIndexDistal, "Left Index Distal"},
            {HumanBodyBones.LeftMiddleProximal, "Left Middle Proximal"},
            {HumanBodyBones.LeftMiddleIntermediate, "Left Middle Intermediate"},
            {HumanBodyBones.LeftMiddleDistal, "Left Middle Distal"},
            {HumanBodyBones.LeftRingProximal, "Left Ring Proximal"},
            {HumanBodyBones.LeftRingIntermediate, "Left Ring Intermediate"},
            {HumanBodyBones.LeftRingDistal, "Left Ring Distal"},
            {HumanBodyBones.LeftLittleProximal, "Left Little Proximal"},
            {HumanBodyBones.LeftLittleIntermediate, "Left Little Intermediate"},
            {HumanBodyBones.LeftLittleDistal, "Left Little Distal"},
            {HumanBodyBones.RightThumbProximal, "Right Thumb Proximal"},
            {HumanBodyBones.RightThumbIntermediate, "Right Thumb Intermediate"},
            {HumanBodyBones.RightThumbDistal, "Right Thumb Distal"},
            {HumanBodyBones.RightIndexProximal, "Right Index Proximal"},
            {HumanBodyBones.RightIndexIntermediate, "Right Index Intermediate"},
            {HumanBodyBones.RightIndexDistal, "Right Index Distal"},
            {HumanBodyBones.RightMiddleProximal, "Right Middle Proximal"},
            {HumanBodyBones.RightMiddleIntermediate, "Right Middle Intermediate"},
            {HumanBodyBones.RightMiddleDistal, "Right Middle Distal"},
            {HumanBodyBones.RightRingProximal, "Right Ring Proximal"},
            {HumanBodyBones.RightRingIntermediate, "Right Ring Intermediate"},
            {HumanBodyBones.RightRingDistal, "Right Ring Distal"},
            {HumanBodyBones.RightLittleProximal, "Right Little Proximal"},
            {HumanBodyBones.RightLittleIntermediate, "Right Little Intermediate"},
            {HumanBodyBones.RightLittleDistal, "Right Little Distal"},
        };
    }
}

