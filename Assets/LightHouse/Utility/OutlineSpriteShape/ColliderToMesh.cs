using NaughtyAttributes;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

public class ColliderToMesh : MonoBehaviour
{
    // Source to bake the mesh.
    [Required]
    [SerializeField]
    Collider2D _collider;
    // Generated mesh.
    // In edit mode, `BakeMesh()` will write the generated mesh to an asset.
    // In playmode, any additional calls to `BakeMesh()` will create a new mesh, instead of modifying the asset.
    [SerializeField]
    Mesh _mesh;

    // Bake a mesh from the collider, save it to an asset.
    [Button("Bake Collider to Mesh")]
    public void BakeMesh()
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
                if (_mesh != null && AssetDatabase.Contains(_mesh))
                {
                    // `_mesh` is already backed by an asset. Overwrite it.
                    dynamicMesh.name = _mesh.name;
                    // `Clear()` is needed to ensure `CopySerialized()` works properly for meshes.
                    _mesh.Clear();
                    EditorUtility.CopySerialized(dynamicMesh, _mesh);
                    DestroyImmediate(dynamicMesh);
                    EditorUtility.SetDirty(_mesh);
                    AssetDatabase.SaveAssetIfDirty(_mesh);
                }
                else
                {
                    string path = EditorUtility.SaveFilePanelInProject("Save Baked Mesh", dynamicMesh.name, "mesh", "Where should we store the baked mesh?");
                    dynamicMesh.hideFlags = HideFlags.None;
                    AssetDatabase.CreateAsset(dynamicMesh, path);
                    AssetDatabase.SaveAssets();
                    var oldMesh = _mesh;
                    _mesh = dynamicMesh;
                    if (oldMesh != null)
                        DestroyImmediate(oldMesh);
                }
            }
            else
            {
                // We are in the editor, play-mode.
                // We don't want changes made here to overwrite the mesh asset.
                if (_mesh != null && AssetDatabase.Contains(_mesh))
                {
                    // `_mesh` is already backed by an asset.
                    // Just forget the reference in hopes that the engine will unload it eventually.
                    _mesh = dynamicMesh;
                }
                else
                {
                    // `_mesh` is not backed by an asset; it's a runtime obeject or just null.
                    var oldMesh = _mesh;
                    _mesh = dynamicMesh;
                    if (oldMesh != null)
                        DestroyImmediate(oldMesh);
                }
            }
        }
        else
        {
            // We are in a build.
            // We don't have "assets" or "disks", so just destroy the old mesh, and assign a new one.
            var oldMesh = _mesh;
            _mesh = dynamicMesh;
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
        if (_mesh == null)
            return;

        if (Application.isEditor)
        {
            if (!AssetDatabase.Contains(_mesh))
            {
                // Destroy mesh in editor only if it is not an asset.
                var oldMesh = _mesh;
                _mesh = null;
                Destroy(oldMesh);
            }
        }
        else
        {
            var oldMesh = _mesh;
            _mesh = null;
            Destroy(oldMesh);
        }
    }
}
