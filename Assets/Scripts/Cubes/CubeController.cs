using UnityEngine;

public class CubeController : MonoBehaviour
{
    private float speed;
    public float Speed {
        get { return speed; }
        set { speed = value; }
    }

    void Update()
    {
        Vector3 dir;
        dir.x = Input.GetAxis("Horizontal");
        dir.y = 0;
        dir.z = Input.GetAxis("Vertical");

        if (dir != Vector3.zero)
        {
            transform.position += dir * speed * Time.deltaTime;
            MovingCubes.Instance.onCubeUpdated.Invoke(transform.position);
        }
    }
}