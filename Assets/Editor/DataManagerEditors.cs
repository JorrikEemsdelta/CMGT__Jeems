using UnityEngine;
using UnityEditor;

// --- 1. SECURITY MANAGER BUTTON ---
[CustomEditor(typeof(SecurityManager))]
public class SecurityManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); 

        EditorGUILayout.Space(10);
        
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f); 
        if (GUILayout.Button("🗑️ Delete Security Save Data", GUILayout.Height(30)))
        {
            ((SecurityManager)target).ClearSaveData();
        }
        GUI.backgroundColor = Color.white; 
    }
}

// --- 2. GENERAL REPORT MANAGER BUTTON ---
[CustomEditor(typeof(GeneralReportManager))]
public class GeneralReportManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); 

        EditorGUILayout.Space(10);

        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("🗑️ Delete General Report Data", GUILayout.Height(30)))
        {
            ((GeneralReportManager)target).ClearSaveData();
        }
        GUI.backgroundColor = Color.white;
    }
}

// --- 3. BOOKING MANAGER BUTTONS ---
[CustomEditor(typeof(BookingManager))]
public class BookingManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); 

        EditorGUILayout.Space(10);

        // The blue Generate button
        GUI.backgroundColor = new Color(0.4f, 0.8f, 1f); 
        if (GUILayout.Button("💻 Genereer Computer Boekingen", GUILayout.Height(30)))
        {
            ((BookingManager)target).GenerateDummyBookings();
        }
        
        EditorGUILayout.Space(5);

        // The red Delete button
        GUI.backgroundColor = new Color(1f, 0.4f, 0.4f);
        if (GUILayout.Button("🗑️ Delete Player Booking Data", GUILayout.Height(30)))
        {
            ((BookingManager)target).ClearSaveData();
        }
        GUI.backgroundColor = Color.white;
    }
}