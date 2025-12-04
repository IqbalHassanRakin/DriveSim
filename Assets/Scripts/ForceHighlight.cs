using UnityEngine;
using UnityEngine.UI;

public class ForceHighlight : MonoBehaviour {

    public Button firstSelect;

    public void OnMouseClick()
    {
        firstSelect.Select();
    }

}
