using UnityEngine;

// [CreateAssetMenu(fileName = "SkyboxEditorManager", menuName = "Editor/SkyboxEditorManager")]
public class SkyboxEditorSO : ScriptableObject
{
    public Gradient previewGradient;
    [HideInInspector] public int previewTextureHeight = 30;
    [HideInInspector] public FilterMode previewFilterMode = FilterMode.Point;
    
    [HideInInspector] public int realTextureHeight = 100;
    [HideInInspector] public FilterMode realFilterMode = FilterMode.Bilinear;

    [HideInInspector] public bool autoUpdate = false;
    
    public string skyboxName()
    {
        return name;
    }

    
    
}