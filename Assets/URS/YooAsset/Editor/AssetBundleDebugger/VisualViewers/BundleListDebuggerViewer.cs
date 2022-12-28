#if UNITY_2019_4_OR_NEWER
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace YooAsset.Editor
{
	internal class BundleListDebuggerViewer
	{
		private VisualTreeAsset _visualAsset;
		private TemplateContainer _root;

		private ListView _bundleListView;
		private ListView _usingListView;
		private DebugReport _debugReport;

		/// <summary>
		/// 初始化页面
		/// </summary>
		public void InitViewer()
		{
			// 加载布局文件
			string rootPath = "Assets/Packages/URS/YooAsset";
			string uxml = $"{rootPath}/Editor/AssetBundleDebugger/VisualViewers/{nameof(BundleListDebuggerViewer)}.uxml";
			_visualAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxml);
			if (_visualAsset == null)
			{
				Debug.LogError($"Not found {nameof(BundleListDebuggerViewer)}.uxml : {uxml}");
				return;
			}
			_root = _visualAsset.CloneTree();
			_root.style.flexGrow = 1f;

			// 资源包列表
			_bundleListView = _root.Q<ListView>("TopListView");
			_bundleListView.makeItem = MakeAssetListViewItem;
			_bundleListView.bindItem = BindAssetListViewItem;

#if UNITY_2020_1_OR_NEWER
			_bundleListView.onSelectionChange += BundleListView_onSelectionChange;
#else
			_bundleListView.onSelectionChanged += BundleListView_onSelectionChange;
#endif
			// 使用列表
			_usingListView = _root.Q<ListView>("BottomListView");
			_usingListView.makeItem = MakeIncludeListViewItem;
			_usingListView.bindItem = BindIncludeListViewItem;
		}

		/// <summary>
		/// 填充页面数据
		/// </summary>
		public void FillViewData(DebugReport debugReport, string searchKeyWord)
		{
			_debugReport = debugReport;
			_bundleListView.Clear();
			_bundleListView.ClearSelection();
			_bundleListView.itemsSource = FilterViewItems(debugReport, searchKeyWord);
			_bundleListView.Rebuild();
		}
		private List<DebugBundleInfo> FilterViewItems(DebugReport debugReport, string searchKeyWord)
		{
			Dictionary<string, DebugBundleInfo> result = new Dictionary<string, DebugBundleInfo>(debugReport.ProviderInfos.Count);
			foreach (var providerInfo in debugReport.ProviderInfos)
			{
				foreach (var bundleInfo in providerInfo.BundleInfos)
				{
					if (string.IsNullOrEmpty(searchKeyWord) == false)
					{
						if (bundleInfo.BundleName.Contains(searchKeyWord) == false)
							continue;
					}
					if (result.ContainsKey(bundleInfo.BundleName) == false)
						result.Add(bundleInfo.BundleName, bundleInfo);
				}
			}
			return result.Values.ToList();
		}

		/// <summary>
		/// 挂接到父类页面上
		/// </summary>
		public void AttachParent(VisualElement parent)
		{
			parent.Add(_root);
		}

		/// <summary>
		/// 从父类页面脱离开
		/// </summary>
		public void DetachParent()
		{
			_root.RemoveFromHierarchy();
		}


		// 顶部列表相关
		private VisualElement MakeAssetListViewItem()
		{
			VisualElement element = new VisualElement();
			element.style.flexDirection = FlexDirection.Row;

			{
				var label = new Label();
				label.name = "Label1";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				label.style.flexGrow = 1f;
				label.style.width = 280;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label3";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 100;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label4";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 120;
				element.Add(label);
			}

			return element;
		}
		private void BindAssetListViewItem(VisualElement element, int index)
		{
			var sourceData = _bundleListView.itemsSource as List<DebugBundleInfo>;
			var bundleInfo = sourceData[index];

			// Bundle Name
			var label1 = element.Q<Label>("Label1");
			label1.text = bundleInfo.BundleName;

			// Ref Count
			var label3 = element.Q<Label>("Label3");
			label3.text = bundleInfo.RefCount.ToString();

			// Status
			StyleColor textColor;
			if (bundleInfo.Status == AssetBundleLoader.EStatus.Fail)
				textColor = new StyleColor(Color.yellow);
			else
				textColor = label1.style.color;
			var label4 = element.Q<Label>("Label4");
			label4.text = bundleInfo.Status.ToString();
			label4.style.color = textColor;
		}
		private void BundleListView_onSelectionChange(IEnumerable<object> objs)
		{
			foreach (var item in objs)
			{
				DebugBundleInfo bundleInfo = item as DebugBundleInfo;
				FillUsingListView(bundleInfo.BundleName);
			}
		}

		// 底部列表相关
		private VisualElement MakeIncludeListViewItem()
		{
			VisualElement element = new VisualElement();
			element.style.flexDirection = FlexDirection.Row;

			{
				var label = new Label();
				label.name = "Label1";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				label.style.flexGrow = 1f;
				label.style.width = 280;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label2";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 150;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label3";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 150;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label4";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 100;
				element.Add(label);
			}

			{
				var label = new Label();
				label.name = "Label5";
				label.style.unityTextAlign = TextAnchor.MiddleLeft;
				label.style.marginLeft = 3f;
				//label.style.flexGrow = 1f;
				label.style.width = 120;
				element.Add(label);
			}

			return element;
		}
		private void BindIncludeListViewItem(VisualElement element, int index)
		{
			List<DebugProviderInfo> providers = _usingListView.itemsSource as List<DebugProviderInfo>;
			DebugProviderInfo providerInfo = providers[index];

			// Asset Path
			var label1 = element.Q<Label>("Label1");
			label1.text = providerInfo.AssetPath;

			// Spawn Scene
			var label2 = element.Q<Label>("Label2");
			label2.text = providerInfo.SpawnScene;

			// Spawn Time
			var label3 = element.Q<Label>("Label3");
			label3.text = providerInfo.SpawnTime;

			// Ref Count
			var label4 = element.Q<Label>("Label4");
			label4.text = providerInfo.RefCount.ToString();

			// Status
			var label5 = element.Q<Label>("Label5");
			label5.text = providerInfo.Status.ToString();
		}
		private void FillUsingListView(string bundleName)
		{	
			List<DebugProviderInfo> source = new List<DebugProviderInfo>();
			foreach (var providerInfo in _debugReport.ProviderInfos)
			{
				foreach (var bundleInfo in providerInfo.BundleInfos)
				{
					if (bundleInfo.BundleName == bundleName)
					{
						source.Add(providerInfo);
						continue;
					}
				}
			}

			_usingListView.Clear();
			_usingListView.ClearSelection();
			_usingListView.itemsSource = source;
			_usingListView.Rebuild();
		}
	}
}
#endif