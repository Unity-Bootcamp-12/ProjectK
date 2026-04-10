using UnityEngine;
public class BotInputProvider : MonoBehaviour
{
    public Transform target;
    private PlayerController controller;
    private PlayerSight sight;
    private float strafeDir = 1f;
    private float strafeTimer;
    private float dodgeTimer;
    private float shootTimer;
    private float moveTimer;
    private Vector3 currentMove;
    private const float ATTACK_RANGE = 25f;
    private const float KEEP_DISTANCE = 20f;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        sight = GetComponent<PlayerSight>();
    }

    private void Update()
    {
        if (target == null || controller == null) return;
        Vector3 toTarget = target.position - transform.position;
        float dist = toTarget.magnitude;

        // 조준
        Vector3 randomOffset = new Vector3(
            Random.Range(-10f, 10f),
            0f,
            Random.Range(-10f, 10f)
        );
        Vector3 aimPos = target.position + randomOffset;
        controller.mouseWorldPosition = aimPos;
        sight.SetSightDirection(aimPos);
        // 이동 - 0.5~1초 유지
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0f)
        {
            Vector3 strafe = Vector3.Cross(toTarget.normalized, Vector3.up) * strafeDir;
            currentMove = dist > KEEP_DISTANCE
                ? (toTarget.normalized + strafe * 0.4f).normalized
                : strafe;
            moveTimer = Random.Range(1.5f, 3f);
        }
        controller.InputMove(MoveType.Walk, currentMove.x, currentMove.z);

        // 스트레이프 전환
        strafeTimer -= Time.deltaTime;
        if (strafeTimer <= 0f)
        {
            strafeDir = Random.value > 0.5f ? 1f : -1f;
            strafeTimer = Random.Range(1.5f, 2.5f);
        }

        // 구르기
        dodgeTimer -= Time.deltaTime;
        if (dodgeTimer <= 0f)
        {
            controller.Dodge();
            dodgeTimer = Random.Range(5f, 10f);
        }

        // 사격 - 0.8초에 한번
        if (dist <= ATTACK_RANGE)
        {
            shootTimer -= Time.deltaTime;
            if (shootTimer <= 0f)
            {
                if (!controller.playerGun.canShoot)
                {
                    controller.InputReload();
                }
                controller.InputAttack();
                shootTimer = 0.8f;
            }
        }
    }
}