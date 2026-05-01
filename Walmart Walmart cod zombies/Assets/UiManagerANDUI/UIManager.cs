using System.Collections;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;


//we are goign to use leanTween for the UI animations
//theres fading in/out and moving
//Should be Dont Destroy on Load along with the canvas
//LeanTweens are coroutines so becareful because it will skip to the next line right away;
//its instances.load so the inscene gets used not prefab
//If you are wondering this doesn't work with some other ratio or work in the build
//you will need dyanmic UI stuff for it
public class UIManager : MonoBehaviour
{
    GameObject PSCanvas;
    GameObject pasueScreen;
    public static UIManager Instance;
    Vector3 originalPosition;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            gameObject.SetActive(true);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        Instance.PSCanvas = GameObject.FindGameObjectWithTag("PSCanvas");
        Instance.pasueScreen = Instance.PSCanvas.transform.GetChild(0).gameObject;
        Instance.originalPosition = Instance.pasueScreen.transform.position;
        updateSettingSliders();
    }


    //pulls up the pause menu and stops time
    public void pauseScreen()
    {
        Time.timeScale = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        Instance.fade(Instance.pasueScreen, 0.3f, 1f, () => PSCanvas.SetActive(true));

    }
    //unpasues the game
    public void unPauseScreen()
    {
        Time.timeScale = 1f;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        Instance.fade(Instance.pasueScreen, 0.3f, 0f, () => PSCanvas.SetActive(false));
        Instance.backButton();
    }
    //-------------------------------------------------------------------------------------------------------------------------------------------
    //Method for changing volume for audioManager
    public void changeSFX(UnityEngine.UI.Slider volumeSlider)
    {
        audioManagerZombies.instance.changeSfxVolume(volumeSlider.value);
    }
    public void changeMaster(UnityEngine.UI.Slider volumeSlider)
    {
        audioManagerZombies.instance.changeMasterVolume(volumeSlider.value);
    }
    public void changeMusic(UnityEngine.UI.Slider volumeSlider)
    {
        audioManagerZombies.instance.changeMusicVolume(volumeSlider.value);
    }
    //-------------------------------------------------------------------------------------------------------------------------------------------
    public void changeMouseSensitivity(UnityEngine.UI.Slider volumeSlider)
    {
        audioManagerZombies.instance.changeMouseSensitivity(volumeSlider.value);
    }
    public void playTitleScreenButton()
    {
        Instance.StartCoroutine(waitForSceneToLoad(1));
    }
    public void quitTitleScreen() 
    {
        Application.Quit(1);
    }
    public void credits() 
    {
        Instance.move(Instance.pasueScreen, new Vector3(
           Instance.originalPosition.x,
           Instance.originalPosition.y - Screen.height,
           Instance.originalPosition.z), 0.3f);
    }
    
    //PAUSE SCREEN BUTTONS---------------------------------------------------------------------------------------------------------------------------------
    public void reStart()
    {
        LeanTween.value(Instance.pasueScreen, 0f, 1f, 0.3f).setIgnoreTimeScale(true).setOnUpdate((float val) =>
        {
            Instance.pasueScreen.GetComponent<UnityEngine.UI.Image>().color = new Color(23 / 255f, 23 / 255f, 23 / 255f, val);
        }).setIgnoreTimeScale(true).setOnComplete(() =>
        {
            Time.timeScale = 1f;
            Instance.StartCoroutine(waitForSceneToLoad(1));
        });

    }
    public void quitPauseMenu() 
    {
        LeanTween.value(Instance.pasueScreen, 0f, 1f, 0.3f).setIgnoreTimeScale(true).setOnUpdate((float val) =>
        {
            Instance.pasueScreen.GetComponent<UnityEngine.UI.Image>().color = new Color(23 / 255f, 23 / 255f, 23 / 255f, val);
        }).setIgnoreTimeScale(true).setOnComplete(() =>
        {
            Time.timeScale = 1f;
            Instance.StartCoroutine(waitForSceneToLoad(0));
        });
    }

    IEnumerator waitForSceneToLoad(int sceneNumber)
    {
        GameObject loadingScreen = null;
        UnityEngine.UI.Slider loadingSlider = null;

        if (sceneNumber == 1) 
        {
            AsyncOperation aP = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(2);
            while (!aP.isDone) 
            {
                yield return null;
            }
            loadingScreen = GameObject.FindGameObjectWithTag("LoadingScreen");
            loadingSlider = loadingScreen.transform.Find("LoadingBar").GetComponent<UnityEngine.UI.Slider>();
        }
                
        AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneNumber);
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            Debug.Log(progress);
            if (loadingSlider!=null) 
            {
                //note in the original game the loading screen is a fake loading screen that just waits for a certain amount of time but this is an actual loading screen that shows the progress of the scene loading
                loadingSlider.value = progress;
            }
           yield return null;
        }
        Instance.onSceneChange(sceneNumber);
    }
    public void resumeButtonOnClick()
    {
        Instance.unPauseScreen();
    }
    //screen width
    public void pauseMenuSettingButtonOnClick()
    {
            Instance.move(Instance.pasueScreen, new Vector3(
            Instance.originalPosition.x - Screen.width,
            Instance.originalPosition.y,
            Instance.originalPosition.z), 0.3f);
        // Instance.move(Instance.pasueScreen, new Vector3(Instance.pasueScreen.transform.position.x + -2300, Instance.pasueScreen.transform.position.y, Instance.pasueScreen.transform.position.z), 0.3f);
    }
    public void controlMenuSettingButtonOnClick()
    {
           Instance.move(Instance.pasueScreen, new Vector3(
           Instance.originalPosition.x + Screen.width,
           Instance.originalPosition.y,
           Instance.originalPosition.z), 0.3f);
    }
    public void backButton()
    {
        Instance.move(Instance.pasueScreen, Instance.originalPosition, 0.3f);
    }
    //-------------------------------------------------------------------------------------------------------------------------------------------

    //fade the UI group in/out
    //system Action just holds a refernce to a function
    //I couldn't find function pointer so I ended up using delgates or action(which are probabley just function pointers underneath)
    //Source: https://discussions.unity.com/t/how-to-pass-a-function-as-an-argument-in-unity/724802/5
    //delgates are basically events. Action are delgates that return void and can have parameters. 
    void fade(GameObject target, float duration, float alpha, System.Action callOnComplete = null)
    {
        LeanTween.alphaCanvas(target.GetComponent<CanvasGroup>(), alpha, duration).setIgnoreTimeScale(true).setOnComplete(() =>
        {
            //? is just short hand for if statment that checks if callOnComplete is not null before invoking it
            callOnComplete?.Invoke();
        });
    }

    void updateSettingSliders() 
    {
       Transform settingScreen = pasueScreen.transform.Find("SettingScreen");
       Transform sfxVol = settingScreen.Find("SFXVolume");
       Transform masterVol = settingScreen.Find("MasterVolume");
       Transform musicVol = settingScreen.Find("Music");
       Transform mouseSense = settingScreen.Find("MuseSensitivity");
       sfxVol.Find("VolumeSilder").GetComponent<UnityEngine.UI.Slider>().value = audioManagerZombies.instance.sfxVolume/100f;
       masterVol.Find("VolumeSilder").GetComponent<UnityEngine.UI.Slider>().value = audioManagerZombies.instance.masterVolume/100f;
       musicVol.Find("VolumeSilder").GetComponent<UnityEngine.UI.Slider>().value = audioManagerZombies.instance.musicVolume/100f;
       mouseSense.Find("VolumeSilder").GetComponent<UnityEngine.UI.Slider>().value = audioManagerZombies.instance.mouseSensitivity/100f;
    }
    //moves the UI element to a new position
    void move(GameObject target, Vector3 to, float duration)
    {
        LeanTween.move(target, to, duration).setIgnoreTimeScale(true); ;
    }
    public void onSceneChange(int scene)
    {
        Instance.PSCanvas = GameObject.FindGameObjectWithTag("PSCanvas");
        Instance.pasueScreen = Instance.PSCanvas.transform.GetChild(0).gameObject;
        Instance.originalPosition = Instance.pasueScreen.transform.position;
        if (scene==1) 
        {
            Instance.PSCanvas.SetActive(false);
            Instance.pasueScreen.GetComponent<UnityEngine.UI.Image>().color = new Color(96 / 255f, 96 / 255f, 96 / 255f, 160 / 255f);
        }
        updateSettingSliders();
    }
}
