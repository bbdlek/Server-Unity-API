using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UILineRenderer : Graphic
{
    public List<Vector2> points;
    public float thickness = 2f;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (points == null || points.Count < 2)
            return;

        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;
        float xMin = rectTransform.rect.xMin;
        float yMin = rectTransform.rect.yMin;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector2 start = points[i];
            Vector2 end = points[i + 1];

            Vector2 dir = (end - start).normalized;
            Vector2 normal = new Vector2(-dir.y, dir.x) * thickness / 2;

            Vector2 v1 = start + normal;
            Vector2 v2 = start - normal;
            Vector2 v3 = end + normal;
            Vector2 v4 = end - normal;

            UIVertex[] quad = new UIVertex[4];

            quad[0] = UIVertex.simpleVert;
            quad[0].color = color;
            quad[0].position = new Vector2(v1.x * width + xMin, v1.y * height + yMin);

            quad[1] = UIVertex.simpleVert;
            quad[1].color = color;
            quad[1].position = new Vector2(v2.x * width + xMin, v2.y * height + yMin);

            quad[2] = UIVertex.simpleVert;
            quad[2].color = color;
            quad[2].position = new Vector2(v4.x * width + xMin, v4.y * height + yMin);

            quad[3] = UIVertex.simpleVert;
            quad[3].color = color;
            quad[3].position = new Vector2(v3.x * width + xMin, v3.y * height + yMin);

            vh.AddUIVertexQuad(quad);
        }
    }

    public void SetPoints(List<Vector2> points)
    {
        this.points = points;
        SetVerticesDirty();
    }
}