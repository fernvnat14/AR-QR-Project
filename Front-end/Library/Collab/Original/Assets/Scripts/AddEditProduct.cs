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

public class AddEditProduct : MonoBehaviour
{
    //API
    readonly string postURL = "http://35.239.78.55:8080/api/v1/nutritions";
    readonly string getURL = "http://api.qrserver.com/v1/create-qr-code";

    //Image temp
    Texture2D texture;

    //String temp
    public string QRcodeID;

    //Input field
    public InputField product_name;
    public InputField calories;
    public InputField protein;
    public InputField fat;
    public InputField carbs;
    public InputField sugars;
    public InputField sodium;
    public InputField cholesterol;

    private Boolean gotQRcode = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public class Product
    {
        /*public string username = LoginPage.usernameStatic;
        public string name = LoginPage.nameStatic;*/
        public string username = "pigboss1";
        public string name = "Jirayu Promsongwong";
        public string product_name;
        public string calories;
        public string protein;
        public string fat;
        public string carbs;
        public string sugars;
        public string sodium;
        public string cholesterol;
        public string product_id;
    }

    public void AddButton()
    {
        StartCoroutine(QRcodeGenerator());
    }

    IEnumerator QRcodeGenerator()
    {
        var content = "";

        if (gotQRcode)
        {
            content = QRcodeID;

            var getURL_QRcode = getURL + "/?data=" + content + "&size=300x300";

            Debug.Log(getURL_QRcode);

            UnityWebRequest www = UnityWebRequestTexture.GetTexture(getURL_QRcode);

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                Debug.Log(www.downloadHandler.text);
                StartCoroutine(UpdateTarget());
            }
        }

        else
        {
            content = "temp";
            var getURL_QRcode = getURL + "/?data=" + content + "&size=300x300";

            Debug.Log(getURL_QRcode);

            UnityWebRequest www = UnityWebRequestTexture.GetTexture(getURL_QRcode);

            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
            }
            else
            {
                texture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                Debug.Log(www.downloadHandler.text);
                StartCoroutine(PostNewTarget());
            }
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

        string json = JsonUtility.ToJson(product);

        var request = new UnityWebRequest(postURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        Debug.Log("Status Code: " + request.responseCode);

        if (request.responseCode == 201)
        {
            Debug.Log("Product added.");
        }
        else
        {
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

    private string access_key = "5891c4514fb9d691dd3aaff6581532967cb06487";
    private string secret_key = "376be379bdfafc6c15742737e10a1afc95d81af5";
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

        Debug.Log(date);
        Debug.Log(serviceURI);

        // if your texture2d has RGb24 type, don't need to redraw new texture2d
        Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
        tex.SetPixels(texture.GetPixels());
        tex.Apply();
        byte[] image = tex.EncodeToPNG();

        string metadataStr = product_name.text;//May use for key,name...in game
        byte[] metadata = System.Text.ASCIIEncoding.ASCII.GetBytes(metadataStr);
        PostNewTrackableRequest model = new PostNewTrackableRequest();
        model.name = product_name.text;
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
            Debug.Log("request temp success");
            Debug.Log("returned data" + request.text);
            myObject = JsonUtility.FromJson<QRcode>(request.text);
            QRcodeID = myObject.target_id;
            gotQRcode = true;
            Debug.Log(QRcodeID);
            StartCoroutine(QRcodeGenerator());
        }
    }

    IEnumerator UpdateTarget()
    {
        yield return new WaitForSeconds(40);
        bool success = true;
        while (success)
        {
            QRcode myObject = new QRcode();
            string requestPath = "/targets/" + QRcodeID;
            string serviceURI = url + requestPath;
            string httpAction = "PUT";
            string contentType = "application/json";
            string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime());

            Debug.Log(date);
            Debug.Log(serviceURI);

            // if your texture2d has RGb24 type, don't need to redraw new texture2d
            Texture2D tex = new Texture2D(texture.width, texture.height, TextureFormat.RGB24, false);
            tex.SetPixels(texture.GetPixels());
            tex.Apply();
            byte[] image = tex.EncodeToPNG();

            string metadataStr = product_name.text;//May use for key,name...in game
            byte[] metadata = System.Text.ASCIIEncoding.ASCII.GetBytes(metadataStr);
            PostNewTrackableRequest model = new PostNewTrackableRequest();
            model.name = product_name.text;
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
            httpWReq.Method = "PUT";

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
            Debug.Log("<color=green>VWS: " + string.Format("VWS {0}:{1}", access_key, signature) + "</color>");

            WWW request = new WWW(serviceURI, System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(model)), headers);
            
            yield return request;

            Debug.Log(request);

            if (request.error != null)
            {
                Debug.Log("request error: " + request.error);
            }
            else
            {
                Debug.Log("request temp success");
                Debug.Log("returned data" + request.text);
                myObject = JsonUtility.FromJson<QRcode>(request.text);
                QRcodeID = myObject.target_id;
                gotQRcode = true;
                Debug.Log(QRcodeID);
                StartCoroutine(AddProduct());
                success = false;
            }
        }
    }
}
