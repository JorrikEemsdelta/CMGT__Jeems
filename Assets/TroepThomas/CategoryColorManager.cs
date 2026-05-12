using System.Collections.Generic;
using UnityEngine;

public class CategoryColorManager : MonoBehaviour
{
    [System.Serializable]
    public class CategoryDefault
    {
        public string categorieNaam;
        public Color defaultKleur = Color.gray;
    }

    public List<CategoryDefault> presets;

    public Color GetColorForCategory(string naam, Color fallbackKleur)
    {
        foreach (var preset in presets)
        {
            // We negeren hoofdletters voor het gemak
            if (preset.categorieNaam.ToLower() == naam.ToLower())
            {
                return preset.defaultKleur;
            }
        }
        return fallbackKleur;
    }
}
