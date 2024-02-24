using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tools.PreviewModule
{
    public partial class AssetExplorer
    {
        #region Ops

        internal void Play(PreviewItem item)
        {
            if (item.playing) return;
            if (item.index < _CurtStartIndex || item.index > _CurtEndIndex) return;
            if (!item.go) return;
            item.playing = true;
            if (item.playHandle == null)
            {
                item.playHandle = new PreviewHandle();
                item.playHandle.ChangeHandleEffect(item.go);
                item.playHandle.UpdatePlayRange(_PlayRange);
            }
            item.playHandle.Play();
        }

        internal void Pause(PreviewItem item)
        {
            if (!item.playing) return;
            item.playing = false;
            if (item.playHandle == null) return;
            item.playHandle.Pause();
        }

        internal void PauseAll()
        {
            foreach (var item in _Items)
            {
                Pause(item);
            }
        }

        internal void ResumeAll()
        {
            _CurtStartIndex = -1;
            _CurtEndIndex = -1;
        }

        private void _InitItem(PreviewItem item)
        {
            if (!(item.obj is GameObject go)) return;
            item.go = _PreviewStub.InstantiatePrefabInScene(go);
            if (!item.go.activeSelf) item.go.SetActive(true);
            var list = new List<Renderer>();
            item.go.GetComponentsInChildren(true, _GetRenderCache);
            foreach (var r in _GetRenderCache)
            {
                if (!(r is ParticleSystemRenderer psr))
                {
                    list.Add(r);
                    continue;
                }
                var t = r.transform;
                if (!r.TryGetComponent<ParticleSystem>(out var ps)) continue;
                if (!ps.emission.enabled) continue;
                list.Add(r);
            }
            _GetRenderCache.Clear();
            item.renders = list.ToArray();
            _VisiableItem(item, false);
            if (_AutoPlay) Play(item);
        }

        private void _VisiableItem(PreviewItem item, bool v)
        {
            if (v)
            {
                if (item.renders == null) return;
                foreach (var r in item.renders)
                {
                    r.enabled = true;
                }
            }
            else
            {
                if (!item.go) return;
                item.go.GetComponentsInChildren(_GetRenderCache);
                foreach (var r in _GetRenderCache)
                {
                    r.enabled = false;
                }
                _GetRenderCache.Clear();
            }
        }

        private void _ShowItem(PreviewItem item)
        {
            if (_AutoPlay) Play(item);
        }

        private void _HideItem(PreviewItem item)
        {
            Pause(item);
        }

        #endregion Ops

        #region GUI

        private void _BuildContentLayout()
        {
            ResumeAll();
            _ReCreate();
        }

        private void _ReCreate()
        {
            var content = CreatePanelContent();
            this.rootVisualElement.Q<VisualElement>("overlay-window-root").Add(content);
        }

        private VisualElement CreatePanelContent()
        {
            var scroll = new ScrollView()
            {
                name = "Content Scroll",
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Auto,
                style =
                {
                    top=10,
                }
            };
            var root = new VisualElement()
            {
                name = "Content View",
                style =
                {
                    display = DisplayStyle.Flex,
                    justifyContent =  Justify.FlexStart,
                    flexDirection = FlexDirection.Row,
                    flexGrow = 1,
                    flexShrink = 1,
                    flexWrap = Wrap.Wrap,
                }
            };
            scroll.Add(root);
            scroll.Add(new VisualElement() { style = { height = 20 } });

            var view = new IMGUIContainer(() =>
            {
                _DrawItems(scroll);
                _CollectFps();
            });
            root.Add(view);
            return scroll;
        }

        private void _CollectFps()
        {
            if (Event.current.type != EventType.Repaint) return;
            var now = EditorApplication.timeSinceStartup;
            var cost = (float)(now - _SampleTick);
            _SampleTick = now;
            _FpsTick += cost;
            _FpsQueue.Enqueue((int)(1 / Mathf.Max(1 / 240, cost)));
            if (_FpsTick > _FpsDuration)
            {
                _FpsTick = 0;
                _Fps = (int)_FpsQueue.Average();
                _FpsQueue.Clear();
            }
        }

        private void _DrawItems(ScrollView scroll)
        {
            if (_Items.Count < 1)
            {
                _Interrupt("空");
                return;
            }
            if (Application.isPlaying)
            {
                _Interrupt("不兼容运行模式");
                return;
            }
            if (EditorApplication.isUpdating)
            {
                _Interrupt("正在导入资源");
                return;
            }
            if (EditorApplication.isCompiling)
            {
                _Interrupt("正在编译代码");
                return;
            }
            var moleSize = _Cfg.CardSize[(int)_CardSize];
            var parentR = rootVisualElement.Q<VisualElement>("overlay-window-root").localBound;
            var colume = (int)(parentR.width - 20) / moleSize.x;
            var scale = (parentR.width - 20) / colume / moleSize.x;
            if (colume < 1)
            {
                _Interrupt("宽度不够");
                return;
            }
            var row = (_Items.Count - 1) / colume + 1;
            var itemW = (int)(moleSize.x * scale);
            var itemH = (int)(moleSize.y * scale);
            GUILayoutUtility.GetRect(itemW * colume, itemH * row + _Offset);

            var rangMin = scroll.scrollOffset.y - _Offset;
            var rangMax = scroll.scrollOffset.y + scroll.localBound.height;
            var startRow = Mathf.Max(0, (int)rangMin / itemH);
            var endRow = Mathf.Max(0, (int)rangMax / itemH) + 1;
            var startIndex = Mathf.Min(_Items.Count, startRow * colume);
            var endIndex = Mathf.Min(_Items.Count, endRow * colume);

            if (Event.current.type == EventType.Repaint)
            {
                _OnChangeViewItems(startIndex, endIndex);
                Repaint();
            }
            for (var i = startIndex; i < endIndex; i++)
            {
                var iRow = i / colume;
                var iColume = i - iRow * colume;
                var rect = default(Rect);
                rect.x = scale * moleSize.x * iColume;
                rect.y = scale * moleSize.y * iRow + _Offset;
                rect.width = scale * moleSize.x;
                rect.height = scale * moleSize.y;
                _DrawItem(rect, i, _Items[i]);
            }
        }

        private void _DrawItem(Rect rect, int i, PreviewItem item)
        {
            var e = Event.current;
            var isRepaint = e.type == EventType.Repaint;
            var r = rect;
            r.xMin += 2;
            r.xMax -= 2;
            r.yMax -= 4;
            var labelR = r;
            labelR.height = Style_Title.CalcHeight(TempContent(item.name), r.width);
            if (!item.obj)
            {
                if (isRepaint)
                {
                    EditorGUI.DrawRect(r, Color.gray);
                    GUI.Label(r, GC_Loading);
                    _DrawHead(labelR, item);
                }
                return;
            }
            if(isRepaint && item.go)
            {
                _DrawPreview(r, item);
                GUI.Label(labelR, "", EditorStyles.textArea);
                _DrawHead(labelR, item);
                labelR.y += 40;
            }
            if (rect.Contains(e.mousePosition) && item.go)
            {
                _DrawHover(r, item);
            }
        }

        private void _DrawHead(Rect labelR, PreviewItem item)
        {
            GUI.Label(labelR, item.name, Style_Title);
            labelR.y += labelR.height;
            labelR.height = 20;
            EditorGUI.IntField(labelR, item.index, EditorStyles.label);
        }

        private void _DrawHover(Rect rect, PreviewItem item)
        {
            var e = Event.current;
            var btnR = rect;
            btnR.size *= _CardBtnSize;
            btnR.center = rect.center;
            if (btnR.Contains(e.mousePosition) &&
                GUI.Button(btnR, item.playing?GC_Pause:GC_Play, Style_PlayBtn))
            {
                if (item.playing) Pause(item);
                else Play(item);
                e.Use();
            }
            var sbR = rect;
            sbR.yMin = rect.yMax - 20;
            sbR.width = 36;
            sbR.x = rect.xMax - sbR.width;
            if (GUI.Button(sbR, "创建"))
            {
                EditorGUIUtility.PingObject(GameObject.Instantiate(item.go));
                EditorApplication.Beep();
                e.Use();
            }
            sbR.x -= sbR.width;
            if (GUI.Button(sbR, "定位"))
            {
                Selection.activeObject = item.obj;
                EditorGUIUtility.PingObject(item.obj);
                EditorApplication.Beep();
                e.Use();
            }
            sbR.x -= sbR.width;
            if (GUI.Button(sbR, "信息"))
            {
                _PopupInfoMenu(item);
                e.Use();
            }
        }

        private void _PopupInfoMenu(PreviewItem item)
        {
            var info = new EffectInfo();
            info.effect = item.go;
            info.Collect();
            var menu = new GenericMenu();
            info.FillMenu(menu);
            menu.AddItem(TempContent($"加载时间\t{item.loadTime}"), false, null);
            menu.ShowAsContext();
        }

        private void _DrawPreview(Rect rect, PreviewItem item)
        {
            _VisiableItem(item, true);
            _Camera.target = item.go;
            _Camera.ControlCamera(item.renders);
            _PreviewStub.BeginPreview(rect, EditorStyles.toolbarButton);
            _PreviewStub.Render(_AllowSRP);
            _PreviewStub.EndAndDrawPreview(rect);
            _VisiableItem(item, false);
        }

        private void _OnChangeViewItems(int startIndex, int endIndex)
        {
            var lastS = _CurtStartIndex;
            var lastE = _CurtEndIndex;
            _CurtStartIndex = startIndex;
            _CurtEndIndex = endIndex;
            _LoadingQueue.Clear();
            var length = _Items.Count;
            for (var i = 0; i < length; i++)
            {
                var item = _Items[i];
                if (i < startIndex || i > endIndex)
                {
                    if (i < lastS || i > lastE) { }
                    else _HideItem(item);
                }
                else
                {
                    if (i < lastS || i > lastE) _ShowItem(item);
                    else { }
                }
                _VisiableItem(item, false);
            }
            for (var i = startIndex; i < endIndex; i++)
            {
                _AddToLoadingQueue(_Items[i]);
            }
            for (var i = endIndex; i < length; i++)
            {
                _AddToLoadingQueue(_Items[i]);
            }
            for (var i = 0; i < startIndex; i++)
            {
                _AddToLoadingQueue(_Items[i]);
            }
        }

        void _AddToLoadingQueue(PreviewItem item)
        {
            if (item.obj) return;
            if (item.ignoreLoad) return;
            _LoadingQueue.Enqueue(item);
        }

        private void _Interrupt(string v)
        {
            PauseAll();
            ResumeAll();
            ShowNotification(new GUIContent(v));
        }

        #endregion GUI

        GUIContent TempContent(string v)
        {
            if (_Temp == null) _Temp = new GUIContent();
            _Temp.text = v;
            return _Temp;
        }

        GUIContent _Temp;

        private int _CurtStartIndex;
        private int _CurtEndIndex;
        private List<Renderer> _GetRenderCache;
        private float _Offset = 20;
        float _CardBtnSize = 0.75f;
        bool _AllowSRP = false;

        public class PreviewItem
        {
            public int index;
            public string name;
            public string path;
            public int loadTime;
            public UnityEngine.Object obj;
            public GameObject go;
            public PreviewHandle playHandle;
            public Renderer[] renders;
            public bool ignoreLoad;
            public bool playing;

            internal void Clear()
            {
                index = 0;
                name = null;
                path = null;

                loadTime = 0;
                obj = null;
                if (go)
                {
                    GameObject.DestroyImmediate(go);
                }
                go = null;
                renders = null;
                playHandle = null;
                playing = false;
                ignoreLoad = false;
            }

        }
    }
}
