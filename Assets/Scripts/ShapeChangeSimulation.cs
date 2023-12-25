// ******************************************************************
//       /\ /|       @file       ShapeChangeSimulation
//       \ V/        @brief      形状改变模拟组件
//       | "")       @author     Shadowrabbit, yingtu0401@gmail.com
//       /  |                    
//      /  \\        @Modified   2023-12-19 14:21
//    *(__\_\        @Copyright  Copyright (c) 2023, Shadowrabbit
// ******************************************************************

using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using Sirenix.OdinInspector;

public class ShapeChangeSimulation : MonoBehaviour
{
    [LabelText("形态列表")] public List<Sprite> sprites; //支持改变的状态
    [LabelText("形态恢复间隔(秒)")] public float revertInterval = 10f; //形态恢复间隔 秒
    private SpriteRenderer _spriteRenderer; //渲染器
    private int _curShapeIndex; //当前的形态索引 默认从0
    private int _maxShapeCount; //形态的数量
    private float _lastRevertTime; //上次还原形态的时间
    private Coroutine _coPlayAniMaxShape; //最大形变后重复播放的协程句柄
    [SerializeField] private AudioSource audioSourceClick; //点击音效
    [SerializeField] private AudioSource audioSourceRecover; //恢复音效

    private static float CurrentTime => Time.time; //当前时间

    protected void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>() ?? new SpriteRenderer();
        if (sprites.Count <= 0)
        {
            Debug.LogError($"没有设置形态的sprite");
        }

        _maxShapeCount = sprites.Count;
        _lastRevertTime = CurrentTime;
    }

    protected void Update()
    {
        if (_curShapeIndex <= 0)
        {
            return;
        }

        //还原状态
        if (CurrentTime - _lastRevertTime >= revertInterval)
        {
            RevertShape();
        }
    }

    /// <summary>
    /// 模拟点击
    /// </summary>
    [UsedImplicitly]
    public void SimulateClick()
    {
        _lastRevertTime = CurrentTime;
        if (audioSourceClick)
        {
            audioSourceClick.Play();
        }

        //正常情况 形态变化+1
        if (_curShapeIndex < _maxShapeCount - 1)
        {
            _curShapeIndex++;
            RefreshPerf();
            return;
        }

        //最大程度的形变 无法再触发的情况
        if (_coPlayAniMaxShape != null)
        {
            StopCoroutine(_coPlayAniMaxShape);
        }

        _coPlayAniMaxShape = StartCoroutine(nameof(CoPlayAniMaxShape));
    }

    /// <summary>
    /// 还原形态
    /// </summary>
    private void RevertShape()
    {
        //已经回到最初形态的情况
        if (_curShapeIndex <= 0)
        {
            return;
        }

        _curShapeIndex = 0;
        _lastRevertTime = CurrentTime;
        RefreshPerf();
        if (audioSourceRecover)
        {
            audioSourceRecover.Play();
        }
    }

    /// <summary>
    /// 刷新表现
    /// </summary>
    private void RefreshPerf()
    {
        _spriteRenderer.sprite = sprites[_curShapeIndex];
    }

    /// <summary>
    /// 播放最大变化状态时的一个程序动画 倒数第二帧->最后一帧
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoPlayAniMaxShape()
    {
        if (_maxShapeCount - 2 < 0)
        {
            Debug.LogError($"至少需要2帧sprite才可以实现尾部动画");
            yield break;
        }

        _curShapeIndex = _maxShapeCount - 2;
        RefreshPerf();
        yield return new WaitForSeconds(0.1f);
        _curShapeIndex = _maxShapeCount - 1;
        RefreshPerf();
    }
}