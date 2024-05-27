using TMPro;
using UnityEngine;
using VInspector;

public class ShopItem : Interactable
{
    [Tab("Main")]
    public float Price;

    [Variants("Extra Film", "Render Speed", "Adrenaline Shot", "Smoke Bomb")]
    public string Item;


    [Header("Other")]
    public GameObject collectParticle;
    public GameObject MoneyText;

    
    [Tab("Settings")]
    public Outline outline;
    public float   TargetOutline = 0;
    public float   BlendOutline;
    
    public PlayerMovement playerMovement;
    public PlayerStats playerStats;
    public PlayerSFX playerSFX;
    public CameraManager cameraManager;


    void Awake()
    {
        outline = GetComponent<Outline>();
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerStats = FindAnyObjectByType<PlayerStats>();
        playerSFX = FindAnyObjectByType<PlayerSFX>();
        cameraManager = FindAnyObjectByType<CameraManager>();


        outline = gameObject.AddComponent<Outline>();
        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineWidth = 0;
    }

    void Update()
    {
        outline.OutlineWidth = Mathf.SmoothDamp(outline.OutlineWidth, TargetOutline, ref BlendOutline, 0.075f);
    }


    public override void MouseOver()
    {
        TargetOutline = 20f;
    }

    public override void MouseExit()
    {
        TargetOutline = 0f;
    }


    public override void InteractStart()
    {
        if(playerStats.Money < Price)
        {
            Instantiate(collectParticle, transform.position, Quaternion.identity);
            TextMeshPro NoDoshText = Instantiate(MoneyText, transform.position, Quaternion.identity).GetComponent<TextMeshPro>();
            int RandomText = Random.Range(1,11);
            switch(RandomText)
            {
                case 1: NoDoshText.text = "Not Enough Money!"; break;
                case 2: NoDoshText.text = "Not Enough Money!"; break;
                case 3: NoDoshText.text = "Not Enough Money!"; break;
                case 4: NoDoshText.text = "Not Enough Dosh!"; break;
                case 5: NoDoshText.text = "Not Enough Moolah!"; break;
                case 6: NoDoshText.text = "Not Enough Munayy!"; break;
                case 7: NoDoshText.text = "Not Enough Quid!"; break;
                case 8: NoDoshText.text = "Not Enough GBP!"; break;
                case 9: NoDoshText.text = "Not Enough Pounds!"; break;
                case 10: NoDoshText.text = "Not Enough Dough!"; break;
                default: NoDoshText.text = "Not Enough Money!"; break;
            }
            return;
        }

        if(Item == "Extra Film")       playerStats.extraFilm++;
        if(Item == "Render Speed")     playerStats.renderSpeed++;
        if(Item == "Adrenaline Shot")  playerStats.adrenalineShot++;
        if(Item == "Smoke Bomb")       playerStats.SmokeBomb++;

        playerStats.ObtainMoney(-Price, true);
        Visualisers[] visualisers = FindObjectsByType<Visualisers>(FindObjectsSortMode.None);
        foreach(Visualisers visualiser in visualisers)
        {
            visualiser.UpdateText();
        }

        Instantiate(collectParticle, transform.position, Quaternion.identity);
        TextMeshPro PurchaseText = Instantiate(MoneyText, transform.position, Quaternion.identity).GetComponent<TextMeshPro>();
        PurchaseText.text = "-" + Price;
    }

}
