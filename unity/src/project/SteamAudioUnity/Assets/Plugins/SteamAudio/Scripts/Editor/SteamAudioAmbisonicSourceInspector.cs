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

using UnityEditor;
using UnityEngine;

namespace SteamAudio
{
    [CustomEditor(typeof(SteamAudioAmbisonicSource))]
    [CanEditMultipleObjects]
    public class SteamAudioAmbisonicSourceInspector : Editor
    {
        SerializedProperty mApplyHRTF;
        readonly GUIContent mApplyHRTFGUI = new("Apply HRTF", "When enabled, the Ambisonic audio clip is spatialized using HRTF-based binaural rendering. " +
        "Provides an improvement in spatialization quality at the cost of a slight increase in CPU usage. Default is Enabled.");

        private void OnEnable()
        {
            mApplyHRTF = serializedObject.FindProperty("applyHRTF");
        }

        [MenuItem("GameObject/Steam Audio/Steam Audio Ambisonic Source", false, 9)]
        static void CreateGameObjectWithProbeBatch(MenuCommand menuCommand)
        {
            var name = GameObjectUtility.GetUniqueNameForSibling(null, "Steam Audio Ambisonic Source");
            var gameObject = ObjectFactory.CreateGameObject(name, typeof(SteamAudioAmbisonicSource));
            UnityEditorInternal.ComponentUtility.MoveComponentUp(gameObject.GetComponent<SteamAudioAmbisonicSource>());

            ObjectFactory.PlaceGameObject(gameObject, menuCommand.context as GameObject);
            Selection.activeGameObject = gameObject;
            if (SteamAudioSettings.Singleton.audioEngine != AudioEngineType.Unity)
            {
                Debug.LogWarning("Steam Audio Ambisonic Source requires the audio engine to be set to Unity. Click" +
                "Steam Audio > Settings to change this.");
            }
        }

        public override void OnInspectorGUI()
        {
            if (SteamAudioSettings.Singleton.audioEngine != AudioEngineType.Unity)
            {
                EditorGUILayout.HelpBox(
                    "This component requires the audio engine to be set to Unity. Click" +
                    "Steam Audio > Settings to change this.", MessageType.Warning);

                return;
            }

            serializedObject.Update();

            EditorGUILayout.PropertyField(mApplyHRTF, mApplyHRTFGUI);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
