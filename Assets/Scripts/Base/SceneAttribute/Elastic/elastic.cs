using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class elastic : MonoBehaviour
{
    // 音效
    public AudioSource ASForElastic;

    // 力度
    public float Strength = 3;

    // 获取刚体组件
    private Rigidbody2D rgb;

    // 添加冷却时间变量
    private float cooldownTime = 0.5f; // 设置0.5秒的冷却时间
    private float lastBounceTime = 0f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // 检查冷却时间
            if (Time.time - lastBounceTime < cooldownTime)
            {
                return;
            }

            rgb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (rgb != null)
            {
                // 播放音乐
                MusicControl.Instance.CreateAndPlay(gameObject, ref ASForElastic, MusicType.Movement, 0, false, 0.5f);

                ContactPoint2D contact = collision.GetContact(0);
                float normalY = contact.normal.y;
                float normalX = contact.normal.x;
                
                // 判断是否是水平碰撞（左右侧面）
                if (Mathf.Abs(normalX) > 0.7f)  // 水平碰撞
                {
                    // 将玩家速度设为0
                    rgb.velocity = Vector2.zero;
                    lastBounceTime = Time.time;
                }
                // 垂直碰撞（上下方向）保持原有逻辑
                else if (normalY < -0.7f || normalY > 0.7f)
                {
                    Vector2 bounceDirection = normalY < 0 ? Vector2.up : Vector2.down;
                    rgb.velocity = bounceDirection * Strength;
                    
                    // 获取Player组件并重置跳跃次数
                    Player player = collision.gameObject.GetComponent<Player>();
                    if (player != null)
                    {
                        player.ResetJumpCount();
                    }
                    
                    lastBounceTime = Time.time;
                }
            }
        }
    }
}
