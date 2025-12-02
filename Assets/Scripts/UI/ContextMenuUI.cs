using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContextMenuUI : MonoBehaviour
{
    public static ContextMenuUI Instance { get; private set; }

    [Header("References")]
    public RectTransform menuRoot;        // Panel containing the buttons
    public Button optionButtonPrefab;     // Prefab for each menu entry

    private readonly List<Button> _spawnedButtons = new List<Button>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Hide();
    }

    public void Show(Vector2 screenPosition, List<(string label, Action action)> options)
    {
        ClearButtons();

        foreach (var (label, action) in options)
        {
            var btn = Instantiate(optionButtonPrefab, menuRoot);
            _spawnedButtons.Add(btn);

            var text = btn.GetComponentInChildren<Text>();
            if (text != null)
                text.text = label;

            btn.onClick.AddListener(() =>
            {
                Hide();
                action?.Invoke();
            });
        }

        // Position the menu at the cursor
        menuRoot.gameObject.SetActive(true);
        RectTransform canvasRect = menuRoot.GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            null,
            out Vector2 localPoint
        );
        menuRoot.anchoredPosition = localPoint;
    }

    public void Hide()
    {
        ClearButtons();
        if (menuRoot != null)
            menuRoot.gameObject.SetActive(false);
    }

    private void ClearButtons()
    {
        foreach (var btn in _spawnedButtons)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }
        _spawnedButtons.Clear();
    }
}
