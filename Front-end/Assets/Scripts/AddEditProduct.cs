using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor;

public class AddEditProduct : MonoBehaviour
{

    //API
    readonly string postURL = "https://unityproject-270307.uc.r.appspot.com/api/v1/product";
    readonly string getURL = "http://api.qrserver.com/v1/create-qr-code";
    readonly string generatingUUID_URL = "https://unityproject-270307.uc.r.appspot.com/api/v1/generating-uuid";

    //variable
    private string uuid;
    private string admin_username = "admin";
    private string admin_password = "12345";

    //Image temp
    Texture2D texture;

    //String temp
    public string QRcodeID;

    //Input field
    public TMPro.TMP_InputField product_name;
    public TMPro.TMP_InputField calories;
    public TMPro.TMP_InputField protein;
    public TMPro.TMP_InputField fat;
    public TMPro.TMP_InputField carbs;
    public TMPro.TMP_InputField sugars;
    public TMPro.TMP_InputField sodium;
    public TMPro.TMP_InputField cholesterol;

    //func
    public RefreshToken refreshToken;

    //GameObject
    public GameObject addNewProductWindows;

    //iTween
    public iTween.EaseType easeType;
    public iTween.LoopType loopType;

    public class UUID
    {
        public string uuid;
    }

    public class Product
    {
        public string username = LoginPage.usernameStatic;
        public string name = LoginPage.nameStatic;
        public string product_name;
        public string calories;
        public string protein;
        public string fat;
        public string carbs;
        public string sugars;
        public string sodium;
        public string cholesterol;
        public string product_id;
        public string uuid;
    }

    public void AddButton()
    {
        StartCoroutine(GeneratingUUID());
    }

    IEnumerator GeneratingUUID()
    {
        UUID uuidObject = new UUID();
        UnityWebRequest www = UnityWebRequest.Get(generatingUUID_URL);
        www.SetRequestHeader("AUTHORIZATION", LoginPage.adminAuthStatic);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
            refreshToken.ToCallRefreshToken(LoginPage.adminAuthRefreshStatic, "admin");
            StartCoroutine(GeneratingUUID());
        }
        else
        {
            uuidObject = JsonUtility.FromJson<UUID>(www.downloadHandler.text);
            uuid = uuidObject.uuid;
            StartCoroutine(QRcodeGenerator());
        }
    }

    IEnumerator QRcodeGenerator()
    {
        var getURL_QRcode = getURL + "/?data=" + uuid + "&size=300x300";

        UnityWebRequest www = UnityWebRequestTexture.GetTexture(getURL_QRcode);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
        }
        else
        {
            texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
            StartCoroutine(PostNewTarget());
        }
    }

    IEnumerator AddProduct()
    {
        Product product = new Product();
        product.product_name = product_name.text;
        product.calories = calories.text;
        product.protein = protein.text;
        product.fat = fat.text;
        product.carbs = carbs.text;
        product.sugars = sugars.text;
        product.sodium = sodium.text;
        product.cholesterol = cholesterol.text;
        product.product_id = QRcodeID;
        product.uuid = uuid;

        string json = JsonUtility.ToJson(product);

        var request = new UnityWebRequest(postURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", LoginPage.adminAuthStatic);
        yield return request.SendWebRequest();
        Debug.Log("Status Code: " + request.responseCode);

        if (request.responseCode == 201)
        {
            iTween.MoveTo(addNewProductWindows, iTween.Hash("y", -(Screen.height / 1.5), "time", 1f, "easetype", easeType, "Looptype", loopType));
            yield return new WaitForSeconds(1.0f);
            SceneManager.LoadScene("MYPRODUCT-PAGE");
        }
        else if (request.responseCode == 401)
        {
            refreshToken.ToCallRefreshToken(LoginPage.authRefreshStatic, "user");
            StartCoroutine(AddProduct());
        }
        else
        {
            var targetID = QRcodeID;
            SceneManager.GetActiveScene();
        }
    }

    public void CancelButton()
    {
        SceneManager.LoadScene("LOGIN-PAGE");
    }

    // Vuforia part

    public class PostNewTrackableRequest
    {
        public string name;
        public float width;
        public string image;
        public string application_metadata;
    }

    public class QRcode
    {
        public string transaction_id;
        public string result_code;
        public string target_id;
    }

    private string access_key = "d21d7aee3c5a84650b8f159ecf2b7543a2a32176";
    private string secret_key = "efced3b8868db29b876fa8993682eab979b24c82";
    private string url = @"https://vws.vuforia.com";//@"<a href="https://vws.vuforia.com";//">https://vws.vuforia.com";</a>

    private byte[] requestBytesArray;

    IEnumerator PostNewTarget()
    {
        QRcode myObject = new QRcode();
        string requestPath = "/targets";
        string serviceURI = url + requestPath;
        string httpAction = "POST";
        string contentType = "application/json";
        string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime());

        // if your texture2d has RGb24 type, don't need to redraw new texture2d
        Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
        tex.SetPixels(texture.GetPixels());
        tex.Apply();
        byte[] image = tex.EncodeToPNG();

        string metadataStr = uuid;//May use for key,name...in game
        byte[] metadata = System.Text.ASCIIEncoding.ASCII.GetBytes(metadataStr);
        PostNewTrackableRequest model = new PostNewTrackableRequest();
        model.name = uuid;
        model.width = 64.0f; // don't need same as width of texture
        model.image = System.Convert.ToBase64String(image);

        model.application_metadata = System.Convert.ToBase64String(metadata);
        //string requestBody = JsonWriter.Serialize(model);
        string requestBody = JsonUtility.ToJson(model);

        WWWForm form = new WWWForm();

        var headers = form.headers;
        byte[] rawData = form.data;
        headers["host"] = url;
        headers["date"] = date;
        headers["Content-Type"] = contentType;

        HttpWebRequest httpWReq = (HttpWebRequest)HttpWebRequest.Create(serviceURI);

        MD5 md5 = MD5.Create();
        var contentMD5bytes = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(requestBody));
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        for (int i = 0; i < contentMD5bytes.Length; i++)
        {
            sb.Append(contentMD5bytes[i].ToString("x2"));
        }

        string contentMD5 = sb.ToString();

        string stringToSign = string.Format("{0}\n{1}\n{2}\n{3}\n{4}", httpAction, contentMD5, contentType, date, requestPath);

        HMACSHA1 sha1 = new HMACSHA1(System.Text.Encoding.ASCII.GetBytes(secret_key));
        byte[] sha1Bytes = System.Text.Encoding.ASCII.GetBytes(stringToSign);
        MemoryStream stream = new MemoryStream(sha1Bytes);
        byte[] sha1Hash = sha1.ComputeHash(stream);
        string signature = System.Convert.ToBase64String(sha1Hash);

        headers["Authorization"] = string.Format("VWS {0}:{1}", access_key, signature);

        Debug.Log("<color=green>Signature: " + signature + "</color>");

        WWW request = new WWW(serviceURI, System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(model)), headers);
        yield return request;

        if (request.error != null)
        {
            Debug.Log("request error: " + request.error);
        }
        else
        {
            myObject = JsonUtility.FromJson<QRcode>(request.text);
            QRcodeID = myObject.target_id;
            StartCoroutine(AddProduct());
        }
    }
}
