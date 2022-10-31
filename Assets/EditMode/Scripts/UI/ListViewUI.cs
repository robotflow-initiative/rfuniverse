using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Michsky.UI.ModernUIPack;
using System;

public class ListViewUI : MonoBehaviour
{
    public ButtonManagerBasicWithIcon button;

    public List<ButtonManagerBasicWithIcon> itemButtons = new List<ButtonManagerBasicWithIcon>();

    public void Awake()
    {
        button.gameObject.SetActive(false);
    }
    public void RefeshList(List<string> names, Action<string> OnSelectItem = null)
    {
        foreach (var item in itemButtons)
        {
            Destroy(item.gameObject);
        }
        itemButtons.Clear();
        foreach (var item in names)
        {
            ButtonManagerBasicWithIcon one = Instantiate(button, button.transform.parent);
            itemButtons.Add(one);
            one.gameObject.SetActive(true);
            one.buttonText = item;
            one.GetComponent<Button>().onClick.AddListener(() =>
            {
                OnSelectItem?.Invoke(item);
            });
            one.UpdateUI();
        }
    }
}
