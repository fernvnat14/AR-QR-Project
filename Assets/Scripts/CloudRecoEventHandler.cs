/*==============================================================================
Copyright (c) 2015-2018 PTC Inc. All Rights Reserved.

Copyright (c) 2012-2015 Qualcomm Connected Experiences, Inc. All Rights Reserved.

Vuforia is a trademark of PTC Inc., registered in the United States and other 
countries.
==============================================================================*/
using System.Collections;
using System.Collections.Generic;
using System.Text;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Vuforia;

/// <summary>
/// This MonoBehaviour implements the Cloud Reco Event handling for this sample.
/// It registers itself at the CloudRecoBehaviour and is notifiedf of new search results as well as error messages
/// The current state is visualized and new results are enabled using the TargetFinder API.
/// </summary>
public class CloudRecoEventHandler : MonoBehaviour, IObjectRecoEventHandler
{
    #region PRIVATE_MEMBERS
    CloudRecoBehaviour m_CloudRecoBehaviour;
    ObjectTracker m_ObjectTracker;
    TargetFinder m_TargetFinder;

    [TextArea]
    public string jsonData;
    ArrayList TempleArray;
    public JSONNode jsonNode;

    //slider
    public UnityEngine.UI.Image carbs_bar;
    public UnityEngine.UI.Image cholesterol_bar;
    public UnityEngine.UI.Image fat_bar;
    public UnityEngine.UI.Image sodium_bar;

    public UnityEngine.UI.Image seemore_carbs_bar;
    public UnityEngine.UI.Image seemore_cholesterol_bar;
    public UnityEngine.UI.Image seemore_fat_bar;
    public UnityEngine.UI.Image seemore_sodium_bar;
    public UnityEngine.UI.Image seemore_protein_bar;
    public UnityEngine.UI.Image seemore_sugar_bar;

    public TMPro.TextMeshProUGUI detectText;

    //private
    private string admin_username = "admin";
    private string admin_password = "12345";

    //target information
    public TMPro.TextMeshProUGUI calories;
    public TMPro.TextMeshProUGUI carbs;
    public TMPro.TextMeshProUGUI fat;
    public TMPro.TextMeshProUGUI cholesterol;
    public TMPro.TextMeshProUGUI sodium;
    public TMPro.TextMeshProUGUI product_name;
    public TMPro.TextMeshProUGUI owner;

    public TMPro.TextMeshProUGUI seemoreowner;
    public TMPro.TextMeshProUGUI seemoreproductname;
    public TMPro.TextMeshProUGUI seemorecalories;
    public TMPro.TextMeshProUGUI seemorecarbs;
    public TMPro.TextMeshProUGUI seemorefat;
    public TMPro.TextMeshProUGUI seemoreprotein;
    public TMPro.TextMeshProUGUI seemoresugar;
    public TMPro.TextMeshProUGUI seemoresodium;
    public TMPro.TextMeshProUGUI seemorecholesterol;

    //new UI
    public TMPro.TextMeshProUGUI per_fat;
    public TMPro.TextMeshProUGUI per_sodium;
    public TMPro.TextMeshProUGUI per_cholesterol;
    public TMPro.TextMeshProUGUI per_carbs;

    public TMPro.TextMeshProUGUI seemore_per_fat;
    public TMPro.TextMeshProUGUI seemore_per_sodium;
    public TMPro.TextMeshProUGUI seemore_per_cholesterol;
    public TMPro.TextMeshProUGUI seemore_per_carbs;

    string targetID;

    //toAddProduct
    public float toadd_sugars;
    public float toadd_sodium;
    public float toadd_cholesterol;
    public float toadd_calories;
    public float toadd_carbs;
    public float toadd_fat;
    public float toadd_protein;
    public string toadd_product_name;
    public string toadd_product_id;
    public string toadd_username;

    //gameObject
    public GameObject seeMore;
    public GameObject seeLessTarget;
    public GameObject seeLess;
    public GameObject onProduct;
    public GameObject productDetail;
    public GameObject loginRequired;

    //animation
    public Animator windows_Animator;
    public Animator targetAR_Animator;

    //func
    public DailyIntake script_DailyIntake;
    public RefreshToken refreshToken;

    bool seeMoreBool = false;

    //
    public float dv_fat;
    public float dv_sodium;
    public float dv_cholesterol;
    public float dv_carbs;
    public float value_calories;
    public float value_fat;
    public float value_sodium;
    public float value_cholesterol;
    public float value_carbs;
    public float value_sugar;
    public float value_protein;
    public string value_product_id;
    public string value_product_name;
    public string value_product_owner;

    //iTween
    public iTween.EaseType easeType;
    public iTween.LoopType loopType;

    public UnityEngine.UI.Image statusImage;
    public Sprite red;
    public Sprite green;

    #endregion // PRIVATE_MEMBERS
    #region PUBLIC_MEMBERS
    /// <summary>
    /// Can be set in the Unity inspector to reference a ImageTargetBehaviour 
    /// that is used for augmentations of new cloud reco results.
    /// </summary>
    [Tooltip("Here you can set the ImageTargetBehaviour from the scene that will be used to " +
             "augment new cloud reco search results.")]
    public ImageTargetBehaviour m_ImageTargetBehaviour;
    public UnityEngine.UI.Image m_CloudActivityIcon;
    public UnityEngine.UI.Image m_CloudIdleIcon;

    readonly string getURL = "https://unityproject-270307.uc.r.appspot.com/api/v1/product";
    readonly string dailyIntakeURL = "https://unityproject-270307.uc.r.appspot.com/api/v1/daily-intake";
    #endregion // PUBLIC_MEMBERS

    string authenticate(string username, string password)
    {
        string auth = username + ":" + password;
        auth = System.Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
        auth = "Basic " + auth;
        return auth;
    }

    #region MONOBEHAVIOUR_METHODS
    /// <summary>
    /// Register for events at the CloudRecoBehaviour
    /// </summary>
    void Start()
    {
        fat_bar.fillAmount = 0;
        sodium_bar.fillAmount = 0;
        cholesterol_bar.fillAmount = 0;
        carbs_bar.fillAmount = 0;

        seemore_fat_bar.fillAmount = 0;
        seemore_sodium_bar.fillAmount = 0;
        seemore_cholesterol_bar.fillAmount = 0;
        seemore_carbs_bar.fillAmount = 0;
        seemore_protein_bar.fillAmount = 0;
        seemore_sugar_bar.fillAmount = 0;

        // Register this event handler at the CloudRecoBehaviour
        m_CloudRecoBehaviour = GetComponent<CloudRecoBehaviour>();
        if (m_CloudRecoBehaviour)
        {
            m_CloudRecoBehaviour.RegisterEventHandler(this);
        }
        detectText.text = "No Product Detected";
        statusImage.sprite = red;
    }

    void Update()
    {
        if (m_CloudRecoBehaviour.CloudRecoInitialized && m_TargetFinder != null)
        {
            SetCloudActivityIconVisible(m_TargetFinder.IsRequesting());
        }

        if (m_ImageTargetBehaviour.CurrentStatusInfo.ToString() == "UNKNOWN")
        {
            detectText.text = "No Product Detected";
            seeLessTarget.transform.position = new Vector3(2000, 0, 0);
            statusImage.sprite = red;
            //seeLessTarget.SetActive(false);
        }

        else if (m_ImageTargetBehaviour.CurrentStatusInfo.ToString() == "NORMAL" && !seeMoreBool)
        {
            detectText.text = "Product Detected";
            statusImage.sprite = green;
            //seeLessTarget.SetActive(true);
        }

        /*
        if (m_CloudIdleIcon)
        {
            m_CloudIdleIcon.color = m_CloudRecoBehaviour.CloudRecoEnabled ? Color.white : Color.gray;
        } */
    }
    #endregion // MONOBEHAVIOUR_METHODS


    #region INTERFACE_IMPLEMENTATION_ICloudRecoEventHandler
    /// <summary>
    /// called when TargetFinder has been initialized successfully
    /// </summary>
    public void OnInitialized()
    {
        Debug.Log("Cloud Reco initialized successfully.");

        m_ObjectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        m_TargetFinder = m_ObjectTracker.GetTargetFinder<ImageTargetFinder>();
    }

    public void OnInitialized(TargetFinder targetFinder)
    {
        Debug.Log("Cloud Reco initialized successfully.");

        m_ObjectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        m_TargetFinder = targetFinder;
    }

    // Error callback methods implemented in CloudErrorHandler
    public void OnInitError(TargetFinder.InitState initError) { }
    public void OnUpdateError(TargetFinder.UpdateState updateError) { }


    /// <summary>
    /// when we start scanning, unregister Trackable from the ImageTargetBehaviour, 
    /// then delete all trackables
    /// </summary>
    public void OnStateChanged(bool scanning)
    {
        Debug.Log("<color=blue>OnStateChanged(): </color>" + scanning);
        // Changing CloudRecoBehaviour.CloudRecoEnabled to false will call:
        // 1. TargetFinder.Stop()
        // 2. All registered ICloudRecoEventHandler.OnStateChanged() with false.

        // Changing CloudRecoBehaviour.CloudRecoEnabled to true will call:
        // 1. TargetFinder.StartRecognition()
        // 2. All registered ICloudRecoEventHandler.OnStateChanged() with true.
    }

    /// <summary>
    /// Handles new search results
    /// </summary>
    /// <param name="targetSearchResult"></param>
    public void OnNewSearchResult(TargetFinder.TargetSearchResult targetSearchResult)
    {           
        Debug.Log("<color=blue>OnNewSearchResult(): </color>" + targetSearchResult.TargetName);
        detectText.text = "Product Detected";
        statusImage.sprite = green;
        TargetFinder.CloudRecoSearchResult cloudRecoResult = (TargetFinder.CloudRecoSearchResult)targetSearchResult;
        targetID = cloudRecoResult.UniqueTargetId;
        StartCoroutine(getTargetData());

        // StartCoroutine(RunAnimations());
        // This code demonstrates how to reuse an ImageTargetBehaviour for new search results
        // and modifying it according to the metadata. Depending on your application, it can
        // make more sense to duplicate the ImageTargetBehaviour using Instantiate() or to
        // create a new ImageTargetBehaviour for each new result. Vuforia will return a new
        // object with the right script automatically if you use:
        // TargetFinder.EnableTracking(TargetSearchResult result, string gameObjectName

        /*("MetaData: " + cloudRecoResult.MetaData);
        Debug.Log("TargetName: " + cloudRecoResult.TargetName);
        Debug.Log("Pointer: " + cloudRecoResult.TargetSearchResultPtr);
        Debug.Log("TrackingRating: " + cloudRecoResult.TrackingRating);
        Debug.Log("UniqueTargetId: " + cloudRecoResult.UniqueTargetId);*/

        // Changing CloudRecoBehaviour.CloudRecoEnabled to false will call TargetFinder.Stop()
        // and also call all registered ICloudRecoEventHandler.OnStateChanged() with false.
        m_CloudRecoBehaviour.CloudRecoEnabled = true;

        // Clear any existing trackables
        m_TargetFinder.ClearTrackables(false);

        // Enable the new result with the same ImageTargetBehaviour:
        m_TargetFinder.EnableTracking(cloudRecoResult, m_ImageTargetBehaviour.gameObject);

        // Pass the TargetSearchResult to the Trackable Event Handler for processing
        m_ImageTargetBehaviour.gameObject.SendMessage("TargetCreated", cloudRecoResult, SendMessageOptions.DontRequireReceiver);
    }
    #endregion // INTERFACE_IMPLEMENTATION_ICloudRecoEventHandler


    #region PRIVATE_METHODS
    void SetCloudActivityIconVisible(bool visible)
    {
        /*
        if (!m_CloudActivityIcon) return;

        m_CloudActivityIcon.enabled = visible; */
    }
    #endregion // PRIVATE_METHODS

    public void SeeMoreButton()
    {
        StartCoroutine(SeeMore());
    }

    IEnumerator SeeMore()
    {
        seeLessTarget.SetActive(true);
        yield return new WaitForEndOfFrame();
        /*seeMoreBool = true;
        seeLessTarget.SetActive(false);
        seeMore.SetActive(true);*/

        seemorecalories.text = value_calories.ToString("F0") + " Calories";
        seemorecarbs.text = value_carbs.ToString("F2") + "g";
        seemorefat.text = value_fat.ToString("F2") + "g";
        seemoreprotein.text = value_protein.ToString("F2") + "g";
        seemoresugar.text = value_sugar.ToString("F2") + "g";
        seemoresodium.text = value_sodium.ToString("F2") + "g";
        seemorecholesterol.text = value_cholesterol.ToString("F2") + "g";

        seemore_fat_bar.fillAmount = dv_fat / 100;
        seemore_sodium_bar.fillAmount = dv_sodium / 100;
        seemore_cholesterol_bar.fillAmount = dv_cholesterol / 100;
        seemore_carbs_bar.fillAmount = dv_carbs / 100;
        seemore_protein_bar.fillAmount = 0;
        seemore_sugar_bar.fillAmount = 0;

        if (dv_fat >= 15)
        {
            seemore_fat_bar.color = Color.red;
        }

        if (dv_carbs >= 15)
        {
            seemore_carbs_bar.color = Color.red;
        }

        if (dv_cholesterol >= 15)
        {
            seemore_cholesterol_bar.color = Color.red;
        }

        if (dv_sodium >= 15)
        {
            seemore_sodium_bar.color = Color.red;
        }

        iTween.MoveTo(seeMore, iTween.Hash("y", Screen.height / 2, "time", 1f, "easetype", easeType, "Looptype", loopType));
    }

    public void SeeLessButton()
    {
        StartCoroutine(SeeLess());
    }

    IEnumerator SeeLess()
    {
        yield return new WaitForEndOfFrame();
        seeMoreBool = false;
        seeLessTarget.SetActive(true);
        iTween.MoveTo(seeMore, iTween.Hash("y", -(Screen.height / 2), "time", 1f, "easetype", easeType, "Looptype", loopType));
    }

    public void ProductDetail()
    {
        productDetail.SetActive(true);
    }

    public void CancelTargetButton()
    {
   
    }

    IEnumerator RunAnimations()
    {
        seeLessTarget.SetActive(true);
        yield return new WaitForSeconds(1);
        onProduct.SetActive(true);
    }

    IEnumerator getTargetData()
    {
        UnityWebRequest www = UnityWebRequest.Get(getURL + "?product_id=" + targetID);
        www.SetRequestHeader("AUTHORIZATION", LoginPage.adminAuthStatic);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.LogError(www.error);
            refreshToken.ToCallRefreshToken(LoginPage.adminAuthRefreshStatic, "admin");
            StartCoroutine(getTargetData());
        }
        else
        {
            seeLessTarget.SetActive(true);
            yield return new WaitForEndOfFrame();

            jsonData = www.downloadHandler.text;
            jsonNode = SimpleJSON.JSON.Parse(jsonData);

            dv_fat = jsonNode["fat"]["dv"];
            dv_sodium = jsonNode["sodium"]["dv"];
            dv_cholesterol = jsonNode["cholesterol"]["dv"];
            dv_carbs = jsonNode["carbs"]["dv"];

            value_calories = jsonNode["calories"];
            value_fat = jsonNode["fat"]["value"];
            value_sodium = jsonNode["sodium"]["value"];
            value_cholesterol = jsonNode["cholesterol"]["value"];
            value_carbs = jsonNode["carbs"]["value"];
            value_sugar = jsonNode["sugars"];
            value_protein = jsonNode["protein"];

            value_product_id = jsonNode["product_id"];
            value_product_name = jsonNode["product_name"];
            value_product_owner = jsonNode["username"];

            calories.text = value_calories.ToString("F2") + " Calories";
            carbs.text = value_carbs.ToString("F2") + "g";
            fat.text = value_fat.ToString("F2") + "g";
            sodium.text = value_sodium.ToString("F2") + "g";
            cholesterol.text = value_cholesterol.ToString("F2") + "g";
            product_name.text = value_product_name;
            owner.text = "by " + value_product_owner;

            toadd_calories = value_calories;
            toadd_protein = value_protein;
            toadd_fat = value_fat;
            toadd_carbs = value_carbs;
            toadd_sugars = value_sugar;
            toadd_sodium = value_sodium;
            toadd_cholesterol = value_cholesterol;
            toadd_product_id = targetID;
            toadd_username = LoginPage.usernameStatic;

            per_carbs.text = dv_carbs.ToString("F0") + "%";
            per_cholesterol.text = dv_cholesterol.ToString("F0") + "%";
            per_fat.text = dv_fat.ToString("F0") + "%";
            per_sodium.text = dv_sodium.ToString("F0") + "%";

            seemore_per_carbs.text = dv_carbs.ToString("F0") + "%";
            seemore_per_cholesterol.text = dv_cholesterol.ToString("F0") + "%";
            seemore_per_fat.text = dv_fat.ToString("F0") + "%";
            seemore_per_sodium.text = dv_sodium.ToString("F0") + "%";
            seemoreowner.text = "by " + value_product_owner;
            seemoreproductname.text = value_product_name;

            fat_bar.fillAmount = dv_fat / 100;
            sodium_bar.fillAmount = dv_sodium / 100;
            cholesterol_bar.fillAmount = dv_cholesterol / 100;
            carbs_bar.fillAmount = dv_carbs / 100;

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

    public class ToAddProduct
    {
        public float sugars;
        public float sodium;
        public float cholesterol;
        public float calories;
        public float carbs;
        public float fat;
        public float protein;
        public string product_id;
        public string username;
    }

    public void addToDailyIntakeButton()
    {
        if (LoginPage.usernameStatic == "Annonymous")
        {
            loginRequired.SetActive(true);
        }
        else
        {
            StartCoroutine(addToDailyIntake());
        }
    }

    IEnumerator addToDailyIntake()
    {
        ToAddProduct toAddJson = new ToAddProduct();
        toAddJson.calories = toadd_calories;
        toAddJson.protein = toadd_protein;
        toAddJson.fat = toadd_fat;
        toAddJson.carbs = toadd_carbs;
        toAddJson.sugars = toadd_sugars;
        toAddJson.sodium = toadd_sodium;
        toAddJson.cholesterol = toadd_cholesterol;
        toAddJson.product_id = toadd_product_id;
        toAddJson.username = toadd_username;

        string json = JsonUtility.ToJson(toAddJson);

        var request = new UnityWebRequest(dailyIntakeURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", LoginPage.authStatic);
        yield return request.SendWebRequest();

        if (request.responseCode == 201)
        {
            seeMoreBool = false;
            seeLessTarget.SetActive(true);
            iTween.MoveTo(seeMore, iTween.Hash("y", -(Screen.height / 2), "time", 1f, "easetype", easeType, "Looptype", loopType));
            yield return new WaitForSeconds(1);
            script_DailyIntake.InfoButton();
        }
        else if (request.responseCode == 401)
        {
            refreshToken.ToCallRefreshToken(LoginPage.authRefreshStatic, "user");
            StartCoroutine(addToDailyIntake());
        }
        else
        {
            SceneManager.GetActiveScene();
        }
    }
}
