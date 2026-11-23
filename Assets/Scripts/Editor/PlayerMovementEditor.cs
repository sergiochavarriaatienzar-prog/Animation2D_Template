using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Inspector personalizado de PlayerMovement.
// Añade un botón de Flip por animación para que el artista pueda corregir
// animaciones dibujadas mirando hacia el lado contrario sin tocar código.
// Al estar en una carpeta "Editor", este script no se incluye en la build.
[CustomEditor(typeof(PlayerMovement))]
public class PlayerMovementEditor : Editor
{
    private const string SettingsPath = "Assets/Settings/PlayerAnimationFlip.asset";

    public override void OnInspectorGUI()
    {
        // Primero, el Inspector normal con todas las variables
        DrawDefaultInspector();

        PlayerMovement player = (PlayerMovement)target;

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("Animation Direction (Flip)", EditorStyles.boldLabel);

        // Sin asset de ajustes todavía: ofrecer crearlo
        if (player.flipSettings == null)
        {
            EditorGUILayout.HelpBox(
                "Create the Flip Settings once. Then you get one Flip button per animation.",
                MessageType.Info);

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Stop Play Mode to create the Flip Settings.", MessageType.Warning);
                return;
            }

            if (GUILayout.Button("Create Flip Settings", GUILayout.Height(28)))
            {
                CreateSettings(player);
            }
            return;
        }

        EditorGUILayout.HelpBox(
            "All animations must face RIGHT when the player moves right. " +
            "If one faces the wrong way, press its button. " +
            "It also works during Play Mode and the change is kept when you stop.",
            MessageType.Info);

        AnimationFlipSettings settings = player.flipSettings;

        foreach (AnimationFlipSettings.AnimationFlip animation in settings.animations)
        {
            EditorGUILayout.BeginHorizontal();

            string shortName = animation.stateName.Replace("Player_", "");
            EditorGUILayout.LabelField(shortName, GUILayout.Width(80));

            // Botón verde = normal, naranja = volteado
            Color previousColor = GUI.backgroundColor;
            GUI.backgroundColor = animation.flip ? new Color(1f, 0.6f, 0.2f) : new Color(0.5f, 0.9f, 0.5f);

            string label = animation.flip ? "Flipped  ◄" : "Normal  ►";
            if (GUILayout.Button(label))
            {
                Undo.RecordObject(settings, "Flip " + shortName);
                animation.flip = !animation.flip;
                EditorUtility.SetDirty(settings);
                PreviewInScene(player);
            }

            GUI.backgroundColor = previousColor;
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(6);
        if (GUILayout.Button("Flip ALL animations"))
        {
            Undo.RecordObject(settings, "Flip all animations");
            foreach (AnimationFlipSettings.AnimationFlip animation in settings.animations)
            {
                animation.flip = !animation.flip;
            }
            EditorUtility.SetDirty(settings);
            PreviewInScene(player);
        }
    }

    // Fuera de Play Mode, muestra en la escena cómo queda el Idle
    private void PreviewInScene(PlayerMovement player)
    {
        if (Application.isPlaying) return;

        SpriteRenderer spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) return;

        Undo.RecordObject(spriteRenderer, "Preview flip");
        spriteRenderer.flipX = player.flipSettings.IsFlipped("Player_Idle");
        EditorUtility.SetDirty(spriteRenderer);
    }

    private void CreateSettings(PlayerMovement player)
    {
        AnimationFlipSettings settings = AssetDatabase.LoadAssetAtPath<AnimationFlipSettings>(SettingsPath);

        if (settings == null)
        {
            settings = CreateInstance<AnimationFlipSettings>();
            AssetDatabase.CreateAsset(settings, SettingsPath);
            AssetDatabase.SaveAssets();
        }

        Undo.RecordObject(player, "Assign Flip Settings");
        player.flipSettings = settings;
        EditorUtility.SetDirty(player);
        EditorSceneManager.MarkSceneDirty(player.gameObject.scene);
    }
}
