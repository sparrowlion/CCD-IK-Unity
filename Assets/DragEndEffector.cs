using UnityEngine;

public class DragEndEffector : MonoBehaviour
{
    private Camera mainCam;
    private bool isDragging;

    void Start()
    {
        mainCam = Camera.main;
    }

    void OnMouseDown()
    {
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        if (!isDragging) return;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.up, transform.position);

        if (plane.Raycast(ray, out float distance))
        {
            transform.position = ray.GetPoint(distance);
        }
    }
}
