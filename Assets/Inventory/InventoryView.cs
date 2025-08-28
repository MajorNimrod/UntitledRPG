using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [SerializeField] SlotView slotPrefab;

    SlotView[] slotViews;
    int? selectedIndex; // for simple click-to-move

    private void OnEnable()
    {
        // Ensure singleton exists, then subscribe
        InventoryManager.Instance.OnInventoryChanged += Redraw;
        BuildSlotsIfNeeded();
        Redraw();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= Redraw;
    }

    void BuildSlotsIfNeeded()
    {
        if (slotViews != null) return;

        var inv = InventoryManager.Instance.PlayerInventory;
        slotViews = new SlotView[inv.Slots.Count];

        for (int i = 0; i < slotViews.Length; i++)
        {
            var view = Instantiate(slotPrefab, transform);
            view.Bind(i, OnSlotClicked);
            slotViews[i] = view;
        }
    }

    void Redraw()
    {
        var inv = InventoryManager.Instance.PlayerInventory;

        for (int i = 0; i < inv.Slots.Count; i++)
        {
            var s = inv.Slots[i];
            if (s.isEmpty) slotViews[i].ShowEmpty();
            else slotViews[i].Show(s.def.icon, s.quantity);
        }

        // TODO (optional) visually indicate selection
        if (selectedIndex.HasValue)
        {
            // e.g., scale/outline the selected slot
            // slotViews[selectedIndex.Value].transform.localScale = Vector3.one * 1.05f;
        }
    }

    void OnSlotClicked(int index)
    {
        if (!selectedIndex.HasValue)
        {
            // first click = select source (ignore empty)
            var s = InventoryManager.Instance.PlayerInventory.Slots[index];
            if (!s.isEmpty) selectedIndex = index;
        }
        else
        {
            // second click = move/merge to target
            int from = selectedIndex.Value;
            int to = index;
            selectedIndex = null;

            InventoryManager.Instance.MoveSlot(from, to);
            // Redraw will be called by the manager's event
        }
    }
}
