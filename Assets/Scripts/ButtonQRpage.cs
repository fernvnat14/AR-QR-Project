using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonQRpage : MonoBehaviour
{
    //user information
    public TMPro.TextMeshProUGUI userName;
    public TMPro.TextMeshProUGUI userStatus;
    public TMPro.TextMeshProUGUI userFacebook;
    public TMPro.TextMeshProUGUI userIg;
    public TMPro.TextMeshProUGUI userTwitter;

    //input field
    public InputField caption;

    //animation
    public Animator user_Animator;
    public Animator edit_Animator;
    public Animator editButton_Animator;

    //Image temp
    Texture2D texture;
    public RawImage Rimage;

    //api
    readonly string getURL = "https://unityproject-270307.uc.r.appspot.com/api/v1/viewall/";
    readonly string getQRURL = "http://api.qrserver.com/v1/create-qr-code";
    readonly string postURL = "https://unityproject-270307.uc.r.appspot.com/api/v1/editCaption/";

    //json temp
    public class UserInfo
    {
        public string Firstname;
        public string Lastname;
        public string Username;
        public string Password;
        public string Twitter;
        public string FB;
        public string IG;
        public string QR_ID;
        public string caption;
    }

    public class Edit
    {
        public string UserQuote;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StatusEdit()
    {
        edit_Animator.SetBool("EditButton", true);
        edit_Animator.SetBool("Finishededit", false);
    }

    public void MyProduct()
    {
        SceneManager.LoadScene("MYPRODUCT-PAGE");
    }

    public void PostEdit()
    {
        StartCoroutine(EditStatus());
    }

    public void InfoButton()
    {
        //StartCoroutine(QRcodeGenerator());
        user_Animator.SetBool("CancelButton", false);
        user_Animator.SetBool("InformationButton", true);
    }

    public void CancelInfoButton()
    {
        user_Animator.SetBool("InformationButton", false);
        user_Animator.SetBool("CancelButton", true);
        editButton_Animator.SetBool("Cancel", true);
        editButton_Animator.SetBool("EditButtonPress", false);
    }

    IEnumerator EditStatus()
    {
        Edit edit = new Edit();
        edit.UserQuote = caption.text;

        string json = JsonUtility.ToJson(edit);

        var request = new UnityWebRequest(postURL + LoginPage.usernameStatic, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        Debug.Log("Status Code: " + request.responseCode);

        if (request.responseCode == 201)
        {
            UserInfo myObject = new UserInfo();
            UnityWebRequest www = UnityWebRequest.Get(getURL + "username/" + LoginPage.usernameStatic);

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                myObject = JsonUtility.FromJson<UserInfo>(www.downloadHandler.text);
                //user info
                userName.text = myObject.Firstname + '\n' + myObject.Lastname;
                userStatus.text = myObject.caption;
                userFacebook.text = myObject.FB;
                userIg.text = myObject.IG;
                userTwitter.text = myObject.Twitter;

                //user_animation
                edit_Animator.SetBool("EditButton", false);
                edit_Animator.SetBool("Finishededit", true);
            }

        }
    }

    IEnumerator QRcodeGenerator()
    {
        var getURL_QRcode = getQRURL + "/?data=" + LoginPage.usernameStatic + "&size=300x300";

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(getURL_QRcode);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            Rimage.texture = texture;
            StartCoroutine(getUserData());
        }
    }

    IEnumerator getUserData()
    {
        UserInfo myObject = new UserInfo(); 
        UnityWebRequest www = UnityWebRequest.Get(getURL + "username/" + LoginPage.usernameStatic);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            myObject = JsonUtility.FromJson<UserInfo>(www.downloadHandler.text);
            //user info
            userName.text = myObject.Firstname + '\n' + myObject.Lastname;
            userStatus.text = myObject.caption;
            userFacebook.text = myObject.FB;
            userIg.text = myObject.IG;
            userTwitter.text = myObject.Twitter;

            //user_animation
            user_Animator.SetBool("CancelButton", false);
            user_Animator.SetBool("InformationButton", true);
            editButton_Animator.SetBool("EditButtonPress", true);
            editButton_Animator.SetBool("Cancel", false);
        }
    }
}
