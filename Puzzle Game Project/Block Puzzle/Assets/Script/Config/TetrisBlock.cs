using Puzzle.Block.GameUIManager;
using Puzzle.Block.ScorePopUp;
using Puzzle.Game.Definition;
using Puzzle.Game.Tiles;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using UnityEngine;

namespace Puzzle.Block.TertrisBlock
{
    public class TetrisBlock : MonoBehaviour
    {
        [SerializeField]
        GameObject blockToSpawn;
        [SerializeField]
        GameObject prefabScorePopup;      

        UnityEngine.Vector3 spawnPosVector;
        int spawnPosNumber;
        float movingSpeed = 18;
        bool isPlaced;
        bool isBeingHeld = true;
        float currentPositionX;
        float currentPositionY;
        float colliderY;

        GameObject currentObject, previousObject;
        List<int> listRowToCheck;
        List<int> listColumnToCheck;
        List<GameObject> listTilesToPlace;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (isBeingHeld)
            {
#if UNITY_EDITOR
                //Test
                UnityEngine.Vector3 mousePos = Input.mousePosition;
                mousePos = Camera.main.ScreenToWorldPoint(mousePos);
                this.gameObject.transform.position = new UnityEngine.Vector3(mousePos.x - currentPositionX, mousePos.y - currentPositionY, this.gameObject.transform.position.z);
                //lấy layer của Board
                int layer = LayerMask.GetMask("Board");
                // bắn raycast từ tiles đầu tiên -> board               
                RaycastHit2D hit = Physics2D.Raycast(this.transform.GetChild(0).transform.position, UnityEngine.Vector2.zero, 100, layer);
                // chỉ khi nào đổi ô mới kiểm tra lại tetris      

                if (hit.collider != null && hit.transform.gameObject.tag == "Tiles")
                {
                    CheckPlaceTetris();
                }
                else if (hit.collider == null)
                {
                    // Chạy ra ngoài bàn cờ
                    if (isPlaced)
                    {
                        for (int i = 0; i < listTilesToPlace.Count; i++)
                        {
                            listTilesToPlace[i].transform.GetChild(0).gameObject.SetActive(false);
                            listTilesToPlace[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                        }
                        listTilesToPlace.Clear();
                        isPlaced = false;
                        GameManager.GameManager.Instance.ResetBreakEffect();
                    }
                }
#else
            if (Input.touchCount == 1)
                {
                    //Test
                    UnityEngine.Vector3 mousePos = Input.mousePosition;
                    mousePos = Camera.main.ScreenToWorldPoint(mousePos);
                    this.gameObject.transform.position = new UnityEngine.Vector3(mousePos.x - currentPositionX, mousePos.y - currentPositionY, this.gameObject.transform.position.z);

                    //lấy layer của Board
                    int layer = LayerMask.GetMask("Board");
                    // bắn raycast từ tiles đầu tiên -> board               
                    RaycastHit2D hit = Physics2D.Raycast(this.transform.GetChild(0).transform.position, UnityEngine.Vector2.zero, 100, layer);
                    // chỉ khi nào đổi ô mới kiểm tra lại tetris      

                    if (hit.collider != null && hit.transform.gameObject.tag == "Tiles")
                    {
                        CheckPlaceTetris();
                    }
                    else if (hit.collider == null)
                    {
                        // Chạy ra ngoài bàn cờ
                        if (isPlaced)
                        {
                            for (int i = 0; i < listTilesToPlace.Count; i++)
                            {
                                listTilesToPlace[i].transform.GetChild(0).gameObject.SetActive(false);
                                listTilesToPlace[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                            }
                            listTilesToPlace.Clear();
                            isPlaced = false;
                            GameManager.GameManager.Instance.ResetBreakEffect();
                        }
                    }
                }
#endif
            }
        }

        private void OnMouseDown()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Nhận biết khi click vào Tetris phóng to ra 
                isBeingHeld = true;
                colliderY = this.GetComponent<BoxCollider2D>().size.y;
                // Khi nhấc mảnh ghép lên thì scale = 1
                if(GameManager.GameManager.Instance.GetTimeModeBool())
                {
                    SetScale(1.25f);
                }
                else
                {
                    SetScale(1);
                }
                           
                SetScaleChildren(0.85f);

                // Di chuyển Tetris theo chuột + khoảng cách = 1/4 boxcolliderY
                UnityEngine.Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - this.transform.position;
                this.transform.Translate(0, mousePos.y + colliderY , 0, Space.World);

               // UnityEngine.Vector3 mousePos;
                mousePos = Input.mousePosition;
                mousePos = Camera.main.ScreenToWorldPoint(mousePos);
                currentPositionX = mousePos.x - this.transform.position.x;
                currentPositionY = mousePos.y - this.transform.position.y;

                // Turn off Skill
                GameManager.GameManager.Instance.TurnOffAllSkill();

                // Chuyen Layer cao len
                for(int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = 3;
                    transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = "UI";
                }
            }
        }

        private void OnMouseUp()
        {
            if (Input.GetMouseButtonUp(0))
            {              
                // Khi Tetris không được click vào
                isBeingHeld = false;
                if (!isPlaced)
                {
                    // Nếu Tetris chưa được đặt vào ô nào thì quay trở về vị trí cũ scale nhỏ đi
                    SetScale(0.45f);
                    SetScaleChildren(1);
                    MoveToSpawn();

                    //Chuyen layer xuong
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).GetComponent<SpriteRenderer>().sortingOrder = 2;
                        transform.GetChild(i).GetComponent<SpriteRenderer>().sortingLayerName = "Gameplay";
                    }

                    for (int i = 0; i < listTilesToPlace.Count; i++)
                    {
                        listTilesToPlace[i].transform.GetChild(0).gameObject.SetActive(false);
                        listTilesToPlace[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                    }
                    listTilesToPlace.Clear();
                    isPlaced = false;
                    GameManager.GameManager.Instance.ResetBreakEffect();
                }
                else
                {
                    // Nếu Tetris được đặt vào ô
                    listColumnToCheck.Clear();
                    listRowToCheck.Clear();
                    for (int i = 0; i < listTilesToPlace.Count; i++)
                    {
                        // Đổi màu alpha về 1
                        listTilesToPlace[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                        listTilesToPlace[i].transform.GetComponent<Tiles>().SetTitlesID(TilesID.NORMAL_BLOCK);
                        if (listTilesToPlace[i].GetComponent<Tiles>().GetCurrentColor() == BlockColorID.HEART)
                        {
                            listTilesToPlace[i].transform.GetComponent<Tiles>().SetTitlesID(TilesID.HEART);
                        }
                        else if (listTilesToPlace[i].GetComponent<Tiles>().GetCurrentColor() == BlockColorID.BELL)
                        {
                            listTilesToPlace[i].transform.GetComponent<Tiles>().SetTitlesID(TilesID.BELL);
                        }    
                        // Add các tọa độ Y cần check vào 1 list                     
                        if (!listColumnToCheck.Contains(listTilesToPlace[i].GetComponent<Tiles>().GetPositionY()))
                            listColumnToCheck.Add(listTilesToPlace[i].GetComponent<Tiles>().GetPositionY());
                        // Add các tọa độ X cần check vào 1 list
                        if (!listRowToCheck.Contains(listTilesToPlace[i].GetComponent<Tiles>().GetPositionX()))
                            listRowToCheck.Add(listTilesToPlace[i].GetComponent<Tiles>().GetPositionX());
                    }                   
                    // Gọi hàm check nếu đủ hàng để destroy
                    GameManager.GameManager.Instance.CheckBlockMatches(listRowToCheck, listColumnToCheck,this.transform.childCount,this.transform.position);
                    // Gọi hàm bên Game Manager -> Kiểm tra xem có cần add thêm block không
                    GameManager.GameManager.Instance.SpawnNewRandomBlock(spawnPosNumber);                         
                    // Update score
                    GameplayUI.Instance.UpdateScoreBoard();
                    // Spawn Rocket
                    GameManager.GameManager.Instance.RandomRocketItem();
                    // Spawn Time Item
                    if(GameManager.GameManager.Instance.GetTimeModeBool())
                    {
                        GameManager.GameManager.Instance.RandomTimeItem();
                    }
                    // Hủy object                 
                    Destroy(this.gameObject);
                }
            }
        }

        public void ChangeBlockColor(Sprite _sprite)
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                this.GetComponentsInChildren<SpriteRenderer>()[i].sprite = _sprite;
            }
        }

        public void SetSpawnPos(int _pos)
        {
            spawnPosNumber = _pos;
        }

        public void SetUpTetrisBlock(Sprite _sprite, int _pos, UnityEngine.Vector3 _posVec)
        {
            for (int i = 0; i < this.transform.childCount; i++)
            {
                this.GetComponentsInChildren<SpriteRenderer>()[i].sprite = _sprite;
            }
            spawnPosNumber = _pos;
            spawnPosVector = _posVec;
            isBeingHeld = false;
            listTilesToPlace = new List<GameObject>();
            listRowToCheck = new List<int>();
            listColumnToCheck = new List<int>();
            StartCoroutine(MoveToSpawnPos());
        }

        IEnumerator MoveToSpawnPos()
        {
            while (transform.position != spawnPosVector)
            {
                transform.position = UnityEngine.Vector3.MoveTowards(transform.position, spawnPosVector, Time.deltaTime * movingSpeed);
                yield return null;
            }            
            yield return new WaitForSeconds(0.2f);
            if(spawnPosNumber == 2) 
            {
                GameManager.GameManager.Instance.CheckIfLose();
            }            
        }

        public void MoveToSpawn()
        {
            StartCoroutine(MoveToSpawnPos());
        }

        void SetScale(float _number)
        {
            this.transform.localScale = new UnityEngine.Vector3(_number, _number);
        }

        void CheckPlaceTetris()
        {
            // Nếu frame trước đặt vào 1 chỗ nào đặt được thì tắt đi để cho sang cái khác        
            if (isPlaced)
            {
                for (int i = 0; i < listTilesToPlace.Count; i++)
                {
                    listTilesToPlace[i].transform.GetChild(0).gameObject.SetActive(false);
                    listTilesToPlace[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                }
            }
            isPlaced = false;
            listTilesToPlace.Clear();
            int layer = LayerMask.GetMask("Board");
            for (int i = 0; i < this.transform.childCount; i++)
            {
                // Bắn raycast để check xem có đặt được vào chỗ mới không
                RaycastHit2D hit = Physics2D.Raycast(this.transform.GetChild(i).position, UnityEngine.Vector2.zero, 100, layer);
                if (hit.collider != null)
                {
                    // bắn được thì add vào 1 cái list
                    if (hit.transform.gameObject.tag == "Tiles" && hit.transform.GetComponent<Tiles>().GetTilesID() == TilesID.EMPTY)
                    {
                        listTilesToPlace.Add(hit.transform.gameObject);
                    }                  
                    else
                    {
                        GameManager.GameManager.Instance.ResetBreakEffect();
                        break;
                    }
                }
            }

            // Nếu đủ chỗ để đặt
            if (listTilesToPlace.Count == this.transform.childCount)
            {
                listColumnToCheck.Clear();
                listRowToCheck.Clear();
                isPlaced = true;
                for (int i = 0; i < listTilesToPlace.Count; i++)
                {
                    // Enable cái block có sẵn trong bàn
                    listTilesToPlace[i].transform.GetChild(0).gameObject.SetActive(true);
                    listTilesToPlace[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.4f);
                    listTilesToPlace[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = this.transform.GetChild(i).GetComponent<SpriteRenderer>().sprite;
                    SetColor(listTilesToPlace[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite.name, listTilesToPlace[i].GetComponent<Tiles>());
                }

                for (int i = 0; i < listTilesToPlace.Count; i++)
                {
                    // Add các tọa độ Y cần check vào 1 list
                    if (!listColumnToCheck.Contains(listTilesToPlace[i].GetComponent<Tiles>().GetPositionY()))
                        listColumnToCheck.Add(listTilesToPlace[i].GetComponent<Tiles>().GetPositionY());
                    // Add các tọa độ X cần check vào 1 list
                    if (!listRowToCheck.Contains(listTilesToPlace[i].GetComponent<Tiles>().GetPositionX()))
                        listRowToCheck.Add(listTilesToPlace[i].GetComponent<Tiles>().GetPositionX());
                }
                // Kiểm tra tạo effect cho các block có thể ăn được
                GameManager.GameManager.Instance.CheckBlockMatchesEffect(listRowToCheck, listColumnToCheck);
            }          
        }

        void SetScaleChildren(float _number)
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).transform.localScale = new UnityEngine.Vector3(_number, _number);
            }
        }

        void SetColor(string tilesToSetColor, Tiles tiles)
        {
            if (tilesToSetColor.Equals("blue_"))
            {
                tiles.SetCurrentColor(BlockColorID.BLUE);
            }
            else if (tilesToSetColor.Equals("deep blue_"))
            {
                tiles.SetCurrentColor(BlockColorID.DEEP_BLUE);
            }
            else if (tilesToSetColor.Equals("green_"))
            {
                tiles.SetCurrentColor(BlockColorID.GREEN);
            }
            else if (tilesToSetColor.Equals("orange_"))
            {
                tiles.SetCurrentColor(BlockColorID.ORANGE);
            }
            else if (tilesToSetColor.Equals("red_"))
            {
                tiles.SetCurrentColor(BlockColorID.RED);
            }
            else if (tilesToSetColor.Equals("violet_"))
            {
                tiles.SetCurrentColor(BlockColorID.VIOLET);
            }
            else if (tilesToSetColor.Equals("yellow_"))
            {
                tiles.SetCurrentColor(BlockColorID.YELLOW);
            }
            else if(tilesToSetColor.Equals("heart_"))
            {
                tiles.SetCurrentColor(BlockColorID.HEART);
            }
            else if (tilesToSetColor.Equals("bell_"))
            {
                tiles.SetCurrentColor(BlockColorID.BELL);
            }
        }

        public void SetNewPosSpawn(UnityEngine.Vector3 _pos)
        {
            spawnPosVector = _pos;
        }

        public void RotateBlockZasis()
        {
            for(int i = 0; i < transform.childCount;i++)
            {
                transform.GetChild(i).GetComponent<BlockTetris>().RotateBlock();
            }
        }

        public void TestFunction()
        {
            if (isBeingHeld)
            {
                isBeingHeld = false;
                transform.position = spawnPosVector;
                for (int i = 0; i < listTilesToPlace.Count; i++)
                {
                    listTilesToPlace[i].transform.GetChild(0).gameObject.SetActive(false);
                    listTilesToPlace[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                }
                listTilesToPlace.Clear();
                isPlaced = false;               
                SetScale(0.45f);
                SetScaleChildren(1);              
            }
        }

        private void OnApplicationPause(bool pause)
        {
            if(pause)
            {
                if(isBeingHeld)
                {
                    isBeingHeld = false;
                    transform.position = spawnPosVector;
                    for (int i = 0; i < listTilesToPlace.Count; i++)
                    {
                        listTilesToPlace[i].transform.GetChild(0).gameObject.SetActive(false);
                        listTilesToPlace[i].transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                    }
                    listTilesToPlace.Clear();
                    isPlaced = false;
                    GameManager.GameManager.Instance.ResetBreakEffect();
                    SetScale(0.45f);
                    SetScaleChildren(1);
                    GameplayUI.Instance.PauseButton();
                }                        
            }
        }
    }
    
}


