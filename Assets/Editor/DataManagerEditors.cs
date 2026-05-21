using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SecurityManager))]
public class SecurityManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); 
        EditorGUILayout.Space(10);
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f); 
        if (GUILayout.Button("🗑️ Verwijder Security Data", GUILayout.Height(30))) ((SecurityManager)target).ClearSaveData();
        GUI.backgroundColor = Color.white; 
    }
}

[CustomEditor(typeof(GeneralReportManager))]
public class GeneralReportManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); 
        EditorGUILayout.Space(10);
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("🗑️ Verwijder Algemene Meldingen Data", GUILayout.Height(30))) ((GeneralReportManager)target).ClearSaveData();
        GUI.backgroundColor = Color.white;
    }
}

[CustomEditor(typeof(BookingManager))]
public class BookingManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); 
        EditorGUILayout.Space(10);
        GUI.backgroundColor = new Color(0.4f, 0.8f, 1f); 
        if (GUILayout.Button("💻 Genereer Computer Boekingen", GUILayout.Height(30))) ((BookingManager)target).GenerateDummyBookings();
        EditorGUILayout.Space(5);
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("🗑️ Verwijder Speler Boekingen", GUILayout.Height(30))) ((BookingManager)target).ClearSaveData();
        GUI.backgroundColor = Color.white;
    }
}

[CustomEditor(typeof(AssignmentManager))]
public class AssignmentManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxActiveTasks"), new GUIContent("Max Actieve Opdrachten"));
        
        EditorGUILayout.Space(10);
        
        EditorGUILayout.LabelField("Vragenlijsten (Generator Bronnen)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("securityPrompts"), new GUIContent("Beveiligingsincidenten"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("dataBreachPrompts"), new GUIContent("Datalekken"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("generalReportPrompts"), new GUIContent("Algemene Meldingen"), true);
        
        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Huidige Opdrachtenlijst (Actief)", EditorStyles.boldLabel);

        SerializedProperty assignmentsProp = serializedObject.FindProperty("assignments");
        
        for (int i = 0; i < assignmentsProp.arraySize; i++)
        {
            SerializedProperty element = assignmentsProp.GetArrayElementAtIndex(i);
            
            EditorGUILayout.BeginVertical(GUI.skin.box);
            
            EditorGUILayout.PropertyField(element.FindPropertyRelative("title"), new GUIContent("Titel"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("description"), new GUIContent("Omschrijving"));
            
            SerializedProperty typeProp = element.FindPropertyRelative("type");
            EditorGUILayout.PropertyField(typeProp, new GUIContent("Soort Opdracht"));
            
            int typeIndex = typeProp.enumValueIndex;
            
            if (typeIndex == (int)AssignmentType.RoomBooking)
            {
                EditorGUILayout.PropertyField(element.FindPropertyRelative("targetCategory"), new GUIContent("Vereiste Kamer (Optioneel)"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("targetPeople"), new GUIContent("Aantal Personen (Capaciteit check)"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("targetDate"), new GUIContent("Vereiste Datum"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("targetStartHour"), new GUIContent("Vereist Start Uur"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("targetEndHour"), new GUIContent("Vereist Eind Uur"));
            }
            else if (typeIndex == (int)AssignmentType.ManualQuestionText)
            {
                EditorGUILayout.PropertyField(element.FindPropertyRelative("correctTextAnswer"), new GUIContent("Verwacht Antwoord (Tekst)"));
            }
            else if (typeIndex == (int)AssignmentType.ManualQuestionNumber)
            {
                EditorGUILayout.PropertyField(element.FindPropertyRelative("correctNumberAnswer"), new GUIContent("Verwacht Antwoord (Getal)"));
            }
            else 
            {
                EditorGUILayout.PropertyField(element.FindPropertyRelative("targetCategory"), new GUIContent("Specifieke Categorie (Optioneel)"));
            }
            
            EditorGUILayout.PropertyField(element.FindPropertyRelative("isCompleted"), new GUIContent("Is Voltooid?"));
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(10);
        GUI.backgroundColor = new Color(1f, 0.7f, 0.3f); 
        if (GUILayout.Button("🔄 Reset Alle Opdrachten & Genereer Nieuwe", GUILayout.Height(30)))
        {
            ((AssignmentManager)target).ClearSaveData();
            EditorUtility.SetDirty(target); 
        }
        GUI.backgroundColor = Color.white; 
    }
}