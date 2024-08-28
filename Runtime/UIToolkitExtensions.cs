using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Extensions.Runtime
{
	public static class UIToolkitExtensions
	{
		public static async UniTask ScrollToCenter(this ScrollView scrollView, VisualElement childElement, float transitionTime)
		{
			if (childElement == null || !scrollView.contentContainer.Contains(childElement))
			{
				Debug.LogWarning("The specified child element is not a direct child of the ScrollView's content container.");
				
				return;
			}
			
			float scrollViewHeight = scrollView.contentViewport.resolvedStyle.height;
			
			float childYMin = childElement.worldBound.y - scrollView.contentContainer.worldBound.y;
			float childHeight = childElement.resolvedStyle.height;
			float childCenter = childYMin + (childHeight / 2);

			float targetScrollYOffset = childCenter - (scrollViewHeight / 2);

			targetScrollYOffset = Mathf.Clamp(targetScrollYOffset, scrollView.verticalScroller.lowValue, scrollView.verticalScroller.highValue);

			Vector2 initialScrollOffset = scrollView.scrollOffset;
			Vector2 targetScrollOffset = new(scrollView.scrollOffset.x, targetScrollYOffset);

			float elapsedTime = 0;

			while (elapsedTime < transitionTime)
			{
				elapsedTime += Time.deltaTime;

				Vector2 currentOffset = Vector2.Lerp(initialScrollOffset, targetScrollOffset, elapsedTime / transitionTime);
				scrollView.scrollOffset = currentOffset;

				await UniTask.Yield();
			}
		}

		public static void SetVisible(this VisualElement element, bool isVisible)
		{
			element.style.display = new StyleEnum<DisplayStyle>(isVisible ? DisplayStyle.Flex : DisplayStyle.None);
		}
		
		public static async UniTask SetVisible(this VisualElement element, bool isVisible, float transitionTime)
		{
			float initialOpacity = element.resolvedStyle.opacity;
			
			float startOpacity = isVisible ? 0 : initialOpacity;
			float endOpacity = isVisible ? initialOpacity : 0;

			if (isVisible)
			{
				element.style.opacity = new StyleFloat(startOpacity);
				element.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
			}

			float elapsedTime = 0;

			while (elapsedTime < transitionTime)
			{
				elapsedTime += Time.deltaTime;
				
				element.style.opacity = Mathf.Lerp(startOpacity, endOpacity, elapsedTime / transitionTime);
				await UniTask.Yield();
			}
			
			if (isVisible == false)
			{
				element.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
				element.style.opacity = new StyleFloat(initialOpacity);
			}
		}

		public static void SetBackground(this VisualElement element, Sprite sprite)
		{
			element.style.backgroundImage = new StyleBackground(sprite);
		}

		public static void CreateEditor<TConfig>(this VisualElement element, TConfig config)
			where TConfig : ScriptableObject
		{
			element.Clear();

			Editor editor = Editor.CreateEditor(config);

			if (editor == null)
			{
				return;
			}

			IMGUIContainer container = new(() => editor.OnInspectorGUI());
			element.Add(container);
		}
	}
}
