using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RefreshToken : MonoBehaviour
{
    [TextArea]
    public string jsonData;
    ArrayList TempleArray;
    JSONNode jsonNode;

    [System.Obsolete]
    public void ToCallRefreshToken(string refreshToken, string type)
    {
        StartCoroutine(ToRefreshToken(refreshToken, type));
    }

    [System.Obsolete]
    IEnumerator ToRefreshToken(string refreshToken, string type)
    {
        var refreshUrl = "https://unityproject-270307.uc.r.appspot.com/api/v1/refresh";

        UnityWebRequest www = UnityWebRequest.Post(refreshUrl, "");
        www.SetRequestHeader("AUTHORIZATION", refreshToken);
        yield return www.Send();
        jsonData = www.downloadHandler.text;
        jsonNode = SimpleJSON.JSON.Parse(jsonData);

        if (www.isNetworkError || www.isHttpError)
        {
            SceneManager.GetActiveScene();
        }
        else
        {
            if (www.responseCode == 200)
            {
                if (type == "user")

                {
                    LoginPage.authStatic = "Bearer " + jsonNode["access_token"];
                }

                else if (type == "admin")
                {
                    LoginPage.adminAuthStatic = "Bearer " + jsonNode["access_token"];
                }
            }
            else if (www.responseCode == 401)
            {
                Debug.Log("Unauthorized");
            }
            else
            {
                Debug.Log("Invalid refresh token");
            }
        }
    }
}
