using NaughtyAttributes;
using UnityEngine;
using System;
using UnityEditor.VersionControl;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ColliderToMesh : MonoBehaviour
{
    // Source to bake the mesh.
    [SerializeField]
    Collider2D _collider;
    // Destination for the mesh.
    // Note that we OWN `_meshFilter.sharedMesh`.
    [SerializeField]
    MeshFilter _meshFilter;
    [SerializeField]
    Mesh _mesh;

    // Bake a mesh from the collider, save it to an asset.
    [Button("Bake Collider to Mesh")]
    public void BakeCollider()
    {
        // If `_collider` is connected to a rigidbody, `_collider.CreateMesh(false, false)` will return a mesh in rigidbody-local space.
        // We call `_collider.CreateMesh(true, true)` to ensure the created mesh is always in world space regardless of collider type.
        Mesh dynamicMesh = _collider.CreateMesh(true, true);
        MeshToLocalSpace(dynamicMesh);
        if (Application.isEditor)
        {
            if (!Application.isPlaying)
            {
                // We are in the editor, edit-mode.
                if (_meshFilter.sharedMesh != null && AssetDatabase.Contains(_meshFilter.sharedMesh))
                {
                    // Mesh filter already has a mesh asset. Overwrite it.
                    dynamicMesh.name = _meshFilter.sharedMesh.name;
                    _meshFilter.sharedMesh.Clear();
                    EditorUtility.CopySerialized(dynamicMesh, _meshFilter.sharedMesh);
                    DestroyImmediate(dynamicMesh);
                    EditorUtility.SetDirty(_meshFilter.sharedMesh);
                    AssetDatabase.SaveAssetIfDirty(_meshFilter.sharedMesh);
                }
                else
                {
                    string path = EditorUtility.SaveFilePanelInProject("Save Baked Mesh", dynamicMesh.name, "mesh", "Where should we store the baked mesh?");
                    dynamicMesh.hideFlags = HideFlags.None;
                    AssetDatabase.CreateAsset(dynamicMesh, path);
                    AssetDatabase.SaveAssets();
                    var oldMesh = _meshFilter.sharedMesh;
                    _meshFilter.sharedMesh = dynamicMesh;
                    if (oldMesh != null)
                        DestroyImmediate(oldMesh);
                }
            }
            else
            {
                // We are in the editor, play-mode.
                // We don't want changes made here to overwrite the mesh asset.
                if (_meshFilter.sharedMesh != null && AssetDatabase.Contains(_meshFilter.sharedMesh))
                {
                    // Mesh filter already has a mesh asset.
                    // Just forget the reference in hopes that the engine will unload it eventually.
                    _meshFilter.sharedMesh = dynamicMesh;
                }
                else
                {
                    // Mesh filter doesn't have an asset; it's a runtime object or just null.
                    var oldMesh = _meshFilter.sharedMesh;
                    _meshFilter.sharedMesh = dynamicMesh;
                    if (oldMesh != null)
                        DestroyImmediate(oldMesh);
                }
            }
        }
        else
        {
            // We are in a build.
            // We don't have "assets" or "disks", so just destroy the old mesh, and assign a new one.
            var oldMesh = _meshFilter.sharedMesh;
            _meshFilter.sharedMesh = dynamicMesh;
            if (oldMesh != null)
                Destroy(oldMesh);
        }
    }

    // Move a mesh in world space to transform-local space.
    void MeshToLocalSpace(Mesh mesh)
    {
        var vertices = mesh.vertices;
        transform.InverseTransformPoints(vertices);
        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

    void OnDestroy()
    {
        if (_meshFilter.sharedMesh == null)
            return;

        if (Application.isEditor)
        {
            if (!AssetDatabase.Contains(_meshFilter.sharedMesh))
            {
                // Destroy mesh in editor only if it is not an asset.
                var oldMesh = _meshFilter.sharedMesh;
                _meshFilter.sharedMesh = null;
                DestroyImmediate(oldMesh);
            }
        }
        else
        {
            var oldMesh = _meshFilter.sharedMesh;
            _meshFilter.sharedMesh = null;
            DestroyImmediate(oldMesh);
        }
    }
}
