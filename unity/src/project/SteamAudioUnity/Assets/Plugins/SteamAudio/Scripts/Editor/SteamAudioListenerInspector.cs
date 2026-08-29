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
    [CustomEditor(typeof(SteamAudioListener))]
    public class SteamAudioListenerInspector : Editor
    {
#if STEAMAUDIO_ENABLED
        SerializedProperty mCurrentBakedListener;
        readonly GUIContent mCurrentBakedListenerGUI = new("Current Baked Listener", "When simulating reflections for a source whose <b>Reflections Type</b> " +
        "is set to <b>Baked Static Listener</b>, the position and orientation of the GameObject specified in this field will be used as the position and " +
        "orientation of the listener.");
        SerializedProperty mApplyReverb;
        readonly GUIContent mApplyReverbGUI = new("Apply Reverb", "When enabled, listener-centric reverb will be simulated. " +
        "This allows the <b>Steam Audio Reverb</b> mixer effect to process audio of whatever channel it is assigned to.");
        SerializedProperty mReverbType;
        readonly GUIContent mReverbTypeGUI = new("Reverb Type", "Specifies how listener-centric reverb is simulated." +
        "\n<b>Realtime</b>. Rays are traced from the listener in real-time, and bounced around the scene to simulate reverberation. " +
        "This allows reverb to vary smoothly and account for dynamic geometry, at the cost of significant CPU usage." +
        "\n<b>Baked.</b> Baked reverb data is used to interpolate the reverberation at the listener position. " +
        "This prevents reverb from accounting for dynamic geometry and results in relatively low CPU usage, at the cost of increased memory and disk space usage.");
        SerializedProperty mUseAllProbeBatches;
        readonly GUIContent mUseAllProbeBatchesGUI = new("Use All Probe Batches", "When enabled, reverb data will be baked into every probe batch in the scene.");
        SerializedProperty mProbeBatches;
        readonly GUIContent mProbeBatchesGUI = new("Probe Batches", "If <b>Use All Probe Batches</b> is disabled, " +
        "this is a list of probe batches into which reverb data will be baked.");

        bool mStatsFoldout = false;
        static bool mShouldShowProgressBar = false;

        private void OnEnable()
        {
            mCurrentBakedListener = serializedObject.FindProperty("currentBakedListener");
            mApplyReverb = serializedObject.FindProperty("applyReverb");
            mReverbType = serializedObject.FindProperty("reverbType");
            mUseAllProbeBatches = serializedObject.FindProperty("useAllProbeBatches");
            mProbeBatches = serializedObject.FindProperty("probeBatches");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (SteamAudioSettings.Singleton.audioEngine == AudioEngineType.Unity)
            {
                var listenerTarget = target as SteamAudioListener; // Consider consolidating this into tgt var later in OnInspectorGUI()
                if (!listenerTarget.gameObject.TryGetComponent(out AudioListener aL))
                {
                    EditorGUILayout.HelpBox(
                    "Audio Engine is set to Unity but no Unity Audio Listener is on the object. This will cause a crash.", MessageType.Error);
                }
            }

            EditorGUILayout.PropertyField(mCurrentBakedListener, mCurrentBakedListenerGUI);

            EditorGUILayout.PropertyField(mApplyReverb, mApplyReverbGUI);
            if (mApplyReverb.boolValue)
            {
                EditorGUILayout.PropertyField(mReverbType, mReverbTypeGUI);
            }

            var oldGUIEnabled = GUI.enabled;
            GUI.enabled = !Baker.IsBakeActive() && !EditorApplication.isPlayingOrWillChangePlaymode;

            var tgt = target as SteamAudioListener;

            EditorGUILayout.PropertyField(mUseAllProbeBatches, mUseAllProbeBatchesGUI);
            if (!mUseAllProbeBatches.boolValue)
            {
                EditorGUILayout.PropertyField(mProbeBatches, mProbeBatchesGUI);
            }

            EditorGUILayout.Space();
            GUIContent bakeButtonGUI = mUseAllProbeBatches.boolValue ? new("Bake All Probe Batches", "Bake reverb data for every " +
            "probe batch in the scene.") : new("Bake Assigned Probe Batches", "Bake reverb data for probe batches assigned in <b>Probe Batches</b>.");
            if (GUILayout.Button(bakeButtonGUI))
            {
                tgt.BeginBake();
                mShouldShowProgressBar = true;
            }

            GUI.enabled = oldGUIEnabled;

            if (mShouldShowProgressBar && !Baker.IsBakeActive())
            {
                mShouldShowProgressBar = false;
            }

            if (mShouldShowProgressBar)
            {
                Baker.DrawProgressBar();
            }

            Repaint();

            EditorGUILayout.Space();
            mStatsFoldout = EditorGUILayout.Foldout(mStatsFoldout, "Baked Data Statistics");
            if (mStatsFoldout && !Baker.IsBakeActive())
            {
                for (var i = 0; i < tgt.GetProbeBatchesUsed().Length; ++i)
                {
                    EditorGUILayout.LabelField(tgt.GetProbeBatchesUsed()[i].gameObject.name, Common.HumanReadableDataSize(tgt.GetProbeDataSizes()[i]));
                }
                EditorGUILayout.LabelField("Total Size", Common.HumanReadableDataSize(tgt.GetTotalDataSize()));
            }

            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("Steam Audio/Steam Audio Listener/Bake All Reverb In Current Scene", false, 64)]
        public static void BakeAllReverbInScene()
        {
            var listeners = FindObjectsByType<SteamAudioListener>();
            if (listeners.Length == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Steam Audio Listeners Found",
                    "No Steam Audio Listener components were found in the currently-open scene.",
                    "OK");
                return;
            }

            SteamAudioListener.BeginBake(listeners);
            mShouldShowProgressBar = true;
        }
#else
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Steam Audio is not supported for the target platform or STEAMAUDIO_ENABLED define symbol is missing.", MessageType.Warning);
        }
#endif
    }
}
