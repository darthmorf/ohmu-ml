using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.InputSystem;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;

public class CameraController : MonoBehaviour
{
    // Config Params
    [SerializeField] float arcBallSpeed = 1.0f;
    [SerializeField] float radius = 1.0f;


    // State
    Vector3 lastMousePosition = new Vector3();
    Vector3 arcBallOrigin = new Vector3();
    Vector3 sphericalCoords = new Vector3();

    // Constants
    const float halfPi = Mathf.PI / 2;


    void Start()
    {
        SetupArcBall();
    }

    void Update()
    {
        ArcBallUpdate();
    }


    void SetupArcBall()
    {
        transform.LookAt(arcBallOrigin);
        sphericalCoords = GetSphericalCoordinates(transform.position);
    }

    void ArcBallUpdate()
    {
        Vector3 currentMousePos = Mouse.current.position.ReadValue();

        if (Mouse.current.leftButton.IsPressed())
        {
            // Get the deltas that describe how much the mouse cursor got moved between frames
            float dx = (lastMousePosition.x - currentMousePos.x) * arcBallSpeed;
            float dy = (lastMousePosition.y - currentMousePos.y) * arcBallSpeed;

            // Only update the camera's position if the mouse got moved in either direction
            if (dx != 0f || dy != 0f)
            {
                // Rotate the camera left and right
                sphericalCoords.y += dx * Time.deltaTime;

                // Rotate the camera up and down
                // Prevent the camera from turning upside down
                sphericalCoords.z = Mathf.Clamp(sphericalCoords.z + dy * Time.deltaTime, -halfPi, halfPi);

                transform.position = GetCartesianCoordinates(sphericalCoords) + arcBallOrigin;
                transform.LookAt(arcBallOrigin);
            }
        }

        lastMousePosition = currentMousePos;
    }


    Vector3 GetSphericalCoordinates(Vector3 cartesian)
    {
        float r = Mathf.Sqrt(
            Mathf.Pow(cartesian.x, 2) +
            Mathf.Pow(cartesian.y, 2) + 
            Mathf.Pow(cartesian.z, 2)
        );

        float phi = Mathf.Atan2(cartesian.z / cartesian.x, cartesian.x);
        float theta = Mathf.Acos(cartesian.y / r);

        if (cartesian.x < 0)
        {
            phi += Mathf.PI;
        }

        return new Vector3(r, phi, theta);
    }

    Vector3 GetCartesianCoordinates(Vector3 spherical)
    {
        Vector3 ret = new Vector3();

        ret.x = spherical.x * Mathf.Cos(spherical.z) * Mathf.Cos(spherical.y);
        ret.y = spherical.x * Mathf.Sin(spherical.z);
        ret.z = spherical.x * Mathf.Cos(spherical.z) * Mathf.Sin(spherical.y);

        return ret;
    }
}
