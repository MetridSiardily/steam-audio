//
// Copyright 2017-2023 Valve Corporation.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using UnityEngine;
using UnityEditor;

namespace SteamAudio
{
    [CustomEditor(typeof(SteamAudioSource))]
    [CanEditMultipleObjects]
    public class SteamAudioSourceInspector : Editor
    {
        SerializedProperty mDirectBinaural;
        readonly GUIContent mDirectBinauralGUI = new("Direct Binaural", "When enabled, HRTF-based binaural rendering will be used to spatialize the source. " +
        "This requires 2-channel (stereo) audio output. If unchecked, panning will be used to the spatialize the source using the user’s speaker layout. " +
        "Binaural rendering provides improved spatialization at the cost of slightly increased CPU usage.");
        SerializedProperty mInterpolation;
        readonly GUIContent mInterpolationGUI = new("Interpolation", "Controls how HRTFs are interpolated when the source moves relative to the listener." +
        "\n<b>Nearest:</b> Uses the HRTF from the direction nearest to the direction of the source for which HRTF data is available. " +
        "The fastest option, but can result in audible artifacts for certain kinds of audio clips, such as white noise or engine sounds." +
        "\n<b>Bilinear:</b> Uses an HRTF generated after interpolating from four directions nearest to the direction of the source, " +
        "for which HRTF data is available. This may result in smoother audio for some kinds of sources when the listener looks around, " +
        "but has higher CPU usage (up to 2x).");
        SerializedProperty mPerspectiveCorrection;
        readonly GUIContent mPerspectiveCorrectionGUI = new("Perspective Correction", "When enabled, perspective correction (based on the projection matrix of the " +
        "current main camera) is applied to this source during spatialization. This can improve the perceived positional accuracy in non-VR applications. " +
        "\nRequires <b>Enable Perspective Correction</b> to be checked in <b>Steam Audio Settings</b>.");
        SerializedProperty mDistanceAttenuation;
        readonly GUIContent mDistanceAttenuationGUI = new("Distance Attenuation", "When enabled, distance attenuation will be calculated and applied to the Audio Source. " +
        "This takes into account the Spatial Blend setting on the Audio Source, so if Spatial Blend is set to 2D, distance attenuation is effectively not applied.");
        SerializedProperty mDistanceAttenuationInput;
        readonly GUIContent mDistanceAttenuationInputGUI = new("Distance Attenuation Input", "Specifies how the distance attenuation value is determined." +
        "\n<b>Curve Driven:</b> Distance attenuation is controlled by the Volume curve on the Audio Source.");
        SerializedProperty mAirAbsorption;
        readonly GUIContent mAirAbsorptionGUI = new("Air Absorption", "When enabled, frequency-dependent distance based air absorption will be calculated and " +
        "applied to the Audio Source.");
        SerializedProperty mAirAbsorptionInput;
        SerializedProperty mAirAbsorptionLow;
        SerializedProperty mAirAbsorptionMid;
        SerializedProperty mAirAbsorptionHigh;
        SerializedProperty mDirectivity;
        readonly GUIContent mDirectivityGUI = new("Directivity", "If checked, attenuation based on the source’s directivity pattern and orientation will be applied " +
        "to the Audio Source.");
        SerializedProperty mDirectivityInput;
        SerializedProperty mDipoleWeight;
        SerializedProperty mDipolePower;
        SerializedProperty mDirectivityValue;
        SerializedProperty mOcclusion;
        GUIContent mOcclusionGUI = new("Occlusion", "When enabled, attenuation based on the occlusion of the source by the " +
        "scene geometry will be applied to the Audio Source.");
        SerializedProperty mOcclusionInput;
        SerializedProperty mOcclusionType;
        SerializedProperty mOcclusionRadius;
        SerializedProperty mOcclusionSamples;
        SerializedProperty mOcclusionValue;
        SerializedProperty mTransmission;
        SerializedProperty mTransmissionType;
        SerializedProperty mTransmissionInput;
        SerializedProperty mTransmissionLow;
        SerializedProperty mTransmissionMid;
        SerializedProperty mTransmissionHigh;
        SerializedProperty mTransmissionRays;
        SerializedProperty mDirectMixLevel;
        SerializedProperty mReflections;
        SerializedProperty mReflectionsType;
        SerializedProperty mUseDistanceCurveForReflections;
        SerializedProperty mCurrentBakedSource;
        SerializedProperty mApplyHRTFToReflections;
        SerializedProperty mReflectionsMixLevel;
        SerializedProperty mPathing;
        SerializedProperty mPathingProbeBatch;
        SerializedProperty mPathValidation;
        SerializedProperty mFindAlternatePaths;
        SerializedProperty mApplyHRTFToPathing;
        SerializedProperty mPathingMixLevel;
        SerializedProperty mNormalizePathingEQ;

        Texture2D mDirectivityPreview = null;
        float[] mDirectivitySamples = null;
        Vector2[] mDirectivityPositions = null;

        private void OnEnable()
        {
            mDirectBinaural = serializedObject.FindProperty("directBinaural");
            mInterpolation = serializedObject.FindProperty("interpolation");
            mPerspectiveCorrection = serializedObject.FindProperty("perspectiveCorrection");
            mDistanceAttenuation = serializedObject.FindProperty("distanceAttenuation");
            mDistanceAttenuationInput = serializedObject.FindProperty("distanceAttenuationInput");
            mAirAbsorption = serializedObject.FindProperty("airAbsorption");
            mAirAbsorptionInput = serializedObject.FindProperty("airAbsorptionInput");
            mAirAbsorptionLow = serializedObject.FindProperty("airAbsorptionLow");
            mAirAbsorptionMid = serializedObject.FindProperty("airAbsorptionMid");
            mAirAbsorptionHigh = serializedObject.FindProperty("airAbsorptionHigh");
            mDirectivity = serializedObject.FindProperty("directivity");
            mDirectivityInput = serializedObject.FindProperty("directivityInput");
            mDipoleWeight = serializedObject.FindProperty("dipoleWeight");
            mDipolePower = serializedObject.FindProperty("dipolePower");
            mDirectivityValue = serializedObject.FindProperty("directivityValue");
            mOcclusion = serializedObject.FindProperty("occlusion");
            mOcclusionInput = serializedObject.FindProperty("occlusionInput");
            mOcclusionType = serializedObject.FindProperty("occlusionType");
            mOcclusionRadius = serializedObject.FindProperty("occlusionRadius");
            mOcclusionSamples = serializedObject.FindProperty("occlusionSamples");
            mOcclusionValue = serializedObject.FindProperty("occlusionValue");
            mTransmission = serializedObject.FindProperty("transmission");
            mTransmissionType = serializedObject.FindProperty("transmissionType");
            mTransmissionInput = serializedObject.FindProperty("transmissionInput");
            mTransmissionLow = serializedObject.FindProperty("transmissionLow");
            mTransmissionMid = serializedObject.FindProperty("transmissionMid");
            mTransmissionHigh = serializedObject.FindProperty("transmissionHigh");
            mTransmissionRays = serializedObject.FindProperty("maxTransmissionSurfaces");
            mDirectMixLevel = serializedObject.FindProperty("directMixLevel");
            mReflections = serializedObject.FindProperty("reflections");
            mReflectionsType = serializedObject.FindProperty("reflectionsType");
            mUseDistanceCurveForReflections = serializedObject.FindProperty("useDistanceCurveForReflections");
            mCurrentBakedSource = serializedObject.FindProperty("currentBakedSource");
            mApplyHRTFToReflections = serializedObject.FindProperty("applyHRTFToReflections");
            mReflectionsMixLevel = serializedObject.FindProperty("reflectionsMixLevel");
            mPathing = serializedObject.FindProperty("pathing");
            mPathingProbeBatch = serializedObject.FindProperty("pathingProbeBatch");
            mPathValidation = serializedObject.FindProperty("pathValidation");
            mFindAlternatePaths = serializedObject.FindProperty("findAlternatePaths");
            mApplyHRTFToPathing = serializedObject.FindProperty("applyHRTFToPathing");
            mPathingMixLevel = serializedObject.FindProperty("pathingMixLevel");
            mNormalizePathingEQ = serializedObject.FindProperty("normalizePathingEQ");
        }

        [MenuItem("GameObject/Steam Audio/Steam Audio Source", false, 12)]
        static void CreateGameObjectWithSource(MenuCommand menuCommand)
        {
            var name = GameObjectUtility.GetUniqueNameForSibling(null, "Steam Audio Source");
            var gameObject = ObjectFactory.CreateGameObject(name, typeof(SteamAudioSource));

            ObjectFactory.PlaceGameObject(gameObject, menuCommand.context as GameObject);
            Selection.activeGameObject = gameObject;
        }

        public override void OnInspectorGUI()
        {
            var audioEngineIsUnity = SteamAudioSettings.Singleton.audioEngine == AudioEngineType.Unity;

            serializedObject.Update();

            if (audioEngineIsUnity)
            {
                EditorGUILayout.PropertyField(mDirectBinaural, mDirectBinauralGUI);

                EditorGUILayout.PropertyField(mInterpolation, mInterpolationGUI);
            }

            if (audioEngineIsUnity && SteamAudioSettings.Singleton.perspectiveCorrection)
            {
                EditorGUILayout.PropertyField(mPerspectiveCorrection, mPerspectiveCorrectionGUI);
            }

            if (audioEngineIsUnity)
            {
                EditorGUILayout.PropertyField(mDistanceAttenuation, mDistanceAttenuationGUI);
                EditorGUILayout.PropertyField(mDistanceAttenuationInput, mDistanceAttenuationInputGUI);
            }

            if (audioEngineIsUnity)
            {
                EditorGUILayout.PropertyField(mAirAbsorption, mAirAbsorptionGUI);
                if (mAirAbsorption.boolValue)
                {
                    EditorGUILayout.PropertyField(mAirAbsorptionInput);
                    if ((AirAbsorptionInput)mAirAbsorptionInput.enumValueIndex == AirAbsorptionInput.UserDefined)
                    {
                        EditorGUILayout.PropertyField(mAirAbsorptionLow);
                        EditorGUILayout.PropertyField(mAirAbsorptionMid);
                        EditorGUILayout.PropertyField(mAirAbsorptionHigh);
                    }
                }
            }

            if (audioEngineIsUnity)
            {
                EditorGUILayout.PropertyField(mDirectivity, mDirectivityGUI);
                if (mDirectivity.boolValue)
                {
                    EditorGUILayout.PropertyField(mDirectivityInput);

                    if ((DirectivityInput) mDirectivityInput.enumValueIndex == DirectivityInput.SimulationDefined)
                    {
                        EditorGUILayout.PropertyField(mDipoleWeight);
                        EditorGUILayout.PropertyField(mDipolePower);
                        DrawDirectivity(mDipoleWeight.floatValue, mDipolePower.floatValue);
                    }
                    else if ((DirectivityInput) mDirectivityInput.enumValueIndex == DirectivityInput.UserDefined)
                    {
                        EditorGUILayout.PropertyField(mDirectivityValue);
                    }
                }
            }

            EditorGUILayout.PropertyField(mOcclusion, mOcclusionGUI);
            if (mOcclusion.boolValue)
            {
                if (audioEngineIsUnity)
                {
                    EditorGUILayout.PropertyField(mOcclusionInput);
                }

                if (!audioEngineIsUnity ||
                    (OcclusionInput) mOcclusionInput.enumValueIndex == OcclusionInput.SimulationDefined)
                {
                    EditorGUILayout.PropertyField(mOcclusionType);
                    if ((OcclusionType) mOcclusionType.enumValueIndex == OcclusionType.Volumetric)
                    {
                        EditorGUILayout.PropertyField(mOcclusionRadius);
                        EditorGUILayout.PropertyField(mOcclusionSamples);
                    }
                }
                else if ((OcclusionInput) mOcclusionInput.enumValueIndex == OcclusionInput.UserDefined)
                {
                    EditorGUILayout.PropertyField(mOcclusionValue);
                }

                EditorGUILayout.PropertyField(mTransmission);
                if (audioEngineIsUnity)
                {
                    if (mTransmission.boolValue)
                    {
                        EditorGUILayout.PropertyField(mTransmissionType);
                        EditorGUILayout.PropertyField(mTransmissionInput);
                        if ((TransmissionInput)mTransmissionInput.enumValueIndex == TransmissionInput.UserDefined)
                        {
                            if (mTransmissionType.enumValueIndex == (int)TransmissionType.FrequencyDependent)
                            {
                                EditorGUILayout.PropertyField(mTransmissionLow);
                                EditorGUILayout.PropertyField(mTransmissionMid);
                                EditorGUILayout.PropertyField(mTransmissionHigh);
                            }
                            else
                            {
                                EditorGUILayout.PropertyField(mTransmissionMid);
                            }
                        }
                    }
                }

                if (!audioEngineIsUnity ||
                    (TransmissionInput) mTransmissionInput.enumValueIndex == TransmissionInput.SimulationDefined)
                {
                    EditorGUILayout.PropertyField(mTransmissionRays);
                }
            }

            if (audioEngineIsUnity)
            {
                EditorGUILayout.PropertyField(mDirectMixLevel);
            }

            EditorGUILayout.PropertyField(mReflections);
            if (mReflections.boolValue)
            {
                EditorGUILayout.PropertyField(mReflectionsType);

                if (audioEngineIsUnity &&
                    mDistanceAttenuation.boolValue &&
                    (DistanceAttenuationInput) mDistanceAttenuationInput.enumValueIndex == DistanceAttenuationInput.CurveDriven)
                {
                    EditorGUILayout.PropertyField(mUseDistanceCurveForReflections);
                }

                if ((ReflectionsType) mReflectionsType.enumValueIndex == ReflectionsType.BakedStaticSource)
                {
                    EditorGUILayout.PropertyField(mCurrentBakedSource);
                }

                if (audioEngineIsUnity)
                {
                    EditorGUILayout.PropertyField(mApplyHRTFToReflections);
                    EditorGUILayout.PropertyField(mReflectionsMixLevel);
                }
            }

            EditorGUILayout.PropertyField(mPathing);
            if (mPathing.boolValue)
            {
                EditorGUILayout.PropertyField(mPathingProbeBatch);
                EditorGUILayout.PropertyField(mPathValidation);
                EditorGUILayout.PropertyField(mFindAlternatePaths);

                if (audioEngineIsUnity)
                {
                    EditorGUILayout.PropertyField(mApplyHRTFToPathing);
                    EditorGUILayout.PropertyField(mPathingMixLevel);
                    EditorGUILayout.PropertyField(mNormalizePathingEQ);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawDirectivity(float dipoleWeight, float dipolePower)
        {
            if (mDirectivityPreview == null)
            {
                mDirectivityPreview = new Texture2D(65, 65);
            }

            if (mDirectivitySamples == null)
            {
                mDirectivitySamples = new float[360];
                mDirectivityPositions = new Vector2[360];
            }

            for (var i = 0; i < mDirectivitySamples.Length; ++i)
            {
                var theta = (i / 360.0f) * (2.0f * Mathf.PI);
                mDirectivitySamples[i] = Mathf.Pow(Mathf.Abs((1.0f - dipoleWeight) + dipoleWeight * Mathf.Cos(theta)), dipolePower);

                var r = 31 * Mathf.Abs(mDirectivitySamples[i]);
                var x = r * Mathf.Cos(theta) + 32;
                var y = r * Mathf.Sin(theta) + 32;
                mDirectivityPositions[i] = new Vector2(-y, x);
            }

            for (var v = 0; v < mDirectivityPreview.height; ++v)
            {
                for (var u = 0; u < mDirectivityPreview.width; ++u)
                {
                    mDirectivityPreview.SetPixel(u, v, Color.gray);
                }
            }

            for (var u = 0; u < mDirectivityPreview.width; ++u)
            {
                mDirectivityPreview.SetPixel(u, 32, Color.black);
            }

            for (var v = 0; v < mDirectivityPreview.height; ++v)
            {
                mDirectivityPreview.SetPixel(32, v, Color.black);
            }

            for (var i = 0; i < mDirectivitySamples.Length; ++i)
            {
                var color = (mDirectivitySamples[i] > 0.0f) ? Color.red : Color.blue;
                mDirectivityPreview.SetPixel((int) mDirectivityPositions[i].x, (int) mDirectivityPositions[i].y, color);
            }

            mDirectivityPreview.Apply();

            EditorGUILayout.PrefixLabel("Preview");
            EditorGUILayout.Space();
            var rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            var center = rect.center;
            center.x += 4;
            rect.center = center;
            rect.width = 65;
            rect.height = 65;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUI.DrawPreviewTexture(rect, mDirectivityPreview);
        }
    }
}
