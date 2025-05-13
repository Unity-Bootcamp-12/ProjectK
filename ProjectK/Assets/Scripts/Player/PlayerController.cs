using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("�ѱ�")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float defaultSpeed;
    [SerializeField] private float runSpeed;

    private PlayerMove playerMove;
    private float inputHorizontal;
    private float inputVertical;

    private void Awake()
    {
        defaultSpeed = 5.0f;
        runSpeed = 8.0f;
        moveSpeed = defaultSpeed;
        playerMove = GetComponent<PlayerMove>();
    }

    private void Update()
    {
        InputMove();
    }

    private void InputMove()
    {
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");

        moveSpeed = defaultSpeed; // �⺻ ������ �ӵ�

        if (Input.GetKey(KeyCode.LeftShift)) // �޸���
        {
            moveSpeed = runSpeed;
        }

        playerMove.Move(inputHorizontal * Time.deltaTime * moveSpeed, inputVertical * Time.deltaTime * moveSpeed);
    }
}
