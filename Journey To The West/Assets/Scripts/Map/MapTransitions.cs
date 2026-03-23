using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;


public class MapTransitions : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundary;
    CinemachineConfiner2D confiner;

    [SerializeField] Direction direction;

    enum Direction { Up, Down, Left, Right };

    private void Awake(){
        confiner = FindObjectOfType<CinemachineConfiner2D>();

    }

    private void OnTriggerEnter2D(Collider2D collision){
        if(collision.gameObject.CompareTag("Player")){
            confiner.BoundingShape2D = mapBoundary;
            UpdatePlayerPosition(collision.gameObject);
        }
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        Vector2 newPos = player.transform.position;
        switch (direction)
        {
            case Direction.Up:
                newPos.y += 2;
                break;
            case Direction.Down:
                newPos.y -= 2;
                break;
            case Direction.Left:
                newPos.x -= 2;
                break;
            case Direction.Right:
                newPos.x += 2;
                break;
        }

        player.transform.position = newPos;
    }

}
