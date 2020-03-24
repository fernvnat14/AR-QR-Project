using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginPage : MonoBehaviour
{
    //Input field
    public InputField usernameInput;
    public InputField passwordInput;

    //input line
    public GameObject usernameLine;
    public GameObject passwordLine;

    //static
    public static string usernameStatic;

    //API
    readonly string postURL = "http://35.239.78.55:8080/api/v1/Login";

    public class User
    {
        public string Username;
        public string Password;
    }

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoginButton()
    {
        StartCoroutine(Login());
    }

    IEnumerator Login()
    {
        var usernameRenderer = usernameLine.GetComponent<Image>();
        var passwordRenderer = passwordInput.GetComponent<Image>();

        User user = new User();
        user.Username = usernameInput.text;
        user.Password = passwordInput.text;
        string json = JsonUtility.ToJson(user);
        Debug.Log(json);

        var request = new UnityWebRequest(postURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        Debug.Log("Status Code: " + request.responseCode);

        if(request.responseCode == 200)
        {
            usernameStatic = usernameInput.text;
            SceneManager.LoadScene("QR-AR-PROJECT");
        }
        else
        {
            SceneManager.GetActiveScene();
        }
    }

    public void Register()
    {
        SceneManager.LoadScene("REGISTER-PAGE");
    }
}
