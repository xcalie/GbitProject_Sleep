using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class voice : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI text;

    // Start is called before the first frame update
    void Awake()
    {
        slider.maxValue = 100f; // 设置默认音量为最大（对应100%）
        slider.minValue = 0f;
    }

    // Update is called once per frame
    private void Start()
    {
        slider.onValueChanged.AddListener(ValueChanged);
    }

    void Update()
    {
        volumecontrol(); // 每帧更新音量
    }

    public void volumecontrol()
    {
        // 设置音量
        audioSource.volume = slider.value;
        // 显示音量百分比
        //text.text = ((int)(volume * 100)).ToString() + "%";
        text.text = ((int)slider.value).ToString() + "%";
    }

    private void ValueChanged(float value)
    {
        GlobalAudioManager.instance.setGlobalAudioVolume(value/100f);
        text.text = ((int)value).ToString() + "%";
    }
}