using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


//we are goign to use leanTween for the UI animations
//theres fading in/out and moving
//Should be Dont Destroy on Load along with the canvas
//LeanTweens are coroutines so becareful because it will skip to the next line right away;
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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        PSCanvas = GameObject.FindGameObjectWithTag("PSCanvas");
        pasueScreen = PSCanvas.transform.GetChild(0).gameObject;
        originalPosition = pasueScreen.transform.position;
    }

    //pulls up the pause menu and stops time
    public void pauseScreen()
    {
        Time.timeScale = 0f;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        fade(pasueScreen, 0.3f, 1f, () => PSCanvas.SetActive(true));

    }
    //unpasues the game
    public void unPauseScreen()
    {
        Time.timeScale = 1f;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        fade(pasueScreen, 0.3f, 0f, () => PSCanvas.SetActive(false));
        backButton();
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

    }
    public void playTitleScreenButton() 
    {
       StartCoroutine(waitForSceneToLoad(1));
    }
    //PAUSE SCREEN BUTTONS---------------------------------------------------------------------------------------------------------------------------------
    public void reStart()
    {
        LeanTween.value(pasueScreen, 0f, 1f, 0.3f).setIgnoreTimeScale(true).setOnUpdate((float val) =>
        {
            pasueScreen.GetComponent<UnityEngine.UI.Image>().color = new Color(23 / 255f, 23 / 255f, 23 / 255f, val);
        }).setIgnoreTimeScale(true).setOnComplete(() =>
        {
            Time.timeScale = 1f;
            StartCoroutine(waitForSceneToLoad(1));
        });

    }
    IEnumerator waitForSceneToLoad(int sceneNumber)
    {
        AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneNumber);
        while (!operation.isDone)
        {
            yield return null;
        }
        onSceneChange();
    }
    public void resumeButtonOnClick()
    {
        unPauseScreen();
    }
    public void pauseMenuSettingButtonOnClick()
    {
        move(pasueScreen, new Vector3(pasueScreen.transform.position.x + -800, pasueScreen.transform.position.y, pasueScreen.transform.position.z), 0.3f);
    }
    public void controlMenuSettingButtonOnClick()
    {
        move(pasueScreen, new Vector3(pasueScreen.transform.position.x + 1500, pasueScreen.transform.position.y, pasueScreen.transform.position.z), 0.3f);
    }
    public void backButton()
    {
        move(pasueScreen, originalPosition, 0.3f);
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

    //moves the UI element to a new position
    void move(GameObject target, Vector3 to, float duration)
    {
        LeanTween.move(target, to, duration).setIgnoreTimeScale(true); ;
    }
    public void onSceneChange()
    {
        PSCanvas = GameObject.FindGameObjectWithTag("PSCanvas");
        PSCanvas.SetActive(false);
        pasueScreen = PSCanvas.transform.GetChild(0).gameObject;
        originalPosition = pasueScreen.transform.position;
        pasueScreen.GetComponent<UnityEngine.UI.Image>().color = new Color(96 / 255f, 96 / 255f, 96 / 255f, 160 / 255f);
    }
}
