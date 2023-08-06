using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;

    private Transform cam;
    private World world;

    public float walkSpeed = 3f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float gravity = -9.8f;

    public float playerWidth = 0.3f;
    public float boundsTolerance = 0.1f;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0f;
    private bool jumpRequeset;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();
    }

    private void FixedUpdate()
    {
        CalculateVelocity();
        if (jumpRequeset)
            Jump();

        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.Rotate(Vector3.right * -mouseVertical);
        transform.Translate(velocity, Space.World);
    }

    public void Update()
    {
        GetPlayerInputs();
    }


    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequeset = false;
    }

    private void CalculateVelocity()
    {
        if (verticalMomentum > gravity)
            verticalMomentum += Time.fixedDeltaTime * gravity;

        if (isSprinting)
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime *
                       sprintSpeed;
        else
            velocity = ((transform.forward * vertical) + (transform.right * horizontal)) * Time.fixedDeltaTime *
                       walkSpeed;

        velocity += Vector3.up * verticalMomentum * Time.fixedDeltaTime;

        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;

        if (velocity.y < 0)
            velocity.y = CheckDownSpeed(velocity.y);
        else if (velocity.y > 0)
            velocity.y = CheckUpSpeed(velocity.y);
    }

    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
            isSprinting = true;
        if (Input.GetButtonUp("Sprint"))
            isSprinting = false;

        if (isGrounded && Input.GetButtonDown("Jump"))
            jumpRequeset = true;
    }

    private float CheckDownSpeed(float downSpeed)
    {
        if (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed,
                transform.position.z - playerWidth)) || world.CheckForVoxel(new Vector3(
                transform.position.x + playerWidth,
                transform.position.y + downSpeed,
                transform.position.z - playerWidth)) || world.CheckForVoxel(new Vector3(
                transform.position.x + playerWidth,
                transform.position.y + downSpeed,
                transform.position.z + playerWidth)) || world.CheckForVoxel(new Vector3(
                transform.position.x - playerWidth,
                transform.position.y + downSpeed,
                transform.position.z + playerWidth)))
        {
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }


    private float CheckUpSpeed(float upSpeed)
    {
        if (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 2f + upSpeed,
                transform.position.z - playerWidth)) || world.CheckForVoxel(new Vector3(
                transform.position.x + playerWidth,
                transform.position.y + 2f + upSpeed,
                transform.position.z - playerWidth)) || world.CheckForVoxel(new Vector3(
                transform.position.x + playerWidth,
                transform.position.y + 2f + upSpeed,
                transform.position.z + playerWidth)) || world.CheckForVoxel(new Vector3(
                transform.position.x - playerWidth,
                transform.position.y + 2f + upSpeed,
                transform.position.z + playerWidth)))
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    public bool front
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y,
                    transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f,
                    transform.position.z + playerWidth)))
                return true;
            else
                return false;
        }
    }

    public bool back
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y,
                    transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + 1f,
                    transform.position.z - playerWidth)))
                return true;
            else
                return false;
        }
    }


    public bool left
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y,
                    transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + 1f,
                    transform.position.z)))
                return true;
            else
                return false;
        }
    }

    public bool right
    {
        get
        {
            if (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y,
                    transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + 1f,
                    transform.position.z)))
                return true;
            else
                return false;
        }
    }
}