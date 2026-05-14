using UnityEngine;
using UnityEngine.UI;

namespace LLL
{
    public class TransparentRaycastTarget : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}
