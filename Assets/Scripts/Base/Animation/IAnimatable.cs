/// <summary>
/// 定义动画系统的基本行为接口
/// </summary>
public interface IAnimatable
{
    /// <summary>
    /// 播放指定名称的动画
    /// </summary>
    /// <param name="stateName">动画状态名称</param>
    /// <param name="forceRestart">是否强制重新开始播放,即使当前正在播放该动画</param>
    void PlayAnimation(string stateName, bool forceRestart = false);

    /// <summary>
    /// 停止当前播放的动画
    /// </summary>
    void StopAnimation();

    /// <summary>
    /// 暂停当前播放的动画
    /// </summary>
    void PauseAnimation();

    /// <summary>
    /// 恢复播放暂停的动画
    /// </summary>
    void ResumeAnimation();

    /// <summary>
    /// 检查指定动画是否正在播放
    /// </summary>
    /// <param name="stateName">要检查的动画状态名称</param>
    /// <returns>如果指定动画正在播放则返回true,否则返回false</returns>
    bool IsPlaying(string stateName);

    /// <summary>
    /// 获取当前动画播放的进度
    /// </summary>
    /// <returns>返回0-1之间的值,表示动画播放进度</returns>
    float GetAnimationProgress();

    /// <summary>
    /// 翻转精灵图像
    /// </summary>
    /// <param name="flip">true表示翻转,false表示恢复原始方向</param>
    void FlipSprite(bool flip);
}