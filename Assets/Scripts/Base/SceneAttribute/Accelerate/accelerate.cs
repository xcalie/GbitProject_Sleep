using UnityEngine;
using System.Collections;

// 定义加速类型枚举
public enum AccelerateType
{
    FinalSpeed,    // 最终速度
    FastSlow,      // 快慢交替
    UniformSpeed,  // 均匀加速
}

// 加速器类
public class Accelerate : MonoBehaviour
{
    // 公共变量
    public float speedMultiple = 3.0f;        // 速度倍数
    public float maxSpeedLimit = 25f;         // 最大速度限制
    public float delayTime = 0.05f;           // 延迟时间
    public float transitionDuration = 0.3f;   // 过渡持续时间
    public AccelerateType accelerateType;     // 加速类型

    // 私有变量
    private GameObject playerObject;          // 玩家对象
    private Rigidbody2D playerRigidbody;      // 玩家刚体
    private Vector2 lastPosition;             // 上一帧位置

    private float resultSpeed;                // 目标速度
    private float timer = 0;                  // 计时器
    private Coroutine resetSpeedCoroutine;    // 重置速度协程
    private float originalSpeed;              // 原始速度

    // 判断是否为玩家或其子对象
    private bool IsPlayerOrChild(GameObject obj)
    {
        if (obj.CompareTag("Player"))
        {
            playerObject = obj;
            return true;
        }
            
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            if (parent.CompareTag("Player"))
            {
                playerObject = parent.gameObject;
                return true;
            }
            parent = parent.parent;
        }
        
        return false;
    }

    // 进入触发器
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsPlayerOrChild(collision.gameObject)) return;

        lastPosition = playerObject.transform.position;
        
        // 停止之前的重置速度协程
        if (resetSpeedCoroutine != null)
        {
            StopCoroutine(resetSpeedCoroutine);
        }
        
        // 计算目标速度
        originalSpeed = MainControl.Instance.OrginMaxMoveSpeed;
        resultSpeed = Mathf.Min(originalSpeed * speedMultiple, maxSpeedLimit);
        
        // 根据加速类型执行不同的加速逻辑
        switch (accelerateType)
        {
            case AccelerateType.FinalSpeed:
                MainControl.Instance.MaxMoveSpeed = resultSpeed;
                break;
            case AccelerateType.FastSlow:
            case AccelerateType.UniformSpeed:
                MainControl.Instance.MaxMoveSpeed = originalSpeed;
                timer = 0;
                break;
        }
    }

    // 持续触发
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!IsPlayerOrChild(collision.gameObject)) return;

        // 根据加速类型执行不同的加速逻辑
        switch (accelerateType)
        {
            case AccelerateType.FinalSpeed:
                MainControl.Instance.MaxMoveSpeed = resultSpeed;
                break;
            
            case AccelerateType.FastSlow:
                // 使用正弦函数实现快慢交替
                float t = Mathf.Sin(Time.time * 3f) * 0.5f + 0.5f;
                MainControl.Instance.MaxMoveSpeed = Mathf.Lerp(originalSpeed, resultSpeed, t);
                break;
            
            case AccelerateType.UniformSpeed:
                // 均匀加速
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / transitionDuration);
                float smoothProgress = progress * progress * (3f - 2f * progress);
                float currentSpeed = Mathf.Lerp(originalSpeed, resultSpeed, smoothProgress);
                MainControl.Instance.MaxMoveSpeed = currentSpeed;
                break;
        }
    }

    // 退出触发器
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsPlayerOrChild(collision.gameObject)) return;
        
        resetSpeedCoroutine = StartCoroutine(ResetSpeedWithDelay());
        
        playerObject = null;
        playerRigidbody = null;
    }

    // 延迟重置速度的协程
    private IEnumerator ResetSpeedWithDelay()
    {
        if (delayTime > 0)
        {
            yield return new WaitForSeconds(delayTime);
        }

        float startSpeed = MainControl.Instance.MaxMoveSpeed;
        float elapsedTime = 0;

        // 平滑过渡到原始速度
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            float smoothT = t * t * (3f - 2f * t);
            float currentSpeed = Mathf.Lerp(startSpeed, originalSpeed, smoothT);
            MainControl.Instance.MaxMoveSpeed = currentSpeed;
            yield return null;
        }

        MainControl.Instance.MaxMoveSpeed = originalSpeed;
    }
}
