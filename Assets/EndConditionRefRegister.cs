using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndConditionRefRegister : MonoBehaviour
{
    
    [SerializeField] private GameObject _VictoryPopup;
    [SerializeField] private TextMeshPro _VictoryPopupUsername;
    [SerializeField] private TextMeshPro _VictoryPopupBlurb;
    [SerializeField] private GameObject _TiePopup;

    private void Start()
    {
        UI_GameHUD.RegisterRefs(_VictoryPopup, _VictoryPopupUsername, _VictoryPopupBlurb, _TiePopup);
    }
}
