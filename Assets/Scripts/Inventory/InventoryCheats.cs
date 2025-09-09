using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryCheats : MonoBehaviour
{
    public ItemDefinition testItem;

    [ContextMenu("Give 1 Test Item")]
    void GiveOne()
    {
        if (testItem) InventoryManager.Instance.Add(testItem, 1);
    }

    [ContextMenu("Remove 1 Test Item")]
    void RemoveOne()
    {
        if (testItem) InventoryManager.Instance.Remove(testItem, 1);
    }
}
