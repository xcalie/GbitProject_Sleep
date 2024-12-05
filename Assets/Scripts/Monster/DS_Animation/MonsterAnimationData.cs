using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterAnimationData", menuName = "Animation/Monster Animation Data")]
public class MonsterAnimationData : AnimationDataBase
{
    [System.Serializable]
    public new class AnimationClipData : AnimationDataBase.AnimationClipData
    {
        // 攻击相关数据
        public bool isAttackAnimation;    // 是否是攻击动画
        public float attackDamage;        // 攻击伤害
        public Vector2 attackRange;       // 攻击范围
        public Vector2 attackOffset;      // 攻击判定偏移
    }

    [SerializeField]
    private new AnimationClipData[] clips;

    public new AnimationClipData[] Clips => clips;
} 