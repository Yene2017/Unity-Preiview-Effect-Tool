using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Overlays;
using UnityEngine;

namespace Tools.PreviewModule
{
    public partial class AssetExplorer
    {
        [Overlay(typeof(AssetExplorer), "MenuTree", true)]
        class MenuTreeOverlay : CustomWindowOverlay<AssetExplorer>
        {
            MenuTreeView _View;
            void _TryInitView()
            {
                if (_View != null) return;
                _View = new MenuTreeView(owner);
                _View.ReloadView();
            }

            public override void OnGUI()
            {
                _TryInitView();
                _View.Layout(out var w, out var h);
                var max = 200f;
                var min = 60f;
                if (this.layout == Layout.VerticalToolbar)
                {
                    max = owner.position.height / 2;
                }
                h = Mathf.Min(Mathf.Max(h, min), max);
                var rect = GUILayoutUtility.GetRect(w, h);
                _View.OnGUI(rect);
            }
        }

        class MenuTreeView : TreeView
        {
            private AssetExplorer owner;
            private GUIStyle Style_Search;

            public MenuTreeView(AssetExplorer owner) : base(new TreeViewState())
            {
                this.owner = owner;
            }

            protected override TreeViewItem BuildRoot()
            {
                var root = new TreeViewItem();
                root.id = 0;
                root.depth = -1;
                var files = Directory.GetFiles(owner.Root, "*", SearchOption.AllDirectories);
                _CollectFileItems(root, files);
                return root;
            }

            private void _CollectFileItems(TreeViewItem root, string[] files)
            {
                var map = new Dictionary<string, FileItem>();
                var menuRoot = new FileItem();
                menuRoot.depth = 0;
                menuRoot.id = 1;
                menuRoot.guid = AssetDatabase.AssetPathToGUID(owner.Root);
                menuRoot.displayName = Path.GetFileName(owner.Root);
                root.AddChild(menuRoot);
                foreach(var file in files)
                {
                    if (Path.GetExtension(file) != owner.Extension) continue;
                    var dir = _GetUnityPath(Path.GetDirectoryName(file));
                    if (dir == owner.Root) continue;
                    var nodes = dir.Replace($"{owner.Root}/", "").Split('/');
                    if (nodes.Length > 0)
                    {
                        var path = nodes[0];
                        if (!map.TryGetValue(path, out var item))
                        {
                            item = new FileItem();
                            item.depth = 1;
                            item.id = path.GetHashCode();
                            item.guid = AssetDatabase.AssetPathToGUID($"{owner.Root}/{path}");
                            item.displayName = nodes[0];
                            map.Add(path, item);
                            menuRoot.AddChild(item);
                        }
                    }
                    if (nodes.Length > 1)
                    {
                        var path = $"{nodes[0]}/{nodes[1]}";
                        if(!map.TryGetValue(path, out var item))
                        {
                            item = new FileItem();
                            item.depth = 2;
                            item.id = path.GetHashCode();
                            item.guid = AssetDatabase.AssetPathToGUID($"{owner.Root}/{path}");
                            item.displayName = nodes[1];
                            map.Add(path, item);
                            map.TryGetValue(nodes[0], out var parent);
                            parent.AddChild(item);
                        }
                    }
                }
            }

            protected override bool CanMultiSelect(TreeViewItem item) => false;

            protected override void SelectionChanged(IList<int> selectedIds)
            {
                if (selectedIds.Count < 1) return;
                var id = selectedIds[0];
                var item = FindItem(id, rootItem) as FileItem;
                var folder = AssetDatabase.GUIDToAssetPath(item.guid);
                owner._Folder = folder;
                owner.RefreshContent();
            }

            private string _GetUnityPath(string v)
            {
                return v.Replace("\\", "/");
            }

            public override void OnGUI(Rect rect)
            {
                _InitStyle();
                _DrawHead(rect);
                rect.yMin += EditorGUIUtility.singleLineHeight;
                base.OnGUI(rect);
            }

            private void _DrawHead(Rect rect)
            {
                rect.height = EditorGUIUtility.singleLineHeight;
                var newStr = EditorGUI.DelayedTextField(rect, state.searchString, Style_Search);
                if (newStr != state.searchString)
                {
                    state.searchString = newStr;
                    ReloadView();
                }
            }

            private void _InitStyle()
            {
                if (Style_Search != null) return;
                Style_Search = (GUIStyle)"SearchTextField";
            }

            internal void ReloadView()
            {
                Reload();
                SetExpanded(0, true);
                SetExpanded(1, true);
            }

            internal void Layout(out float width , out float height)
            {
                width = 200;
                height = 0;
                foreach(var id in state.expandedIDs)
                {
                    var item = FindItem(id, rootItem);
                    if (item == null) continue;
                    if (item.children == null) continue;
                    height += this.rowHeight * item.children.Count;
                }
                height += EditorGUIUtility.singleLineHeight;
            }

            class FileItem : TreeViewItem
            {
                public string guid;
            }
        }
    }
}
