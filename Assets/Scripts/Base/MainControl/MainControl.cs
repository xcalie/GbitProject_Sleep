using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 定义玩家的运动状态枚举
public enum MotionState
{
    Run,   // 跑步
    Walk,  // 行走
    Stand, // 站立
}

// 定义玩家的位置状态枚举
public enum AddressState
{
    ground, // 地面
    air,    // 空中
}

// 定义玩家信息的类，用于保存玩家的基本属性
public class PlayerInfoBefore
{
    public float MoveSpeed; // 移动速度
    public float JumpSpeed; // 跳跃速度
    public int HP;          // 生命值
    public float GroundAcceleration; // 地面加速度
    public float GroundSpeed; // 地面速度
    public float GroundDeceleration; // 地面减速度
    public float AirAcceleration; // 空中加速度
    public float AirDeceleration; // 空中减速度
    public float MaxMoveSpeed; // 最大移动速度
    public float MoveAcceleration; // 移动加速度
    public float JumpHeight; // 跳跃高度
    public float JumpTimeToApex; // 到达最高点所需时间

    // 构造函数，初始化玩家信息
    public PlayerInfoBefore()
    {
        MoveSpeed = MainControl.Instance.PlayerMoveSpeed;
        JumpSpeed = MainControl.Instance.PlayerJumpSpeed;
        HP = MainControl.Instance.PlayerHP;
        GroundAcceleration = MainControl.Instance.GroundAcceleration;
        GroundSpeed = MainControl.Instance.PlayerMoveSpeed;
        GroundDeceleration = MainControl.Instance.GroundDeceleration;
        AirAcceleration = MainControl.Instance.AirAcceleration;
        AirDeceleration = MainControl.Instance.AirDeceleration;
        MaxMoveSpeed = MainControl.Instance.MaxMoveSpeed;
        MoveAcceleration = MainControl.Instance.MoveAcceleration;
        JumpHeight = MainControl.Instance.JumpHeight;
        JumpTimeToApex = MainControl.Instance.JumpTimeToApex;
    }
}

// 主控制类，继承自SingletonAutoMono<MainControl>
public class MainControl : SingletonAutoMono<MainControl>
{
    // 中控激活标志
    public Boolean Live = false;

    //判断背景音乐是否存在对应文件
    public bool isBack = false;

    //判断是否死亡
    public bool isDie = false;


    [Header("血量追寻")]
    [SerializeField]
    private Canvas canvas;
    private blood blood;
    private Player player;

    // 添加睡眠系统引用
    [Header("睡眠系统")]
    private SleepSystem sleepSystem;


    #region 场景数据

    // 场景切换标志
    private int nowSceneIndex = 0;
    [Header("切换关卡是否回血")]
    public Boolean ReFillHP = true; // 切换关卡是否回血
    // 第二关激活睡梦环境标志
    public Boolean FlagSleepSystem = false;
    public GameObject CameraSleepSystem;

    #endregion

    #region 玩家数据

    #region 位置状态

    [Header("玩家位置状态")]
    public MotionState PlayerMotionSt = MotionState.Stand; // 玩家当前的运动状态
    public AddressState PlayerAddressSt = AddressState.ground; // 玩家当前的位置状态

    #endregion

    #region 移动数据

    [Header("玩家移动数据")]
    [SerializeField]
    private float playerMoveSpeed = 10; // 玩家当前的移动速度
    public float PlayerMoveSpeed
    {
        get => playerMoveSpeed;
        set => playerMoveSpeed = value;
    }

    [SerializeField]
    private float playerJumpSpeed = 15; // 玩家当前的跳跃速度
    public float PlayerJumpSpeed
    {
        get => playerJumpSpeed;
        set => playerJumpSpeed = value;
    }

    [SerializeField]
    private float playerOrginMove; // 玩家原始的移动速度
    public float PlayerOrginMove
    {
        get => playerOrginMove;
        set => playerOrginMove = value;
    }

    [SerializeField]
    private float playerOrginJump; // 玩家原始的跳跃速度
    public float PlayerOrginJump
    {
        get => playerOrginJump;
        set => playerOrginJump = value;
    }

    [Header("移动惯性参数")]
    [SerializeField] private float groundAcceleration = 75f;    // 地面加速度
    public float GroundAcceleration
    {
        get => groundAcceleration;
        set => groundAcceleration = value;
    }

    [SerializeField] private float groundDeceleration = 70f;    // 地面减速度
    public float GroundDeceleration
    {
        get => groundDeceleration;
        set => groundDeceleration = value;
    }

    [SerializeField] private float airAcceleration = 20f;       // 空中加速度
    public float AirAcceleration
    {
        get => airAcceleration;
        set => airAcceleration = value;
    }

    [SerializeField] private float airDeceleration = 10f;       // 空中减速度
    public float AirDeceleration
    {
        get => airDeceleration;
        set => airDeceleration = value;
    }

    [Header("移动和跳跃参数")]
    [SerializeField] private float maxMoveSpeed = 9f;        // 最大移动速度
    public float MaxMoveSpeed
    {
        get => maxMoveSpeed;
        set => maxMoveSpeed = value;
    }


    [SerializeField] private float orginMaxMoveSpeed;        // 原始最大移动速度
    public float OrginMaxMoveSpeed
    {
        get => orginMaxMoveSpeed;
        set => orginMaxMoveSpeed = value;
    }


    [SerializeField] private float moveAcceleration = 90f;   // 移动加速度
    public float MoveAcceleration
    {
        get => moveAcceleration;
        set => moveAcceleration = value;
    }

    [SerializeField] private float jumpHeight = 3f;          // 跳跃高度
    public float JumpHeight
    {
        get => jumpHeight;
        set => jumpHeight = value;
    }

    [SerializeField] private float jumpTimeToApex = 0.4f;    // 到达最高点所需时间
    public float JumpTimeToApex
    {
        get => jumpTimeToApex;
        set => jumpTimeToApex = value;
    }

    // 计算得出的跳跃参数
    public float JumpForce => (2f * jumpHeight) / jumpTimeToApex;  // 初始跳跃速度
    public float Gravity => (-2f * jumpHeight) / (jumpTimeToApex * jumpTimeToApex);  // 重力

    #endregion

    #region 血量数据

    [Header("玩家血量数据")]
    [SerializeField]
    private int playerHP = 3; // 玩家当前的生命值
    public int PlayerHP
    {
        get => playerHP;
        set => playerHP = value;
    }

    [SerializeField]
    private int playerOrginHP = 3; // 玩家最大的生命值
    public int PlayerOrginHP
    {
        get => playerOrginHP;
        set => playerOrginHP = value;
    }

    #endregion

    #endregion

    #region 怪物数据

    #region 移动数据&血量数据

    /* 怪物数据标号
     * 
     * 0 : 梦魇
     * 1 : 梦影
     */

    [SerializeField]
    private float[] monsterMoveSpeed = new float[] { 5, 5 }; // 怪物的移动速度数组
    public float[] MonsterMoveSpeed
    {
        get => monsterMoveSpeed;
        set => monsterMoveSpeed = value;
    }

    [SerializeField]
    private float[] monsterJumpSpeed = new float[] { 1, 1 }; // 怪物的跳跃速度数组
    public float[] MonsterJumpSpeed
    {
        get => monsterJumpSpeed;
        set => monsterJumpSpeed = value;
    }

    [SerializeField]
    private int[] monsterHP = new int[] { 1, -1 }; // 怪物的最大生命值数组

    #region 调试数据(废弃)

    // 以下是被注释掉的调试数据，目前已废弃
    //[SerializeField]
    //private float[] monsterOrginSpeed = new float[] { 1, 1 };//原始移动速度(用于调试)
    //public float[] MonsterOrginSpeed
    //{
    //    get => monsterOrginSpeed;
    //    set => monsterOrginSpeed = value;
    //}

    //[SerializeField]
    //private float[] monsterOrginJump = new float[] { 1, 1 };//原始跳跃��度(用于调试)
    //public float[] MonsterOrginJump
    //{
    //    get => monsterOrginJump;
    //    set => monsterOrginJump = value;
    //}

    #endregion

    #endregion

    #endregion

    #region 关卡碎片统计

    [Header("关卡碎片统计")]
    public int numShard = 0; // 碎片数量,初始为零

    #endregion

    #region 交互属性

    public Dictionary<IntractionType, CircleInfo> IntractionInfo = new Dictionary<IntractionType, CircleInfo>{
        {IntractionType.CrystalBall, new CircleInfo(10f, Vector2.zero)},
        {IntractionType.Guitar, new CircleInfo(2f, new Vector2(0.0f , 3.0f))},
        {IntractionType.Sugar, new CircleInfo(5f, Vector2.zero)},
        {IntractionType.OverBridge, new CircleInfo(1f, Vector2.zero)},
        {IntractionType.Shard, new CircleInfo(2f, Vector2.zero)},
        {IntractionType.BlackHole, new CircleInfo(2f, Vector2.zero)},
        {IntractionType.Triangle, new CircleInfo(6f, Vector2.zero) },
        {IntractionType.Clock, new CircleInfo(10f, Vector2.zero) }
    };

    #endregion

    [Header("击退参数")]
    [SerializeField] private float knockbackMultiplier = 0.8f;    // 进一步减小击退力度倍数，从1.2改为0.8
    public float KnockbackMultiplier
    {
        get => knockbackMultiplier;
        set => knockbackMultiplier = value;
    }

    /// <summary>
    /// 加载数据
    /// </summary>
    private void Start()
    {
        AbilityPickup.Instance.isActive = true;//激活能力拾取
        MusicControl.Instance.isMusic = true;//激活音乐中控

        // 加载玩家数据
        MainControl.Instance.LoadData();
        //MainControl.Instance.SaveJson();

        // 加载画布
        canvas = GameObject.Find("UI").GetComponent<Canvas>();
        blood = canvas.GetComponent<blood>();
        player = GameObject.Find("Player").GetComponent<Player>();

        // 获取睡眠系统引用
        if (Camera.main != null)
        {
            sleepSystem = Camera.main.GetComponent<SleepSystem>();
            if (sleepSystem == null)
            {
                Debug.LogWarning("主相机上没有找到SleepSystem组件！");
            }
        }
    }

    private void Update()
    {
        // 激活睡梦环境
        //OpenSleepSystem();
        JudgeChangeScene();
        JudgeAndPlayBack();
        ReLoad();
    }

    public bool isReload = false;
    private void ReLoad()
    {
        if (Input.GetKeyDown(KeyCode.P) || isDie)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Invoke("ReLoadPlayer", 0.1f);
            player.ResetPlayer();
            player.ResetPlayerState();
            isReload = true;
            isDie = false;
        }
    }


    /// <summary>
    /// 判断场景切换
    /// </summary>
    private void JudgeChangeScene()
    {
        if (nowSceneIndex != SceneManager.GetActiveScene().buildIndex || isReload)
        {
            Debug.Log("场景切换");
            //再次加载数据
            canvas = GameObject.Find("UI").GetComponent<Canvas>();
            blood = canvas.GetComponent<blood>();
            player = GameObject.Find("Player").GetComponent<Player>();

            nowSceneIndex = SceneManager.GetActiveScene().buildIndex;
            ASForBack = null;
            isBack = true;
            isReload = false;
            // 切换关卡回血
            PlayerHP = PlayerOrginHP;
            numShard = 0;
            if (nowSceneIndex == 4) numShard = 3;
        }
    }

    /// <summary>
    /// 返回关卡背景音乐选择
    /// </summary>
    /// <returns></returns>
    private int[] LevelBKSelect()
    {
        switch (nowSceneIndex)
        {
            case 0:
                return new int[] { 1 };
            case 1:
                return new int[] { 1 };
            case 2:
                return new int[] { 3, 0 };
            case 3:
                return new int[] { 0 };
            case 4:
                return new int[] { 4 };
            case 5:
                return new int[] { 5 };
            case 6:
                return new int[] { 5 };
        }
        return new int[] { 0 };
    }


    // 背景音乐播放
    public AudioSource[] ASForBack;
    private void JudgeAndPlayBack()
    {
        if (MainControl.Instance.isBack)
        {
            MainControl.Instance.isBack = false;
            Debug.Log(nowSceneIndex);
            //ASForBack = new AudioSource[MusicControl.Instance.MusicList[(int)MusicType.BGM].Count];
            ASForBack = new AudioSource[50];

            int[] ints = LevelBKSelect();
            Debug.Log(ints.Length);
            for (int i = 0; i < ints.Length; i++)
            {
                MusicControl.Instance.CreateAndPlay(player.gameObject, ref ASForBack[i], MusicType.BGM, ints[i], true, MusicControl.Instance.BGMVolume);
            }

        }
    }



    /// <summary>
    /// 激活睡梦环境(废弃,手动开启)
    /// </summary>
    [Obsolete("原方法已经废弃，请手动开启睡梦环境")]
    private void OpenSleepSystem()
    {
        //如果在第二关激活睡梦环境(测试关卡为TestScene3)
        if (SceneManager.GetActiveScene().name == "level-2" && FlagSleepSystem == false)
        {
            //找到主摄像机
            CameraSleepSystem = GameObject.Find("Main Camera");
            SleepSystem sleepSystem = CameraSleepSystem.GetComponent<SleepSystem>();
            if (CameraSleepSystem != null)
            {
                Debug.Log("激活睡梦环境");
                // 激活睡梦环境
                sleepSystem.ActivateSystem();   
                FlagSleepSystem = true;
            }
        }
    }

    private AudioSource ASForTakeDamage;
    // 受伤方法
    public void TakeDamage(float damage, Vector2 attackerPosition, float knockbackForce = 2f)
    {
        // 如果玩家处于无敌状态则直接返回
        if (player.IsInvincible) return;

        MusicControl.Instance.CreateAndPlay(gameObject, ref ASForTakeDamage, MusicType.Death, 2, false, MusicControl.Instance.AUDIOVolume);

        // 扣除生命值并更新UI
        MainControl.Instance.PlayerHP -= Mathf.RoundToInt(damage);  // 将float类型的伤害转换为int
        MainControl.Instance.PlayerHP = Mathf.Clamp(MainControl.Instance.PlayerHP, 0, MainControl.Instance.PlayerOrginHP);
        blood.UpdateHealthImages(false);
        Debug.Log(attackerPosition);
        Debug.Log(MainControl.Instance.PlayerHP);

        // 启动无敌时间
        player.StartInvincibilityPeriod();

        // 重置疲劳值
            if (sleepSystem != null)
            {
            sleepSystem.FullyAwake();
            }

        // 启动击退协程
        StartCoroutine(ApplyKnockback(attackerPosition, knockbackForce));
        StartCoroutine(DisablePlayerControlTemporarily());

        // 检查是否死亡
        Debug.Log("玩家生命值：" + MainControl.Instance.PlayerHP);
        if (MainControl.Instance.PlayerHP <= 0)
        {
            Debug.Log("玩家死亡");
            player.Die();
        }
    }


    public void GetHP(int hp)
    {
        PlayerHP += hp;
        PlayerHP = Mathf.Clamp(PlayerHP, 0, PlayerOrginHP);
        blood.UpdateHealthImages(true);
    }

    // 处理击退效果的协程
    private IEnumerator ApplyKnockback(Vector2 attackerPosition, float knockbackForce)
    {
        if (player == null) yield break;
        
        Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb == null) yield break;

        // 保存并临时修改时间缩放
        float originalTimeScale = Time.timeScale;
        Time.timeScale = 1f;

        // 计算击退参数，进一步减小垂直方向的力
        float knockbackAmount = knockbackForce * knockbackMultiplier;
        Vector2 knockbackDirection = ((Vector2)player.transform.position - attackerPosition).normalized;
        knockbackDirection += Vector2.up * 0.2f; // 进一步减小垂直方向的力，从0.4改为0.2
        knockbackDirection.Normalize();

        // 清除当前速度
        playerRb.velocity = Vector2.zero;

        // 应用击退力，减小持续时间
        float knockbackDuration = 0.12f; // 一步减小击退持续时间，从0.15改为0.12
        float elapsedTime = 0f;
        Vector2 initialPosition = playerRb.position;

        while (elapsedTime < knockbackDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / knockbackDuration;
            
            // 使用缓动函数使击退更自然
            float easedProgress = 1 - (1 - progress) * (1 - progress);
            
            // 计算并设置新位置
            Vector2 newPosition = initialPosition + (knockbackDirection * knockbackAmount * easedProgress);
            playerRb.MovePosition(newPosition);

            yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime);
        }

        // 恢复原来的时间缩放值
        Time.timeScale = originalTimeScale;
    }

    // 临时禁用玩家控制的协程
    private IEnumerator DisablePlayerControlTemporarily()
    {
        player.DisableControl();
        yield return new WaitForSecondsRealtime(0.2f); // 减小控制禁用时间，从0.3改为0.2
        player.EnableControl();
    }

    #region Json配置文件

    // 保存玩家数据到Json文件
    public void SaveJson()
    {
        PlayerInfoBefore playerInfoBefore = new PlayerInfoBefore();
        JsonDataManager.Instance.SaveData(playerInfoBefore, "PlayerInfoBasic");
        Debug.Log("保存完毕");
    }

    // 从Json文件加载玩家数据
    public void LoadData()
    {
        PlayerInfoBefore playerInfoBefore = JsonDataManager.Instance.LoadData<PlayerInfoBefore>("PlayerInfoBasic");
        if (playerInfoBefore != null)
        {
            playerOrginMove = playerMoveSpeed = playerInfoBefore.MoveSpeed;
            playerOrginJump = playerJumpSpeed = playerInfoBefore.JumpSpeed;
            playerHP = playerOrginHP = playerInfoBefore.HP;
            GroundAcceleration = playerInfoBefore.GroundAcceleration;
            PlayerMoveSpeed = playerInfoBefore.GroundSpeed;
            GroundDeceleration = playerInfoBefore.GroundDeceleration;
            AirAcceleration = playerInfoBefore.AirAcceleration;
            AirDeceleration = playerInfoBefore.AirDeceleration;
            OrginMaxMoveSpeed = MaxMoveSpeed = playerInfoBefore.MaxMoveSpeed;
            MoveAcceleration = playerInfoBefore.MoveAcceleration;
            JumpHeight = playerInfoBefore.JumpHeight;
            JumpTimeToApex = playerInfoBefore.JumpTimeToApex;

        }
    }

    #endregion

}
