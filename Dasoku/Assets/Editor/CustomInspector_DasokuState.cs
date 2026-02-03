using UnityEditor;

[CustomEditor(typeof(DasokuStateObject))]
public class CustomInspector_DasokuState : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty typeProp = serializedObject.FindProperty("commons.kinds");
        EditorGUILayout.PropertyField(typeProp);

        var type = (DasokuStateObject.DASOKUSTATE_KINDS)typeProp.enumValueIndex;

        /* 共通表示 */
        // 選択中の表示オブジェクト
        EditorGUILayout.PropertyField(serializedObject.FindProperty("commons"));

        // enumの値によって特定の表示を行う
        switch (type)
        {
            case DasokuStateObject.DASOKUSTATE_KINDS.addKey:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("KeyPrefab"));
                break;

            case DasokuStateObject.DASOKUSTATE_KINDS.dasokuDelete:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("deleteType"));
                break;

            case DasokuStateObject.DASOKUSTATE_KINDS.speedChange:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("moveType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("changeMoveSpeed"));
                break;

            case DasokuStateObject.DASOKUSTATE_KINDS.meterSpeedChenge:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("meterType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("changeMeterSpeed"));
                break;

            case DasokuStateObject.DASOKUSTATE_KINDS.addBody:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("addBodyType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("addBodyNum"));
                break;

        }

        serializedObject.ApplyModifiedProperties();
    }
}
