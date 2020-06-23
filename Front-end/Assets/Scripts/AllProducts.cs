using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.SceneManagement;
using System.Text;
using System.IO;
using System;
using System.Net;
using Vuforia;
using System.Security.Cryptography;
using UnityEditor;

public class AllProducts : MonoBehaviour
{
    public static string v;
    public GameObject scriptV;

    //
    public string selectedProductID;

    //Image temp
    Texture2D texture;
    public RawImage Rimage;

    [TextArea]
    public string jsonData;
    ArrayList TempleArray;
    JSONNode jsonNode;

    [TextArea]
    public string jsonData_temp;
    JSONNode jsonNode_temp;

    bool inFirst = false;
    bool changed = false;

    //api
    readonly string allProductURL = "https://unityproject-270307.uc.r.appspot.com/api/v1/allproduct";
    readonly string getQRURL = "http://api.qrserver.com/v1/create-qr-code";
    readonly string getProductByIdURL = "https://unityproject-270307.uc.r.appspot.com/api/v1/product";

    public UnityEngine.UI.Image carbs_bar;
    public UnityEngine.UI.Image cholesterol_bar;
    public UnityEngine.UI.Image fat_bar;
    public UnityEngine.UI.Image sodium_bar;
    public UnityEngine.UI.Image protein_bar;
    public UnityEngine.UI.Image sugar_bar;

    //percent
    public TMPro.TextMeshProUGUI per_fat;
    public TMPro.TextMeshProUGUI per_sodium;
    public TMPro.TextMeshProUGUI per_cholesterol;
    public TMPro.TextMeshProUGUI per_carbs;

    //text
    public TMPro.TextMeshProUGUI calories;
    public TMPro.TextMeshProUGUI carbs;
    public TMPro.TextMeshProUGUI cholesterol;
    public TMPro.TextMeshProUGUI fat;
    public TMPro.TextMeshProUGUI product_id;
    public TMPro.TextMeshProUGUI product_name;
    public TMPro.TextMeshProUGUI protein;
    public TMPro.TextMeshProUGUI sodium;
    public TMPro.TextMeshProUGUI sugars;
    public TMPro.TextMeshProUGUI owner;

    //Input field to edit
    public TMPro.TMP_InputField toedit_product_name;
    public TMPro.TMP_InputField toedit_calories;
    public TMPro.TMP_InputField toedit_protein;
    public TMPro.TMP_InputField toedit_fat;
    public TMPro.TMP_InputField toedit_carbs;
    public TMPro.TMP_InputField toedit_sugars;
    public TMPro.TMP_InputField toedit_sodium;
    public TMPro.TMP_InputField toedit_cholesterol;

    //animation
    public Animator productInfo;
    public Animator addProduct;

    int index = 0;

    //gameobject
    public GameObject itemTemplate;
    public GameObject content;
    public GameObject inputName;
    public GameObject inputNutrition;
    public GameObject inputNutritionButton;
    public GameObject inputNameButton;
    public GameObject showProduct;
    public GameObject editProduct;
    public GameObject editProductButton;
    public GameObject confirmDeleteButton;
    public GameObject editButton;
    public GameObject addNewProductWindows;

    //func
    public RefreshToken refreshToken;

    //iTween
    public iTween.EaseType easeType;
    public iTween.LoopType loopType;

    //Server keys are placed here
    private string access_key = "d21d7aee3c5a84650b8f159ecf2b7543a2a32176";
    private string secret_key = "efced3b8868db29b876fa8993682eab979b24c82";
    //Address of Vuforia's server
    private string url = @"https://vws.vuforia.com";

    private void Start()
    {
        fat_bar.fillAmount = 0;
        sodium_bar.fillAmount = 0;
        cholesterol_bar.fillAmount = 0;
        carbs_bar.fillAmount = 0;
        protein_bar.fillAmount = 0;
        sugar_bar.fillAmount = 0;

        inputName.SetActive(false);
        inputNutrition.SetActive(false);
        inputNameButton.SetActive(false);
        inputNutritionButton.SetActive(false);
        //showProduct.SetActive(false);
        editProduct.SetActive(false);

        StartCoroutine(GetNutrition());
        StartCoroutine(DoLast());
    }

    public void AddNewProductButton()
    {
        iTween.MoveTo(addNewProductWindows, iTween.Hash("y", Screen.height / 1.7, "time", 1f, "easetype", easeType, "Looptype", loopType));
    }

    public void CancelAddNewProductButton()
    {
        iTween.MoveTo(addNewProductWindows, iTween.Hash("y", -(Screen.height / 2), "time", 1f, "easetype", easeType, "Looptype", loopType));
    }

    public class ToEditNutrition
    {
        public string calories;
        public string protein;
        public string fat;
        public string carbs;
        public string sugars;
        public string sodium;
        public string cholesterol;
        public string product_id;
        public string update_type = "nutrition labels";
    }

    public class ToEditProductName
    {
        public string product_name;
        public string product_id;
        public string update_type = "product_name";
    }

    public void EditButton()
    {
        editProduct.SetActive(true);
        inputName.SetActive(false);
        inputNutrition.SetActive(false);
        confirmDeleteButton.SetActive(false);
        editButton.SetActive(false);
        editProductButton.SetActive(true);
    }

    public void EditNameButton()
    {
        inputName.SetActive(true);
        inputNameButton.SetActive(true);
        inputNutritionButton.SetActive(false);
        editProductButton.SetActive(false);
    }

    public void EditNutritionButton()
    {
        inputNutrition.SetActive(true);
        inputNameButton.SetActive(false);
        inputNutritionButton.SetActive(true);
        editProductButton.SetActive(false);
    }

    public void CancelConfirmEdit()
    {
        inputName.SetActive(false);
        inputNutrition.SetActive(false);
        inputNameButton.SetActive(false);
        inputNutritionButton.SetActive(false);
        editProductButton.SetActive(true);
    }

    public void CancelEdit()
    {
        inputName.SetActive(false);
        inputNutrition.SetActive(false);
        inputNameButton.SetActive(false);
        inputNutritionButton.SetActive(false);
        editProduct.SetActive(false);
        editButton.SetActive(true);
    }

    public void ConfirmEditNameButton()
    {
        StartCoroutine(ConfirmEditName());
    }

    IEnumerator ConfirmEditName()
    {
        var editUrl = "https://unityproject-270307.uc.r.appspot.com/api/v1/product/" + selectedProductID;
        ToEditProductName data = new ToEditProductName();
        data.product_id = selectedProductID;
        data.product_name = toedit_product_name.text;

        string json = JsonUtility.ToJson(data);

        var request = new UnityWebRequest(editUrl, "PATCH");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", LoginPage.authStatic);
        yield return request.SendWebRequest();
        Debug.Log("Status Code: " + request.responseCode);

        if (request.responseCode == 200)
        {
            product_name.text = toedit_product_name.text;
            inputName.SetActive(false);
            inputNutrition.SetActive(false);
            inputNameButton.SetActive(false);
            inputNutritionButton.SetActive(false);
            editProduct.SetActive(false);
            editButton.SetActive(true);

            changed = true;
        }
        else if (request.responseCode == 401)
        {
            refreshToken.ToCallRefreshToken(LoginPage.authRefreshStatic, "user");
            StartCoroutine(ConfirmEditName());
        }
        else
        {
            SceneManager.GetActiveScene();
        }
    }

    public void ConfirmEditNutritionButton()
    {
        StartCoroutine(ConfirmEditNutrition());
    }

    IEnumerator ConfirmEditNutrition()
    {
        var editUrl = "https://unityproject-270307.uc.r.appspot.com/api/v1/product/" + selectedProductID;
        ToEditNutrition data = new ToEditNutrition();
        data.product_id = selectedProductID;
        data.calories = toedit_calories.text;
        data.carbs = toedit_carbs.text;
        data.cholesterol = toedit_cholesterol.text;
        data.fat = toedit_fat.text;
        data.protein = toedit_protein.text;
        data.sodium = toedit_sodium.text;
        data.sugars = toedit_sugars.text;

        string json = JsonUtility.ToJson(data);

        var request = new UnityWebRequest(editUrl, "PATCH");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", LoginPage.authStatic);
        yield return request.SendWebRequest();
        Debug.Log("Status Code: " + request.responseCode);

        if (request.responseCode == 200)
        {
            calories.text = toedit_calories.text;
            carbs.text = toedit_carbs.text + "g";
            cholesterol.text = toedit_cholesterol.text + "g";
            fat.text = toedit_fat.text + "g";
            protein.text = toedit_protein.text + "g";
            sodium.text = toedit_sodium.text + "g";
            sugars.text = toedit_sugars.text + "g";

            inputName.SetActive(false);
            inputNutrition.SetActive(false);
            inputNameButton.SetActive(false);
            inputNutritionButton.SetActive(false);
            editProduct.SetActive(false);
            editButton.SetActive(true);
            changed = true;
        }
        else if (request.responseCode == 401)
        {
            refreshToken.ToCallRefreshToken(LoginPage.authRefreshStatic, "user");
            StartCoroutine(ConfirmEditNutrition());
        }
        else
        {
            SceneManager.GetActiveScene();
        }
    }

    IEnumerator DoLast()
    {
        StartCoroutine(GetNutrition());
        while (inFirst)
            yield return new WaitForSeconds(0.1f);
        for (int i = 0; i < jsonNode.Count; i++)
        {
            var copy = Instantiate(itemTemplate);
            copy.transform.parent = content.transform;

            copy.GetComponentInChildren<Text>().text = jsonNode[i]["product_name"];

            int copyOfIndex = index;

            copy.GetComponent<Button>().onClick.AddListener(
                () =>
                {
                    StartCoroutine(QRcodeGenerator(copyOfIndex));
                }
            );

            index++;
        }
    }

    IEnumerator GetNutrition()
    {
        inFirst = true;

        yield return new WaitForSeconds(1.0f);

        var getNutrition = allProductURL;

        UnityWebRequest www = UnityWebRequest.Get(getNutrition);
        www.SetRequestHeader("AUTHORIZATION", LoginPage.authStatic);
        yield return www.SendWebRequest();

        jsonData = www.downloadHandler.text;
        jsonNode = SimpleJSON.JSON.Parse(jsonData);

        if (www.isNetworkError || www.isHttpError)
        {
            refreshToken.ToCallRefreshToken(LoginPage.authRefreshStatic, "user");
            StartCoroutine(GetNutrition());
        }
        else
        {
            
        }

        inFirst = false;
    }

    IEnumerator getProductById(string product_id_temp)
    {
        UnityWebRequest www = UnityWebRequest.Get(getProductByIdURL + "?product_id=" + product_id_temp);
        www.SetRequestHeader("AUTHORIZATION", LoginPage.adminAuthStatic);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
            refreshToken.ToCallRefreshToken(LoginPage.adminAuthRefreshStatic, "admin");
            StartCoroutine(getProductById(product_id_temp));
        }
        else
        {
            jsonData_temp = www.downloadHandler.text;
            jsonNode_temp = SimpleJSON.JSON.Parse(jsonData_temp);

            float dv_fat = jsonNode_temp["fat"]["dv"];
            float dv_sodium = jsonNode_temp["sodium"]["dv"];
            float dv_cholesterol = jsonNode_temp["cholesterol"]["dv"];
            float dv_carbs = jsonNode_temp["carbs"]["dv"];

            calories.text = jsonNode_temp["calories"];
            carbs.text = jsonNode_temp["carbs"]["value"] + "g";
            cholesterol.text = jsonNode_temp["cholesterol"]["value"] + "g";
            fat.text = jsonNode_temp["fat"]["value"] + "g";
            product_id.text = jsonNode_temp["product_id"];
            product_name.text = jsonNode_temp["product_name"];
            protein.text = jsonNode_temp["protein"] + "g";
            sodium.text = jsonNode_temp["sodium"]["value"] + "g";
            sugars.text = jsonNode_temp["sugars"] + "g";
            selectedProductID = jsonNode_temp["product_id"];
            owner.text = "by " + jsonNode_temp["username"];

            per_carbs.text = dv_carbs.ToString("F0") + "%";
            per_cholesterol.text = dv_cholesterol.ToString("F0") + "%";
            per_fat.text = dv_fat.ToString("F0") + "%";
            per_sodium.text = dv_sodium.ToString("F0") + "%";

            fat_bar.fillAmount = dv_fat / 100;
            sodium_bar.fillAmount = dv_sodium / 100;
            cholesterol_bar.fillAmount = dv_cholesterol / 100;
            carbs_bar.fillAmount = dv_carbs / 100;
            protein_bar.fillAmount = 0;
            sugar_bar.fillAmount = 0;

            if (dv_fat >= 15)
            {
                fat_bar.color = Color.red;
            }

            if (dv_carbs >= 15)
            {
                carbs_bar.color = Color.red;
            }

            if (dv_cholesterol >= 15)
            {
                cholesterol_bar.color = Color.red;
            }

            if (dv_sodium >= 15)
            {
                sodium_bar.color = Color.red;
            }
        }
    }

    IEnumerator Product_Click(int index)
    {
        StartCoroutine(getProductById(jsonNode[index]["product_id"]));
        yield return new WaitForSeconds(1.0f);
        iTween.MoveTo(showProduct, iTween.Hash("y", Screen.height / 2, "time", 1f, "easetype", easeType, "Looptype", loopType));
    }

    IEnumerator QRcodeGenerator(int index)
    {
        var getURL_QRcode = getQRURL + "/?data=" + jsonNode[index]["uuid"] + "&size=300x300";

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
            StartCoroutine(Product_Click(index));
        }
    }

    public void Cancel_Click()
    {
        StartCoroutine(CancelFunc());
    }

    IEnumerator CancelFunc()
    {
        iTween.MoveTo(showProduct, iTween.Hash("y", -(Screen.height * 0.67), "time", 1f, "easetype", easeType, "Looptype", loopType));
        if (changed)
        {
            yield return new WaitForSeconds(1.0f);
            SceneManager.LoadScene("MYPRODUCT-PAGE");
        }
    }

    public void DeleteProductButton()
    {
        editProduct.SetActive(true);
        confirmDeleteButton.SetActive(true);

        inputName.SetActive(false);
        inputNutrition.SetActive(false);
        editProductButton.SetActive(false);
    }

    public void ConFirmDeleteProductButton()
    {
        StartCoroutine(DeleteTarget());
    }

    IEnumerator DeleteProduct()
    {
        var editUrl = "https://unityproject-270307.uc.r.appspot.com/api/v1/product/" + selectedProductID;

        var request = new UnityWebRequest(editUrl, "DELETE");
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", LoginPage.authStatic);
        yield return request.SendWebRequest();
        Debug.Log("Status Code: " + request.responseCode);

        if (request.responseCode == 200)
        {
            confirmDeleteButton.SetActive(false);
            editButton.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            iTween.MoveTo(showProduct, iTween.Hash("y", -(Screen.height * 0.67), "time", 1f, "easetype", easeType, "Looptype", loopType));
            yield return new WaitForSeconds(1.0f);
            SceneManager.LoadScene("MYPRODUCT-PAGE");
        }
        else if (request.responseCode == 401)
        {
            refreshToken.ToCallRefreshToken(LoginPage.authRefreshStatic, "user");
            StartCoroutine(DeleteProduct());
        }
        else
        {
            SceneManager.GetActiveScene();
        }
    }

    IEnumerator DeleteTarget()
    {
        var targetID = selectedProductID;
        string requestPath = "/targets/" + targetID;
        string serviceURI = url + requestPath;
        string httpAction = "DELETE";
        string contentType = "";
        string requestBody = "";

        UnityWebRequest unityWebRequest = UnityWebRequest.Delete(serviceURI);
        string returnString = VuforiaRequest(requestPath, httpAction, contentType, requestBody, unityWebRequest);

        yield return null;
        StartCoroutine(DeleteProduct());
    }

    public void CancelDeleteButton()
    {
        confirmDeleteButton.SetActive(false);
        editProduct.SetActive(false);
    }

    public void BackButton()
    {
        SceneManager.LoadScene("QR-AR-PROJECT");
    }

    public void ClickShare()
    {
        StartCoroutine(LoadTGAndShare());
    }

    private IEnumerator TakeSSAndShare()
    {
        yield return new WaitForEndOfFrame();

        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        string filePath = Path.Combine(Application.temporaryCachePath, "shared img.png");
        File.WriteAllBytes(filePath, ss.EncodeToPNG());

        // To avoid memory leaks
        Destroy(ss);

        new NativeShare().AddFile(filePath).SetSubject("My QRcode").SetText("This is QR-Code for my product!").Share();

        // Share on WhatsApp only, if installed (Android only)
        //if( NativeShare.TargetExists( "com.whatsapp" ) )
        //	new NativeShare().AddFile( filePath ).SetText( "Hello world!" ).SetTarget( "com.whatsapp" ).Share();
    }

    private IEnumerator LoadTGAndShare()
    {
        yield return null;
        string filePath = Path.Combine(Application.temporaryCachePath, "shared img.png");
        File.WriteAllBytes(filePath, texture.EncodeToPNG());

        new NativeShare().AddFile(filePath).Share();
    }

    public string VuforiaRequest(string requestPath, string httpAction, string contentType, string requestBody, UnityWebRequest unityWebRequest)
    {
        string serviceURI = url + requestPath;
        string date = string.Format("{0:r}", DateTime.Now.ToUniversalTime());

        unityWebRequest.SetRequestHeader("host", url);
        unityWebRequest.SetRequestHeader("date", date);
        unityWebRequest.SetRequestHeader("content-type", contentType);

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

        unityWebRequest.SetRequestHeader("authorization", string.Format("VWS {0}:{1}", access_key, signature));


        unityWebRequest.SendWebRequest();
        while (!unityWebRequest.isDone && !unityWebRequest.isNetworkError) { }
        //If request error, return fail
        if (unityWebRequest.error != null)
        {
            Debug.Log("requestError: " + unityWebRequest.error);

            return "fail";
        }
        else
        {
            if (httpAction == "DELETE")
            {
                return "Deleted";
            }
            return unityWebRequest.downloadHandler.text;
        }
    }
}
