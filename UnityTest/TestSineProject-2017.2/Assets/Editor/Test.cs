using System.Collections;
using UnityEditor;
using UnityEngine;

public static class Test {

    [MenuItem("Test/Run Sine Tests (Editor will be unresponsive for a few minutes)...")]
    public static void RunSineTests()
    {
        Program p = new Program();
        Debug.Log(p.RunTests());
    }
}
