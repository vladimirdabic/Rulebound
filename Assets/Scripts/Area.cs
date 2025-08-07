using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider2D))]
public class Area : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private BoxCollider2D _collider;

    void SetColliderSizeToCameraView()
    {
        float height = 2f * _camera.orthographicSize;
        float width = height * _camera.aspect;

        _collider.size = new Vector2(width + 0.001f, height + 0.001f);
        _collider.offset = Vector2.zero;
    }

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

    private void Reset()
    {
        //if (Application.isPlaying) return;

        _camera = Camera.main;
        if (_camera == null) return;

        _collider = GetComponent<BoxCollider2D>();
        SetColliderSizeToCameraView();
    }
}