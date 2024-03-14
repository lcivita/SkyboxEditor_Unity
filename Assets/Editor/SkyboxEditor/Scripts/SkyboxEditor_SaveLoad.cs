using UnityEngine;
using UnityEditor;

namespace Editor.SkyboxEditor.Scripts
{
    public partial class SkyboxEditor
    {
        private SkyboxEditorSO _so;
        private SkyboxManager _manager;
        
        private const string _skyboxMatPath = "Assets/Editor/SkyboxEditor/Materials/";
        private const string _skyboxShaderPath = "Assets/Editor/SkyboxEditor/Shaders/SkyboxProcedural.shader";
        // private string textureName = "SkyboxTex.png";
        private string _skyboxTexPath()
        {
            return "Assets/Editor/SkyboxEditor/Textures/" + _skyboxName + ".png";
        } 

        private void OnEnable()
        {
            LoadSettings();
            
            _manager.CurSkyboxEditorSo = _so;
            
            if (_gradient == null)
            {
                _manager.InitializeGradient(out _gradient);
            }
            
            Gradient newGradient = new Gradient();
            newGradient.colorKeys = _gradient.colorKeys;
            newGradient.alphaKeys = _gradient.alphaKeys;
            _manager.CurSkyboxEditorSo.previewGradient = newGradient;
            
            GenerateGradientTexture();
            
            _previewIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/SkyboxEditor/Icons/previewIcon.png", typeof(Texture2D));
            _skyboxIcon = (Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/SkyboxEditor/Icons/skyboxIcon.png", typeof(Texture2D));
        }

        private void OnDisable()
        {
            _manager.CurSkyboxEditorSo = _so;
            
            try
            {
                // save settings to project
                Gradient newGradient = new Gradient();
                newGradient.colorKeys = _gradient.colorKeys;
                newGradient.alphaKeys = _gradient.alphaKeys;
                _so.previewGradient = newGradient;
                _so.previewTextureHeight = _previewTextureHeight;
                _so.previewFilterMode = _previewFilterMode;

                _so.realTextureHeight = _realTextureHeight;
                _so.realFilterMode = _realFilterMode;

                // _manager.skyboxName = _skyboxName;

                EditorUtility.SetDirty(_so);
                AssetDatabase.SaveAssets();
            }
            catch
            {
                
            }
        }

        private void OnDestroy()
        {
            // Debug.Log("on destroy ran");
            if (_previewGradientTexture != null) DestroyImmediate(_previewGradientTexture);
        }


        private void LoadSettings()
        {
            string managerPath = "Assets/Editor/SkyboxEditor/Manager/Manager.asset";
            _manager = AssetDatabase.LoadAssetAtPath<SkyboxManager>(managerPath);
            if (_manager == null)
            {
                _manager = CreateInstance<SkyboxManager>();
                AssetDatabase.CreateAsset(_manager, managerPath);
                AssetDatabase.SaveAssets();
            }
            
            string settingsPath = "Assets/Editor/SkyboxEditor/SO/" + _skyboxName + ".asset";
            // _so = AssetDatabase.LoadAssetAtPath<SkyboxEditorSO>(settingsPath);
            _so = _manager.CurSkyboxEditorSo;
            if (_so == null)
            {
                _so = CreateInstance<SkyboxEditorSO>();
                AssetDatabase.CreateAsset(_so, settingsPath);
                AssetDatabase.SaveAssets();
            }
            _gradient = _so.previewGradient;
            _previewTextureHeight = _so.previewTextureHeight;
            _previewFilterMode = _so.previewFilterMode;

            _realTextureHeight = _so.realTextureHeight;
            _realFilterMode = _so.realFilterMode;

            _skyboxName = _so.skyboxName();
        }
        
        private void SaveRealTexture()
        {
            Texture2D curTex = AssetDatabase.LoadAssetAtPath<Texture2D>(_so.skyboxName());

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

