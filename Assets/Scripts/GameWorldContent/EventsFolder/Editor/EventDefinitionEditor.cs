using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EventDefinition))]
public class EventDefinitionEditor : Editor
{
    private SerializedProperty eventId;
    private SerializedProperty title;
    private SerializedProperty description;
    private SerializedProperty triggerType;
    private SerializedProperty triggerParameters;
    private SerializedProperty chance;
    private SerializedProperty minDay;
    private SerializedProperty canRepeat;
    private SerializedProperty cooldownDays;
    private SerializedProperty consequenceMode;
    private SerializedProperty fixedConsequence;
    private SerializedProperty choiceConsequences;

    private void OnEnable()
    {
        eventId = serializedObject.FindProperty("eventId");
        title = serializedObject.FindProperty("title");
        description = serializedObject.FindProperty("description");
        triggerType = serializedObject.FindProperty("triggerType");
        triggerParameters = serializedObject.FindProperty("triggerParameters");
        chance = serializedObject.FindProperty("chance");
        minDay = serializedObject.FindProperty("minDay");
        canRepeat = serializedObject.FindProperty("canRepeat");
        cooldownDays = serializedObject.FindProperty("cooldownDays");
        consequenceMode = serializedObject.FindProperty("consequenceMode");
        fixedConsequence = serializedObject.FindProperty("fixedConsequence");
        choiceConsequences = serializedObject.FindProperty("choiceConsequences");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Event", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(eventId, new GUIContent("ID"));
        EditorGUILayout.PropertyField(title, new GUIContent("Название"));
        EditorGUILayout.PropertyField(description, new GUIContent("Описание"));

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Trigger", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(triggerType, new GUIContent("Trigger Type"));
        DrawTriggerParameters((GameEventTriggerType)triggerType.enumValueIndex);
        EditorGUILayout.PropertyField(chance, new GUIContent("Шанс срабатывания"));
        EditorGUILayout.PropertyField(minDay, new GUIContent("Минимальный день"));
        EditorGUILayout.PropertyField(canRepeat, new GUIContent("Повторяется"));
        EditorGUILayout.PropertyField(cooldownDays, new GUIContent("Кулдаун, дней"));

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Consequences", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(consequenceMode, new GUIContent("Режим последствия"));

        GameEventConsequenceMode mode = (GameEventConsequenceMode)consequenceMode.enumValueIndex;
        if (mode == GameEventConsequenceMode.PlayerChoice)
            EditorGUILayout.PropertyField(choiceConsequences, new GUIContent("Варианты последствий"), true);
        else
            EditorGUILayout.PropertyField(fixedConsequence, new GUIContent("Последствие"), true);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawTriggerParameters(GameEventTriggerType type)
    {
        switch (type)
        {
            case GameEventTriggerType.PathBuilt:
            case GameEventTriggerType.PathRemoved:
                DrawPathSelector();
                break;
            case GameEventTriggerType.PathBuiltWithLength:
                DrawParameter("tileCount", "Количество тайлов");
                break;
            case GameEventTriggerType.TrainTravelledPathTimes:
                DrawParameter("passCount", "Количество проездов");
                break;
            case GameEventTriggerType.TrainDeliveredCargo:
                DrawParameter("cargoType", "Тип груза");
                DrawParameter("cargoCount", "Количество");
                DrawParameter("destinationStationName", "Станция назначения");
                break;
            case GameEventTriggerType.TrainArrivedAtStation:
                DrawParameter("stationName", "Станция");
                DrawParameter("arrivalCount", "Количество прибытий");
                break;
            case GameEventTriggerType.TrainDestroyed:
                DrawParameter("count", "Количество");
                break;
            case GameEventTriggerType.FinanceAmountReached:
            case GameEventTriggerType.AmountSpent:
                DrawParameter("amount", "Сумма");
                break;
            case GameEventTriggerType.IncomeEarnedForPeriod:
                DrawParameter("amount", "Сумма");
                DrawParameter("timePeriodHours", "Период времени, часов");
                break;
            case GameEventTriggerType.StationPopulationDecreased:
            case GameEventTriggerType.StationPopulationIncreased:
                DrawParameter("populationCount", "Количество населения");
                DrawParameter("stationName", "Станция");
                break;
            case GameEventTriggerType.StationsConnected:
                DrawParameter("count", "Количество");
                break;
            case GameEventTriggerType.ArtifactObtained:
                DrawParameter("count", "Количество");
                break;
            case GameEventTriggerType.QuestCompleted:
                DrawParameter("count", "Количество");
                break;
            case GameEventTriggerType.TechnologyUnlocked:
                break;
            case GameEventTriggerType.SpecificDayReached:
                DrawParameter("dayNumber", "Номер дня");
                break;
            case GameEventTriggerType.SpecificHourReached:
                DrawParameter("hour", "Час");
                break;
            case GameEventTriggerType.TimePassed:
                DrawParameter("elapsedTimeAmount", "Количество");
                DrawParameter("elapsedTimeUnit", "Дни / часы");
                break;
            case GameEventTriggerType.RandomEvent:
                DrawParameter("checkIntervalHours", "Интервал проверки, часов");
                DrawParameter("randomEventChance", "Шанс появления");
                break;
            case GameEventTriggerType.PathPassesThroughBiome:
                DrawParameter("biomeType", "Тип биома");
                DrawParameter("pathLength", "Длина пути");
                break;
            case GameEventTriggerType.TrainCount:
                DrawParameter("trainBuiltCount", "Число поездов построено");
                break;
            case GameEventTriggerType.StationCount:
                DrawParameter("stationConnectedCount", "Число станций с путями");
                break;
        }
    }

    private void DrawPathSelector()
    {
        SerializedProperty mode = triggerParameters.FindPropertyRelative("stationParameterMode");
        EditorGUILayout.PropertyField(mode, new GUIContent("Параметр"));

        GameEventStationParameterMode stationMode = (GameEventStationParameterMode)mode.enumValueIndex;
        if (stationMode == GameEventStationParameterMode.Count)
        {
            DrawParameter("count", "Количество");
            return;
        }

        DrawParameter("startStationName", "Начальная станция");
        DrawParameter("endStationName", "Конечная станция");
    }

    private void DrawParameter(string propertyName, string label)
    {
        SerializedProperty property = triggerParameters.FindPropertyRelative(propertyName);
        if (property != null)
            EditorGUILayout.PropertyField(property, new GUIContent(label));
    }
}

[CustomPropertyDrawer(typeof(GameEventEffect))]
public class GameEventEffectDrawer : PropertyDrawer
{
    private const float VerticalGap = 2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty effectType = property.FindPropertyRelative("effectType");
        SerializedProperty floatAmount = property.FindPropertyRelative("floatAmount");
        SerializedProperty intAmount = property.FindPropertyRelative("intAmount");
        SerializedProperty resourceType = property.FindPropertyRelative("resourceType");

        Rect row = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(row, effectType, new GUIContent("Последствие"));

        GameEventEffectType type = (GameEventEffectType)effectType.enumValueIndex;
        row.y += EditorGUIUtility.singleLineHeight + VerticalGap;

        switch (type)
        {
            case GameEventEffectType.AddBalance:
            case GameEventEffectType.SubtractBalance:
                EditorGUI.PropertyField(row, floatAmount, new GUIContent("Сумма"));
                break;
            case GameEventEffectType.AddResearchPoints:
                EditorGUI.PropertyField(row, intAmount, new GUIContent("Количество очков"));
                break;
            case GameEventEffectType.ChangeTrainSpeed:
                EditorGUI.PropertyField(row, floatAmount, new GUIContent("Скорость"));
                break;
            case GameEventEffectType.AddStationProducedResource:
            case GameEventEffectType.AddStationRequiredResource:
                EditorGUI.PropertyField(row, resourceType, new GUIContent("Ресурс"));
                row.y += EditorGUIUtility.singleLineHeight + VerticalGap;
                EditorGUI.PropertyField(row, intAmount, new GUIContent("Количество"));
                break;
            case GameEventEffectType.AddStationPopulation:
            case GameEventEffectType.SubtractStationPopulation:
                EditorGUI.PropertyField(row, intAmount, new GUIContent("Количество"));
                break;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty effectType = property.FindPropertyRelative("effectType");
        GameEventEffectType type = (GameEventEffectType)effectType.enumValueIndex;

        int lineCount = type switch
        {
            GameEventEffectType.AddStationProducedResource => 3,
            GameEventEffectType.AddStationRequiredResource => 3,
            GameEventEffectType.AddBalance => 2,
            GameEventEffectType.SubtractBalance => 2,
            GameEventEffectType.AddResearchPoints => 2,
            GameEventEffectType.ChangeTrainSpeed => 2,
            GameEventEffectType.AddStationPopulation => 2,
            GameEventEffectType.SubtractStationPopulation => 2,
            _ => 1
        };

        return lineCount * EditorGUIUtility.singleLineHeight + (lineCount - 1) * VerticalGap;
    }
}
