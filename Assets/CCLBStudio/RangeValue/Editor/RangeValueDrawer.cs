using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(RangeFloat))]
[CustomPropertyDrawer(typeof(RangeInt))]
public class RangeValueDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Début de la propriété
        EditorGUI.BeginProperty(position, label, property);

        // Dessine le nom de la variable (ex: "Delay Between Spawns") 
        // et récupère l'espace restant pour les champs
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // On enlève l'indentation pour les sous-champs pour éviter un décalage
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        // Calcul de la taille des boîtes Min et Max
        float spacing = 5f;
        float halfWidth = (position.width - spacing) / 2f;
        
        Rect minRect = new Rect(position.x, position.y, halfWidth, position.height);
        Rect maxRect = new Rect(position.x + halfWidth + spacing, position.y, halfWidth, position.height);

        // Récupération des propriétés "min" et "max"
        SerializedProperty minProp = property.FindPropertyRelative("min");
        SerializedProperty maxProp = property.FindPropertyRelative("max");

        // On réduit temporairement la place que prennent les mots "Min" et "Max"
        float originalLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 30f; 

        // Dessin des champs
        EditorGUI.PropertyField(minRect, minProp, new GUIContent("Min"));
        EditorGUI.PropertyField(maxRect, maxProp, new GUIContent("Max"));

        // Restauration des paramètres par défaut
        EditorGUIUtility.labelWidth = originalLabelWidth;
        EditorGUI.indentLevel = indent;

        EditorGUI.EndProperty();
    }

    // On s'assure que la propriété ne prend qu'une seule ligne en hauteur
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}