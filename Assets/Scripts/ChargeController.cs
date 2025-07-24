using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ChargeBarRect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    
    private FishData currentFish;
    public GameObject fillBarGO;           // fillBar 的 GameObject，用于控制显示/隐藏
    public BobberFloat bobberFloatScript;  // 引用控制浮漂的脚本（如需暂停）
    private float currentPower;
    private float currentDistance;


    [Header("鱼列表")]
    public FishData[] allFish;

    [Header("鱼竿形态变化")]
    public SpriteRenderer rodSpriteRenderer;
    public Sprite rodLv1;
    public Sprite rodLv2;
    public Sprite rodLv3;

    [Header("鱼展示")]
    public GameObject fishPanel; // 控制整个鱼展示面板
    public Image fishImage;   // 上钩的鱼
    public Text fishNameText;
    public Text fishStarText;
    public Text fishDescText;


    [Header("蓄力条")]
    public RectTransform fillBar;
    public float maxWidth = 150f;
    public float chargeSpeed = 5f;

    [Header("判定区域 UI")]
    public RectTransform validZone;
    public RectTransform perfectZone;

    [Header("判定结果 UI")]
    public GameObject resultPanel;
    public Text resultText;

    [Header("UI 控制组")]
    public GameObject ChargeBarGroup;

    [Header("判定区间")]
    private bool isCharging = false;
    private float currentWidth = 0f;
    private bool isIncreasing = true;

    [Header("准度条")]
    public RectTransform distanceFill;
    public RectTransform powerFill;


    void Start()
    {
        RandomizeFish();
    }

    void Update()
    {
        if (isCharging)
        {
            if (isIncreasing)
            {
                currentWidth += Time.deltaTime * chargeSpeed;
                if (currentWidth >= maxWidth)
                {
                    currentWidth = maxWidth;
                    isIncreasing = false;
                }
            }
            else
            {
                currentWidth -= Time.deltaTime * chargeSpeed;
                if (currentWidth <= 0f)
                {
                    currentWidth = 0f;
                    isIncreasing = true;
                }
            }

            fillBar.sizeDelta = new Vector2(currentWidth, fillBar.sizeDelta.y);
            UpdateRodVisual();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!ChargeBarGroup.activeSelf) return;

        isCharging = true;
        currentWidth = 0f;
        fillBar.sizeDelta = new Vector2(0f, fillBar.sizeDelta.y);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!ChargeBarGroup.activeSelf) return;

        isCharging = false;
        StartCoroutine(ShowJudgeThenResult());

    }

    IEnumerator ShowJudgeThenResult()
    {
        ShowJudgeZones();
        ShowResult();
        yield return new WaitForSeconds(2f);


        // 清空 fillBar 显示
        fillBar.sizeDelta = new Vector2(0f, fillBar.sizeDelta.y);
        fillBarGO.SetActive(false);

        // 隐藏整个 UI
        ChargeBarGroup.SetActive(false);

        // 停止浮漂
        if (bobberFloatScript != null)
            bobberFloatScript.StopFloating();

        yield return new WaitForSeconds(2f); // 等待一个循环时间

        // 再次启动浮漂
        if (bobberFloatScript != null)
            bobberFloatScript.RestartFloating();

        // 重置 UI 状态
        fillBarGO.SetActive(true);
        ChargeBarGroup.SetActive(true);
        HideJudgeZones();
        HideResult();
        RandomizeFish();
    }


    void ShowJudgeZones()
    {
        float accurateNum = currentPower + currentDistance;
        float validMin = accurateNum - 10;
        float validMax = accurateNum + 10;
        float perfectMin = accurateNum - 2;
        float perfectMax = accurateNum + 2;
        validZone.gameObject.SetActive(true);
        perfectZone.gameObject.SetActive(true);

        validZone.anchoredPosition = new Vector2(validMin, 0);
        validZone.sizeDelta = new Vector2((validMax) - (validMin), fillBar.sizeDelta.y);

        perfectZone.anchoredPosition = new Vector2(perfectMin, 0);
        perfectZone.sizeDelta = new Vector2(perfectMax - perfectMin, fillBar.sizeDelta.y);
    }

    void HideJudgeZones()
    {
        validZone.gameObject.SetActive(false);
        perfectZone.gameObject.SetActive(false);
    }

    void ShowResult()
    {
        float accurateNum = currentPower + currentDistance;
        float validMin = accurateNum - 10;
        float validMax = accurateNum + 10;
        float perfectMin = accurateNum - 2;
        float perfectMax = accurateNum + 2;
        string result;
        Sprite chosenFish = null;

        if (currentWidth >= perfectMin && currentWidth <= perfectMax)
        {
            result = "完美上钩";
            chosenFish = currentFish.sprite;
        }
        else if (currentWidth >= validMin && currentWidth <= validMax)
        {
            result = "有效上钩";
            chosenFish = currentFish.sprite;
        }
        else
        { result = "鱼跑了"; }

        resultText.text = result;
        // Debug.Log("Result Text: " + resultText.text);
        resultPanel.SetActive(true);
        //Debug.Log("当前上钩鱼: " + currentFish.sprite.name);

        // Debug.Log("ResultPanel activeSelf: " + resultPanel.activeSelf);
        if (chosenFish != null)
        {
            fishPanel.SetActive(true);
            fishImage.sprite = chosenFish;
            fishNameText.text = currentFish.fishName;
            fishDescText.text = currentFish.desc;
            fishStarText.text = new string('*', currentFish.star);
        }
    }

    void HideResult()
    {
        resultPanel.SetActive(false);
        fishPanel.SetActive(false);
    }

    void UpdateRodVisual()
    {
        float progress = currentWidth / maxWidth;

        if (progress < 0.33f)
        {
            rodSpriteRenderer.sprite = rodLv1;
        }
        else if (progress < 0.66f)
        {
            rodSpriteRenderer.sprite = rodLv2;
        }
        else
        {
            rodSpriteRenderer.sprite = rodLv3;
        }
        // Debug.Log("切换鱼竿到：" + rodSpriteRenderer.sprite.name);
    }

    void RandomizeFish()
    {
        int idx = Random.Range(0, allFish.Length);
        currentFish = allFish[idx];

        float total = 140f;
        currentPower = Random.Range(10f, total - 10f);
        currentDistance = Random.Range(10f, total - 10f);
        while (currentPower + currentDistance > total)
        {
            currentDistance = Random.Range(10f, total - 10f);
        }

        float accurateNum = currentPower + currentDistance;

        if (distanceFill != null)
        {
            distanceFill.sizeDelta = new Vector2(currentDistance, distanceFill.sizeDelta.y);
        }
        if (powerFill != null)
        {
            powerFill.sizeDelta = new Vector2(currentPower, powerFill.sizeDelta.y);
        }
    }

}
