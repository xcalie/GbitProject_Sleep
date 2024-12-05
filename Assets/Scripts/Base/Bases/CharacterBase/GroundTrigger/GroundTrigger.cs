using UnityEngine;

public class GroundTrigger : MonoBehaviour
{
    // 地面检测缓冲时间（减小到0.05秒使检测更精确）
    private float groundCheckBuffer = 0.05f;
    // 最后一次接触地面的时间
    private float lastGroundedTime;
    // 是否正在接触地面
    private bool isInContact = false;

    // 设置地面检测缓冲时间
    public void SetGroundCheckBuffer(float buffer)
    {
        groundCheckBuffer = buffer;
    }

    // 当触发器持续接触时调用
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            isInContact = true;
            lastGroundedTime = Time.time;
            MainControl.Instance.PlayerAddressSt = AddressState.ground;
        }
    }

    // 当触发器退出接触时调用
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            isInContact = false;
        }
    }

    // 每帧更新
    private void Update()
    {
        // 使用缓冲时间来判断接地状态
        if (!isInContact && Time.time - lastGroundedTime > groundCheckBuffer)
        {
            MainControl.Instance.PlayerAddressSt = AddressState.air;
        }
    }
}
