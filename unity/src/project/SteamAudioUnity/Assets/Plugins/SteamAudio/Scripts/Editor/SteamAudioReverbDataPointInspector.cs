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
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Data;
using System.Collections.Generic;

namespace SteamAudio
{
    [CustomEditor(typeof(SteamAudioReverbDataPoint)), CanEditMultipleObjects]
    public class SteamAudioReverbDataPointInspector : Editor
    {
#if STEAMAUDIO_ENABLED
        private int[] allowedSampleRates = new int[] { 24000, 44100, 48000, 96000 };
        private GUIContent[] displaySampleRates = new GUIContent[] { new("24000 Hz"), new("44100 Hz"), new("48000 Hz"), new("96000 Hz") };

        // Is there a reason SerializedProperties are public? Causing serialization warning
        public SerializedProperty mSamplingRate;
        readonly GUIContent mSamplingRateGUI = new("Sampling Rate", "The frequency (Hz) used to generate the impulse response (IR). " +
        "Match this to the runtime audio engine’s sample rate; otherwise the IR must be resampled.");
        public SerializedProperty mAmbisonicOrder;
        readonly GUIContent mAmbisonicsOrderGUI = new("Ambisonic Order", "Specifies the Ambisonic order of the IRs generated when baking reverb. " +
        "Increasing this value results in more accurate directional variation of reflected sound, at the cost of increased disk space usage.");
        public SerializedProperty mReverbDuration;
        readonly GUIContent mReverbDurationGUI = new("Reverb Duration (seconds)", "The duration (in seconds) of the IRs generated when baking reverb. " +
        "Increasing this value results in longer, more accurate reverb tails, at the cost of increased disk space usage.");
        public SerializedProperty mReverbDataAsset;
        readonly GUIContent mReverbDataAssetGUI = new("Reverb Data", "Reference to the generated <b>Steam Audio Reverb Data</b> asset containing " +
        "this probe’s baked reverb. These assets are automatically placed in the <i><color=red>Assets/Resources/ReverbData</color></i> folder.");
        public SerializedProperty mStoreEnergyField;
        readonly GUIContent mStoreEnergyFieldGUI = new("Store Energy Field", "When enabled, saves Steam Audio’s intermediate multi-band Energy Field data.");
        public SerializedProperty mStoreImpulseResponse;
        readonly GUIContent mStoreImpulseResponseGUI = new("Store Impulse Response", "When enabled, stores the full multi-channel, " +
        "time-domain IR at the chosen sampling rate.");
        private Editor mReverbDataEditor;

        static bool mShouldShowProgressBar = false;

        private void OnEnable()
        {
            mSamplingRate = serializedObject.FindProperty("sampleRate");
            mAmbisonicOrder = serializedObject.FindProperty("ambisonicOrder");
            mReverbDuration = serializedObject.FindProperty("reverbDuration");
            mReverbDataAsset = serializedObject.FindProperty("reverbData");
            mStoreEnergyField = serializedObject.FindProperty("storeEnergyField");
            mStoreImpulseResponse = serializedObject.FindProperty("storeImpulseResponse");
        }

        [MenuItem("GameObject/Steam Audio/Steam Audio Reverb Data Point", false, 10)]
        static void CreateGameObjectWithProbe(MenuCommand menuCommand)
        {
            var gameObject = new GameObject("Steam Audio Reverb Data Point");
            gameObject.AddComponent<SteamAudioReverbDataPoint>();

            GameObjectUtility.SetParentAndAlign(gameObject, menuCommand.context as GameObject);

            Undo.RegisterCreatedObjectUndo(gameObject, "Create " + gameObject.name);
        }

        [MenuItem("Steam Audio/Steam Audio Reverb Data Point/Bake All", false, 61)]
        public static void BakeAllProbes()
        {
            var allProbes = FindObjectsByType<SteamAudioReverbDataPoint>(FindObjectsSortMode.None);

            if (allProbes.Length == 0)
            {
                EditorUtility.DisplayDialog("No Steam Audio Reverb Data Points Found", "No Steam Audio Reverb Data Point components were found in the currently-open scene.", "OK");
                return;
            }

            SteamAudioReverbDataPoint.BeginBake(allProbes);
            mShouldShowProgressBar = true;
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        [MenuItem("Steam Audio/Steam Audio Reverb Data Point/Clear Unreferenced Data", false, 62)]
        public static void DeleteUnreferencedAssets()
        {
            AssetDatabase.StartAssetEditing();

            // Find all assets in SteamAudioProbe.GetAssetFolderPath() path.
            string[] reverbDataFolder = new string[] { SteamAudioReverbDataPoint.GetAssetFolderPath() };
            string assetType = typeof(SteamAudioReverbData).Name;

            // Createa a list of all SteamAudioReverbData Assets which are inside reverbDataFolder folder.
            string[] reverbDataAssets = AssetDatabase.FindAssets("t:" + assetType, reverbDataFolder);

            if (reverbDataAssets.Length == 0)
            {
                Debug.Log("No cleanup needed. No Steam Audio Reverb Data assets found in " + reverbDataFolder[0] );
                AssetDatabase.StopAssetEditing();
                return;
            }

            // Go through all the assets and make a list of referenced SteamAudioProbe Assets.
            List<string> referencedReverbDataAssetPaths = new();
            var assetPaths = AssetDatabase.GetAllAssetPaths();
            foreach (var assetPath in assetPaths)
            {
                if (!(assetPath.EndsWith(".unity") || assetPath.EndsWith(".prefab")))
                    continue;

                var assetDependencies = AssetDatabase.GetDependencies(assetPath, false);
                foreach (var assetDependency in assetDependencies)
                {
                    if (assetDependency.Contains(reverbDataFolder[0]))
                        referencedReverbDataAssetPaths.Add(assetDependency);
                }
            }

            // Go through all the assets of type SteamAudioReverbData inside reverbDataFolder.
            // Delete the once which are not found in referenced SteamAudioProbe Assets found above.
            int numReverbDataAssetsDeleted = 0;
            foreach (var reverbDataAsset in reverbDataAssets)
            {
                var reverbDataAssetPath = AssetDatabase.GUIDToAssetPath(reverbDataAsset);
                if (string.IsNullOrEmpty(reverbDataAssetPath))
                    continue;

                bool deleteReverbDataAsset = !referencedReverbDataAssetPaths.Contains(reverbDataAssetPath);
                if (deleteReverbDataAsset)
                {
                    bool deleteSuccessful = AssetDatabase.DeleteAsset(reverbDataAssetPath);
                    var deleteSuccessfulString = "Delete Failed:";
                    if (deleteSuccessful)
                    {
                        ++numReverbDataAssetsDeleted;
                        deleteSuccessfulString = "Deleted:";
                    }

                    Debug.Log(deleteSuccessfulString + " " + reverbDataAssetPath);
                }
            }

            Debug.Log("Number of SteamAudioReverbData Assets deleted: " + numReverbDataAssetsDeleted);
            AssetDatabase.StopAssetEditing();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var oldGUIEnabled = GUI.enabled;
            GUI.enabled = !Baker.IsBakeActive() && !EditorApplication.isPlayingOrWillChangePlaymode;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Reverb Settings", EditorStyles.boldLabel);

            var tgt = target as SteamAudioReverbDataPoint;
            int sampleRateIndex = System.Array.IndexOf(allowedSampleRates, tgt.sampleRate);
            if (sampleRateIndex < 0)
            {
                sampleRateIndex = 0;
                tgt.sampleRate = allowedSampleRates[sampleRateIndex];
            }

            EditorGUI.showMixedValue = mSamplingRate.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            int newSampleRate = EditorGUILayout.IntPopup(mSamplingRateGUI, tgt.sampleRate, displaySampleRates, allowedSampleRates);
            var selectedProbes = targets.Cast<SteamAudioReverbDataPoint>().ToArray();

            if (EditorGUI.EndChangeCheck())
            {
                foreach (SteamAudioReverbDataPoint probe in selectedProbes)
                {
                    if (newSampleRate != probe.sampleRate)
                    {
                        probe.sampleRate = newSampleRate;
                        EditorUtility.SetDirty(probe);
                    }
                }
            }

            EditorGUILayout.PropertyField(mAmbisonicOrder, mAmbisonicsOrderGUI);
            EditorGUILayout.PropertyField(mReverbDuration, mReverbDurationGUI);
            EditorGUILayout.PropertyField(mStoreEnergyField, mStoreEnergyFieldGUI);
            EditorGUILayout.PropertyField(mStoreImpulseResponse, mStoreImpulseResponseGUI);

            GUI.enabled = !Baker.IsBakeActive() && !EditorApplication.isPlayingOrWillChangePlaymode;
            EditorGUILayout.Space();
            string bakeButtonString = serializedObject.isEditingMultipleObjects ? "Bake Selected Probes" : "Bake";
            if (GUILayout.Button(bakeButtonString))
            {
                SteamAudioReverbDataPoint.BeginBake(selectedProbes);
                mShouldShowProgressBar = true;
                EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            }

            GUI.enabled = oldGUIEnabled;

            if (mShouldShowProgressBar && !Baker.IsBakeActive())
            {
                mShouldShowProgressBar = false;
            }

            if (mShouldShowProgressBar)
            {
                Baker.DrawProgressBar();
                Repaint();
            }

            // Display Stats
            EditorGUILayout.PropertyField(mReverbDataAsset, mReverbDataAssetGUI);
            EditorGUILayout.Space();
            if (mReverbDataAsset.objectReferenceValue != null)
            {
                CreateCachedEditor(mReverbDataAsset.objectReferenceValue, null, ref mReverbDataEditor);

                // Pass the flag down for multi editing.
                if (mReverbDataEditor is ReverbDataEditor reverbDataEditor)
                    reverbDataEditor.mOwnerIsMultiEditing = serializedObject.isEditingMultipleObjects;

                mReverbDataEditor.OnInspectorGUI();
            }

            GUI.enabled = oldGUIEnabled;
            serializedObject.ApplyModifiedProperties();
        }

#else
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Steam Audio is not supported for the target platform or STEAMAUDIO_ENABLED define symbol is missing.", MessageType.Warning);
        }
#endif

    }
}
