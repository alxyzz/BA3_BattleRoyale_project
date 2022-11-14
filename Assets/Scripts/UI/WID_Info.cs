using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WID_Info : MonoBehaviour
{
    private Transform Following { get; set; }
    [SerializeField] private TextMeshProUGUI _tmpName;
    [SerializeField] private TextMeshProUGUI _tmpDetails;

    public void Initialise(Transform following, string wpnName, int curAmmo, int backupAmmo, WeaponRangeType type)
    {
        Following = following;
        _tmpName.SetText(wpnName);
        _tmpDetails.SetText($"AMMO: <#FFF200>{curAmmo} / {backupAmmo}</color>\n{type}");
    }

    void Update()
    {
        transform.position = Following.position + Vector3.up * 0.5f;
        // make the widget always face to camera
        transform.forward =Camera.main.transform.forward;
    }
}
