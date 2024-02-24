using System;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace Tools.PreviewModule
{
    [InitializeOnLoad]
    class CommandMenu
    {
        const string NAME = "Editor Preview";
        const string KEY = "PreviewEffect";
        const string menuPath = "Tools/";
        static bool toggle;
        static PreviewHandle handle;
        static Rect rect;
        static EffectInfo efInfo;

        [MenuItem(menuPath + NAME)]
        static void PreviewEffect()
        {
            var sv = SceneView.lastActiveSceneView;
            if (!sv) return;
            if (sv.TryGetOverlay(nameof(PreviewEffectOverlay), out var overlay))
            {
                overlay.displayed = !overlay.displayed;
            }
        }

        ~CommandMenu()
        {
            if (handle)
            {
                handle.Clear();
            }
        }

        [Overlay(typeof(SceneView), nameof(PreviewEffectOverlay))]
        public class PreviewEffectOverlay : IMGUIOverlay
        {
            public override void OnGUI()
            {
                if (handle == null)
                {
                    handle = new PreviewHandle();
                }
                using (new GUILayout.HorizontalScope())
                {
                    OprationBar();
                }
                using (new GUILayout.VerticalScope(GUILayout.Width(200)))
                {
                    if (handle.selectEffect)
                    {
                        efInfo.DrawInfo();
                        EditorUtility.SetDirty(handle.selectEffect);
                        HandleUtility.Repaint();
                    }
                }
            }

            private void OprationBar()
            {
                if (handle.isPlaying)
                {
                    if (GUILayout.Button("Pause"))
                    {
                        handle.Pause();
                    }
                }
                else
                {
                    if (GUILayout.Button("Play"))
                    {
                        if (!efInfo.lockEffect || !handle.selectEffect)
                        {
                            var select = Selection.activeGameObject;
                            handle.ChangeHandleEffect(select);

                            efInfo.effect = select;
                            efInfo.Collect();
                        }
                        handle.Play();
                    }
                    if (handle.isPaused)
                    {
                        if (GUILayout.Button(">", GUILayout.Width(20)))
                        {
                            handle.Step();
                        }
                        if (GUILayout.Button(">>", GUILayout.Width(40)))
                        {
                            handle.Strides();
                        }
                    }
                }
                if (GUILayout.Button("Stop"))
                {
                    handle.Stop();
                }
            }
        }
    }
}
