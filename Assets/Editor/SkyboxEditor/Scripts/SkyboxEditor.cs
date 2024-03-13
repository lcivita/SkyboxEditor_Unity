using UnityEngine;
using UnityEditor;

namespace Editor.SkyboxEditor.Scripts
{
    public partial class SkyboxEditor : EditorWindow
    {
        private Gradient _gradient;
        private Texture2D _previewGradientTexture;
        private Vector2 _lastWindowSize = Vector2.zero;
        
        private int _previewTextureHeight = 30;
        private FilterMode _previewFilterMode = FilterMode.Point;

        private Texture2D _previewIcon;
        private Texture2D _skyboxIcon;
        
        private int _realTextureHeight = 100;
        private FilterMode _realFilterMode = FilterMode.Bilinear;

        private bool _autoUpdate = false;

        [MenuItem("Tools/Skybox Editor")]
        private static void OpenSkyboxEditor()
        {
            GetWindow<SkyboxEditor>("Skybox Editor").Show();
        }

        private void OnGUI()
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
            
            EditorGUI.BeginChangeCheck();
            _gradient = EditorGUILayout.GradientField("Skybox Gradient", _gradient);
            
            if (EditorGUI.EndChangeCheck())
            {
                GenerateGradientTexture();
                if (_autoUpdate)
                {
                    UpdateSkybox();
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
                    UpdateSkybox();
                }
            }
            

            // push stuff beneath this to bottom of page
            GUILayout.FlexibleSpace();

            _autoUpdate = GUILayout.Toggle(_autoUpdate, "Auto-Update");
            if (_autoUpdate) EditorGUILayout.HelpBox("Enabling auto-update may impact performance", MessageType.Warning);
            
            
            if (GUILayout.Button("Update Skybox"))
            {
                UpdateSkybox();
            }
            
            if (GUILayout.Button("Reset Gradient"))
            {
                _manager.InitializeGradient(out _gradient);
                GenerateGradientTexture();
                if (_autoUpdate)
                {
                    UpdateSkybox();
                }
            }
        }
        
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

        private void DrawGradientBG()
        {
            GUI.DrawTexture(new Rect(0, 0, position.width, position.height), _previewGradientTexture, ScaleMode.StretchToFill);
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        private void UpdateSkybox()
        {
            SaveRealTexture();
            
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(_skyboxMatPath);
            if (mat == null)
            {
                mat = new Material(AssetDatabase.LoadAssetAtPath<Shader>(_skyboxShaderPath));
                AssetDatabase.CreateAsset(mat, _skyboxMatPath);
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
    }
}