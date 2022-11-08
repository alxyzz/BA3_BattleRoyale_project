using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Panel_Statistics : UI_Widget
{
    [Header("Components")]
    [SerializeField] private RectTransform _list;
    [SerializeField] private Text _txtFps;

    private List<UI_Stat_PlayerSlot> _slots = new List<UI_Stat_PlayerSlot>();
    
    // Start is called before the first frame update
    void Start()
    {
        RenderOpacity = 0.0f;
        StartCoroutine(UpdateFps());
    }

    IEnumerator UpdateFps()
    {
        int frame = 0;
        float delta = 0.0f;
        while (true)
        {
            if (delta > 1)
            {
                _txtFps.text = (frame / delta).ToString("#0.0") + " FPS";
                frame = 0;
                delta = 0.0f;
            }
            else
            {
                frame++;
                delta += Time.deltaTime;
            }
            yield return null;
        }
    }

    public void SetShown(bool shown)
    {
        if (shown)
        { 
            Fade(1, 0.2f);
        }
        else
        {
            Fade(0, 0.1f);
        }
    }

    public void Refresh()
    {
        foreach (var item in _slots)
        {
            Destroy(item.gameObject);
        }
        _slots.Clear();
        foreach (var item in GameState.PlayerStates)
        {
            UI_Stat_PlayerSlot slot = Instantiate(Resources.Load<GameObject>("UI/Statistics/PlayerSlot"), _list).GetComponent<UI_Stat_PlayerSlot>();
            slot.Initialise(item);
            _slots.Add(slot);

        }
    }
}