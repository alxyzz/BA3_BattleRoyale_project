using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Cmn_PopupHint : UI_Widget
{
    private TextMeshProUGUI _tmpContent;
    private Image _imgBackground;

    private readonly float _existingTime = 2.0f;
    protected override void Awake()
    {
        base.Awake();
        _imgBackground = GetComponent<Image>();
        _tmpContent = GetComponentInChildren<TextMeshProUGUI>();

        _imgBackground.fillAmount = 0;
        RenderOpacity = 0;
    }

    public void Appear(string content)
    {
        _tmpContent.SetText(content);
        StartCoroutine(UpdateAppear(0.4f));
    }
    private IEnumerator UpdateAppear(float duration)
    {
        Fade(1, duration, Disappear);
        float time = 0;
        while (time < duration)
        {
            time = Mathf.Min(duration, time + Time.unscaledDeltaTime);
            _imgBackground.fillAmount = time / duration;
            yield return null;
        }
    }
    private void Disappear()
    {
        StartCoroutine(UpdateDisapper(0.2f));
    }
    private IEnumerator UpdateDisapper(float duration)
    {
        yield return new WaitForSecondsRealtime(_existingTime);
        Fade(0, duration, () => { Destroy(gameObject); });
    }
}
