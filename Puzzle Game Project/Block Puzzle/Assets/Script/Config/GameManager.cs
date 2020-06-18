using Puzzle.Block.ScorePopUp;
using Puzzle.Block.TertrisBlock;
using Puzzle.Game.Definition;
using Puzzle.Game.Tiles;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

namespace Puzzle.Block.GameManager
{    public class GameManager : MonoBehaviour
    {
        static GameManager instance;
        public static GameManager Instance
        {
            get
            {
                return instance;
            }
        }
        // Start is called before the first frame update
        private const int LEVEL_MODE_BOARD_WIDTH = 10;
        private const int LEVEL_MODE_BOARD_HEIGHT = 10;
        private const int TOTAL_CLASSIC_BLOCK_NUMBER = 16;

        [SerializeField]
        GameObject tilesPrefab;
        [SerializeField]
        GameObject tilesBlockPrefab;
        [SerializeField]
        GameObject rocket1Prefab;
        [SerializeField]
        GameObject rocket2Prefab;
        [SerializeField]
        GameObject rocket3Prefab;
        [SerializeField]
        GameObject[] blockPrefab;
        [SerializeField]
        GameObject boardPanel;
        [SerializeField]
        GameObject gamePlayUIPanel;
        [SerializeField]
        GameObject blackImage;
        [SerializeField]
        GameObject scorePopUp;
        [SerializeField]
        GameObject blockSpawnPanel;
        [SerializeField]
        GameObject[] startSpawnPos;
        [SerializeField]
        GameObject[] blockSpawnLocation;
        [SerializeField]
        Sprite[] listBlockColor;
       

        Tiles[,] boardMatrix;
        List<Tiles> tilesColumnToDestroy;
        List<Tiles> tilesRowToDestroy;
        List<Tiles> tilesToDestroy;
        List<Tiles> previousTilesToDestroy;
        List<Tiles> rocketDestroyList;
        List<GameObject> currentBlockList;
        bool isRowMatch;
        bool isColumnMatch;
        int totalTilesEmpty;

        int totalLineDestroy;
        int totalTetrisSpawn;
        int scoreToPlus;
        int energyCount;
        int scoreCount;
        int bellCount;
        int heartCount;

        bool isUsingWoodenSkill;
        bool isUsingSameColor;
        bool isReplaceBlockSkill;

        [SerializeField]
        GameObject tileMap;
        [SerializeField]
        GameObject blockSpawn;
        void Awake()
        {           
            instance = this;
        }

        void Start()
        {
            SetUpGame();
            SpawnNewRandomBlock();
            ScaleGamePlay();
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetMouseButtonDown(1))
            {
                energyCount += 10;
                GameplayUI.Instance.UpdateEnergyBar();
            }
        }

        void SetUpGame()
        {
            // Tạo các cột title cho map từ tọa độ 0,0 -> 10,10
            totalTilesEmpty = LEVEL_MODE_BOARD_HEIGHT * LEVEL_MODE_BOARD_WIDTH;
            boardMatrix = new Tiles[LEVEL_MODE_BOARD_WIDTH, LEVEL_MODE_BOARD_HEIGHT];
            // Tìm độ cao và độ rộng để dịch sau mỗi block
            float paddingWidth = tilesPrefab.GetComponent<SpriteRenderer>().size.x;
            float paddingHeight = tilesPrefab.GetComponent<SpriteRenderer>().size.y;
            // Generate map 
            for (int i = 0; i < LEVEL_MODE_BOARD_WIDTH; i++)
            {
                for (int j = 0; j < LEVEL_MODE_BOARD_HEIGHT; j++)
                {
                    GameObject tempTiles;
                    tempTiles = Instantiate(tilesPrefab, boardPanel.transform);
                    // Tính pixel scale vì đổi từ 100 -> 240 trong inspector của texture
                    float ratioScalePixelPerUnits = 100f / 240f;
                    // Di chuyển tiles vào đúng vị trí
                    tempTiles.transform.Translate(i * paddingWidth * ratioScalePixelPerUnits, j * paddingWidth * ratioScalePixelPerUnits, 0);
                    // Đổi tên tiles cho sang theo tọa độ
                    tempTiles.name = "( " + i + ", " + j + " )";
                    boardMatrix[i, j] = tempTiles.GetComponent<Tiles>();
                    boardMatrix[i, j].SetPosition(i, j);
                    // Instantie block sẵn trong bàn
                    GameObject tempPrefabTile = Instantiate(tilesBlockPrefab, tempTiles.transform);
                    tempPrefabTile.transform.position = new UnityEngine.Vector3(tempTiles.transform.position.x, tempTiles.transform.position.y);
                    tempPrefabTile.SetActive(false);      

                    //TODO : Code Test Lava Stone
                    if(i == 5 && j == 5)
                    {
                        boardMatrix[i, j].SetUpLavaStone();
                    }
                    if (i == 1 && j == 1)
                    {
                        boardMatrix[i, j].SetUpLavaStone();
                    }
                    if (i == 8 && j == 7)
                    {
                        boardMatrix[i, j].SetUpLavaStone();
                    }
                    if (i == 1 && j == 8)
                    {
                        boardMatrix[i, j].SetUpLavaStone();
                    }
                }
            }

            // Tạo list các cột block để destroy
            tilesColumnToDestroy = new List<Tiles>();
            tilesRowToDestroy = new List<Tiles>();
            tilesToDestroy = new List<Tiles>();
            previousTilesToDestroy = new List<Tiles>();
            currentBlockList = new List<GameObject>();
            rocketDestroyList = new List<Tiles>();
            // Set up score game
            scoreCount = 0;
            bellCount = 0;
            heartCount = 0;
            energyCount = 0;

            //Set time
            Time.timeScale = 1;
        }

        public void CheckBlockMatches(List<int> blockRowToCheck, List<int> blockColumnToCheck, int blockCount, UnityEngine.Vector3 blockPos)
        {
            // Check tọa độ X 
            for (int i = 0; i < blockRowToCheck.Count; i++)
            {
                isColumnMatch = true;
                int currentX = blockRowToCheck[i];

                // kiểm tra xem tọa độ x nào có cột Y có thể destroy được
                for (int j = 0; j < LEVEL_MODE_BOARD_HEIGHT; j++)
                {
                    if (boardMatrix[currentX, j].GetTilesID() != Game.Definition.TilesID.EMPTY)
                    {
                        tilesColumnToDestroy.Add(boardMatrix[currentX, j]);
                    }
                    else
                    {
                        // nếu 1 block empty break trả false luôn
                        isColumnMatch = false;
                        break;
                    }
                }
                // Nếu return true thì tắt những hàng đấy -> Đoạn này sẽ tạo effect
                if (isColumnMatch)
                {
                    for (int j = 0; j < tilesColumnToDestroy.Count; j++)
                    {
                        tilesToDestroy.Add(tilesColumnToDestroy[j]);
                    }
                    totalLineDestroy++;
                }
                tilesColumnToDestroy.Clear();
            }

            // Check tọa độ Y
            for (int i = 0; i < blockColumnToCheck.Count; i++)
            {
                isRowMatch = true;
                int currentY = blockColumnToCheck[i];

                // kiểm tra xem tọa độ y nào có hàng ngang X có thể destroy được
                for (int j = 0; j < LEVEL_MODE_BOARD_WIDTH; j++)
                {
                    if (boardMatrix[j, currentY].GetTilesID() != Game.Definition.TilesID.EMPTY)
                    {
                        tilesRowToDestroy.Add(boardMatrix[j, currentY]);
                    }
                    else
                    {
                        // nếu 1 block empty break trả false luôn
                        isRowMatch = false;
                        break;
                    }
                }
                // Nếu return true thì tắt những hàng đấy -> Đoạn này sẽ tạo effect
                if (isRowMatch)
                {
                    for (int j = 0; j < tilesRowToDestroy.Count; j++)
                    {
                        tilesToDestroy.Add(tilesRowToDestroy[j]);
                    }
                    totalLineDestroy++;
                }
                tilesRowToDestroy.Clear();

            }

            if (tilesToDestroy.Count > 0)
            {
                // TODO: Tạo effect nổ
                rocketDestroyList.Clear();
                for (int i = 0; i < tilesToDestroy.Count; i++)
                {
                    if (tilesToDestroy[i].GetTilesID() == TilesID.ROCKET_ONE || tilesToDestroy[i].GetTilesID() == TilesID.ROCKET_TWO ||
                        tilesToDestroy[i].GetTilesID() == TilesID.ROCKET_THREE)
                    {                    
                        RocketItemExplode(tilesToDestroy[i].GetTilesID(), tilesToDestroy[i]);
                        tilesToDestroy[i].SetTitlesID(Game.Definition.TilesID.EMPTY);
                        tilesToDestroy[i].transform.GetChild(0).gameObject.SetActive(false);
                    }
                    else if (tilesToDestroy[i].GetTilesID() == TilesID.LAVA_STONE)
                    {
                        tilesToDestroy[i].DecreaseLavaStoneLife();
                    }
                    else
                    {
                        tilesToDestroy[i].SetTitlesID(Game.Definition.TilesID.EMPTY);
                        tilesToDestroy[i].transform.GetChild(0).gameObject.SetActive(false);
                    }                                    
                }
                DestroyRocketListBlock();
                IncreaseScoreByLine(totalLineDestroy);
                GameplayUI.Instance.UpdateEnergyBar();
                tilesToDestroy.Clear();
                totalLineDestroy = 0;
                int totalScore = blockCount + scoreToPlus;
                GameObject tempScorePopUp = Instantiate(scorePopUp, blockPos, UnityEngine.Quaternion.identity, gamePlayUIPanel.transform);
                tempScorePopUp.transform.SetAsFirstSibling(); 
                tempScorePopUp.GetComponentInChildren<PopUpScore>().SetScore(totalScore);
                IncreaseScore(blockCount);
                scoreToPlus = 0;
            }
            else
            {
                IncreaseScore(blockCount);
                GameObject tempScorePopUp = Instantiate(scorePopUp, blockPos, UnityEngine.Quaternion.identity, gamePlayUIPanel.transform);
                tempScorePopUp.transform.SetAsFirstSibling();
                tempScorePopUp.GetComponentInChildren<PopUpScore>().SetScore(blockCount);
            }
        }

        public void SpawnNewRandomBlock(int blockIndex = 5)
        {            
            totalTetrisSpawn--;
            if (totalTetrisSpawn <= 0)
            {
                if (blockIndex < 5)
                {
                    currentBlockList.RemoveAt(0);
                }
                for (int i = 0; i < 3; i++)
                {
                    //Random loại block classsic
                    int random = Random.Range(0, 16);
                    GameObject tempBlock = Instantiate(blockPrefab[random], blockSpawnPanel.transform);
                    // Chọn địa điểm spawn cho block
                    tempBlock.transform.position = new UnityEngine.Vector3(startSpawnPos[i].transform.position.x, startSpawnPos[i].transform.position.y, 1);
                    // Random màu cho block
                    random = Random.Range(0, 7);
                    tempBlock.GetComponent<TetrisBlock>().SetUpTetrisBlock(listBlockColor[random], i, blockSpawnLocation[i].transform.position);
                    currentBlockList.Add(tempBlock);
                }
                totalTetrisSpawn = 3;
            }
            else
            {                
                if (blockIndex < 5)
                {
                    currentBlockList.RemoveAt(blockIndex);
                    for (int i = 0; i < currentBlockList.Count; i++)
                    {
                        currentBlockList[i].GetComponent<TetrisBlock>().SetSpawnPos(i);
                        currentBlockList[i].GetComponent<TetrisBlock>().SetNewPosSpawn(blockSpawnLocation[i].transform.position);
                        currentBlockList[i].GetComponent<TetrisBlock>().MoveToSpawn();
                    }
                    //Random loại block classsic
                    int random = Random.Range(0, 16);
                    GameObject tempBlock = Instantiate(blockPrefab[random], blockSpawnPanel.transform);
                    // Chọn địa điểm spawn cho block
                    tempBlock.transform.position = new UnityEngine.Vector3(startSpawnPos[2].transform.position.x, startSpawnPos[2].transform.position.y, 1);
                    // Random màu cho block
                    random = Random.Range(0, 7);
                    tempBlock.GetComponent<TetrisBlock>().SetUpTetrisBlock(listBlockColor[random], 2, blockSpawnLocation[2].transform.position);
                    currentBlockList.Add(tempBlock);
                }
                totalTetrisSpawn++;
            }
            CheckTetrisAvailable();
        }

        public void IncreaseScore(int _score)
        {
            scoreCount += _score;
        }

        public void IncreaseEnergy(int _energy)
        {
            energyCount += _energy;
        }

        public int GetScore()
        {
            return scoreCount;
        }

        public int GetBellCount()
        {
            return bellCount;
        }

        public int GetHeartCount()
        {
            return heartCount;
        }

        public void CheckBlockMatchesEffect(List<int> blockRowToCheck, List<int> blockColumnToCheck)
        {
            ResetBreakEffect();
            // Check tọa độ X 
            for (int i = 0; i < blockRowToCheck.Count; i++)
            {
                isColumnMatch = true;
                int currentX = blockRowToCheck[i];

                // kiểm tra xem tọa độ x nào có cột Y có thể destroy được
                for (int j = 0; j < LEVEL_MODE_BOARD_HEIGHT; j++)
                {
                    if (boardMatrix[currentX, j].transform.GetChild(0).gameObject.activeSelf)
                    {
                        tilesColumnToDestroy.Add(boardMatrix[currentX, j]);
                    }
                    else
                    {
                        // nếu 1 block empty break trả false luôn
                        isColumnMatch = false;
                        break;
                    }
                }
                // Nếu return true thì tắt những hàng đấy -> Đoạn này sẽ tạo effect
                if (isColumnMatch)
                {
                    for (int j = 0; j < tilesColumnToDestroy.Count; j++)
                    {
                        tilesToDestroy.Add(tilesColumnToDestroy[j]);
                    }
                }
                tilesColumnToDestroy.Clear();
            }

            // Check tọa độ Y
            for (int i = 0; i < blockColumnToCheck.Count; i++)
            {
                isRowMatch = true;
                int currentY = blockColumnToCheck[i];

                // kiểm tra xem tọa độ y nào có hàng ngang X có thể destroy được
                for (int j = 0; j < LEVEL_MODE_BOARD_WIDTH; j++)
                {
                    if (boardMatrix[j, currentY].transform.GetChild(0).gameObject.activeSelf)
                    {
                        tilesRowToDestroy.Add(boardMatrix[j, currentY]);
                    }
                    else
                    {
                        // nếu 1 block empty break trả false luôn
                        isRowMatch = false;
                        break;
                    }
                }
                if (isRowMatch)
                {
                    for (int j = 0; j < tilesRowToDestroy.Count; j++)
                    {
                        tilesToDestroy.Add(tilesRowToDestroy[j]);
                    }
                }
                tilesRowToDestroy.Clear();
            }
            if (tilesToDestroy.Count > 0)
            {
                previousTilesToDestroy.Clear();
                for (int i = 0; i < tilesToDestroy.Count; i++)
                {
                    previousTilesToDestroy.Add(tilesToDestroy[i]);                    
                    //TODO: đổi sang effect vỡ
                    tilesToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = listBlockColor[7];
                }
                tilesToDestroy.Clear();
                totalLineDestroy = 0;
            }
        }

        public void ResetBreakEffect()
        {
            if (previousTilesToDestroy.Count > 0)
            {
                for (int i = 0; i < previousTilesToDestroy.Count; i++)
                {
                    if (previousTilesToDestroy[i].GetTilesID() != Game.Definition.TilesID.EMPTY)
                    {
                        if(previousTilesToDestroy[i].GetTilesID() == TilesID.LAVA_STONE)
                        {
                            previousTilesToDestroy[i].ResetBreakEffectLava();
                        }
                        else if (previousTilesToDestroy[i].GetTilesID() == TilesID.BOMB_MINE)
                        {
                            previousTilesToDestroy[i].ResetBreakEffectBombMine();
                        }
                        else
                        {
                            previousTilesToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = listBlockColor[(int)previousTilesToDestroy[i].GetCurrentColor()];
                        }                       
                    }
                }
            }
        }

        void IncreaseScoreByLine(int _totalLine)
        {
            switch (_totalLine)
            {
                case 7:
                    IncreaseScore(70);
                    IncreaseEnergy(7);
                    scoreToPlus = 70;
                    break;
                case 6:
                    IncreaseScore(70);
                    IncreaseEnergy(7);
                    scoreToPlus = 70;
                    break;
                case 5:
                    IncreaseScore(70);
                    IncreaseEnergy(7);
                    scoreToPlus = 70;
                    break;
                case 4:
                    IncreaseScore(50);
                    IncreaseEnergy(5);
                    scoreToPlus = 50;
                    break;
                case 3:
                    IncreaseScore(35);
                    IncreaseEnergy(3);
                    scoreToPlus = 35;
                    break;
                case 2:
                    IncreaseScore(25);
                    scoreToPlus = 25;
                    break;
                case 1:
                    IncreaseScore(10);
                    scoreToPlus = 10;
                    break;
                default:
                    break;
            }
        }

        public void DecreaseTotalTiles(int _number)
        {
            totalTilesEmpty -= _number;
        }

        public void IncreaseTotalTiles(int _number)
        {
            totalTilesEmpty += _number;
        }

        void CheckTetrisAvailable()
        {
            int totalDisableBlock = 0;
            for (int i = 0; i < currentBlockList.Count; i++)
            {
                int countAvailable = 0;
                bool isAvailable = false;
                if (totalTilesEmpty < currentBlockList[i].transform.childCount)
                {
                    // disable cai block day vi ko du cho~
                }
                else
                {
                    for (int j = 0; j < LEVEL_MODE_BOARD_HEIGHT; j++)
                    {
                        for (int k = 0; k < LEVEL_MODE_BOARD_WIDTH; k++)
                        {
                            if (boardMatrix[j, k].GetTilesID() == Game.Definition.TilesID.EMPTY)
                            {
                                for (int l = 0; l < currentBlockList[i].transform.childCount; l++)
                                {
                                    if ((j + currentBlockList[i].transform.GetChild(l).GetComponent<BlockTetris>().GetPositionX() >= 0 &&
                                        k + currentBlockList[i].transform.GetChild(l).GetComponent<BlockTetris>().GetPositionY() >= 0) &&
                                        (j + currentBlockList[i].transform.GetChild(l).GetComponent<BlockTetris>().GetPositionX() < 10 &&
                                        k + currentBlockList[i].transform.GetChild(l).GetComponent<BlockTetris>().GetPositionY() < 10))
                                    {
                                        if (boardMatrix[j + currentBlockList[i].transform.GetChild(l).GetComponent<BlockTetris>().GetPositionX(),
                                        k + currentBlockList[i].transform.GetChild(l).GetComponent<BlockTetris>().GetPositionY()].GetTilesID() == Game.Definition.TilesID.EMPTY)
                                        {
                                            countAvailable++;
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                if (countAvailable == currentBlockList[i].transform.childCount)
                                {
                                    isAvailable = true;
                                    countAvailable = 0;
                                    break;
                                }
                                else
                                {
                                    countAvailable = 0;
                                }
                            }
                        }
                        if (isAvailable)
                        {
                            break;
                        }
                    }
                }

                for (int j = 0; j < currentBlockList[i].transform.childCount; j++)
                {
                    if (!isAvailable)
                    {
                        currentBlockList[i].transform.GetChild(j).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
                        currentBlockList[i].GetComponent<BoxCollider2D>().enabled = false;
                    }
                    else
                    {
                        currentBlockList[i].transform.GetChild(j).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                        currentBlockList[i].GetComponent<BoxCollider2D>().enabled = true;
                    }
                }
                if (!isAvailable)
                {
                    totalDisableBlock++;
                }
            }
            if (totalDisableBlock == currentBlockList.Count)
            {
                print("game over");
            }
        }

        public void ReplayResetGame()
        {
            scoreCount = 0;
            bellCount = 0;
            heartCount = 0;
            energyCount = 0;
            GameplayUI.Instance.UpdateScoreBoard();
            isUsingWoodenSkill = false;
            isUsingSameColor = false;
            isReplaceBlockSkill = false;

            for (int i = 0; i < LEVEL_MODE_BOARD_WIDTH; i++)
            {
                for (int j = 0; j < LEVEL_MODE_BOARD_HEIGHT; j++)
                {                   
                    if(boardMatrix[i,j].GetTilesID() != TilesID.NORMAL_BLOCK && boardMatrix[i,j].GetTilesID() != TilesID.EMPTY)
                    {
                        if(boardMatrix[i,j].GetTilesID() == TilesID.ROCKET_ONE || boardMatrix[i, j].GetTilesID() == TilesID.ROCKET_TWO
                            || boardMatrix[i, j].GetTilesID() == TilesID.ROCKET_THREE)
                        Destroy(boardMatrix[i, j].transform.GetChild(1).gameObject);
                    }
                    boardMatrix[i, j].transform.GetChild(0).gameObject.SetActive(false);
                    boardMatrix[i, j].SetTitlesID(Game.Definition.TilesID.EMPTY);
                    // TODO CODE TEST LAVA
                    if (i == 5 && j == 5)
                    {
                        boardMatrix[i, j].SetUpLavaStone();
                    }
                    if (i == 1 && j == 1)
                    {
                        boardMatrix[i, j].SetUpLavaStone();
                    }
                    if (i == 8 && j == 7)
                    {
                        boardMatrix[i, j].SetUpLavaStone();
                    }
                    if (i == 1 && j == 8)
                    {
                        boardMatrix[i, j].SetUpLavaStone();
                    }
                }
            }
       


            StartCoroutine(ResetGame());
        }

        public void ReturnMainMenu()
        {
            StartCoroutine(ReturnToMainMenu());
        }

        IEnumerator ResetGame()
        {
            blackImage.SetActive(true);
            Image blackImageComp = blackImage.GetComponent<Image>();
            while (blackImageComp.color.a < 1)
            {
                blackImageComp.color = new Color(0, 0, 0, blackImageComp.color.a + 0.15f);
                yield return null;

            }

            for(int i = 0; i < currentBlockList.Count;i++)
            {
                Destroy(currentBlockList[i].gameObject);
            }

            currentBlockList.Clear();
            totalTetrisSpawn = 0;
            SpawnNewRandomBlock();

            while (blackImageComp.color.a > 0)
            {
                blackImageComp.color = new Color(0, 0, 0, blackImageComp.color.a - 0.15f);
                yield return null;
            }
            blackImage.SetActive(false);
        }

        IEnumerator ReturnToMainMenu()
        {
            blackImage.SetActive(true);
            Image blackImageComp = blackImage.GetComponent<Image>();
            while (blackImageComp.color.a < 1)
            {
                blackImageComp.color = new Color(0, 0, 0, blackImageComp.color.a + 0.2f);
                yield return null;
            }
            UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        }

        public void RotateBlockSkill()
        {
            for(int i = 0; i < currentBlockList.Count; i++)
            {
                currentBlockList[i].transform.Rotate(0, 0, -90);
               // currentBlockList[i].transform.rotation = UnityEngine.Quaternion.Euler(UnityEngine.Vector3.forward * 90);
                currentBlockList[i].GetComponent<TetrisBlock>().RotateBlockZasis();
            }
            CheckTetrisAvailable();
        }

        public void WoodenAxeSkill()
        {
            isUsingWoodenSkill = !isUsingWoodenSkill;
            if(isUsingWoodenSkill)
            {
                StartCoroutine(WoodenAxeSkillCour());
            }
            isUsingSameColor = false;
        }

        IEnumerator WoodenAxeSkillCour()
        {
            List<Tiles> listToDestroy = new List<Tiles>();
            GameObject previousObject = this.gameObject;
            while (isUsingWoodenSkill)
            {         
                if(Time.timeScale == 1)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        UnityEngine.Vector3 mousePos = Input.mousePosition;
                        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
                        //lấy layer của Board
                        int layer = LayerMask.GetMask("Board");
                        // bắn raycast từ tiles đầu tiên -> board               
                        RaycastHit2D hit = Physics2D.Raycast(mousePos, UnityEngine.Vector2.zero, 100, layer);
                        if (hit.collider != null && hit.transform.gameObject.tag == "Tiles")
                        {
                            if (previousObject != hit.transform.gameObject)
                            {
                                if (listToDestroy.Count > 0)
                                {
                                    for (int i = 0; i < listToDestroy.Count; i++)
                                    {
                                        if (listToDestroy[i].GetTilesID() == TilesID.NORMAL_BLOCK)
                                        {
                                            listToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                                        }
                                        else if (listToDestroy[i].GetTilesID() == TilesID.ROCKET_ONE ||
                                           listToDestroy[i].GetTilesID() == TilesID.ROCKET_TWO ||
                                           listToDestroy[i].GetTilesID() == TilesID.ROCKET_THREE)
                                        {
                                            listToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                                        }
                                        else if(listToDestroy[i].GetTilesID() == TilesID.LAVA_STONE)
                                        {
                                            listToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                                        }
                                        else if (listToDestroy[i].GetTilesID() == TilesID.BOMB_MINE)
                                        {
                                            listToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                                        }
                                        else if (listToDestroy[i].GetTilesID() == TilesID.EMPTY)
                                        {
                                            listToDestroy[i].transform.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                                        }
                                    }
                                    listToDestroy.Clear();
                                }
                                previousObject = hit.transform.gameObject;
                                int currentX = hit.transform.GetComponent<Tiles>().GetPositionX();
                                int currentY = hit.transform.GetComponent<Tiles>().GetPositionY();
                                listToDestroy.Add(boardMatrix[currentX, currentY]);
                                if (currentY + 1 < 10)
                                {
                                    listToDestroy.Add(boardMatrix[currentX, currentY + 1]);
                                }
                                if (currentY - 1 >= 0)
                                {
                                    listToDestroy.Add(boardMatrix[currentX, currentY - 1]);
                                }
                                if (currentX - 1 >= 0)
                                {
                                    listToDestroy.Add(boardMatrix[currentX - 1, currentY]);
                                    if (currentY + 1 < 10)
                                    {
                                        listToDestroy.Add(boardMatrix[currentX - 1, currentY + 1]);
                                    }
                                    if (currentY - 1 >= 0)
                                    {
                                        listToDestroy.Add(boardMatrix[currentX - 1, currentY - 1]);
                                    }
                                }
                                if (currentX + 1 < 10)
                                {
                                    listToDestroy.Add(boardMatrix[currentX + 1, currentY]);
                                    if (currentY + 1 < 10)
                                    {
                                        listToDestroy.Add(boardMatrix[currentX + 1, currentY + 1]);
                                    }
                                    if (currentY - 1 >= 0)
                                    {
                                        listToDestroy.Add(boardMatrix[currentX + 1, currentY - 1]);
                                    }
                                }
                                for (int i = 0; i < listToDestroy.Count; i++)
                                {
                                    if (listToDestroy[i].GetTilesID() != TilesID.EMPTY)
                                        listToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
                                    else if (listToDestroy[i].GetTilesID() == TilesID.EMPTY)
                                    {
                                        listToDestroy[i].transform.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
                                    }
                                }
                            }
                            else if (previousObject == hit.transform.gameObject)
                            {
                                for (int i = 0; i < listToDestroy.Count; i++)
                                {
                                    if (listToDestroy[i].GetTilesID() != TilesID.EMPTY)
                                    {
                                        if (listToDestroy[i].GetTilesID() == TilesID.ROCKET_ONE ||
                                           listToDestroy[i].GetTilesID() == TilesID.ROCKET_TWO ||
                                           listToDestroy[i].GetTilesID() == TilesID.ROCKET_THREE)
                                        {
                                            Destroy(listToDestroy[i].transform.GetChild(1).gameObject);
                                            listToDestroy[i].GetComponent<Tiles>().SetTitlesID(TilesID.EMPTY);
                                            listToDestroy[i].transform.GetChild(0).gameObject.SetActive(false);
                                        }
                                        else if (listToDestroy[i].GetTilesID() == TilesID.LAVA_STONE)
                                        {
                                            listToDestroy[i].DecreaseLavaStoneLife();
                                            listToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                                        }
                                        else
                                        {
                                            listToDestroy[i].GetComponent<Tiles>().SetTitlesID(TilesID.EMPTY);
                                            listToDestroy[i].transform.GetChild(0).gameObject.SetActive(false);
                                        }                                                                   
                                    }
                                    else if (listToDestroy[i].GetTilesID() == TilesID.EMPTY)
                                    {
                                        listToDestroy[i].transform.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                                    }
                                }
                                energyCount -= 10;
                                GameplayUI.Instance.UpdateEnergyBar();
                                CheckTetrisAvailable();
                                listToDestroy.Clear();
                                isUsingWoodenSkill = false;
                            }
                        }
                    }
                }
              
                yield return null;
            }

            if (listToDestroy.Count > 0)
            {
                for (int i = 0; i < listToDestroy.Count; i++)
                {
                    if (listToDestroy[i].GetTilesID() == TilesID.NORMAL_BLOCK)
                        listToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                    else if (listToDestroy[i].GetTilesID() == TilesID.LAVA_STONE)
                    {
                        listToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                    }
                    else if (listToDestroy[i].GetTilesID() == TilesID.ROCKET_ONE ||
                                          listToDestroy[i].GetTilesID() == TilesID.ROCKET_TWO ||
                                          listToDestroy[i].GetTilesID() == TilesID.ROCKET_THREE)
                    {
                        listToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                    }
                    else if (listToDestroy[i].GetTilesID() == TilesID.BOMB_MINE)
                    {
                        listToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                    }
                    else if (listToDestroy[i].GetTilesID() == TilesID.EMPTY)
                    {
                        listToDestroy[i].transform.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                    }
                }
            }            
        }

        public void SameColorSkill()
        {
            isUsingSameColor = !isUsingSameColor;
            if(isUsingSameColor)
            {
                StartCoroutine(SameColorCour());
            }
            isUsingWoodenSkill = false;
        }

        IEnumerator SameColorCour()
        {
            BlockColorID previousColorID = BlockColorID.NO_COLOR;
            List<Tiles> listToDestroy = new List<Tiles>();
            while(isUsingSameColor)
            {
                // lấy vị trí chuột
                if(Time.timeScale == 1 )
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        UnityEngine.Vector3 mousePos = Input.mousePosition;
                        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
                        //lấy layer của Board
                        int layer = LayerMask.GetMask("Board");
                        // bắn raycast từ tiles đầu tiên -> board               
                        RaycastHit2D hit = Physics2D.Raycast(mousePos, UnityEngine.Vector2.zero, 100, layer);
                        if (hit.collider != null && hit.transform.gameObject.tag == "Tiles" && (hit.transform.GetComponent<Tiles>().GetTilesID() == TilesID.NORMAL_BLOCK ||
                            hit.transform.GetComponent<Tiles>().GetTilesID() == TilesID.ROCKET_ONE || hit.transform.GetComponent<Tiles>().GetTilesID() == TilesID.ROCKET_TWO
                            || hit.transform.GetComponent<Tiles>().GetTilesID() == TilesID.ROCKET_THREE))
                        {
                            if (previousColorID != hit.transform.GetComponent<Tiles>().GetCurrentColor())
                            {
                                if (listToDestroy.Count > 0)
                                {
                                    for (int i = 0; i < listToDestroy.Count; i++)
                                    {
                                        listToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                                    }
                                    listToDestroy.Clear();
                                }
                                previousColorID = hit.transform.GetComponent<Tiles>().GetCurrentColor();
                                for (int i = 0; i < LEVEL_MODE_BOARD_WIDTH; i++)
                                {
                                    for (int j = 0; j < LEVEL_MODE_BOARD_HEIGHT; j++)
                                    {
                                        if (boardMatrix[i, j].GetTilesID() != TilesID.EMPTY && boardMatrix[i, j].GetCurrentColor() == previousColorID)
                                        {
                                            // TODO: Đổi effect vỡ
                                            boardMatrix[i, j].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
                                            listToDestroy.Add(boardMatrix[i, j]);
                                        }
                                    }
                                }
                            }
                            else if (previousColorID == hit.transform.GetComponent<Tiles>().GetCurrentColor())
                            {
                                // TODO : effect destroy nổ puzzle
                                for (int i = 0; i < listToDestroy.Count; i++)
                                {
                                    if (listToDestroy[i].GetTilesID() == TilesID.ROCKET_ONE ||
                                          listToDestroy[i].GetTilesID() == TilesID.ROCKET_TWO ||
                                          listToDestroy[i].GetTilesID() == TilesID.ROCKET_THREE)
                                    {
                                        Destroy(listToDestroy[i].transform.GetChild(1).gameObject);
                                    }
                                    listToDestroy[i].GetComponent<Tiles>().SetTitlesID(TilesID.EMPTY);
                                    //listToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                                    listToDestroy[i].transform.GetChild(0).gameObject.SetActive(false);
                                }
                                energyCount -= 12;
                                GameplayUI.Instance.UpdateEnergyBar();
                                CheckTetrisAvailable();
                                listToDestroy.Clear();
                                isUsingSameColor = false;
                            }
                        }
                    }
                }
              
                yield return null;
            }
            if (listToDestroy.Count > 0)
            {
                for (int i = 0; i < listToDestroy.Count; i++)
                {
                    listToDestroy[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                }
            }            
        }

        public void ReplaceBlockSkill()
        {
            isReplaceBlockSkill = true;
            for (int i = 0; i < currentBlockList.Count; i++)
            {
                Destroy(currentBlockList[i].gameObject);
            }
            currentBlockList.Clear();
            for(int i = 0; i < 3; i++)
            {
                GameObject tempBlock = Instantiate(blockPrefab[8], blockSpawnPanel.transform);
                // Chọn địa điểm spawn cho block
                tempBlock.transform.position = new UnityEngine.Vector3(startSpawnPos[i].transform.position.x, startSpawnPos[i].transform.position.y, 1);
                // Random màu cho block
                int random = Random.Range(0, 7);
                tempBlock.GetComponent<TetrisBlock>().SetUpTetrisBlock(listBlockColor[random], i, blockSpawnLocation[i].transform.position);
                currentBlockList.Add(tempBlock);
            }
            totalTetrisSpawn = 3;
            energyCount -= 5;
            GameplayUI.Instance.UpdateEnergyBar();
            isReplaceBlockSkill = false;
        }      

        public int GetEnergy()
        {
            return energyCount;
        }

        public void RandomRocketItem()
        {
            int random = Random.Range(0, 100);
            if(random < 90)
            {
                bool isSpawnRocket = false;
                for (int i = 0; i < LEVEL_MODE_BOARD_WIDTH; i++)
                {
                    for (int j = 0; j < LEVEL_MODE_BOARD_HEIGHT; j++)
                    {
                        if (boardMatrix[i, j].GetTilesID() == TilesID.NORMAL_BLOCK)
                        {
                            random = Random.Range(0, 2);
                            if (random == 0)
                            {
                                random = Random.Range(0, 11);
                                if (random < 3)
                                {
                                    Instantiate(rocket3Prefab, boardMatrix[i, j].transform.position, UnityEngine.Quaternion.identity, boardMatrix[i, j].transform);
                                    boardMatrix[i, j].SetTitlesID(TilesID.ROCKET_THREE);
                                }
                                else
                                {
                                    random = Random.Range(0, 2);
                                    if (random == 0)
                                    {
                                        Instantiate(rocket1Prefab, boardMatrix[i, j].transform.position, UnityEngine.Quaternion.identity, boardMatrix[i, j].transform);
                                        boardMatrix[i, j].SetTitlesID(TilesID.ROCKET_ONE);
                                    }
                                    else
                                    {
                                        GameObject temp = Instantiate(rocket2Prefab, boardMatrix[i, j].transform);
                                        temp.transform.position = boardMatrix[i, j].transform.position;
                                        boardMatrix[i, j].SetTitlesID(TilesID.ROCKET_TWO);
                                    }

                                }

                                isSpawnRocket = true;
                                break;
                            }
                        }
                    }
                    if (isSpawnRocket)
                        break;
                }
            }           
        }

        public void RocketItemExplode(TilesID _rocketID, Tiles _tile)
        {
            if(_rocketID == TilesID.ROCKET_ONE)
            {
                for (int i = 0; i < LEVEL_MODE_BOARD_HEIGHT; i++)
                {
                    if(!rocketDestroyList.Contains(boardMatrix[_tile.GetPositionX(), i]))
                    {
                        if (boardMatrix[_tile.GetPositionX(), i].GetTilesID() != TilesID.EMPTY)
                        {
                            rocketDestroyList.Add(boardMatrix[_tile.GetPositionX(), i]);
                            if (boardMatrix[_tile.GetPositionX(), i].GetTilesID() == TilesID.ROCKET_ONE || 
                                boardMatrix[_tile.GetPositionX(), i].GetTilesID() == TilesID.ROCKET_TWO ||
                                boardMatrix[_tile.GetPositionX(), i].GetTilesID() == TilesID.ROCKET_THREE)
                            {
                                RocketItemExplode(boardMatrix[_tile.GetPositionX(), i].GetTilesID(), boardMatrix[_tile.GetPositionX(), i]);
                            }
                        }
                    }                  
                }               
            } 
            else if(_rocketID == TilesID.ROCKET_TWO)
            {
                for (int i = 0; i < LEVEL_MODE_BOARD_WIDTH; i++)
                {
                    if (!rocketDestroyList.Contains(boardMatrix[i, _tile.GetPositionY()]))
                    {
                        if (boardMatrix[i, _tile.GetPositionY()].GetTilesID() != TilesID.EMPTY)
                        {
                            rocketDestroyList.Add(boardMatrix[i, _tile.GetPositionY()]);
                            if (boardMatrix[i, _tile.GetPositionY()].GetTilesID() == TilesID.ROCKET_ONE || 
                                boardMatrix[i, _tile.GetPositionY()].GetTilesID() == TilesID.ROCKET_TWO ||
                                boardMatrix[i, _tile.GetPositionY()].GetTilesID() == TilesID.ROCKET_THREE)
                            {
                                RocketItemExplode(boardMatrix[i, _tile.GetPositionY()].GetTilesID(), boardMatrix[i, _tile.GetPositionY()]);
                            }
                        }
                    }                      
                }              
            }
            else if (_rocketID == TilesID.ROCKET_THREE)
            {
                for (int i = 0; i < LEVEL_MODE_BOARD_HEIGHT; i++)
                {
                    if (!rocketDestroyList.Contains(boardMatrix[_tile.GetPositionX(), i]))
                    {
                        if (boardMatrix[_tile.GetPositionX(), i].GetTilesID() != TilesID.EMPTY)
                        {
                            rocketDestroyList.Add(boardMatrix[_tile.GetPositionX(), i]);
                            if (boardMatrix[_tile.GetPositionX(), i].GetTilesID() != TilesID.EMPTY)
                            {
                                rocketDestroyList.Add(boardMatrix[_tile.GetPositionX(), i]);
                                if (boardMatrix[_tile.GetPositionX(), i].GetTilesID() == TilesID.ROCKET_ONE ||
                                    boardMatrix[_tile.GetPositionX(), i].GetTilesID() == TilesID.ROCKET_TWO ||
                                    boardMatrix[_tile.GetPositionX(), i].GetTilesID() == TilesID.ROCKET_THREE)
                                {
                                    RocketItemExplode(boardMatrix[_tile.GetPositionX(), i].GetTilesID(), boardMatrix[_tile.GetPositionX(), i]);
                                }
                            }
                        }
                    }                               
                }

                for (int i = 0; i < LEVEL_MODE_BOARD_WIDTH; i++)
                {
                    if (!rocketDestroyList.Contains(boardMatrix[i, _tile.GetPositionY()]))
                    {
                        if (boardMatrix[i, _tile.GetPositionY()].GetTilesID() != TilesID.EMPTY)
                        {
                            rocketDestroyList.Add(boardMatrix[i, _tile.GetPositionY()]);
                            if (boardMatrix[i, _tile.GetPositionY()].GetTilesID() == TilesID.ROCKET_ONE ||
                                boardMatrix[i, _tile.GetPositionY()].GetTilesID() == TilesID.ROCKET_TWO ||
                                boardMatrix[i, _tile.GetPositionY()].GetTilesID() == TilesID.ROCKET_THREE)
                            {
                                RocketItemExplode(boardMatrix[i, _tile.GetPositionY()].GetTilesID(), boardMatrix[i, _tile.GetPositionY()]);
                            }
                        }
                    }
                }               
            }
          //  DestroyListBlock();
            if(_tile.transform.GetChild(1).gameObject != null)
            Destroy(_tile.transform.GetChild(1).gameObject);            
        }

        void DestroyRocketListBlock()
        {
            for (int i = 0; i < rocketDestroyList.Count; i++)
            {
                if (rocketDestroyList[i].GetTilesID() == TilesID.ROCKET_ONE || rocketDestroyList[i].GetTilesID() == TilesID.ROCKET_TWO ||
                    rocketDestroyList[i].GetTilesID() == TilesID.ROCKET_THREE)
                {
                    if (rocketDestroyList[i].transform.GetChild(1).gameObject != null)
                        rocketDestroyList[i].transform.GetChild(1).gameObject.SetActive(false);
                    rocketDestroyList[i].SetTitlesID(Game.Definition.TilesID.EMPTY);
                    rocketDestroyList[i].transform.GetChild(0).gameObject.SetActive(false);
                }
                else if (rocketDestroyList[i].GetTilesID() == TilesID.LAVA_STONE)
                {
                    rocketDestroyList[i].DecreaseLavaStoneLife();
                }
                else
                {
                    rocketDestroyList[i].SetTitlesID(Game.Definition.TilesID.EMPTY);
                    rocketDestroyList[i].transform.GetChild(0).gameObject.SetActive(false);
                }               
            }
            rocketDestroyList.Clear(); 
        }

        void ScaleGamePlay()
        {
            float mainScreenHeight = 1920;
            float mainScreenWidth = 1080;

            float currentScreenHeight = Screen.height;
            float currentScreenWidth = Screen.width;

            float ratioMain = mainScreenWidth / mainScreenHeight;
            float ratioCurrent = currentScreenWidth / currentScreenHeight;

            if (ratioCurrent == 0.75)
            {
                Camera.main.orthographicSize = 6.5f;
            }
            else
            {
                float ratioTemp = ratioMain / ratioCurrent;
                Camera.main.orthographicSize = Camera.main.orthographicSize * ratioTemp;
            }           
        }    

        public void TurnOffAllSkill()
        {
            isUsingWoodenSkill = false;
            isUsingSameColor = false;
        }

        public void DisableColliderOnBlock()
        {
            for(int i = 0; i < currentBlockList.Count; i++)
            {
                currentBlockList[i].GetComponent<BoxCollider2D>().enabled = false;
            }
        }
        public void EnableColliderOnBlock()
        {
            for (int i = 0; i < currentBlockList.Count; i++)
            {
                currentBlockList[i].GetComponent<BoxCollider2D>().enabled = true;
            }
        }
    }
}

