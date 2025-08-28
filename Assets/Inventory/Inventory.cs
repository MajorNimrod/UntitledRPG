using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Inventory
{
    [SerializeField] int capacity = 10;
    [SerializeField] List<ItemStack> slots;

    public int Capacity => capacity;
    public IReadOnlyList<ItemStack> Slots => slots;

    public Inventory(int capacity = 10)
    {
        this.capacity = Mathf.Max(1, capacity);     // cannot be empty, min of 1
        slots = new List<ItemStack>(capacity);
        for (int i = 0; i < capacity; i++) slots.Add(new ItemStack());      // struct of ID and quantity
    }

    public bool TryAdd(ItemDefinition def, int qty, out int remainder)
    {
        remainder = qty;
        if (def == null || qty <= 0) return false;

        // 1. Fill existing stacks
        for (int i = 0; i < slots.Count && remainder > 0; i++)
        {
            var s = slots[i];
            if (s.def == def && s.quantity < def.maxStack)
            {
                int move = Mathf.Min(s.SpaceLeft, remainder);
                s.quantity += move;
                slots[i] = s;
                remainder -= move;
            }
        }

        // 2. Use empty slots
        for (int i = 0; i < slots.Count && remainder > 0; i++)
        {
            if (slots[i].isEmpty)
            {
                int move = Mathf.Min(def.maxStack, remainder);
                slots[i] = new ItemStack { def = def, quantity = move };
                remainder -= move;
            }
        }
        return remainder < qty; // true if we added anything
    }

    public bool TryRemove(ItemDefinition def, int qty)
    {
        if (def == null || qty <= 0) return false;

        int toRemove = qty;

        // Count available
        int available = 0;
        foreach (var s in slots) if (s.def == def) available += s.quantity;
        if (available < qty) return false;

        // Remove
        for (int i = 0; i < slots.Count && toRemove > 0; i++)
        {
            var s = slots[i];
            if (s.def != def) continue;
            int take = Mathf.Min(s.quantity, toRemove);
            s.quantity -= take;
            toRemove -= take;
            if (s.quantity <= 0) s = new ItemStack();   // incase the quantity doesn't exist after, replace the slot
            slots[i] = s;
        }
        return true;
    }

    public void Move(int fromIndex, int toIndex)
    {
        if (fromIndex == toIndex) return;
        if (fromIndex < 0 || fromIndex >= slots.Count) return;
        if (toIndex < 0 || toIndex >= slots.Count) return;

        var a = slots[fromIndex];
        var b = slots[toIndex];

        // Merge if same def and room
        if (!a.isEmpty && !b.isEmpty && a.def == b.def && b.quantity < b.def.maxStack)
        {
            int move = Mathf.Min(a.quantity, b.def.maxStack - b.quantity);
            b.quantity += move;
            a.quantity -= move;
            if (a.quantity <= 0) a = new ItemStack(); // if a is empty, initialize an ItemStack :)
            slots[fromIndex] = a;
            slots[toIndex] = b;
        }
        else
        {
            // Swap
            slots[fromIndex] = b;
            slots[toIndex] = a;
        }
    }
}
