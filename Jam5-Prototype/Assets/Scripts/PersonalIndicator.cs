using UnityEngine;
using UnityEngine.UI;
public class PersonalIndicator : MonoBehaviour
{
    private Camera mainCamera;
 
    
    //Indicates if the object is out of the screen
    private bool m_outOfScreen;

    private float velocity;
    private float originalScale;
    private float originalY;
    
    
    public Transform Target;
    private Transform Player;
    [SerializeField] private GameObject Indicator;
    [SerializeField] private float SmoothTime;
    void Awake()
    {
        mainCamera = Camera.main;
        originalScale = Indicator.transform.localScale.y;
        originalY = Indicator.transform.localPosition.y;
        Player = GetComponentInParent<Player>().transform;
    }
    void Update()
    {
        if (Target)
        {
            UpdateTargetIconPosition();
        }
    }
    
    public void Initialize()
    {
        Indicator.GetComponent<SpriteRenderer>().enabled = false;
    }

    public void setTarget(Transform _target)
    {
        Target = _target;
    }
    
    public void Enable()
    {
        Indicator.GetComponent<SpriteRenderer>().enabled = true;
    }

    private void UpdateTargetIconPosition()
    {
        Vector3 newPos = Target.position;
        newPos = mainCamera.WorldToViewportPoint(newPos);
        //Simple check if the target object is out of the screen or inside
        //Operations if the object is out of the screen
        if (newPos.x > 1 || newPos.y > 1 || newPos.x < 0 || newPos.y < 0)
            m_outOfScreen = true;
        else
            m_outOfScreen = false;
        
        if (m_outOfScreen)
        {
            if(GameManager.Instance.highlight != true)
                Target.gameObject.GetComponent<ItemIndicator>().Disable();
            // TODO change here to adapt the game manager
            var distance = (Target.position - Player.position).magnitude;
            if (distance < 3f)
            {
                Indicator.GetComponent<SpriteRenderer>().enabled = false;
            }else if (distance > 5f)
            {
                if (!Indicator.GetComponent<SpriteRenderer>().enabled)
                {
                    if (GameManager.Instance.CurrentGameState == GameState.Battle)
                        Indicator.GetComponent<SpriteRenderer>().enabled = false;
                    else
                        Indicator.GetComponent<SpriteRenderer>().enabled = true;
                }
                    var YChange = Mathf.Clamp(distance/12, 0.0f,2.0f);
                var ScaleChange = Mathf.Clamp(4f/(distance*10), 0f, 0.2f);
                var ratio = Mathf.Clamp(100f/(distance), 2f, 4f);
                ScaleChange = Mathf.SmoothDamp(Indicator.transform.localScale.y, ScaleChange + originalScale, ref velocity,
                    SmoothTime);
                Indicator.transform.localScale = new Vector3((originalScale + ScaleChange)/ratio,ScaleChange,Indicator.transform.localScale.z);
                Indicator.transform.localPosition = new Vector3(0,YChange + originalY,0);
                var targetPosLocal = Player.InverseTransformPoint(Target.position);
                var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.y) * Mathf.Rad2Deg;
                transform.eulerAngles = new Vector3(0, 0, targetAngle);
            }
        }
        else
        {
            if (GameManager.Instance.CurrentGameState != GameState.Battle) {
                Target.gameObject.GetComponent<ItemIndicator>().Enable();
            }
            var distance = (Target.position - Player.position).magnitude;
            if (distance < 3f)
            {
                Indicator.GetComponent<SpriteRenderer>().enabled = false;
            }else if (distance > 5f)
            {
                if (!Indicator.GetComponent<SpriteRenderer>().enabled)
                {
                    if (GameManager.Instance.CurrentGameState == GameState.Battle)
                        Indicator.GetComponent<SpriteRenderer>().enabled = false;
                    else
                        Indicator.GetComponent<SpriteRenderer>().enabled = true;
                }
                Indicator.GetComponent<SpriteRenderer>().enabled = false;
                var YChange = Mathf.Clamp(distance/12, 0.0f,2.0f);
                var ScaleChange = Mathf.Clamp(4f/(distance*10), 0f, 0.2f);
                var ratio = Mathf.Clamp(100f/(distance), 2f, 4f);
                ScaleChange = Mathf.SmoothDamp(Indicator.transform.localScale.y, ScaleChange + originalScale, ref velocity,
                    SmoothTime);
                Indicator.transform.localScale = new Vector3((originalScale + ScaleChange)/ratio,ScaleChange,Indicator.transform.localScale.z);
                Indicator.transform.localPosition = new Vector3(0,YChange + originalY,0);
                var targetPosLocal = Player.InverseTransformPoint(Target.position);
                var targetAngle = -Mathf.Atan2(targetPosLocal.x, targetPosLocal.y) * Mathf.Rad2Deg;
                transform.eulerAngles = new Vector3(0, 0, targetAngle);
            }
        } 
    }
    
}