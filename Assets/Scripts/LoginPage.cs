using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginPage : MonoBehaviour
{
    [TextArea]
    public string jsonData;
    ArrayList TempleArray;
    JSONNode jsonNode;

    //text
    public TMPro.TextMeshProUGUI errorMessage;

    //Input field
    public TMPro.TMP_InputField usernameInput;
    public TMPro.TMP_InputField passwordInput;

    //input line
    public GameObject errorObject;

    //static
    public static string usernameStatic;
    public static string nameStatic;
    public static string authStatic;
    public static string authRefreshStatic;
    public static string adminAuthStatic;
    public static string adminAuthRefreshStatic;

    //API
    readonly string loginURL = "https://unityproject-270307.uc.r.appspot.com/api/v1/login";

    //private
    private string admin_username = "admin";
    private string admin_password = "12123";

    private static int screenSize;

    public class User
    {
        public string Username;
        public string Password;
    }

    // Start is called before the first frame update
    void Start()
    {
        errorObject.SetActive(false);
        StartCoroutine(LoginAdmin());
        screenSize = Screen.height;
    }

    public void BackButton()
    {
        errorObject.SetActive(false);
    }

    [System.Obsolete]
    IEnumerator LoginAdmin()
    {
        string authorization = authenticate(admin_username, admin_password);

        UnityWebRequest www = UnityWebRequest.Get(loginURL);
        www.SetRequestHeader("AUTHORIZATION", authorization);
        yield return www.Send();
        jsonData = www.downloadHandler.text;
        jsonNode = SimpleJSON.JSON.Parse(jsonData);

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log("Unauthorized admin");
            StartCoroutine(LoginAdmin());
        }
        else
        {
            if (www.responseCode == 200)
            {
                adminAuthStatic = "Bearer " + jsonNode["access_token"];
                adminAuthRefreshStatic = "Bearer " + jsonNode["refresh_token"];
            }
            else if (www.responseCode == 401)
            {
                Debug.Log("Unauthorized admin");
            }
            else
            {
                SceneManager.GetActiveScene();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoginButton()
    {
        StartCoroutine(Login());
    }

    string authenticate(string username, string password)
    {
        string auth = username + ":" + password;
        auth = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
        auth = "Basic " + auth;
        return auth;
    }

    [System.Obsolete]
    IEnumerator Login()
    {
        string authorization = authenticate(usernameInput.text, passwordInput.text);

        UnityWebRequest www = UnityWebRequest.Get(loginURL);
        www.SetRequestHeader("AUTHORIZATION", authorization);
        yield return www.Send();
        jsonData = www.downloadHandler.text;
        jsonNode = SimpleJSON.JSON.Parse(jsonData);

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log("Unauthorized");
            errorMessage.text = "Incorrect Username or Password";
            errorObject.SetActive(true);
        }
        else
        {
            if (www.responseCode == 200)
            {
                nameStatic = jsonNode["name"];
                usernameStatic = jsonNode["username"];
                authStatic = "Bearer " + jsonNode["access_token"];
                authRefreshStatic = "Bearer " + jsonNode["refresh_token"];
                SceneManager.LoadScene("QR-AR-PROJECT");
            }
            else if (www.responseCode == 401)
            {
                Debug.Log("Unauthorized");
                SceneManager.GetActiveScene();
            }
            else
            {
                SceneManager.GetActiveScene();
            }
        }
    }

    public void Register()
    {
        SceneManager.LoadScene("REGISTER-PAGE");
    }

    public void Skip()
    {
        usernameStatic = "Annonymous";
        nameStatic = "Annonymous";
        SceneManager.LoadScene("QR-AR-PROJECT");
    }
}
