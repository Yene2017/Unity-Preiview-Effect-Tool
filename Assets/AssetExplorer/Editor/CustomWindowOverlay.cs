using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace Tools.PreviewModule
{
    public class CustomWindowOverlay<T> : IMGUIOverlay, ICreateHorizontalToolbar, ICreateVerticalToolbar
        where T : EditorWindow
    {
        public override void OnGUI()
        {
            throw new System.NotImplementedException();
        }

        public override void OnCreated()
        {
            base.OnCreated();
            owner = containerWindow as T;
        }

        protected GUI.Scope LayoutScope()
        {
            switch (this.layout)
            {
                case Layout.HorizontalToolbar:
                    return new GUILayout.HorizontalScope();
                case Layout.VerticalToolbar:
                case Layout.Panel:
                    return new GUILayout.VerticalScope();
            }
            return new GUILayout.VerticalScope();
        }

        public OverlayToolbar CreateHorizontalToolbarContent()
        {
            var root = new OverlayToolbar();
            root.Add(CreatePanelContent());
            return root;
        }

        public OverlayToolbar CreateVerticalToolbarContent()
        {
            var root = new OverlayToolbar();
            root.Add(CreatePanelContent());
            return root;
        }

        protected override Layout supportedLayouts => Layout.All;
     
        internal T owner;
    }
}
