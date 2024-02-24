using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace Tools.PreviewModule
{
    public partial class AssetExplorer : EditorWindow, ISupportsOverlays
    {
        [MenuItem("Tools/AssetExplorer")]
        static void OpenDefault()
        {
            var w = EditorWindow.GetWindow<AssetExplorer>();
            w.Root = "Assets";
            w.Extension = ".prefab";
        }

        void OnEnable()
        {
            _LastLoadTick = EditorApplication.timeSinceStartup;
            _BuildContentLayout();
            EditorApplication.update -= _UpdateLoad;
            EditorApplication.update += _UpdateLoad;
        }

        void OnDisable()
        {
            EditorApplication.update -= _UpdateLoad;
            _DisposeRenderItems();
        }

        void OnGUI()
        {
            _TryInitRenderItems();
            _TryInitConfig();
        }

        private void _TryInitConfig()
        {
            _Cfg = new Config();
            _Cfg.CardSize = new List<Vector2Int>()
            {
                new Vector2Int(120,140),
                new Vector2Int(160,180),
                new Vector2Int(240,260),
            };
            _Cfg.IgnorePsNodeNameArray = new List<string>();
        }

        private void _TryInitRenderItems()
        {
            if (_Inited) return;
            _Inited = true;

            _PreviewStub = new PreviewRenderUtility();
            _Camera = _PreviewStub.camera.gameObject.AddComponent<PreviewSceneCameraHandler>();
            _Camera.isManualUpdate = true;
            GC_Loading = EditorGUIUtility.IconContent("Loading");
            GC_Pause = EditorGUIUtility.IconContent("PauseButton On");
            GC_Play = EditorGUIUtility.IconContent("PlayButton On");
            Style_PlayBtn = (GUIStyle)"LODLevelNotifyText";
            Style_Title = (GUIStyle)"Label";
            Style_Title.wordWrap = true;
            Style_Search = (GUIStyle)"SearchTextField";
        }

        private void _DisposeRenderItems()
        {
            if (_PreviewStub != null)
            {
                _PreviewStub.Cleanup();
                _PreviewStub = null;
            }
            _Inited = false;
        }

        public string Root = "Assets";
        public string Extension = ".prefab";
        [SerializeField]
        private string _Folder;

        private Regex _SearchReg;
        private PreviewRenderUtility _PreviewStub;
        private PreviewSceneCameraHandler _Camera;

        private double _SampleTick;
        private float _FpsTick;
        private Queue<float> _FpsQueue = new Queue<float>();
        private float _FpsDuration = 1;
        private int _Fps;

        private bool _Inited;
        private GUIContent GC_Loading;
        private GUIContent GC_Pause;
        private GUIContent GC_Play;
        private GUIStyle Style_PlayBtn;
        private GUIStyle Style_Title;
        private GUIStyle Style_Search;

        float Width_Label = 60;
        float Width_Content = 120;
        float Width_All = 200;

    }
}
