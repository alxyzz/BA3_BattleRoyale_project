using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Game_KillMsg : MonoBehaviour
{
    [Header("Default Settings")]
    [SerializeField] private GameObject _pfbKillMsgItem;
    [SerializeField] private Sprite _fallKillIcon;
    [SerializeField] private Sprite _poisonKillIcon;

    public void AddKillMessage(string killerName, string objectName, Sprite icon, DamageType type)
    {
        UI_Game_KillMsgItem item = Instantiate(_pfbKillMsgItem, transform).GetComponent<UI_Game_KillMsgItem>();
        switch (type)
        {
            case DamageType.DEFAULT:
                item.StartCoroutine(item.SetKillContent(killerName, objectName, icon));
                break;
            case DamageType.SHOOT:
                item.StartCoroutine(item.SetKillContent(killerName, objectName, icon));
                break;
            case DamageType.EXPLOSION:
                item.StartCoroutine(item.SetKillContent(killerName, objectName, icon));
                break;
            case DamageType.FALL:
                item.StartCoroutine(item.SetKillContent(killerName, objectName, _fallKillIcon));
                break;
            case DamageType.POISON:
                item.StartCoroutine(item.SetKillContent(killerName, objectName, _poisonKillIcon));
                break;
            default:
                break;
        }
    }
}
