using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [SerializeField] string id;     // unique, stable
    public string Id => id;

    public string displayName;
    public Sprite icon;
    public int maxStack = 99;
}
