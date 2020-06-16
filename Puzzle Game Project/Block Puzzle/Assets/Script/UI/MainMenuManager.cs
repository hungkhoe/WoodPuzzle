using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Puzzle.Block.MainMenu
{ 
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField]
        string levelModeSceneName;
        
        // Start is called before the first frame update
        void Start()
        {
           
        }

        // Update is called once per frame
        void Update()
        {

        }

        // =========== Hàm cho các Button MainMenu ==================
        public void LevelModeButton()
        {
            SceneManager.LoadScene(levelModeSceneName);
        }

        public void HiveModeButton()
        {

        }

        public void TimeModeButton()
        {

        }

        public void LeaderBoardButton()
        {

        }

        public void RateUsButton()
        {

        }

        public void SharingButton()
        {

        }

        public void SettingButton()
        {

        }
    }
}

