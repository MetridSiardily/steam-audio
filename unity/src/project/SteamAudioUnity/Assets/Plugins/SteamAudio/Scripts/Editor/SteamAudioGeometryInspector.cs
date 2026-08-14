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
    [CustomEditor(typeof(SteamAudioGeometry))]
    [CanEditMultipleObjects]
    public class SteamAudioGeometryInspector : Editor
    {
#if STEAMAUDIO_ENABLED
        SerializedProperty mMaterial;
        readonly GUIContent mMaterialGUI = new("Material", "Reference to the Steam Audio Material asset that defines the acoustic properties of this geometry. " +
        "If not set, the project-wide default material is used.");
        SerializedProperty mExportAllChildren;
        readonly GUIContent mExportAllChildrenGUI = new("Export All Children", "When enabled, the geometry of all children with a <b>Mesh Filter</b> or <b>Terrain</b> " +
        "attached to them will be exported as acoustic geometry.");
        SerializedProperty mTerrainSimplificationLevel;
        readonly GUIContent mTerrainSimplificationLevelGUI = new("Terrain Simplification Level", "Geometry that is represented by a Terrain can be quite complex. " +
        "This can slow down the calculation of occlusion and sound propagation. To speed things up, you can adjust this slider. As you increase this value, " +
        "Steam Audio will reduce the level of detail in the terrain. This will result in faster simulation.");

        private void OnEnable()
        {
            mMaterial = serializedObject.FindProperty("material");
            mExportAllChildren = serializedObject.FindProperty("exportAllChildren");
            mTerrainSimplificationLevel = serializedObject.FindProperty("terrainSimplificationLevel");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var tgt = target as SteamAudioGeometry;

            EditorGUILayout.PropertyField(mMaterial, mMaterialGUI);

            if (tgt.transform.childCount != 0)
            {
                EditorGUILayout.PropertyField(mExportAllChildren, mExportAllChildrenGUI);
            }

            if (tgt.gameObject.GetComponent<Terrain>() != null)
            {
                EditorGUILayout.PropertyField(mTerrainSimplificationLevel, mTerrainSimplificationLevelGUI);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Geometry Statistics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Vertices", tgt.GetNumVertices().ToString());
            EditorGUILayout.LabelField("Triangles", tgt.GetNumTriangles().ToString());

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
