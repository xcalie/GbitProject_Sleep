using UnityEngine;

[CreateAssetMenu(fileName = "NewAnimationData", menuName = "Animation/Animation Data")]
public class AnimationDataBase : ScriptableObject
{
    [System.Serializable]
    public class AnimationClipData
    {
        public string stateName;               // 动画状态名称
        public Sprite[] sprites;               // 动画精灵帧数组
        [Range(1, 60)]
        public float frameRate = 12f;          // 帧率
        public bool isLoop = true;             // 是否循环播放
        public bool useUnscaledTime = false;   // 是否使用未缩放时间
        
        // 音效相关
        public AudioClip soundEffect;          // 音效
        [Range(0, 1)]
        public float volume = 1f;              // 音量
        
        // 特效相关
        public GameObject effectPrefab;        // 特效预制体
        public Vector2 effectOffset;           // 特效偏移
        
        // 获取动画持续时间
        public float Duration => sprites.Length / frameRate;
    }

    public AnimationClipData[] clips;          // 动画片段数组
}
