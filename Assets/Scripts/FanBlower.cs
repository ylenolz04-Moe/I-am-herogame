using UnityEngine;

public class FanBlower : MonoBehaviour
{
    [Header("风力设置")]
    [Tooltip("吹风方向（单位向量），默认向上")]
    public Vector2 blowDirection = Vector2.up;
    [Tooltip("风力大小")]
    public float blowForce = 15f;
    [Tooltip("风力模式：Continuous=持续吹, Pulse=脉冲吹")]
    public FanMode mode = FanMode.Continuous;

    [Header("脉冲模式（mode=Pulse 时生效）")]
    [Tooltip("每次吹风持续秒数")]
    public float pulseDuration = 1f;
    [Tooltip("两次吹风之间的间隔秒数")]
    public float pulseInterval = 2f;

    [Header("特效")]
    [Tooltip("拖入 WindEffect 粒子系统")]
    public ParticleSystem windParticles;

    [Header("动画")]
    [Tooltip("拖入风扇本体的 Animator")]
    public Animator fanAnimator;

    [Header("开关")]
    public bool isOn = true;

    private float timer;
    private bool isBlowing;

    public enum FanMode { Continuous, Pulse }

    private void Start()
    {
        isBlowing = (mode == FanMode.Continuous);
        timer = 0f;

        // 初始状态同步粒子
        UpdateParticles();
    }

    private void Update()
    {
        if (!isOn)
        {
            UpdateParticles();
            return;
        }

        if (mode == FanMode.Continuous)
        {
            UpdateParticles();
            return;
        }

        // 脉冲计时
        timer += Time.deltaTime;
        if (isBlowing && timer >= pulseDuration)
        {
            isBlowing = false;
            timer = 0f;
            UpdateParticles();
        }
        else if (!isBlowing && timer >= pulseInterval)
        {
            isBlowing = true;
            timer = 0f;
            UpdateParticles();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isOn) return;
        if (mode == FanMode.Pulse && !isBlowing) return;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null)
        {
            rb.AddForce(blowDirection.normalized * blowForce);
        }
    }

    /// <summary>
    /// 外部调用开关风扇
    /// </summary>
    public void SetFanOn(bool on)
    {
        isOn = on;
        UpdateParticles();
    }

    private void UpdateParticles()
    {
        if (windParticles == null) return;

        if (isOn && (mode == FanMode.Continuous || isBlowing))
            windParticles.Play();
        else
            windParticles.Stop();

        // 同步 Animator
        if (fanAnimator != null)
            fanAnimator.SetBool("isBlowing", isOn && (mode == FanMode.Continuous || isBlowing));
    }
}
