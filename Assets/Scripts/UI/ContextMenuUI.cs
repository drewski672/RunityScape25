using System.Collections.Generic;
using Runity.Gameplay.Interactions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Runity.Gameplay.UI
{
    public class ContextMenuUI : MonoBehaviour
    {
        [SerializeField] private Color panelColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private Font font;

        private Canvas canvas;
        private RectTransform panel;
        private readonly List<Button> buttons = new();

        private void Awake()
        {
            EnsureEventSystem();
            SetupCanvas();
        }

        public void Show(Vector2 screenPosition, string title, IEnumerable<ContextMenuAction> actions)
        {
            ClearButtons();
            AddButton(title, null, true);

            foreach (ContextMenuAction action in actions)
            {
                AddButton(action.Label, () =>
                {
                    action.Callback?.Invoke();
                    Hide();
                });
            }

            panel.gameObject.SetActive(true);
            panel.position = screenPosition;
        }

        public void Hide()
        {
            if (panel != null)
            {
                panel.gameObject.SetActive(false);
            }
        }

        private void SetupCanvas()
        {
            font ??= Resources.GetBuiltinResource<Font>("Arial.ttf");

            GameObject canvasObject = new("ContextMenuCanvas");
            canvasObject.layer = LayerMask.NameToLayer("UI");
            canvasObject.transform.SetParent(transform);

            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject panelObject = new("ContextMenuPanel");
            panelObject.transform.SetParent(canvas.transform, false);
            panel = panelObject.AddComponent<RectTransform>();
            Image background = panelObject.AddComponent<Image>();
            background.color = panelColor;

            VerticalLayoutGroup layout = panelObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.spacing = 6f;

            panel.pivot = new Vector2(0f, 1f);
            panel.sizeDelta = new Vector2(220f, 10f);
            panel.gameObject.SetActive(false);
        }

        private void AddButton(string label, System.Action onClick, bool isHeader = false)
        {
            GameObject buttonObject = new(isHeader ? "Header" : "ActionButton");
            buttonObject.transform.SetParent(panel, false);
            buttons.Add(buttonObject.AddComponent<Button>());

            Image image = buttonObject.AddComponent<Image>();
            image.color = isHeader ? panelColor : new Color(0.2f, 0.2f, 0.2f, 0.9f);
            buttons[^1].targetGraphic = image;

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200f, isHeader ? 26f : 32f);

            GameObject textObject = new("Label");
            textObject.transform.SetParent(buttonObject.transform, false);
            Text text = textObject.AddComponent<Text>();
            text.font = font;
            text.text = label;
            text.color = textColor;
            text.alignment = isHeader ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 12;
            text.resizeTextMaxSize = 18;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            if (onClick != null)
            {
                buttons[^1].onClick.AddListener(() => onClick());
            }
            else
            {
                buttons[^1].interactable = false;
            }
        }

        private void ClearButtons()
        {
            foreach (Button button in buttons)
            {
                if (button != null)
                {
                    Destroy(button.gameObject);
                }
            }

            buttons.Clear();
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            GameObject eventSystem = new("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
    }
}
