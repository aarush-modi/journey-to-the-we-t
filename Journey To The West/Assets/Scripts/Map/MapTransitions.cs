using UnityEngine;
using Unity.Cinemachine;
using System;

public class MapTransitions : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundary;
    [SerializeField] Direction direction;
    [SerializeField] Transform teleportTargetPosition;
    [SerializeField] float additionalPosition = 2f;

    CinemachineConfiner2D confiner;
    CinemachineCamera vcam;

    enum Direction { Up, Down, Left, Right, Teleport }

    private void Awake()
    {
        confiner = FindObjectOfType<CinemachineConfiner2D>();
        vcam = FindObjectOfType<CinemachineCamera>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            FadeTransition(collision.gameObject);
        }
    }

    async void FadeTransition(GameObject player)
    {
        await ScreenFader.Instance.FadeOut();

        confiner.BoundingShape2D = mapBoundary;
        confiner.InvalidateBoundingShapeCache();
        UpdatePlayerPosition(player);

        if (direction == Direction.Teleport)
        {
            vcam.ForceCameraPosition(
                teleportTargetPosition.position + new Vector3(0, 0, -10f),
                Quaternion.identity
            );
        }

        await ScreenFader.Instance.FadeIn();
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        if (direction == Direction.Teleport)
        {
            player.transform.position = teleportTargetPosition.position;
            return;
        }

        Vector2 newPosition = player.transform.position;
        switch (direction)
        {
            case Direction.Up:    newPosition.y += additionalPosition; break;
            case Direction.Down:  newPosition.y -= additionalPosition; break;
            case Direction.Left:  newPosition.x -= additionalPosition; break;
            case Direction.Right: newPosition.x += additionalPosition; break;
        }
        player.transform.position = newPosition;
    }
}