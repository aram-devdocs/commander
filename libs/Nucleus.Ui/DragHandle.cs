using UnityEngine;
using UnityEngine.EventSystems;

namespace Nucleus.Ui
{
    /// <summary>uGUI drag handle: dragging this graphic moves the Target RectTransform. Clicks pass through.</summary>
    public sealed class DragHandle : MonoBehaviour, IDragHandler
    {
        public RectTransform Target;

        public void OnDrag(PointerEventData e)
        {
            if (Target != null) Target.anchoredPosition += e.delta;
        }
    }
}
