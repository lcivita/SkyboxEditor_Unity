using UnityEngine;

// [CreateAssetMenu(fileName = "SkyboxEditorManager", menuName = "Editor/SkyboxEditorManager")]
public class SkyboxEditorSO : ScriptableObject
{
    [HideInInspector] public Gradient previewGradient;
    [HideInInspector] public int previewTextureHeight = 30;
    [HideInInspector] public FilterMode previewFilterMode = FilterMode.Point;
    
    [HideInInspector] public int realTextureHeight = 100;
    [HideInInspector] public FilterMode realFilterMode = FilterMode.Bilinear;

    [HideInInspector] public bool autoUpdate = false;
    
    public string skyboxName()
    {
        return name;
    }

    
    // Reset gradient to unity default (almost)
    public void InitializeGradient(out Gradient grad)
    {
        grad = new Gradient();
        grad.colorKeys = new GradientColorKey[]
        {
            new GradientColorKey(new Color(0.4118f, 0.3882f, 0.3686f, 1.0f), 0.46f),
            new GradientColorKey(new Color(0.8745f, 0.9843f, 0.9882f, 1.0f), 0.52f),
            new GradientColorKey(new Color(0.3294f, 0.4196f, 0.5608f, 1.0f), 0.7f)
        };
    }
}