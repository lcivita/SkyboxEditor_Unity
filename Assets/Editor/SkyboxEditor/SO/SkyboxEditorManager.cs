using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkyboxEditorManager", menuName = "Editor/SkyboxEditorManager")]
public class SkyboxEditorManager : ScriptableObject
{
    [HideInInspector] public Gradient gradient;
    [HideInInspector] public int textureHeight = 50;
    [HideInInspector] public FilterMode previewFilterMode = FilterMode.Point;

    
    // Reset gradient to unity default (almost)
    public void InitializeGradient(out Gradient grad)
    {
        grad = new Gradient();
        grad.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0.4118f, 0.3882f, 0.3686f, 1.0f), 0.49f),
            new GradientColorKey(new Color(0.8745f, 0.9843f, 0.9882f, 1.0f), 0.55f),
            new GradientColorKey(new Color(0.3294f, 0.4196f, 0.5608f, 1.0f), 1.0f)
        };
    }
}