using UnityEngine;

public class TestInBuild : MonoBehaviour
{

    private Program p;
    private string resultText;

	void Start ()
    {
        p = new Program();
    }

    void OnGUI()
    {
        GUILayout.TextField(resultText);
        if (GUILayout.Button("Run test now..."))
        {
            resultText = p.RunTests();
        }
        if (GUILayout.Button("Run FAST dummy tests now..."))
        {
            resultText = p.RunTestsFast();
        }
	}
}
