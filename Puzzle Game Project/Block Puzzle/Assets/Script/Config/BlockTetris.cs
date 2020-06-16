using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockTetris : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField]
    int positionX;
    [SerializeField]
    int positionY;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetPositionX()
    {
        return positionX;
    }

    public int GetPositionY()
    {
        return positionY;
    }

    public void RotateBlock()
    {
        // rotate -90 degree
        int tempX = positionX;
        positionX = positionY;
        positionY = tempX * (-1);
    }
}
