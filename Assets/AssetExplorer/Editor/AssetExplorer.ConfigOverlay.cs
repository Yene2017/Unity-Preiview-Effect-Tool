using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace Tools.PreviewModule
{

    public partial class AssetExplorer
    {
        [Overlay(typeof(AssetExplorer), "Config", true)]
        class ConfigOverlay : CustomWindowOverlay<AssetExplorer>
        {

            public override void OnCreated()
            {
                base.OnCreated();
            }

            public override void OnGUI()
            {
                if (owner._Cfg == null) return;
                _DrawCardSize();
                _DrawViewSize();
                _DrawLoadInterval();
                _DrawAutoPlay();
                _DrawPlayRange();
            }

            private void _DrawPlayRange()
            {
                using (new GUILayout.HorizontalScope(GUILayout.Width(owner.Width_All)))
                {
                    GUILayout.Label("播放范围", GUILayout.Width(owner.Width_Label));
                    owner._PlayRange = EditorGUILayout.Slider(owner._PlayRange, 0, 10,
                        GUILayout.Width(owner.Width_Content));
                }
            }

            private void _DrawAutoPlay()
            {
                using (new GUILayout.HorizontalScope(GUILayout.Width(owner.Width_All)))
                {
                    GUILayout.Label("自动播放", GUILayout.Width(owner.Width_Label));
                    owner._AutoPlay = EditorGUILayout.Toggle(owner._AutoPlay,
                        GUILayout.Width(owner.Width_Content));
                }
            }

            private void _DrawLoadInterval()
            {
                using (new GUILayout.HorizontalScope(GUILayout.Width(owner.Width_All)))
                {
                    GUILayout.Label("加载间隔", GUILayout.Width(owner.Width_Label));
                    owner._LoadingInterval = EditorGUILayout.Slider(owner._LoadingInterval, 0.02f, 0.2f,
                        GUILayout.Width(owner.Width_Content));
                }
            }

            private void _DrawViewSize()
            {
                using (new GUILayout.HorizontalScope(GUILayout.Width(owner.Width_All)))
                {
                    GUILayout.Label("拍摄范围", GUILayout.Width(owner.Width_Label));
                    PreviewSceneCameraHandler.viewMode = (PreviewSceneCameraHandler.ViewMode)
                        EditorGUILayout.EnumPopup(PreviewSceneCameraHandler.viewMode,
                            GUILayout.Width(owner.Width_Content));
                }
            }

            private void _DrawCardSize()
            {
                using(new GUILayout.HorizontalScope(GUILayout.Width(owner.Width_All)))
                {
                    GUILayout.Label("卡片大小", GUILayout.Width(owner.Width_Label));
                    owner._CardSize = (CardSize)
                        EditorGUILayout.EnumPopup(owner._CardSize,
                            GUILayout.Width(owner.Width_Content));
                }
            }

            protected override Layout supportedLayouts => Layout.VerticalToolbar | Layout.Panel;

        }

        [SerializeField]
        private CardSize _CardSize = CardSize.Normal;
        [SerializeField]
        private float _LoadingInterval = 0.06f;
        [SerializeField]
        private float _PlayRange = 3f;
        private bool _AutoPlay = true;


        private Config _Cfg;

        [Serializable]
        class Config
        {
            public List<Vector2Int> CardSize;
            public List<string> IgnorePsNodeNameArray;
        }

        enum CardSize
        {
            Small, Normal, Large
        }
    }
}
