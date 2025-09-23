using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.HasInstance)
            AudioManager.Instance.PlayButtonHover();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (AudioManager.HasInstance)
            AudioManager.Instance.PlayButtonClick();
    }
}
