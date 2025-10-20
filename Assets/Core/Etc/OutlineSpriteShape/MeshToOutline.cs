using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.U2D;

public class MeshToOutline : MonoBehaviour
{
    [Required]
    [ValidateInput("HasSpriteShapeController")]
    [SerializeField]
    GameObject _outlinePrafab;

    [SerializeField]
    Material _fillMaterial;
    [SerializeField]
    Material _outlineMaterial;
    // Outline width in pixels.
    [SerializeField]
    float _outlineWidth = 0.1f;
    [SerializeField]
    int _spriteSizeInPixels = 32;
    [SerializeField]
    int _spritePixelsPerUnit = 512;

    [SerializeField]
    Mesh _mesh;

    public List<SpriteShapeController> _generatedOutlines = new();

    [Button]
    public void BakeOutline()
    {
        for (int i = 0; i < _generatedOutlines.Count; i++)
        {
            if (_generatedOutlines[i] == null)
                continue;
            if (Application.isPlaying)
                Destroy(_generatedOutlines[i].gameObject);
            else
                DestroyImmediate(_generatedOutlines[i].gameObject);
        }
        _generatedOutlines.RemoveRange(0, _generatedOutlines.Count);

        if (_mesh == null)
            return;

        var outlines = FetchOutlines(_mesh);
        var vertices = _mesh.vertices;

        for (int i = 0; i < outlines.Count; i++)
        {
            var outline = Instantiate(_outlinePrafab, transform);
            outline.GetComponent<SpriteShapeRenderer>().sharedMaterials = new Material[] { _fillMaterial, _outlineMaterial };
            var spriteShapeController = outline.GetComponent<SpriteShapeController>();
            spriteShapeController.spline.Clear();
            _generatedOutlines.Add(spriteShapeController);
        }

        Assert.IsTrue(outlines.Count == _generatedOutlines.Count);

        float height = _outlineWidth * _spritePixelsPerUnit / _spriteSizeInPixels;
        Debug.Log(height);

        for (int i = 0; i < outlines.Count; i++)
        {
            var outline = outlines[i];
            var spriteShapeController = _generatedOutlines[i];
            spriteShapeController.spline.Clear();
            if (outline[0] == outline[outline.Count - 1])
            {
                spriteShapeController.spline.isOpenEnded = false;
                for (int oidx = 0; oidx < outline.Count - 1; oidx++)
                {
                    spriteShapeController.spline.InsertPointAt(oidx, vertices[outline[oidx]]);
                    spriteShapeController.spline.SetHeight(oidx, height);
                }
            }
            else
            {
                spriteShapeController.spline.isOpenEnded = true;
                for (int oidx = 0; oidx < outline.Count; oidx++)
                {
                    spriteShapeController.spline.InsertPointAt(oidx, vertices[outline[oidx]]);
                }
            }
            spriteShapeController.BakeMesh().Complete();
        }
    }

    List<List<int>> FetchOutlines(Mesh mesh)
    {
        HashSet<(int, int)> edges = new();
        void InsertEdgeToSet(int vidx1, int vidx2)
        {
            if (vidx1 == vidx2)
            {
                Debug.Log("A vertex appears twice in the same triangle.");
                throw new Exception();
            }
            if (vidx1 > vidx2)
                (vidx1, vidx2) = (vidx2, vidx1);

            if (edges.Contains((vidx1, vidx2)))
                edges.Remove((vidx1, vidx2));
            else
                edges.Add((vidx1, vidx2));
        }

        var tCount = mesh.triangles.Length;
        var triangles = mesh.triangles;
        if (tCount % 3 != 0)
        {
            Debug.Log("The size of `mesh.triangles` should always be a multiple of 3.");
            throw new Exception();
        }
        tCount = tCount / 3;
        for (int tidx = 0; tidx < tCount; tidx++)
        {
            int vidx1 = triangles[3 * tidx];
            int vidx2 = triangles[3 * tidx + 1];
            int vidx3 = triangles[3 * tidx + 2];
            InsertEdgeToSet(vidx1, vidx2);
            InsertEdgeToSet(vidx1, vidx3);
            InsertEdgeToSet(vidx2, vidx3);
        }

        // Now only edges that appear once will remain in the set.
        // We need to order the edges in a loop.
        // Build an adjacency map.
        Dictionary<int, List<int>> adj = new();
        foreach ((var vidx1, var vidx2) in edges)
        {
            if (!adj.ContainsKey(vidx1))
                adj[vidx1] = new();
            if (!adj.ContainsKey(vidx2))
                adj[vidx2] = new();
            adj[vidx1].Add(vidx2);
            adj[vidx2].Add(vidx1);
        }

        List<List<int>> outlines = new();

        while (adj.Count != 0)
        {
            List<int> outline = new();
            var current = adj.Keys.First();
            while (true)
            {
                outline.Add(current);
                if (adj[current].Count == 0)
                {
                    // This is possible only when `current` is the last vertex on the loop.
                    adj.Remove(current);
                    break;
                }
                var next = adj[current][0];
                adj[current].Remove(next);
                if (adj[current].Count == 0)
                    adj.Remove(current);
                adj[next].Remove(current);
                current = next;
            }
            outlines.Add(outline);
        }

        return outlines;
    }

    bool HasSpriteShapeController(GameObject prefab)
    {
        if (prefab == null)
            return true;
        if (prefab.GetComponent<SpriteShapeController>() == null)
            return false;
        if (prefab.GetComponent<SpriteShapeRenderer>() == null)
            return false;
        return true;
    }
}
