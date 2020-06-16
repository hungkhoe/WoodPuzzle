using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Block.ScorePopUp
{
    public class PopUpScore : MonoBehaviour
    {
        // Start is called before the first frame update
        int score;
        float delay = 0.3f;
        void Start()
        {
            Destroy(gameObject.transform.parent.gameObject, this.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).length);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetScore(int _score)
        {
            this.gameObject.GetComponent<Text>().text ="+" + _score.ToString();
        }
    }
}

