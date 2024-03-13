using UnityEngine;

// [CreateAssetMenu(fileName = "SkyboxEditorManager", menuName = "Editor/SkyboxEditorManager")]
public class SkyboxManager : ScriptableObject
{
    [HideInInspector] public SkyboxEditorSO CurSkyboxEditorSo;
}