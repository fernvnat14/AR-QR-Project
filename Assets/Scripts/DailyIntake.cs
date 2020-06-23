using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class DailyIntake : MonoBehaviour
{
    //api
    readonly string dailyIntakeURL = "https://unityproject-270307.uc.r.appspot.com/api/v1/daily-intake";

    //error
    public GameObject loginRequired;
    public GameObject dailyIntakeWindows;

    public RefreshToken refreshToken;

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
    public TMPro.TextMeshProUGUI per_protein;
    public TMPro.TextMeshProUGUI per_sugars;

    //text
    public TMPro.TextMeshProUGUI calories;
    public TMPro.TextMeshProUGUI carbs;
    public TMPro.TextMeshProUGUI cholesterol;
    public TMPro.TextMeshProUGUI fat;
    public TMPro.TextMeshProUGUI protein;
    public TMPro.TextMeshProUGUI sodium;
    public TMPro.TextMeshProUGUI sugars;
    public TMPro.TextMeshProUGUI date;
    public TMPro.TextMeshProUGUI owner;

    //json
    public string jsonData;
    ArrayList TempleArray;
    JSONNode jsonNode;

    //iTween
    public iTween.EaseType easeType;
    public iTween.LoopType loopType;

    // Start is called before the first frame update
    void Start()
    {
        loginRequired.SetActive(false);
        // dailyIntakeWindows.SetActive(false);
        fat_bar.fillAmount = 0;
        sodium_bar.fillAmount = 0;
        cholesterol_bar.fillAmount = 0;
        carbs_bar.fillAmount = 0;
        protein_bar.fillAmount = 0;
        sugar_bar.fillAmount = 0;
    }

    public void Logout()
    {
        LoginPage.usernameStatic = "Annonymous";
        LoginPage.nameStatic = "Annonymous";
        SceneManager.LoadScene("LOGIN-PAGE");
    }

    public void InfoButton_Cancel()
    {
        loginRequired.SetActive(false);
        iTween.MoveTo(dailyIntakeWindows, iTween.Hash("y", -(Screen.height / 2), "time", 1f, "easetype", easeType, "Looptype", loopType));
    }

    public void LoginButton()
    {
        SceneManager.LoadScene("LOGIN-PAGE");
    }

    public void InfoButton()
    {
        if (LoginPage.usernameStatic == "Annonymous")
        {
            loginRequired.SetActive(true);
        }
        else
        {
            StartCoroutine(DailyIntakeFunc());
        }
    }

    public void MyProduct()
    {
        SceneManager.LoadScene("MYPRODUCT-PAGE");
    }

    IEnumerator DailyIntakeFunc()
    {
        var getDailyNutrition = dailyIntakeURL + "?username=" + LoginPage.usernameStatic;

        UnityWebRequest www = UnityWebRequest.Get(getDailyNutrition);
        www.SetRequestHeader("AUTHORIZATION", LoginPage.authStatic);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            refreshToken.ToCallRefreshToken(LoginPage.authRefreshStatic, "user");
            StartCoroutine(DailyIntakeFunc());
        }
        else
        {
            if (www.responseCode == 200)
            {
                jsonData = www.downloadHandler.text;
                jsonNode = SimpleJSON.JSON.Parse(jsonData);

                owner.text = LoginPage.nameStatic;
                calories.text = checkNull(jsonNode["calories"]);
                carbs.text = checkNull(jsonNode["carbs"]["value"]) + " g";
                cholesterol.text = checkNull(jsonNode["cholesterol"]["value"]) + " g";
                fat.text = checkNull(jsonNode["fat"]["value"]) + " g";
                date.text = jsonNode["date"];
                protein.text = checkNull(jsonNode["protein"]["value"]) + " g";
                sodium.text = checkNull(jsonNode["sodium"]["value"]) + " g";
                sugars.text = checkNull(jsonNode["sugars"]["value"]) + " g";

                float dv_fat = jsonNode["fat"]["dv"];
                float dv_sodium = jsonNode["sodium"]["dv"];
                float dv_cholesterol = jsonNode["cholesterol"]["dv"];
                float dv_carbs = jsonNode["carbs"]["dv"];
                float dv_protein = jsonNode["protein"]["dv"];
                float dv_sugars = jsonNode["sugars"]["dv"];

                per_carbs.text = dv_carbs.ToString("F0") + "%";
                per_cholesterol.text = dv_cholesterol.ToString("F0") + "%";
                per_fat.text = dv_fat.ToString("F0") + "%";
                per_sodium.text = dv_sodium.ToString("F0") + "%";
                per_protein.text = dv_protein.ToString("F0") + "%";
                per_sugars.text = dv_sugars.ToString("F0") + "%";

                fat_bar.fillAmount = dv_fat / 100;
                sodium_bar.fillAmount = dv_sodium / 100;
                cholesterol_bar.fillAmount = dv_cholesterol / 100;
                carbs_bar.fillAmount = dv_carbs / 100;
                protein_bar.fillAmount = dv_protein / 100; ;
                sugar_bar.fillAmount = dv_sugars / 100; ;
                yield return new WaitForEndOfFrame();
                iTween.MoveTo(dailyIntakeWindows, iTween.Hash("y", Screen.height/2, "time", 1f, "easetype", easeType, "Looptype", loopType));
                /*user_Animator.SetBool("CancelButton", false);
                user_Animator.SetBool("InformationButton", true);*/
            }
        }
    }

    string checkNull(string toCheck)
    {
        if (toCheck != null)
        {
            return toCheck;
        }
        else
        {
            return "0";
        }
    }
}
