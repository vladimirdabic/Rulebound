using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider2D))]
public class AreaGizmos : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        Vector3 position = transform.position + (Vector3)collider.offset;

        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(position, collider.size);

        Vector3 halfSize = collider.size / 2;
        Vector3 location = new Vector3(
            position.x - halfSize.x + 0.1f,
            position.y + halfSize.y - 0.13f
        );

#if UNITY_EDITOR
        Handles.Label(location, name);
#endif
    }
}