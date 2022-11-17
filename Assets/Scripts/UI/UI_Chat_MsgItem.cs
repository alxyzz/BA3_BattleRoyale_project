using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UI_Chat_MsgItem : UI_Widget
{
    [SerializeField] private float _remainingTime = 7f;
    private TextMeshProUGUI _tmpContent;
    protected override void Awake()
    {
        base.Awake();
        _tmpContent = GetComponent<TextMeshProUGUI>();
    }
    public IEnumerator SetChatContent(string nickname, string content, bool isSelf)
    {
        if (isSelf)
            _tmpContent.SetText($"<#60FF00>{nickname}</color> : {content}");
        else
            _tmpContent.SetText($"<#00F0FF>{nickname}</color> : {content}");

        yield return new WaitForSecondsRealtime(_remainingTime);
        Fade(0, 0.5f, () => Destroy(gameObject));
    }
}
