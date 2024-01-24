using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RFUniverse
{
    public interface ICustomGripper
    {
        void Open();
        void Close();

        void OpenDirectly();
        void CloseDirectly();
    }
}