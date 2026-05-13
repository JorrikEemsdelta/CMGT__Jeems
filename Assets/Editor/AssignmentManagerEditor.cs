using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AssignmentManager))]
public class AssignmentManagerEditor : Editor
{
    private string[] availableDates = new string[] { "Geen datums gevonden" };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        BookingManager bm = Object.FindFirstObjectByType<BookingManager>();
        
        if (bm != null && bm.weekDates != null && bm.weekDates.Length > 0)
        {
            availableDates = bm.weekDates;
        }

        SerializedProperty assignments = serializedObject.FindProperty("activeAssignments");

        EditorGUILayout.LabelField("Assignment System Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        for (int i = 0; i < assignments.arraySize; i++)
        {
            SerializedProperty item = assignments.GetArrayElementAtIndex(i);
            
            EditorGUILayout.BeginVertical("box");
            
            EditorGUILayout.BeginHorizontal();
            item.isExpanded = EditorGUILayout.Foldout(item.isExpanded, $"Assignment {i + 1}", true);
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                assignments.DeleteArrayElementAtIndex(i);
                break; 
            }
            EditorGUILayout.EndHorizontal();

            if (item.isExpanded)
            {
                SerializedProperty category = item.FindPropertyRelative("category");
                EditorGUILayout.PropertyField(category, new GUIContent("Make Question or Booking?"));

                if (category.enumValueIndex == (int)AssignmentCategory.Question)
                {
                    SerializedProperty qType = item.FindPropertyRelative("questionType");
                    EditorGUILayout.PropertyField(qType, new GUIContent("Question Type"));
                }

                EditorGUILayout.PropertyField(item.FindPropertyRelative("assignmentText"), new GUIContent("Instruction Text"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("isCompleted"));

                EditorGUILayout.Space();

                if (category.enumValueIndex == (int)AssignmentCategory.Question)
                {
                    EditorGUILayout.LabelField("Expected Answer", EditorStyles.boldLabel);
                    SerializedProperty qType = item.FindPropertyRelative("questionType");
                    
                    if (qType.enumValueIndex == (int)AssignmentQuestionType.AmountOfPeople)
                    {
                        EditorGUILayout.PropertyField(item.FindPropertyRelative("expectedIntAnswer"), new GUIContent("Correct Number"));
                    }
                    else 
                    {
                        EditorGUILayout.PropertyField(item.FindPropertyRelative("expectedStringAnswer"), new GUIContent("Correct Text"));
                    }
                }
                else if (category.enumValueIndex == (int)AssignmentCategory.Action)
                {
                    EditorGUILayout.LabelField("Booking Requirements", EditorStyles.boldLabel);
                    
                    DrawDateToggleField(item, "requireDate", "targetDate", "Target Date");
                    DrawToggleField(item, "requireStartTime", "targetStartHour", "Target Start Hour");
                    
                    // --- UPDATED to End Hour ---
                    DrawToggleField(item, "requireEndTime", "targetEndHour", "Target End Hour");
                    
                    DrawToggleField(item, "requireCapacity", "targetCapacity", "Target Capacity (People)");
                }
            }
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Add New Manual Assignment", GUILayout.Height(30)))
        {
            assignments.InsertArrayElementAtIndex(assignments.arraySize);
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Dynamic Generation", EditorStyles.boldLabel);

        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        if (GUILayout.Button("Force Fill To 3 Assignments", GUILayout.Height(30)))
        {
            ((AssignmentManager)target).EnsureThreeActiveAssignments();
        }
        EditorGUI.EndDisabledGroup();

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Random assignments rely on the computer's simulated bookings. This button is only available while the game is Playing.", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }

    void DrawToggleField(SerializedProperty item, string toggleName, string fieldName, string label)
    {
        EditorGUILayout.BeginHorizontal();
        SerializedProperty toggle = item.FindPropertyRelative(toggleName);
        EditorGUILayout.PropertyField(toggle, GUIContent.none, GUILayout.Width(20)); 
        
        EditorGUI.BeginDisabledGroup(!toggle.boolValue);
        EditorGUILayout.PropertyField(item.FindPropertyRelative(fieldName), new GUIContent(label));
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.EndHorizontal();
    }

    void DrawDateToggleField(SerializedProperty item, string toggleName, string fieldName, string label)
    {
        EditorGUILayout.BeginHorizontal();
        SerializedProperty toggle = item.FindPropertyRelative(toggleName);
        EditorGUILayout.PropertyField(toggle, GUIContent.none, GUILayout.Width(20)); 
        
        EditorGUI.BeginDisabledGroup(!toggle.boolValue);
        
        SerializedProperty dateProp = item.FindPropertyRelative(fieldName);
        
        int currentIndex = System.Array.IndexOf(availableDates, dateProp.stringValue);
        if (currentIndex < 0) currentIndex = 0; 

        int newIndex = EditorGUILayout.Popup(label, currentIndex, availableDates);
        
        dateProp.stringValue = availableDates[newIndex];
        
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.EndHorizontal();
    }
}