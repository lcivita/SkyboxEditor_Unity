using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SkyboxEditor : EditorWindow
{
    private SkyboxEditorManager manager;
    
    private Gradient gradient;
    private Texture2D gradientTexture;
    private Vector2 lastWindowSize = Vector2.zero; // Track the window size to detect resizing
    
    private int textureHeight = 50;
    private FilterMode previewFilterMode = FilterMode.Point;

    private Texture2D previewIcon;

    [MenuItem("Tools/Skybox Editor")]
    private static void OpenSkyboxEditor()
    {
        GetWindow<SkyboxEditor>("Skybox Editor").Show();
    }

    private void OnEnable()
    {
        LoadSettings();
        
        if (gradient == null)
        {
            manager.InitializeGradient(out gradient);
        }
        GenerateGradientTexture();
        
        previewIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/SkyboxEditor/Icons/previewIcon.png", typeof(Texture2D));
    }

    private void OnGUI()
    {
        // Only regenerate the gradient texture if the window has been resized
        if (lastWindowSize.x != position.width || lastWindowSize.y != position.height)
        {
            GenerateGradientTexture();
            lastWindowSize.x = position.width;
            lastWindowSize.y = position.height;
        }
        if (gradientTexture != null)
        {
            DrawGradientBG();
            // DrawTextBG();
        }
        
        EditorGUI.BeginChangeCheck();
        gradient = EditorGUILayout.GradientField("Skybox Gradient", gradient);
        
        GUILayout.Space(20);
        
        GUIContent previewLabelContent = new GUIContent(" Preview Settings", previewIcon);
        GUILayout.Label(previewLabelContent);
        textureHeight = EditorGUILayout.IntSlider("Resolution", textureHeight, 5, 100);
        previewFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", previewFilterMode);
        if (EditorGUI.EndChangeCheck())
        {
            GenerateGradientTexture();
        }

        // push stuff beneath this to bottom of page
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reset Gradient"))
        {
            manager.InitializeGradient(out gradient); // Call your function here
        }
    }
    
    private void GenerateGradientTexture()
    {
        int textureWidth = 1; // Since the gradient is vertical, we only need a single column of pixels

        if (gradientTexture != null) DestroyImmediate(gradientTexture);
        gradientTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        gradientTexture.wrapMode = TextureWrapMode.Mirror;

        for (int i = 0; i < textureHeight; i++)
        {
            Color color = gradient.Evaluate((float)i / (textureHeight - 1));
            gradientTexture.SetPixel(0, i, color);
        }

        gradientTexture.filterMode = previewFilterMode;
        gradientTexture.Apply();
    }

    private void OnDisable()
    {
        // save settings to project
        manager.gradient = gradient;
        manager.textureHeight = textureHeight;
        manager.previewFilterMode = previewFilterMode;
        EditorUtility.SetDirty(manager);
        AssetDatabase.SaveAssets();
    }

    private void OnDestroy()
    {
        // Debug.Log("on destroy ran");
        if (gradientTexture != null) DestroyImmediate(gradientTexture);
    }

    private void DrawGradientBG()
    {
        GUI.DrawTexture(new Rect(0, 0, position.width, position.height), gradientTexture, ScaleMode.StretchToFill);
    }

    // private void DrawTextBG()
    // {
    //     EditorGUI.DrawRect(new Rect(0, 0, 150, position.height), new Color(0, 0, 0, 1f));
    // }

    private void LoadSettings()
    {
        string settingsPath = "Assets/Editor/SkyboxEditor/SO/SkyboxEditorManager.asset";
        manager = AssetDatabase.LoadAssetAtPath<SkyboxEditorManager>(settingsPath);
        
        if (manager == null)
        {
            manager = CreateInstance<SkyboxEditorManager>();
            AssetDatabase.CreateAsset(manager, settingsPath);
            AssetDatabase.SaveAssets();
        }
        gradient = manager.gradient;
        textureHeight = manager.textureHeight;
        previewFilterMode = manager.previewFilterMode;
    }
}
