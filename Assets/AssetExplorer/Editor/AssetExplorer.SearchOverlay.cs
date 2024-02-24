using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;

namespace Tools.PreviewModule
{
    public partial class AssetExplorer
    {
        [Overlay(typeof(AssetExplorer), "Search", true)]
        class SearchOverlay : CustomWindowOverlay<AssetExplorer>
        {
            public override void OnGUI()
            {
                if (!owner._Inited) return;
                var newFilter = EditorGUILayout.DelayedTextField(_Filter, owner.Style_Search,
                    GUILayout.Width(owner.Width_All));
                if(_Filter != newFilter)
                {
                    _Filter = newFilter;
                    owner._SetSearchFilter(newFilter);
                }
            }

            string _Filter = "";
        }

        private void RefreshContent()
        {
            var files = AssetDatabase.GetAllAssetPaths().ToList();
            files.Sort();
            if (_SearchReg == null)
            {
                _InitLoadItems(files, (s) => s.StartsWith(_Folder));
            }
            else
            {
                _InitLoadItems(files, (s) => _SearchReg.IsMatch(s) && s.StartsWith(_Folder));
            }
        }

        private void _SetSearchFilter(string filter)
        {
            if (string.IsNullOrEmpty(filter))
            {
                _SearchReg = null;
            }
            else
            {
                _SearchReg = new Regex(filter, RegexOptions.IgnoreCase);
            }
            RefreshContent();
        }

        void _ClearItems()
        {
            foreach(var item in _Items)
            {
                item.Clear();
                _CacheQueue.Enqueue(item);
            }
            _Items.Clear();
            _LoadingQueue.Clear();
        }

        void _InitLoadItems(List<string> collection, Func<string, bool> func)
        {
            _ClearItems();
            if (collection == null) return;
            var index = 0;
            foreach(var path in collection)
            {
                if (!func.Invoke(path)) continue;
                if (!File.Exists(path)) continue;
                if (Path.GetExtension(path) != Extension) continue;
                index++;
                if (_CacheQueue.Count < 1)
                {
                    _CacheQueue.Enqueue(new PreviewItem());
                }
                var item = _CacheQueue.Dequeue();
                item.index = index;
                item.path = path;
                item.name = Path.GetFileNameWithoutExtension(path);
                _Items.Add(item);
                _LoadingQueue.Enqueue(item);
            }
        }

        private void _UpdateLoad()
        {
            var tick = EditorApplication.timeSinceStartup;
            if (tick - _LastLoadTick < _LoadingInterval) return;
            if (_LoadingQueue.Count < 1) return;
            _LastLoadTick = tick;
            do
            {
                var item = _LoadingQueue.Dequeue();
                if (item.ignoreLoad)
                {
                    if (_LoadingQueue.Count < 1) break;
                    continue;
                }
                if (item.obj) continue;

                item.obj = AssetDatabase.LoadMainAssetAtPath(item.path);
                if (!item.obj) item.ignoreLoad = true;
                item.loadTime = (int)(1000 * (EditorApplication.timeSinceStartup - tick));
                _InitItem(item);
                break;
            }
            while (true);
        }

        List<PreviewItem> _Items = new List<PreviewItem>();
        Queue<PreviewItem> _CacheQueue = new Queue<PreviewItem>();
        Queue<PreviewItem> _LoadingQueue = new Queue<PreviewItem>();
        private double _LastLoadTick;
    }
}
