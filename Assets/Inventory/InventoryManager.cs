using System;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    public event Action OnInventoryChanged;

    [SerializeField] int startingCapacity = 10;
    public Inventory PlayerInventory { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        PlayerInventory = new Inventory(startingCapacity);
    }

    public bool Add(ItemDefinition def, int qty)
    {
        bool added = PlayerInventory.TryAdd(def, qty, out int remainder);
        if (added) OnInventoryChanged?.Invoke();
        // Optional, uncomment if needed. Handle remainder (drop to world)
        return added && remainder == 0;
    }

    public bool Remove(ItemDefinition def, int qty)
    {
        bool ok = PlayerInventory.TryRemove(def, qty);
        if (ok) OnInventoryChanged?.Invoke();
        return ok;
    }
    
    public void MoveSlot(int from, int to)
    {
        PlayerInventory.Move(from, to);
        OnInventoryChanged?.Invoke();
    }
}
