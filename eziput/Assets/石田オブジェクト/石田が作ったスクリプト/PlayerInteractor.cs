using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    public Camera playerCamera;
    public float interactDistance = 2f;
    public LayerMask wallHintLayer;

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F)) return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, wallHintLayer))
        {
            WallHint hint = hit.collider.GetComponentInParent<WallHint>();
            if (hint == null) return;

            if (hint.CanInteract(transform))
            {
                hint.Interact();
            }
        }
    }
}
