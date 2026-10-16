using UnityEngine;

public class ExplosiveBarrel : MonoBehaviour, IDamageable
{
    public enum BarrelState
    {
        Idle,
        Countdown,
        Exploding
    }
     // How much health is lost per second during countdown
    
    public float CurrentHealth { get; private set; }
    private float maxHealth = 30f;

    [SerializeField] private  float fuseHealthPerSecond = 5f;
    [SerializeField] private TextMesh countdownLabel;

    private BarrelState currentState = BarrelState.Idle;
    
    float TimeToExplode => CurrentHealth / fuseHealthPerSecond;
    
    private void Awake()
    {
        //This is to initialize the gameobject if it is created in the editor. In a real game,
        //you would probably want to configure it through a factory or some other means.
        Configure(maxHealth, fuseHealthPerSecond);
    }

    public void Configure(float health, float fuse)
    {
        maxHealth = Mathf.Max(1f, health);
        fuseHealthPerSecond = Mathf.Max(0f, fuse);
        CurrentHealth = maxHealth;
    }
    

    //A very simple state machine. This could be replaced with a more robust implementation if needed.
    //But it is good for quick prototyping and simple state management.
    private void Update()
    {
        switch (currentState)
        {
            case BarrelState.Idle:
                IdleState();
                break;
            case BarrelState.Countdown:
                CountdownState();
                break;
            case BarrelState.Exploding:
                Explode();
                break;
        }
    }

    void IdleState()
    {
        //Dont really do anything here, just wait for damage to be taken
    }

    void CountdownState()
    {
        CurrentHealth -= (fuseHealthPerSecond * Time.deltaTime);
        if (CurrentHealth <= 0f)
        {
            currentState = BarrelState.Exploding;
            return;
        }
        
        ShowCountdown();
    }

    void Explode()
    {
        Debug.Log("Barrel exploded!");
        currentState = BarrelState.Idle;
        HideCountdown();
        
        
        //Do explosion logic here, like dealing damage to nearby objects, playing effects, etc.
        
        //For now, just destroy the barrel
        Destroy(gameObject);
    }
    
    public void TakeDamage(float amount)
    {
        if (amount <= 0f || CurrentHealth <= 0f)
            return;

        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        if (currentState == BarrelState.Idle)
        {
            currentState = BarrelState.Countdown;
        }
        
    }

    private void ShowCountdown()
    {
        countdownLabel.gameObject.SetActive(true);
        countdownLabel.text = Mathf.Max(0f, TimeToExplode).ToString("0.0");
    }

    private void HideCountdown()
    {
        countdownLabel.gameObject.SetActive(false);
    }

}
