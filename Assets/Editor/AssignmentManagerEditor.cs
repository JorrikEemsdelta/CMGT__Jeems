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

        // ==========================================
        // NARRATIVE DESIGN POOL
        // ==========================================
        EditorGUILayout.LabelField("--- NARRATIVE DESIGN POOL ---", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Gebruik de knoppen hieronder om de tekst op te maken (zoals in Word!).", MessageType.Info);
        
        SerializedProperty narratives = serializedObject.FindProperty("narrativeScenarios");

        for (int i = 0; i < narratives.arraySize; i++)
        {
            SerializedProperty narrativeItem = narratives.GetArrayElementAtIndex(i);
            
            EditorGUILayout.BeginVertical("helpbox");
            EditorGUILayout.BeginHorizontal();
            
            SerializedProperty cat = narrativeItem.FindPropertyRelative("category");
            EditorGUILayout.PropertyField(cat, GUIContent.none, GUILayout.Width(100));
            
            if (GUILayout.Button("Delete", GUILayout.Width(60)))
            {
                narratives.DeleteArrayElementAtIndex(i);
                break; 
            }
            EditorGUILayout.EndHorizontal();

            // --- THE NEW TOOLBAR ---
            EditorGUILayout.LabelField("Verhaal/Scenario:");
            SerializedProperty textProp = narrativeItem.FindPropertyRelative("narrativeText");
            DrawRichTextToolbar(textProp);
            
            // Replaced the basic property field with a larger Text Area so they have room to write
            textProp.stringValue = EditorGUILayout.TextArea(textProp.stringValue, GUILayout.Height(60));
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        if (GUILayout.Button("Nieuw Verhaal Toevoegen", GUILayout.Height(25)))
        {
            narratives.InsertArrayElementAtIndex(narratives.arraySize);
        }

        EditorGUILayout.Space(20);
        
        // ==========================================
        // ACTIVE ASSIGNMENTS
        // ==========================================
        EditorGUILayout.LabelField("--- ACTIVE GAME ASSIGNMENTS ---", EditorStyles.boldLabel);
        SerializedProperty assignments = serializedObject.FindProperty("activeAssignments");

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

                EditorGUILayout.Space();
                
                // --- THE NEW TOOLBAR FOR ACTIVE ASSIGNMENTS ---
                EditorGUILayout.LabelField("Narrative Flavor", EditorStyles.boldLabel);
                SerializedProperty activeTextProp = item.FindPropertyRelative("narrativeText");
                DrawRichTextToolbar(activeTextProp);
                activeTextProp.stringValue = EditorGUILayout.TextArea(activeTextProp.stringValue, GUILayout.Height(40));

                EditorGUILayout.Space();
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
        
        EditorGUI.BeginDisabledGroup(!Application.isPlaying);
        if (GUILayout.Button("Force Fill To 3 Assignments", GUILayout.Height(30)))
        {
            ((AssignmentManager)target).EnsureThreeActiveAssignments();
        }
        EditorGUI.EndDisabledGroup();

        serializedObject.ApplyModifiedProperties();
    }

    // ==========================================
    // THE MAGIC WORD-STYLE TOOLBAR
    // ==========================================
    void DrawRichTextToolbar(SerializedProperty textProperty)
    {
        EditorGUILayout.BeginHorizontal();
        
        // Bold Button
        if (GUILayout.Button(new GUIContent("B", "Maak tekst dikgedrukt"), EditorStyles.miniButtonLeft, GUILayout.Width(30)))
        {
            textProperty.stringValue += "<b>tekst</b>";
            // Force the UI to refresh so they see it instantly
            GUI.FocusControl(null); 
        }
        
        // Italic Button
        if (GUILayout.Button(new GUIContent("I", "Maak tekst schuingedrukt"), EditorStyles.miniButtonMid, GUILayout.Width(30)))
        {
            textProperty.stringValue += "<i>tekst</i>";
            GUI.FocusControl(null);
        }

        // Underline Button
        if (GUILayout.Button(new GUIContent("U", "Onderstreep tekst"), EditorStyles.miniButtonMid, GUILayout.Width(30)))
        {
            textProperty.stringValue += "<u>tekst</u>";
            GUI.FocusControl(null);
        }

        // Red Color Button
        if (GUILayout.Button(new GUIContent("Rood", "Maak tekst rood"), EditorStyles.miniButtonMid, GUILayout.Width(50)))
        {
            textProperty.stringValue += "<color=red>tekst</color>";
            GUI.FocusControl(null);
        }
        
        // Blue Color Button
        if (GUILayout.Button(new GUIContent("Blauw", "Maak tekst blauw"), EditorStyles.miniButtonRight, GUILayout.Width(50)))
        {
            textProperty.stringValue += "<color=blue>tekst</color>";
            GUI.FocusControl(null);
        }

        EditorGUILayout.EndHorizontal();
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