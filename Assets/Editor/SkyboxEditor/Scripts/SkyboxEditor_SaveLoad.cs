using UnityEngine;
using UnityEditor;

namespace Editor.SkyboxEditor.Scripts
{
    public partial class SkyboxEditor
    {
        private SkyboxEditorManager _manager;
        
        private const string _skyboxMatPath = "Assets/Editor/SkyboxEditor/Shaders/CustomSkybox.mat";
        private const string _skyboxShaderPath = "Assets/Editor/SkyboxEditor/Shaders/SkyboxProcedural.shader";
        private string textureName = "SkyboxTex.png";
        private string _skyboxTexPath()
        {
            return "Assets/Editor/SkyboxEditor/Textures/" + textureName;
        } 

        private void OnEnable()
        {
            LoadSettings();
            
            if (_gradient == null)
            {
                _manager.InitializeGradient(out _gradient);
            }
            GenerateGradientTexture();
            
            _previewIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/SkyboxEditor/Icons/previewIcon.png", typeof(Texture2D));
            _skyboxIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/SkyboxEditor/Icons/skyboxIcon.png", typeof(Texture2D));
        }

        private void OnDisable()
        {
            // save settings to project
            _manager.previewGradient = _gradient;
            _manager.previewTextureHeight = _previewTextureHeight;
            _manager.previewFilterMode = _previewFilterMode;
            
            _manager.realTextureHeight = _realTextureHeight;
            _manager.realFilterMode = _realFilterMode;
            
            EditorUtility.SetDirty(_manager);
            AssetDatabase.SaveAssets();
        }

        private void OnDestroy()
        {
            // Debug.Log("on destroy ran");
            if (_previewGradientTexture != null) DestroyImmediate(_previewGradientTexture);
        }


        private void LoadSettings()
        {
            string settingsPath = "Assets/Editor/SkyboxEditor/SO/SkyboxEditorManager.asset";
            _manager = AssetDatabase.LoadAssetAtPath<SkyboxEditorManager>(settingsPath);
            
            if (_manager == null)
            {
                _manager = CreateInstance<SkyboxEditorManager>();
                AssetDatabase.CreateAsset(_manager, settingsPath);
                AssetDatabase.SaveAssets();
            }
            _gradient = _manager.previewGradient;
            _previewTextureHeight = _manager.previewTextureHeight;
            _previewFilterMode = _manager.previewFilterMode;

            _realTextureHeight = _manager.realTextureHeight;
            _realFilterMode = _manager.realFilterMode;
        }
        
        private void SaveRealTexture()
        {
            Texture2D curTex = AssetDatabase.LoadAssetAtPath<Texture2D>(_skyboxTexPath());

            if (curTex != null)
            {
                AssetDatabase.DeleteAsset(_skyboxTexPath());
            }
            
            byte[] textureBytes = CurFullTex().EncodeToPNG();
            System.IO.File.WriteAllBytes(_skyboxTexPath(), textureBytes);
            AssetDatabase.Refresh();
            
            TextureImporter importer = AssetImporter.GetAtPath(_skyboxTexPath()) as TextureImporter;
            if (importer != null)
            {
                importer.wrapMode = TextureWrapMode.Mirror;
                importer.filterMode = _realFilterMode;
                importer.SaveAndReimport();
            }
        }
    }

}

