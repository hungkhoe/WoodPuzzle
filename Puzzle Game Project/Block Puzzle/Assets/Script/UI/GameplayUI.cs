using Puzzle.Block.GameManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Block.GameUIManager
{
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
        Text scoreTimeMode;
        [SerializeField]
        Text bestRecordTextTimeMode;
        [SerializeField]
        Text currentTimeCountText;
        [SerializeField]
        Sprite[] speakerSprite;
        [SerializeField]
        Sprite[] musicSprite;
        [SerializeField]
        Image[] energyBar;
        [SerializeField]
        Image procesTimeBar;
        [SerializeField]
        GameObject levelModeUIPanel;
        [SerializeField]
        GameObject timeModeUIPanel;
        private void Awake()
        {
            instance = this;
        }
        void Start()
        {          
            if (GameManager.GameManager.Instance.GetTimeModeBool())
            {
                timeModeUIPanel.gameObject.SetActive(true);
                StartCoroutine(BarTimeCouroutine());
            }
            else
            {
                levelModeUIPanel.gameObject.SetActive(true);
            }
            UpdateScoreBoard();
            UpdateEnergyBar();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateScoreBoard()
        {
            if(GameManager.GameManager.Instance.GetTimeModeBool())
            {
                scoreTimeMode.text = GameManager.GameManager.Instance.GetScore().ToString();
            }            
            else
            {
                scoreCountText.text = GameManager.GameManager.Instance.GetScore().ToString();
                bellCountText.text = GameManager.GameManager.Instance.GetBellCount().ToString();
                heartCountText.text = GameManager.GameManager.Instance.GetHeartCount().ToString();
            }
        }

        public void PauseButton()
        {
#if UNITY_EDITOR
            pausePanel.SetActive(!pausePanel.activeSelf);
            if (pausePanel.activeSelf)
            {
                Time.timeScale = 0;
                GameManager.GameManager.Instance.DisableColliderOnBlock();
            }
            else
            {
                Time.timeScale = 1;
                //GameManager.GameManager.Instance.EnableColliderOnBlock();
                GameManager.GameManager.Instance.CheckTetrisAvailable();
            }
#else
       if (Input.touchCount == 1)
        {
            pausePanel.SetActive(!pausePanel.activeSelf);
            if (pausePanel.activeSelf)
            {
                Time.timeScale = 0;
                GameManager.GameManager.Instance.DisableColliderOnBlock();
            }
            else
            {
                Time.timeScale = 1;
               GameManager.GameManager.Instance.CheckTetrisAvailable();
            }
        }      
#endif
        }

        public void SoundButton()
        {

        }

        public void SpeakerButton()
        {

        }

        public void ReturnToMenu()
        {
            GameManager.GameManager.Instance.ReturnMainMenu();
        }

        public void ReplayButton()
        {
            pausePanel.SetActive(!pausePanel.activeSelf);
            if (pausePanel.activeSelf)
            {
                Time.timeScale = 0;
                GameManager.GameManager.Instance.DisableColliderOnBlock();
            }
            else
            {
                Time.timeScale = 1;               
            }
            GameManager.GameManager.Instance.ReplayResetGame();
        }

        public void RotateBlockButton()
        {
#if UNITY_EDITOR
            GameManager.GameManager.Instance.RotateBlockSkill();
#else
        if (Input.touchCount == 1)
        {
            GameManager.GameManager.Instance.RotateBlockSkill();
            if (GameManager.GameManager.Instance.GetEnergy() >= 7)
            {

            }
        } 
#endif
        }

        public void SameColorButton()
        {
#if UNITY_EDITOR
            GameManager.GameManager.Instance.SameColorSkill();
#else

       if (Input.touchCount == 1)
        {
            GameManager.GameManager.Instance.SameColorSkill();
            if (GameManager.GameManager.Instance.GetEnergy() >= 12)
            {

            }
        }
#endif
        }

        public void WoodenAxeButton()
        {
#if UNITY_EDITOR

            GameManager.GameManager.Instance.WoodenAxeSkill();
#else

        if (Input.touchCount == 1)
        {
            GameManager.GameManager.Instance.WoodenAxeSkill();
            if (GameManager.GameManager.Instance.GetEnergy() >= 10)
            {

            }
        }
#endif
        }

        public void ReplaceBlockButton()
        {
#if UNITY_EDITOR

            GameManager.GameManager.Instance.ReplaceBlockSkill();
#else

        if (Input.touchCount == 1)
        {
            GameManager.GameManager.Instance.ReplaceBlockSkill();
            if (GameManager.GameManager.Instance.GetEnergy() >= 5)
            {

            }
        }  
#endif
        }

        public void UpdateEnergyBar()
        {
            float energy = GameManager.GameManager.Instance.GetEnergy();
            energyBar[(int)EnergyBar.REPLACE_BLOCK].fillAmount = energy / 5;
            energyBar[(int)EnergyBar.ROTATE_BLOCK].fillAmount = energy / 7;
            energyBar[(int)EnergyBar.WOODEN_AXE].fillAmount = energy / 10;
            energyBar[(int)EnergyBar.SAME_COLOR].fillAmount = energy / 12;
        }

        IEnumerator BarTimeCouroutine()
        {
            while(GameManager.GameManager.Instance.GetCurrentTimePlay() > 0)
            {
                procesTimeBar.fillAmount = GameManager.GameManager.Instance.GetCurrentTimePlay() / 180;
                currentTimeCountText.text = GameManager.GameManager.Instance.GetCurrentTimePlay().ToString();
                yield return null;
            }
        }

        public void StartBarTimeCour()
        {
            StartCoroutine(BarTimeCouroutine());
        }
    }
}

