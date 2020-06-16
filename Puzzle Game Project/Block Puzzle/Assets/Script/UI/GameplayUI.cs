using Puzzle.Block.GameManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameplayUI : MonoBehaviour
{
    enum EnergyBar
    {
        REPLACE_BLOCK,
        WOODEN_AXE,
        ROTATE_BLOCK,
        SAME_COLOR
    }
    static GameplayUI instance;
    public static GameplayUI Instance
    {
        get
        {
            return instance;
        }
    }

    // Start is called before the first frame update
    [SerializeField]
    GameObject pausePanel;
    [SerializeField]
    Text scoreCountText;
    [SerializeField]
    Text bellCountText;
    [SerializeField]
    Text heartCountText;

    [SerializeField]
    Sprite[] speakerSprite;
    [SerializeField]
    Sprite[] musicSprite;

    [SerializeField]
    Image[] energyBar;
    private void Awake()
    {      
        instance = this;
    }
    void Start()
    {
        UpdateScoreBoard();
        UpdateEnergyBar();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateScoreBoard()
    {
        scoreCountText.text = GameManager.Instance.GetScore().ToString();
        bellCountText.text = GameManager.Instance.GetBellCount().ToString();
        heartCountText.text = GameManager.Instance.GetHeartCount().ToString();
    }

    public void PauseButton()
    {
        pausePanel.SetActive(!pausePanel.activeSelf);
        if(pausePanel.activeSelf)
        {         
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

    public void SoundButton()
    {

    }

    public void SpeakerButton()
    {

    }

    public void ReturnToMenu()
    {
        GameManager.Instance.ReturnMainMenu();
    }

    public void ReplayButton()
    {
        PauseButton();
        GameManager.Instance.ReplayResetGame();
    }

    public void RotateBlockButton()
    {
        GameManager.Instance.RotateBlockSkill();
        if (GameManager.Instance.GetEnergy() >= 7)
        {
            
        }
        
    }

    public void SameColorButton()
    {
        GameManager.Instance.SameColorSkill();
        if (GameManager.Instance.GetEnergy() >= 12)
        {
            
        }
    }

    public void WoodenAxeButton()
    {
        GameManager.Instance.WoodenAxeSkill();
        if (GameManager.Instance.GetEnergy() >= 10)
        {
           
        }

    }

    public void ReplaceBlockButton()
    {
        GameManager.Instance.ReplaceBlockSkill();
        if (GameManager.Instance.GetEnergy() >= 5)
        {
            
        }
    }

    public void UpdateEnergyBar()
    {
        float energy = GameManager.Instance.GetEnergy();
        energyBar[(int)EnergyBar.REPLACE_BLOCK].fillAmount = energy / 5;
        energyBar[(int)EnergyBar.ROTATE_BLOCK].fillAmount = energy / 7;
        energyBar[(int)EnergyBar.WOODEN_AXE].fillAmount = energy / 10;
        energyBar[(int)EnergyBar.SAME_COLOR].fillAmount = energy / 12;
    }
}
