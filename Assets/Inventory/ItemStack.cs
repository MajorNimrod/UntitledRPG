using UnityEngine;

[System.Serializable]
public struct ItemStack
{
    public ItemDefinition def;
    public int quantity;

    public bool isEmpty => def == null || quantity <= 0;
    public int SpaceLeft => (def == null) ? 0 : Mathf.Max(0, def.maxStack - quantity);
}