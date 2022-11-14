using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MasterUIManager : MonoBehaviour
{
    private static MasterUIManager instance;
    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(gameObject);
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    [Header("Popup Hint")]
    [SerializeField] private RectTransform _popupHintList;
    [SerializeField] private GameObject _pfbPopupHint;

    public static void AddPopupHint(string content)
    {
        UI_Cmn_PopupHint popup = Instantiate(instance._pfbPopupHint, instance._popupHintList).GetComponent<UI_Cmn_PopupHint>();
        popup.Appear(content);
    }
}
