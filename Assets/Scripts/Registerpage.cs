using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Net;
using System;

public class Registerpage : MonoBehaviour
{

    //API
    readonly string registerURL = "https://unityproject-270307.uc.r.appspot.com/api/v1/register";
    readonly string getURL = "http://api.qrserver.com/v1/create-qr-code";

    //private
    private string admin_username = "admin";
    private string admin_password = "12345";

    //Image temp
    Texture2D texture;

    //String temp
    public string QRcodeID;
    public string input_type;

    //toggles
    public Toggle toggle_personal;
    public Toggle toggle_bussiness;

    //Input field
    public TMPro.TMP_InputField input_name;
    public TMPro.TMP_InputField input_username;
    public TMPro.TMP_InputField input_password;
    public TMPro.TMP_InputField input_email;

    //func
    public RefreshToken refreshToken;

    public TMPro.TextMeshProUGUI errorMessage;

    public GameObject errorObject;

    private void Start()
    {
        errorObject.SetActive(false);
    }

    public void OnChangePersonal()
    {
        toggle_bussiness.isOn = false;
        input_type = "Personal";
    }

    public void OnChangeBussiness()
    {
        toggle_personal.isOn = false;
        input_type = "Bussiness";
        
    }

    public class User
    {
        public string name;
        public string username;
        public string password;
        public string email;
        public string type;
    }

    public void RegisterButton()
    {
        StartCoroutine(Register());
    }

    IEnumerator Register()
    {
        User user = new User();
        user.name = input_name.text;
        user.username = input_username.text;
        user.password = input_password.text;
        user.email = input_email.text;
        user.type = input_type;

        string json = JsonUtility.ToJson(user);

        var request = new UnityWebRequest(registerURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", LoginPage.adminAuthStatic);
        yield return request.SendWebRequest();
        Debug.Log("Status Code: " + request.responseCode);

        if (request.responseCode == 201)
        {
            SceneManager.LoadScene("LOGIN-PAGE");
        }
        else if (request.responseCode == 401)
        {
            refreshToken.ToCallRefreshToken(LoginPage.authRefreshStatic, "admin");
            StartCoroutine(Register());
        }
        else
        {
            errorObject.SetActive(true);
            errorMessage.text = "Username or email is already taken";
            SceneManager.GetActiveScene();
        }
    }

    public void CancelButton()
    {
        SceneManager.LoadScene("LOGIN-PAGE");
    }
}
