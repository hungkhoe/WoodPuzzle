using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountDownItem : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    Text countDownText;

    int countDownTime;
    void Start()
    {
        SetCountDownTime();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetCountDownTime()
    {
        countDownTime = 31;       
        StartCoroutine(StartCountDown());
    }

    IEnumerator StartCountDown()
    {        
        while (countDownTime > 0)
        {
            countDownTime--;
            countDownText.text = countDownTime.ToString();         
            yield return new WaitForSeconds(1);
        }
        Destroy(this.gameObject);
    }
}
