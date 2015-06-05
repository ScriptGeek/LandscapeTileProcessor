using UnityEngine;
using UnityEditor;
using Rotorz.Tile;
using Rotorz.Tile.Editor;
using System.IO;


public class LandscapeTileProcessor : AssetPostprocessor
{
    public void OnPostprocessModel(GameObject obj)
    {
        ModelImporter importer = assetImporter as ModelImporter;
        if (Path.GetDirectoryName(importer.assetPath).Equals("Assets/tiles"))
        {
			// Use your own file extension (I used premade assets that were .fbx files)
            if (Path.GetExtension(importer.assetPath) == ".fbx")
            {
                // Transform mesh axis from .fbx to Unity's axis system
                RotateObject(obj.transform);

				// Insert your path here:
                GameObject prefab = PrefabUtility.CreatePrefab("Assets/Landscape Tile Prefabs/" + obj.name + ".prefab", obj, ReplacePrefabOptions.ConnectToPrefab);
                Mesh mesh = Resources.LoadAssetAtPath<Mesh>(importer.assetPath);

                prefab.GetComponent<MeshFilter>().sharedMesh = mesh;
                prefab.AddComponent<MeshCollider>().sharedMesh = mesh;
                prefab.transform.localScale = Vector3.one;

                // Apply prefab to tile system brush selector
                OrientedBrush ob = BrushUtility.CreateOrientedBrush(obj.name) as OrientedBrush;
                var orientation = ob.DefaultOrientation;
                if (orientation.IndexOfVariation(prefab) == -1)
                {
                    orientation.AddVariation(prefab);
                    ob.SyncGroupedVariations(ob.DefaultOrientationMask);
                }
                EditorUtility.SetDirty(ob);
                AssetDatabase.SaveAssets();
                GameObject.DestroyImmediate(obj, true);
            }
        }
    }

    private void RotateObject(Transform obj)
    {
        obj.eulerAngles = new Vector3(obj.eulerAngles.x + 90, obj.eulerAngles.y, obj.eulerAngles.z);

        MeshFilter meshFilter = obj.GetComponent<MeshFilter>() as MeshFilter;
        if (meshFilter)
        {
            RotateMesh(meshFilter.sharedMesh);
        }
        foreach (Transform child in obj.transform)
        {
            RotateObject(child);
        }
    }

    private void RotateMesh(Mesh mesh)
    {
        int index = 0;
        Vector3[] vertices = mesh.vertices;
        for (index = 0; index < vertices.Length; index++)
        {
            vertices[index] = new Vector3(vertices[index].x, vertices[index].z, vertices[index].y);
        }
        mesh.vertices = vertices;
        for (int submesh = 0; submesh < mesh.subMeshCount; submesh++)
        {
            int[] triangles = mesh.GetTriangles(submesh);
            for (index = 0; index < triangles.Length; index += 3)
            {
                int intermediate = triangles[index];
                triangles[index] = triangles[index + 2];
                triangles[index + 2] = intermediate;
            }
            mesh.SetTriangles(triangles, submesh);
        }
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}
