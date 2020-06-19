using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassInformation : MonoBehaviour
{
    // Start is called before the first frame update
    static PassInformation instance;
    public static PassInformation Instance
    {
        get
        {
            return instance;
        }
    }
    bool isTimeMode = false;
    void Start()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetTimeMode()
    {
        isTimeMode = true;
    }

    public bool GetTimeMode()
    {
        return isTimeMode;
    }

    public void DestroyThisObject()
    {
        Destroy(this.gameObject);
    }
}
