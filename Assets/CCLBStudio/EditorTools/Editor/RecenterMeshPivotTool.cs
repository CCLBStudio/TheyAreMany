using UnityEngine;
using UnityEditor;

public class RecenterMeshPivotTool : EditorWindow
{
    private Mesh sourceMesh;

    [MenuItem("Tools/Recenter Mesh Pivot")]
    public static void ShowWindow()
    {
        // Création de la fenêtre
        GetWindow<RecenterMeshPivotTool>("Recenter Pivot");
    }

    private void OnGUI()
    {
        GUILayout.Label("Paramètres", EditorStyles.boldLabel);
        
        // Champ pour glisser-déposer le mesh d'origine
        sourceMesh = (Mesh)EditorGUILayout.ObjectField("Mesh Source", sourceMesh, typeof(Mesh), false);

        EditorGUILayout.Space();

        // Désactive le bouton si aucun mesh n'est assigné
        GUI.enabled = sourceMesh != null;
        
        if (GUILayout.Button("Centrer le pivot et sauvegarder"))
        {
            ProcessAndSaveMesh();
        }
        
        GUI.enabled = true;
    }

    private void ProcessAndSaveMesh()
    {
        // 1. Demander le chemin de sauvegarde via une fenêtre native
        string defaultName = sourceMesh.name + "_Centered.asset";
        string path = EditorUtility.SaveFilePanelInProject(
            "Sauvegarder le nouveau Mesh",
            defaultName,
            "asset",
            "Choisissez l'emplacement de sauvegarde de votre nouveau Mesh"
        );

        if (string.IsNullOrEmpty(path)) 
            return; // L'utilisateur a annulé

        try
        {
            // 2. Dupliquer le mesh pour conserver l'original intact
            Mesh newMesh = Instantiate(sourceMesh);
            newMesh.name = sourceMesh.name + "_Centered";

            // 3. Calculer le décalage (le centre actuel de la Bounding Box)
            Vector3 centerOffset = newMesh.bounds.center;
            Vector3[] vertices = newMesh.vertices;

            // 4. Soustraire ce centre à tous les sommets pour ramener le pivot à (0,0,0)
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] -= centerOffset;
            }

            // 5. Appliquer les nouveaux sommets et recalculer les limites
            newMesh.vertices = vertices;
            newMesh.RecalculateBounds();

            // 6. Créer l'asset sur le disque
            AssetDatabase.CreateAsset(newMesh, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Surligne le nouvel asset dans la fenêtre Project
            EditorGUIUtility.PingObject(newMesh);
            Debug.Log($"<b>Succès :</b> Le mesh recentré a été sauvegardé ici : {path}");
        }
        catch (UnityException e)
        {
            Debug.LogError($"Impossible de lire les données du Mesh. Assurez-vous que l'option 'Read/Write' est cochée dans les paramètres d'importation du modèle 3D. Erreur : {e.Message}");
        }
    }
}