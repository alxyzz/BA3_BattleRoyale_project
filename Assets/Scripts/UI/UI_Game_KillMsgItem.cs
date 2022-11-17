using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Game_KillMsgItem : UI_Widget
{
    [Header("Components")]
    [SerializeField] private TextMeshProUGUI _tmpKillerName;
    [SerializeField] private Image _imgKillIcon;
    [SerializeField] private TextMeshProUGUI _tmpObjectName;

    [Header("Default Settinigs")]
    [SerializeField] private float _remainingTime = 6.0f;

    public IEnumerator SetKillContent(string killerName, string objectName, Sprite icon)
    {
        _tmpKillerName.SetText(killerName);
        _tmpObjectName.SetText(objectName);
        _imgKillIcon.sprite = icon;

        yield return new WaitForSecondsRealtime(_remainingTime);
        Fade(0, 0.5f, () => Destroy(gameObject));
    }
}
