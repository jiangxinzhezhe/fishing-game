using UnityEngine;
using System.Collections;

public class BobberFloat : MonoBehaviour
{
    [Header("浮漂上下浮动")]
    public float floatSpeed = 2f;
    public float floatHeight = 0.1f;
    private Vector3 startPos;
    private bool canFloat = false;

    [Header("蓄力 UI 控制")]
    public GameObject chargeBarGroup;  // ChargeBarGroup 父物体
    public GameObject fillBarGO;       // FillBar 条（Image）

    void Start()
    {
        startPos = transform.position;

        // 一开始隐藏 UI 和禁止浮动
        chargeBarGroup.SetActive(false);
        if (fillBarGO != null)
            fillBarGO.SetActive(false);

        canFloat = false;

        StartCoroutine(StartSequence());
    }

    IEnumerator StartSequence()
    {
        yield return new WaitForSeconds(2f);       // 等 2 秒
        canFloat = true;                           // 开始浮动

        yield return new WaitForSeconds(0.5f);     // 再等 0.5 秒
        if (chargeBarGroup != null)
            chargeBarGroup.SetActive(true);        // 显示 UI
        if (fillBarGO != null)
            fillBarGO.SetActive(true);
    }

    public void StopFloating()
    {
        canFloat = false;
    }

    public void RestartFloating()
    {
        canFloat = true;
    }

    void Update()
    {
        if (canFloat)
        {
            float offset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            transform.position = startPos + new Vector3(0, offset, 0);
        }
    }
}
