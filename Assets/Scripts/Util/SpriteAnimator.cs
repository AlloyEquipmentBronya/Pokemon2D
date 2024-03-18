using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimator : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    List<Sprite> frames;
    float frameRate;

    int currentFrame;
    float timer;

    public SpriteAnimator(List<Sprite> frames, SpriteRenderer spriteRenderer, float frameRate = 0.16f)
    {
        this.frames = frames;
        this.spriteRenderer = spriteRenderer;
        this.frameRate = frameRate;
    }

    public void Start()
    {
        currentFrame = 0;
        timer = 0;
        spriteRenderer.sprite = frames[0];
    }

    /// <summary>
    /// 切换帧动画
    /// </summary>
    public void HandleUpdate()
    {
        timer += Time.deltaTime;
        //时间大于帧率
        if(timer>frameRate)
        {
            // 切换到下一张
            //当切换到最后张时 回到第一张
            currentFrame = (currentFrame + 1)%frames.Count;
            spriteRenderer.sprite = frames[currentFrame];
            //这一行代码将计时器减去帧率(frameRate)，以确保计时器仍然与帧率同步，并控制动画更新的频率。
            timer -= frameRate;
        }
    }

    public List<Sprite> Frames
    {
        get
        {
            return frames;
        }
    }
}
