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
    [CustomEditor(typeof(SteamAudioMaterial))]
    [CanEditMultipleObjects]
    public class SteamAudioMaterialInspector : Editor
    {
        SerializedProperty lowFreqAbsorption;
        readonly GUIContent lowFreqAbsorptionGUI = new("Low Freq Absorption", "Specifies how much sound the material absorbs at low frequencies (up to 800 Hz).");
        SerializedProperty midFreqAbsorption;
        readonly GUIContent midFreqAbsorptionGUI = new("Mid Freq Absorption", "Specifies how much sound the material absorbs at middle frequencies (800 Hz - 8 kHz).");
        SerializedProperty highFreqAbsorption;
        readonly GUIContent highFreqAbsorptionGUI = new("High Freq Absorption", "Specifies how much sound the material absorbs at high frequencies (8 kHz and above).");
        SerializedProperty scattering;
        readonly GUIContent scatteringGUI = new("Scattering", "Specifies the \"roughness\" of the material when reflecting sound. " +
        "0 = Sound is reflected in a perfectly mirror-like manner. 1 = Sound is reflected randomly in all directions.");
        SerializedProperty lowFreqTransmission;
        readonly GUIContent lowFreqTransmissionGUI = new("Low Freq Transmission", "Specifies how much sound the material transmits at low frequencies (up to 800 Hz).");
        SerializedProperty midFreqTransmission;
        readonly GUIContent midFreqTransmissionGUI = new("Mid Freq Transmission", "Specifies how much sound the material transmits at middle frequencies (800 Hz - 8 kHz).");
        SerializedProperty highFreqTransmission;
        readonly GUIContent highFreqTransmissionGUI = new("High Freq Transmission", "Specifies how much sound the material transmits at high frequencies (8 kHz and above).");

        private void OnEnable()
        {
            lowFreqAbsorption = serializedObject.FindProperty("lowFreqAbsorption");
            midFreqAbsorption = serializedObject.FindProperty("midFreqAbsorption");
            highFreqAbsorption = serializedObject.FindProperty("highFreqAbsorption");
            scattering = serializedObject.FindProperty("scattering");
            lowFreqTransmission = serializedObject.FindProperty("lowFreqTransmission");
            midFreqTransmission = serializedObject.FindProperty("midFreqTransmission");
            highFreqTransmission = serializedObject.FindProperty("highFreqTransmission");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(lowFreqAbsorption, lowFreqAbsorptionGUI);
            EditorGUILayout.PropertyField(midFreqAbsorption, midFreqAbsorptionGUI);
            EditorGUILayout.PropertyField(highFreqAbsorption, highFreqAbsorptionGUI);
            EditorGUILayout.PropertyField(scattering, scatteringGUI);
            EditorGUILayout.PropertyField(lowFreqTransmission, lowFreqTransmissionGUI);
            EditorGUILayout.PropertyField(midFreqTransmission, midFreqTransmissionGUI);
            EditorGUILayout.PropertyField(highFreqTransmission, highFreqTransmissionGUI);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
