using Puzzle.Block.GameManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpRevive : MonoBehaviour
{
    // Start is called before the first frame update   
    void Start()
    {
        Time.timeScale = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CloseButton()
    {
        // Close Button -> Retry Game Over
        Destroy(this.gameObject);
    }

    public void WatchAdsButton()
    {
        // TODO Admob Reward Video
        if(GameManager.Instance.GetTimeModeBool())
        {
            GameManager.Instance.WatchAdsNoTime();
            Destroy(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}
