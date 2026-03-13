using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPC : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 200f;

    private float yVelocity;
    private float gravity = -9.8f;

    private float xRotation = 0f;

    private CharacterController controller;
    public Transform cameraHolder;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        Debug.Log($"W:{Input.GetKey(KeyCode.W)} A:{Input.GetKey(KeyCode.A)} S:{Input.GetKey(KeyCode.S)} D:{Input.GetKey(KeyCode.D)}");
        // 鼠标控制视角
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // 移动
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        if (controller.isGrounded && yVelocity < 0)
        {
            yVelocity = -2f;
        }

        yVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveSpeed * move + Vector3.up * yVelocity;

        controller.Move(velocity * Time.deltaTime);
        DebugGroundCheck();
    }

    void DebugGroundCheck()
    {
        // 胶囊底部世界坐标
        float bottomY = transform.position.y + controller.center.y - controller.height / 2f;

        // 胶囊顶部世界坐标
        float topY = transform.position.y + controller.center.y + controller.height / 2f;

        // 是否贴地
        bool grounded = controller.isGrounded;

        // 向下射线检测地面
        RaycastHit hit;
        bool hitGround = Physics.Raycast(transform.position, Vector3.down, out hit, 5f);

        float groundY = hitGround ? hit.point.y : -999f;

        Debug.Log(
            $"PlayerY:{transform.position.y:F2} | " +
            $"BottomY:{bottomY:F2} | " +
            $"GroundY:{groundY:F2} | " +
            $"Grounded:{grounded}"
        );
    }
}