using System;
using UnityEngine;
using UnityEngine.UI;

public class Mass : MonoBehaviour
{

    private static Mass Instance;
    public GameObject Temp;

    void Awake()
    {
        Instance = this;
    }

    public static void ShowMassage(Action action, string text)
    {
        GameObject massageBox = Instantiate(Instance.Temp);

        Transform panel = massageBox.transform.Find("Panel");

        Text massageText = panel.Find("Description").GetComponent<Text>();
        massageText.text = text;

        Button button1 = panel.Find("Button 1").GetComponent<Button>();
        Button button2 = panel.Find("Button 2").GetComponent<Button>();

       button1.onClick.AddListener(() =>
        {
            action();
            Destroy(massageBox);
        });

        button2.onClick.AddListener(() =>
        {
            Destroy(massageBox);
        });

    }
}
