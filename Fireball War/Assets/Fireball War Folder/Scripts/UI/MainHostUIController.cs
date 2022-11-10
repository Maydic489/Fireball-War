using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainHostUIController : MonoBehaviour
{
    public TextMeshProUGUI hostRoomCode;
    public TMP_InputField roomCodeInputField;

    public static MainHostUIController Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

}
