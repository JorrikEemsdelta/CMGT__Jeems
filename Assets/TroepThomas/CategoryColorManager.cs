using System.Collections.Generic;
using UnityEngine;

public class CategoryColorManager : MonoBehaviour
{
    [System.Serializable]
    public class CategoryDefault
    {
        public string categoryName;
        public Color defaultColour = Color.gray;
    }

    public List<CategoryDefault> presets;

    public Color GetColorForCategory(string naam, Color fallbackColour)
    {
        foreach (var preset in presets)
        {
            if (preset.categoryName.ToLower() == naam.ToLower())
            {
                return preset.defaultColour;
            }
        }
        return fallbackColour;
    }
}

