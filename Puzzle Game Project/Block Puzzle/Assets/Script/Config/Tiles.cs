using Puzzle.Block.GameManager;
using Puzzle.Game.Definition;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Puzzle.Game.Tiles
{
    public class Tiles : MonoBehaviour
    {
        enum LavaStoneStatus
        {
            FIRST_CRACK,
            SECOND_CRACK
        }

        Definition.TilesID tilesID;
        Definition.BlockColorID currentColor;
        int positionX;
        int positionY;
        int lavaStoneHp;
        int bombTurnToDestroy;
        [SerializeField]
        Sprite[] lavaStoneStatus;
        [SerializeField]
        Sprite bombMineSprite;
        void Start()
        {
            
        }

        public void SetUpLavaStone()
        {
            tilesID = Definition.TilesID.LAVA_STONE;
            currentColor = BlockColorID.LAVA_STONE;
            lavaStoneHp = 2;
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = lavaStoneStatus[(int)LavaStoneStatus.FIRST_CRACK];
            transform.GetChild(0).gameObject.SetActive(true);
        }

        public void SetUpBombTile()
        {
            tilesID = Definition.TilesID.BOMB_MINE;
            bombTurnToDestroy = 9;
            this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = bombMineSprite;
            transform.GetChild(0).gameObject.SetActive(true);
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

        public void DecreaseLavaStoneLife()
        {
            lavaStoneHp--;
            if(lavaStoneHp == 0 )
            {
                tilesID = Definition.TilesID.EMPTY;
                transform.GetChild(0).gameObject.SetActive(false);
            }
            else if (lavaStoneHp == 1)
            {
                this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = lavaStoneStatus[(int)LavaStoneStatus.SECOND_CRACK];
                currentColor = BlockColorID.LAVA_STONE_FIRST_CRACK;
            }
        }

        public void ResetBreakEffectLava()
        {
            if(tilesID == Definition.TilesID.LAVA_STONE)
            {
                if (lavaStoneHp == 2)
                {
                    this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = lavaStoneStatus[(int)LavaStoneStatus.FIRST_CRACK];
                }
                else
                {
                    this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = lavaStoneStatus[(int)LavaStoneStatus.SECOND_CRACK];
                }
            }
        }

        public void ResetBreakEffectBombMine()
        {
            if (tilesID == Definition.TilesID.BOMB_MINE)
            {
                this.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = bombMineSprite;
            }
        }

        public void DecreaseTurnDestroy()
        {
            bombTurnToDestroy--;
            if(bombTurnToDestroy == 0)
            {
                print("lose game");
            }
        }

        public int GetHPLava()
        {
            return lavaStoneHp;
        }

    }
}


