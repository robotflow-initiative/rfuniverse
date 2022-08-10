using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using System;
using System.Linq;
using TMPro;

public class FileSelectUI : MonoBehaviour
{
    public ListViewUI list;
    public TMP_InputField filed;
    public ButtonManagerBasicWithIcon select;
    public ButtonManagerBasicWithIcon cancel;
    bool selectMode;
    public void Init(Action<string, bool> OnSelect)
    {
        select.clickEvent.AddListener(() =>
        {
            OnSelect(filed.text, selectMode);
            gameObject.SetActive(false);
        }
        );
        cancel.clickEvent.AddListener(() =>
        {
            gameObject.SetActive(false);
        }
        );
    }
    public void Open(bool mode, string path)
    {
        gameObject.SetActive(true);
        selectMode = mode;
        select.buttonText = mode ? "Load" : "Save";
        select.UpdateUI();
        string[] files = System.IO.Directory.GetFiles(path);
        list.RefeshList(files.Where(s => Path.GetExtension(s) != ".meta").Select(s => Path.GetFileName(s)).ToList(), (s) =>
        {
            filed.text = s;
        });
    }
}
