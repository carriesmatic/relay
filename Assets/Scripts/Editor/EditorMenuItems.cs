using UnityEditor;
using UnityEngine;

public class EditorMenuItems : MonoBehaviour
{
	[MenuItem("Custom/Resize transform to Unity Unit")]
	static void ResizeTransformToUnityUnit()
	{
        var gameObject = Selection.activeTransform.gameObject;
        MeshFilter mf = gameObject.GetComponent<MeshFilter>();

        if (mf == null)
            return;
        Mesh mesh = mf.sharedMesh;

        //***Set this to renderer bounds instead of mesh bounds***
        Bounds bounds = gameObject.GetComponent<Renderer>().bounds;

        float size = bounds.size.x;
        if (size < bounds.size.y)
            size = bounds.size.y;
        if (size < bounds.size.z)
            size = bounds.size.z;

        if (Mathf.Abs(1.0f - size) < 0.01f)
        {
            Debug.Log("Already unit size.");
            return;
        }

        float scale = 1.0f / size;

        Vector3[] verts = mesh.vertices;

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] = verts[i] * scale;
        }

        mesh.vertices = verts;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
	}

    [MenuItem("Custom/Rotate transform to isometric view")]
    static void RotateTransformToIsometricView()
    {
        var transform = Selection.activeTransform;
        var rotation = transform.rotation;
        Undo.RecordObject(transform, "Rotate transform to isometric view");
        rotation.y = 230f;
        transform.rotation = rotation;
    }

	// Validate the menu item defined by the function above.
	// The menu item will be disabled if this function returns false.
	[MenuItem("Custom/Resize transform to Unity Unit", true)]
    [MenuItem("Custom/Rotate transform to isometric view", true)]
	static bool ValidateResizeTransformToUnityUnit()
	{
		// Return false if no transform is selected.
		return Selection.activeTransform != null;
	}

	// Add a menu item called "Double Mass" to a Rigidbody's context menu.
	[MenuItem("CONTEXT/Transform/Rotate transform to isometric view")]
	static void RotateTransformToIsometricView(MenuCommand command)
	{
        var transform = (Transform)command.context;
        var rotation = transform.rotation;
        Undo.RecordObject(transform, "Rotate transform to isometric view");
        rotation.x = 0f;
        rotation.y = 230f;
        rotation.z = 0f;
        transform.rotation = rotation;
	}

	// Add a menu item to create custom GameObjects.
	// Priority 1 ensures it is grouped with the other menu items of the same kind
	// and propagated to the hierarchy dropdown and hierarch context menus.
	[MenuItem("GameObject/MyCategory/Custom Game Object", false, 10)]
	static void CreateCustomGameObject(MenuCommand menuCommand)
	{
		// Create a custom game object
		GameObject go = new GameObject("Custom Game Object");
		// Ensure it gets reparented if this was a context click (otherwise does nothing)
		GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
		// Register the creation in the undo system
		Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
		Selection.activeObject = go;
	}
}