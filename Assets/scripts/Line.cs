using UnityEngine;
using System.Collections.Generic;

public class Line : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public BoxCollider2D boxCollider;

    public void DrawLine(Vector2 start, Vector2 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
        int lineWidth = (int)lineRenderer.endWidth;
        int lineLength = (int)Vector2.Distance(start, end);
        Vector2 midPoint = (start + end) / 2;
        boxCollider.size = new Vector2(lineLength, lineWidth);
        boxCollider.transform.position = midPoint;
        float angle = Mathf.Atan2((end.y - start.y), (end.x - start.x));
        angle *= Mathf.Rad2Deg;
        angle *= -1;
        boxCollider.transform.Rotate(0, 0, angle);
    }
}