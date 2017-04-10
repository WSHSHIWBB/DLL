using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestWWW : MonoBehaviour
{
    string dllurl = "http://112.74.34.144:3001/hidepet/getRoomStatus?id=50";

    private IEnumerator Get(WWW www)
    {
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            print("Error downloading: " + www.error);
        }
        else
        {
            Debug.Log(www.text);
        }
    }

    private IEnumerator Post(string url)
    {
        Debug.Log(111);
        WWWForm wwwForm = new WWWForm();
        wwwForm.AddField("id", 50);
        wwwForm.AddField("petName", "金宠");
        WWW www = new WWW(url, wwwForm);
        yield return www;
        if (!string.IsNullOrEmpty(www.error))
        {
            print("Error downloading: " + www.error);
        }
        else
        {
            Debug.Log(www.text);
        }
    }

    IEnumerator Head(string url)
    {
        WWW www = new WWW(url);
        yield return www;

        if (www.responseHeaders.Count > 0)
        {
            foreach (KeyValuePair<string, string> entry in www.responseHeaders)
            {
                Debug.Log(entry.Value + "=" + entry.Key);
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine(Head(dllurl));
        }
    }
}
