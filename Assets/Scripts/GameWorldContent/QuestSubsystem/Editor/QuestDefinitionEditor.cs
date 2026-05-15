#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(QuestCondition))]
public class QuestConditionDrawer : PropertyDrawer
{
    private const float Gap = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty conditionType = property.FindPropertyRelative("conditionType");
        SerializedProperty count = property.FindPropertyRelative("count");
        SerializedProperty resourceType = property.FindPropertyRelative("resourceType");
        SerializedProperty stationName = property.FindPropertyRelative("stationName");
        SerializedProperty startStationName = property.FindPropertyRelative("startStationName");
        SerializedProperty endStationName = property.FindPropertyRelative("endStationName");

        Rect row = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(row, conditionType, label);
        row.y += EditorGUIUtility.singleLineHeight + Gap;

        QuestConditionType type = (QuestConditionType)conditionType.enumValueIndex;
        switch (type)
        {
            case QuestConditionType.RailLineCount:
                EditorGUI.PropertyField(row, count, new GUIContent("Количество путей"));
                break;
            case QuestConditionType.TrainCount:
                EditorGUI.PropertyField(row, count, new GUIContent("Количество поездов"));
                break;
            case QuestConditionType.StationsFoundByName:
                EditorGUI.PropertyField(row, stationName, new GUIContent("Станция"));
                row.y += EditorGUIUtility.singleLineHeight + Gap;
                EditorGUI.PropertyField(row, startStationName, new GUIContent("Станция 2"));
                row.y += EditorGUIUtility.singleLineHeight + Gap;
                EditorGUI.PropertyField(row, endStationName, new GUIContent("Станция 3"));
                break;
            case QuestConditionType.StationsConnectedByName:
                EditorGUI.PropertyField(row, startStationName, new GUIContent("Начальная станция"));
                row.y += EditorGUIUtility.singleLineHeight + Gap;
                EditorGUI.PropertyField(row, endStationName, new GUIContent("Конечная станция"));
                break;
            case QuestConditionType.ResourceDeliveredBetweenStations:
                EditorGUI.PropertyField(row, resourceType, new GUIContent("Ресурс"));
                row.y += EditorGUIUtility.singleLineHeight + Gap;
                EditorGUI.PropertyField(row, count, new GUIContent("Количество"));
                row.y += EditorGUIUtility.singleLineHeight + Gap;
                EditorGUI.PropertyField(row, startStationName, new GUIContent("От станции"));
                row.y += EditorGUIUtility.singleLineHeight + Gap;
                EditorGUI.PropertyField(row, endStationName, new GUIContent("До станции"));
                break;
            case QuestConditionType.ArtifactCount:
                EditorGUI.PropertyField(row, count, new GUIContent("Количество артефактов"));
                break;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty conditionType = property.FindPropertyRelative("conditionType");
        QuestConditionType type = (QuestConditionType)conditionType.enumValueIndex;

        int lines = type switch
        {
            QuestConditionType.Manual => 1,
            QuestConditionType.RailLineCount => 2,
            QuestConditionType.TrainCount => 2,
            QuestConditionType.StationsFoundByName => 4,
            QuestConditionType.StationsConnectedByName => 3,
            QuestConditionType.ResourceDeliveredBetweenStations => 5,
            QuestConditionType.ArtifactCount => 2,
            _ => 1
        };

        return lines * EditorGUIUtility.singleLineHeight + (lines - 1) * Gap;
    }
}

[CustomPropertyDrawer(typeof(QuestReward))]
public class QuestRewardDrawer : PropertyDrawer
{
    private const float Gap = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty rewardType = property.FindPropertyRelative("rewardType");
        SerializedProperty floatAmount = property.FindPropertyRelative("floatAmount");
        SerializedProperty intAmount = property.FindPropertyRelative("intAmount");
        SerializedProperty artifactDefinition = property.FindPropertyRelative("artifactDefinition");

        Rect row = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(row, rewardType, new GUIContent("Награда"));
        row.y += EditorGUIUtility.singleLineHeight + Gap;

        QuestRewardType type = (QuestRewardType)rewardType.enumValueIndex;
        switch (type)
        {
            case QuestRewardType.AddBalance:
                EditorGUI.PropertyField(row, floatAmount, new GUIContent("Деньги"));
                break;
            case QuestRewardType.AddResearchPoints:
                EditorGUI.PropertyField(row, intAmount, new GUIContent("Очки исследования"));
                break;
            case QuestRewardType.AddArtifact:
                EditorGUI.PropertyField(row, artifactDefinition, new GUIContent("Artifact Definition"));
                break;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty rewardType = property.FindPropertyRelative("rewardType");
        QuestRewardType type = (QuestRewardType)rewardType.enumValueIndex;
        int lines = type == QuestRewardType.None ? 1 : 2;
        return lines * EditorGUIUtility.singleLineHeight + (lines - 1) * Gap;
    }
}
#endif
