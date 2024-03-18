using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
// using UnityEngine.Windows;
using System.IO;

namespace Editor.SkyboxEditor.Scripts
{
    public partial class SkyboxEditor : EditorWindow
    {
        private string _skyboxName = "default";
        
        private string _newSkyboxName = "";
        
        private SkyboxEditorSO _selectedSo;

        
        private Gradient _gradient;
        private Texture2D _previewGradientTexture;
        private Vector2 _lastWindowSize = Vector2.zero;
        
        private int _previewTextureHeight = 30;
        private FilterMode _previewFilterMode = FilterMode.Point;

        private Texture2D _previewIcon;
        private Texture2D _skyboxIcon;
        
        private int _realTextureHeight = 100;
        private FilterMode _realFilterMode = FilterMode.Bilinear;

        private bool _autoUpdate;

        [MenuItem("Tools/Skybox Editor")]
        private static void OpenSkyboxEditor()
        {
            GetWindow<SkyboxEditor>("Skybox Editor").Show();
        }

        private void OnGUI()
        {
            
            HandleWindowResizing();
            DrawPreviewSettings();
            DrawRealSettings();
            
            GUILayout.FlexibleSpace();
            DrawAutoUpdateToggle();
            // DrawSkyboxName();
            DrawLoadSkybox();
            DrawNewSkybox();
            DrawUpdateButton();
            DrawResetButton();
            DrawClearSkyboxes();
        }
        
        #region GUI Methods

            private void HandleWindowResizing()
            {
                // Only regenerate the gradient texture when window resized
                if (_lastWindowSize.x != position.width || _lastWindowSize.y != position.height)
                {
                    GenerateGradientTexture();
                    _lastWindowSize.x = position.width;
                    _lastWindowSize.y = position.height;
                }
                if (_previewGradientTexture != null)
                {
                    DrawGradientBG();
                }
            }
            
            private void DrawGradientBG()
            {
                GUI.DrawTexture(new Rect(0, 0, position.width, position.height), _previewGradientTexture, ScaleMode.StretchToFill);
            }

            private void DrawPreviewSettings()
            {
                EditorGUI.BeginChangeCheck();
                _gradient = EditorGUILayout.GradientField("Skybox Gradient", _gradient);
                _manager.CurSkyboxEditorSo.previewGradient.alphaKeys = _gradient.alphaKeys;
                _manager.CurSkyboxEditorSo.previewGradient.colorKeys = _gradient.colorKeys;

                if (EditorGUI.EndChangeCheck())
                {
                    GenerateGradientTexture();
                    if (_autoUpdate)
                    {
                        SetSkybox();
                    }
                }

                GUILayout.Space(20);

                EditorGUI.BeginChangeCheck();
                GUIContent previewLabelContent = new GUIContent(" Preview Settings", _previewIcon);
                GUILayout.Label(previewLabelContent);
                _previewTextureHeight = EditorGUILayout.IntSlider("Resolution", _previewTextureHeight, 5, 100);
                _previewFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", _previewFilterMode);
                if (EditorGUI.EndChangeCheck())
                {
                    GenerateGradientTexture();
                }
            }

            private void DrawRealSettings()
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.Space(20);
                GUIContent realLabelContent = new GUIContent(" Skybox Settings", _skyboxIcon);
                GUILayout.Label(realLabelContent);
                _realTextureHeight = EditorGUILayout.IntSlider("Resolution", _realTextureHeight, 5, 200);
                _realFilterMode = (FilterMode)EditorGUILayout.EnumPopup("Filter Mode", _realFilterMode);
                if (EditorGUI.EndChangeCheck())
                {
                    GenerateGradientTexture();
                    if (_autoUpdate)
                    {
                        SetSkybox();
                    }
                }
            }

            private void DrawAutoUpdateToggle()
            {
                _autoUpdate = _manager.CurSkyboxEditorSo != null ? GUILayout.Toggle(_autoUpdate, "Auto-Update") : _autoUpdate = false;
                if (_autoUpdate) EditorGUILayout.HelpBox("Enabling auto-update may impact performance", MessageType.Warning);
            }

            private void DrawNewSkybox()
            {
                GUILayout.BeginHorizontal();
                _newSkyboxName = GUILayout.TextField(_newSkyboxName, GUILayout.Width(150));

                bool shouldEnable = !(_newSkyboxName == "" || File.Exists("Assets/SkyboxEditor/Editor/SO/" + _newSkyboxName + ".asset"));
                GUI.enabled = shouldEnable;
                if (GUILayout.Button("New Skybox"))
                {
                    CreateNewSkybox(_newSkyboxName);
                }

                GUI.enabled = true;
                
                GUILayout.EndHorizontal();
            }

            private void DrawLoadSkybox()
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Load Skybox", GUILayout.Width(150));
                SkyboxEditorSO prevSelectedSo = _selectedSo;
                _manager.CurSkyboxEditorSo = EditorGUILayout.ObjectField(_manager.CurSkyboxEditorSo, typeof(SkyboxEditorSO), false) as SkyboxEditorSO;
                
                if (_manager.CurSkyboxEditorSo != prevSelectedSo)
                {
                    _selectedSo = _manager.CurSkyboxEditorSo;
                    if (_manager.CurSkyboxEditorSo != null)
                    OnManagerSelected(_manager.CurSkyboxEditorSo);
                }
                
                GUILayout.EndHorizontal();
            }
            
            private void OnManagerSelected(SkyboxEditorSO so)
            {
                _manager.CurSkyboxEditorSo = so;

                _so = so;
                
                _skyboxName = _so.skyboxName();
                
                _gradient = _so.previewGradient;
                _previewTextureHeight = _so.previewTextureHeight;
                _previewFilterMode = _so.previewFilterMode;

                _realTextureHeight = _so.realTextureHeight;
                _realFilterMode = _so.realFilterMode;
                
                // DrawGradientBG();
                // _previewGradientTexture = ;
                GenerateGradientTexture();
            }

            private void DrawUpdateButton()
            {
                GUI.enabled = _manager.CurSkyboxEditorSo != null;
                
                if (GUILayout.Button("Set Skybox"))
                {
                    SetSkybox();
                }

                GUI.enabled = true;
            }

            private void DrawResetButton()
            {
                if (GUILayout.Button("Reset Gradient"))
                {
                    _manager.InitializeGradient(out _gradient);
                    GenerateGradientTexture();
                    if (_autoUpdate)
                    {
                        SetSkybox();
                    }
                }
            }

            private void DrawClearSkyboxes()
            {
                if (GUILayout.Button("Clear All Skyboxes"))
                {
                    bool confirmed = EditorUtility.DisplayDialog("Confirmation", "Are you sure you want to clear all skyboxes?", "I probably shouldn't do this.", "Absolutely!");

                    if (!confirmed)
                    {
                        List<string> folderPaths =  new List<string>();
                        // folderPaths.Add("Assets/SkyboxEditor/Editor/Manager");
                        folderPaths.Add("Assets/SkyboxEditor/Editor/Materials");
                        folderPaths.Add("Assets/SkyboxEditor/Editor/SO");
                        folderPaths.Add("Assets/SkyboxEditor/Editor/Textures");

                        foreach (var s in folderPaths)
                        {
                            DeleteAllFilesInFolder(s);
                        }
                    
                        AssetDatabase.Refresh();
                        _newSkyboxName = "default";
                        CreateNewSkybox(_newSkyboxName);
                    }
                    
                }
            }
            
            private void DeleteAllFilesInFolder(string path)
            {
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path);
                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }
                else
                {
                    Debug.LogWarning("Folder does not exist: " + path);
                }
            }

        #endregion
        
        private void GenerateGradientTexture()
        {
            int textureWidth = 1;

            if (_previewGradientTexture != null) DestroyImmediate(_previewGradientTexture);
            _previewGradientTexture = new Texture2D(textureWidth, _previewTextureHeight, TextureFormat.RGBA32, false);
            _previewGradientTexture.wrapMode = TextureWrapMode.Mirror;

            for (int i = 0; i < _previewTextureHeight; i++)
            {
                Color color = _gradient.Evaluate((float)i / (_previewTextureHeight - 1));
                _previewGradientTexture.SetPixel(0, i, color);
            }

            _previewGradientTexture.filterMode = _previewFilterMode;
            _previewGradientTexture.Apply();
            
        }
        
        private void SetSkybox()
        {
            if (_skyboxName == "")
            {
                return;
            }
            SaveRealTexture();

            string path = _skyboxMatPath + _manager.CurSkyboxEditorSo.skyboxName() + ".mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(AssetDatabase.LoadAssetAtPath<Shader>(_skyboxShaderPath));
                AssetDatabase.CreateAsset(mat, path);
                AssetDatabase.SaveAssets();
            }

            if (RenderSettings.skybox != mat)
            {
                RenderSettings.skybox = mat;
            }
            
            mat.SetTexture("_GradientTex", AssetDatabase.LoadAssetAtPath<Texture2D>(_skyboxTexPath()));
        }

        private Texture2D CurFullTex()
        {
            int textureWidth = 1;

            Texture2D tex = new Texture2D(textureWidth, _realTextureHeight, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Mirror;

            for (int i = 0; i < _realTextureHeight; i++)
            {
                Color color = _gradient.Evaluate((float)i / (_realTextureHeight - 1));
                tex.SetPixel(0, i, color);
            }

            tex.filterMode = _realFilterMode;
            tex.Apply();
            return tex;
        }

        private void CreateNewSkybox(string name)
        {
            string settingsPath = "Assets/SkyboxEditor/Editor/SO/" + name + ".asset";
            _so = AssetDatabase.LoadAssetAtPath<SkyboxEditorSO>(settingsPath);
            
            if (_so == null)
            {
                _so = CreateInstance<SkyboxEditorSO>();
                AssetDatabase.CreateAsset(_so, settingsPath);
                AssetDatabase.SaveAssets();
            }

            _manager.CurSkyboxEditorSo = _so;
            
            _manager.InitializeGradient(out _gradient);
            
            Gradient newGradient = new Gradient();
            newGradient.colorKeys = _gradient.colorKeys;
            newGradient.alphaKeys = _gradient.alphaKeys;
            _so.previewGradient = newGradient;
            _so.previewTextureHeight = _previewTextureHeight;
            _so.previewFilterMode = _previewFilterMode;

            _so.realTextureHeight = _realTextureHeight;
            _so.realFilterMode = _realFilterMode;
            
            // SetSkybox();

            // _skyboxName = _manager.skyboxName();
        }
    }
}