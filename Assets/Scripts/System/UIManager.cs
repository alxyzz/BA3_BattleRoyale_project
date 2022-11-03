using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        ClearInteractionHint();
    }

    [SerializeField] private Text _interactionHint;

    public static void AddInteractionHint(string content)
    {
        instance._interactionHint.text = content;
    }
    public static void ClearInteractionHint()
    {
        instance._interactionHint.text = "";
    }
}
