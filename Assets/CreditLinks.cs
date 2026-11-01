using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TMP_Text))]
public class CreditLinks : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        TMP_Text text = GetComponent<TMP_Text>();
        int index = TMP_TextUtilities.FindIntersectingLink(text, eventData.position, eventData.pressEventCamera);
        if (index < 0) return;

        string url = text.textInfo.linkInfo[index].GetLinkID();
        if (url == "https://game-icons.net/1x1/lorc/alien-stare.html"
            || url == "https://creativecommons.org/licenses/by/3.0/")
        {
            Application.OpenURL(url);
        }
    }
}
