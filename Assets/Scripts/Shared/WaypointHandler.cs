using Unity.Cinemachine;
using UnityEngine;
//using UnityEngine.Splines;

public class WaypointHandler : MonoBehaviour
{
    public JType JumpType = JType.EXACT;
    public Vector3 TeleportTo;
    public BoxCollider2D CameraBounds;
    public CinemachineConfiner2D Confiner;
    public float TeleportOffset;
    public Direction TeleportDirection;

    public enum Direction
    {
        UP, DOWN, LEFT, RIGHT
    }

    public enum JType
    {
        EXACT, RELATIVE
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        //Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
        Vector3 newPosition = other.transform.position;

        switch(JumpType)
        {
            case JType.EXACT:
                newPosition = TeleportTo;
                break;

            case JType.RELATIVE:
                newPosition = RelativeJump(other.transform);
                break;
        }

        other.transform.position = newPosition;
        Confiner.BoundingShape2D = CameraBounds;
        Confiner.InvalidateBoundingShapeCache();
    }

    Vector3 RelativeJump(Transform tr)
    {
        switch(TeleportDirection)
        {
            case Direction.UP:
                return new Vector3(tr.position.x, transform.position.y + TeleportOffset, tr.position.z);

            case Direction.DOWN:
                return new Vector3(tr.position.x, transform.position.y - TeleportOffset, tr.position.z);

            case Direction.LEFT:
                return new Vector3(transform.position.x - TeleportOffset, tr.position.y, tr.position.z);

            case Direction.RIGHT:
                return new Vector3(transform.position.x + TeleportOffset, tr.position.y, tr.position.z);

        }

        return transform.position + Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 location = Vector3.zero;

        switch (JumpType)
        {
            case JType.EXACT:
                location = TeleportTo;
                break;

            case JType.RELATIVE:
                location = RelativeJump(transform);
                break;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(location, 0.15f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, location);
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(
            transform.position + (Vector3)collider.offset,
            collider.size
        );
    }
}
