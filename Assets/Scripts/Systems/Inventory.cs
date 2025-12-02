using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public int maxSlots = 28;

    private List<string> _items = new List<string>();

    public bool HasSpace => _items.Count < maxSlots;

    public void AddItem(string itemId)
    {
        if (!HasSpace)
        {
            Debug.Log("Inventory full!");
            return;
        }

        _items.Add(itemId);
        Debug.Log($"Picked up: {itemId}. Total items: {_items.Count}");
    }
}
