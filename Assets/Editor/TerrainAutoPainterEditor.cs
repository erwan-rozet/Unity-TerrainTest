using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainAutoPainter))]
public class TerrainAutoPainterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainAutoPainter painter = (TerrainAutoPainter)target;

        EditorGUILayout.LabelField("Auto Terrain Texturing", EditorStyles.boldLabel);

        painter.terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", painter.terrain, typeof(Terrain), true);

        SerializedProperty layersProp = serializedObject.FindProperty("terrainLayers");
        EditorGUILayout.PropertyField(layersProp, new GUIContent("Terrain Layers"), true);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Critères de texture", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck(); // ← Début détection des changements

        painter.lowHeight = EditorGUILayout.Slider("Hauteur basse (terre)", painter.lowHeight, 1f, 100f);
        painter.midHeight = EditorGUILayout.Slider("Hauteur moyenne (herbe)", painter.midHeight, 1f, 100f);
        painter.slopeThreshold = EditorGUILayout.Slider("Inclinaison max pour herbe", painter.slopeThreshold, 0f, 90f);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Blending (transitions)", EditorStyles.boldLabel);

        painter.heightBlendRange = EditorGUILayout.Slider("Blend Hauteur", painter.heightBlendRange, 0f, 20f);
        painter.slopeBlendRange = EditorGUILayout.Slider("Blend Pente", painter.slopeBlendRange, 0f, 30f);

        EditorGUILayout.Space();
        painter.autoUpdate = EditorGUILayout.Toggle("Auto Update", painter.autoUpdate);

        EditorGUILayout.Space();

        if (GUILayout.Button("🖌️ Appliquer maintenant"))
        {
            painter.ApplyAutoTexture();
        }

        // Auto-update si une valeur a changé
        if (EditorGUI.EndChangeCheck() && painter.autoUpdate)
        {
            painter.ApplyAutoTexture();
            EditorUtility.SetDirty(painter); // Marque comme modifié
        }

        serializedObject.ApplyModifiedProperties();
    }
}
