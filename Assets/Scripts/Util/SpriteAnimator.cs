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
    /// �л�֡����
    /// </summary>
    public void HandleUpdate()
    {
        timer += Time.deltaTime;
        //ʱ�����֡��
        if(timer>frameRate)
        {
            // �л�����һ��
            //���л��������ʱ �ص���һ��
            currentFrame = (currentFrame + 1)%frames.Count;
            spriteRenderer.sprite = frames[currentFrame];
            //��һ�д��뽫��ʱ����ȥ֡��(frameRate)����ȷ����ʱ����Ȼ��֡��ͬ���������ƶ������µ�Ƶ�ʡ�
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
