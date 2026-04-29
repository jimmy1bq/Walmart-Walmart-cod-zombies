using UnityEngine;


//we are goign to use leanTween for the UI animations
//theres fading in/out and moving
public class UIManager : MonoBehaviour
{
    GameObject PSCanvas;
    GameObject pasueScreen;
    public static UIManager Instance;
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
        PSCanvas.SetActive(false);
        pasueScreen = PSCanvas.transform.GetChild(0).gameObject;
    }

    //pulls up the pause menu and stops time
    public void pauseScreen()
    {
        Time.timeScale = 0f;
        fade(pasueScreen, 0.3f, 1f, () => PSCanvas.SetActive(true));

    }
    //unpasues the game
    public void unPauseScreen()
    {
        Time.timeScale = 1f;
        fade(pasueScreen, 0.3f, 0f,() => PSCanvas.SetActive(false));
       
    }

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
}
