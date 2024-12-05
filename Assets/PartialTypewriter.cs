using System;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class  PartialTypewriter : MonoBehaviour
{
    public byte OutputSpeed = 20; // 字符输出速度（字数/秒）
    public byte FadeRange = 10; // 字符淡化范围（字数）

    private TMP_Text _textComponent;
    private Coroutine _outputCoroutine;

    void Awake()
    {
        _textComponent = GetComponent<TMP_Text>();
        if (_textComponent == null)
        {
            Debug.LogError("Text component is not assigned!");
        }
    }
    public void OutputText(string text, Action<TypewriterState> onOutputEnd = null)
    {
        // 如果当前正在执行字符输出，将其中断
        if (_outputCoroutine != null)
        {
            StopCoroutine(_outputCoroutine);
        }
        _textComponent.text = text;
        // 开始新的字符输出协程
        if (FadeRange > 0)
        {
            _outputCoroutine = StartCoroutine(OutputCharactersFading());
        }
        else
        {
            _outputCoroutine = StartCoroutine(OutputCharactersNoFading(onOutputEnd));
        }
    }
    private IEnumerator OutputCharactersNoFading(Action<TypewriterState> onOutputEnd)
    {
        var textInfo = _textComponent.textInfo;
        _textComponent.maxVisibleCharacters = 0; // 初始时不显示任何字符

        var timer = 0f;
        var interval = 1.0f / OutputSpeed;
        while (_textComponent.maxVisibleCharacters < textInfo.characterCount)
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                _textComponent.maxVisibleCharacters++; // 逐个增加可见字符
                timer = 0;
            }
            yield return null;
        }

        //if (onOutputEnd != null)
        {
            //onOutputEnd.Invoke(TypewriterState.Completed); // 调用回调函数，通知输出完成
        }
    }
    private IEnumerator OutputCharactersFading()
    {
        // 确保字符处于可见状态
        var textInfo = _textComponent.textInfo;
        _textComponent.maxVisibleCharacters = textInfo.characterCount;
        _textComponent.ForceMeshUpdate();

        // 先将所有字符设置到透明状态
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            SetCharacterAlpha(i, 0);
        }

        // 按时间逐渐显示字符
        var timer = 0f;
        var interval = 1.0f / OutputSpeed;
        var headCharacterIndex = 0;
        while (headCharacterIndex < textInfo.characterCount)
        {
            timer += Time.deltaTime;
            var isFadeCompleted = true;
            var tailIndex = headCharacterIndex - FadeRange + 1;
            for (int i = headCharacterIndex; i > -1 && i >= tailIndex; i--)
            {
                var step = headCharacterIndex - i;
                var alpha = (byte)Mathf.Clamp((timer / interval + step) / FadeRange * 255, 0, 255);
                isFadeCompleted &= alpha == 255;
                SetCharacterAlpha(i, alpha);
            }
            _textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            if (timer >= interval)
            {
                timer = 0;
                headCharacterIndex++;
            }
            yield return null;
        }
    }

    private void SetCharacterAlpha(int index, byte alpha)
    {
        var materialIndex = _textComponent.textInfo.characterInfo[index].materialReferenceIndex;
        var vertexColors = _textComponent.textInfo.meshInfo[materialIndex].colors32;
        var vertexIndex = _textComponent.textInfo.characterInfo[index].vertexIndex;
        vertexColors[vertexIndex + 0].a = alpha;
        vertexColors[vertexIndex + 1].a = alpha;
        vertexColors[vertexIndex + 2].a = alpha;
        vertexColors[vertexIndex + 3].a = alpha;
    }

    public enum TypewriterState
    {
        Completed,
        Outputting,
        Interrupted
    }
}