using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzle.Game.Tiles
{
    public class Tiles : MonoBehaviour
    {
        // Start is called before the first frame update
        Definition.TilesID tilesID;
        Definition.BlockColorID currentColor;
        int positionX;
        int positionY;
        void Start()
        {

        }

        void SetUpTiles()
        {
            tilesID = (int)Definition.TilesID.EMPTY;
        }

        public void SetTitlesID(Definition.TilesID _tilesID)
        {
            tilesID = _tilesID;
        }

        public Definition.TilesID GetTilesID()
        {
            return tilesID;
        }

        public void SetPosition(int _x, int _y)
        {
            positionX = _x;
            positionY = _y;
        }

        public int GetPositionX()
        {
            return positionX;
        }

        public int GetPositionY()
        {
            return positionY;
        }    

        public void SetCurrentColor(Definition.BlockColorID _color)
        {
            currentColor = _color;
        }

        public Definition.BlockColorID GetCurrentColor()
        {
            return currentColor;
        }

    }
}


