using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    [Header("Sprites - Pezzi Bianchi")]
    public Sprite wKing, wQueen, wRook, wBishop, wKnight, wPawn;
    [Header("Sprites - Pezzi Neri")]
    public Sprite bKing, bQueen, bRook, bBishop, bKnight, bPawn;
    [Header("Sprites - Scacchiera")]
    public Sprite lightSquareSprite, darkSquareSprite;

    public float squareSize = 1f;
    public PuzzleManager puzzleManager;

    private GameObject[,] squares = new GameObject[8, 8];
    private GameObject[,] pieces = new GameObject[8, 8];

    private List<GameObject> coordinates = new List<GameObject>();
    public bool blackAtBottom = false;

    
    private List<GameObject> botHighlights = new List<GameObject>();

    
    private List<GameObject> selectionHighlights = new List<GameObject>();

    private ChessLogic chess = new ChessLogic();
    private int selectedCol = -1, selectedRow = -1;

    void Start() { CreateBoard(); }

    void CreateBoard()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                GameObject square = new GameObject($"Square_{col}_{row}");
                square.transform.SetParent(this.transform);

                float posX = (col - 3.5f) * squareSize;
                float posY = (row - 3.5f) * squareSize;
                square.transform.localPosition = new Vector3(posX, posY, 0);

                SpriteRenderer sr = square.AddComponent<SpriteRenderer>();
                sr.sprite = (row + col) % 2 != 0 ? lightSquareSprite : darkSquareSprite;
                sr.sortingOrder = 0;
                square.AddComponent<BoxCollider2D>();
                squares[col, row] = square;

            }
        }
        CreateCoordinates();
    }

    void Update()
    {
        Vector2 screenPos = Vector2.zero;
        bool pressed = false;

        if (UnityEngine.InputSystem.Mouse.current != null &&
            UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            pressed = true;
        }

        if (UnityEngine.InputSystem.Touchscreen.current != null &&
            UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPos = UnityEngine.InputSystem.Touchscreen.current.primaryTouch.position.ReadValue();
            pressed = true;
        }

        if (!pressed) return;

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        
        if (hit.collider != null)
        {
            for (int c = 0; c < 8; c++)
            {
                for (int r = 0; r < 8; r++)
                {
                    if (hit.collider.gameObject == squares[c, r])
                    {
                        
                        int logicCol = blackAtBottom ? 7 - c : c;
                        int logicRow = blackAtBottom ? 7 - r : r;
                        OnSquareClicked(logicCol, logicRow);
                        return;
                    }
                }
            }
        }
    }

    void CreateCoordinates()
    {
        foreach (var c in coordinates) Destroy(c);
        coordinates.Clear();

        Color lightSquareColor = new Color(0.93f, 0.78f, 0.53f);
        Color darkSquareColor = new Color(0.6f, 0.35f, 0.15f);

        
        
        string fileChars = blackAtBottom ? "hgfedcba" : "abcdefgh";
        string[] rankNumbers = blackAtBottom
            ? new string[] { "8", "7", "6", "5", "4", "3", "2", "1" }
            : new string[] { "1", "2", "3", "4", "5", "6", "7", "8" };

        for (int i = 0; i < 8; i++)
        {
           
            bool fileSquareIsLight = (i % 2) != 0;
            Color fileTextColor = fileSquareIsLight ? darkSquareColor : lightSquareColor;
            Vector3 filePos = new Vector3((i - 3.5f) * squareSize, -4.2f * squareSize, -0.1f);
            CreateCoordText(fileChars[i].ToString(), filePos, fileTextColor, 4f);

            
            bool rankSquareIsLight = (i % 2) != 0;
            Color rankTextColor = rankSquareIsLight ? darkSquareColor : lightSquareColor;
            Vector3 rankPos = new Vector3(-4.2f * squareSize, (i - 3.5f) * squareSize, -0.1f);
            CreateCoordText(rankNumbers[i], rankPos, rankTextColor, 4f);
        }
    }

    void CreateCoordText(string text, Vector3 localPos, Color color, float fontSize)
    {
        GameObject obj = new GameObject($"Coord_{text}");
        obj.transform.SetParent(this.transform);
        obj.transform.localPosition = localPos;
        obj.transform.localScale = Vector3.one; 

        TMPro.TextMeshPro tmp = obj.AddComponent<TMPro.TextMeshPro>();
        tmp.text = text;
        tmp.fontSize = fontSize; 
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = color;

        
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(1f, 1f);

        coordinates.Add(obj);
    }

    void OnSquareClicked(int col, int row)
    {
        ChessLogic.Piece clickedPiece = chess.board[col, row];

        if (clickedPiece.type != ChessLogic.PieceType.None && clickedPiece.color == chess.currentTurn)
        {
            
            ClearSelectionHighlights();
            selectedCol = col; selectedRow = row;

            AddSelectionHighlight(col, row, new Color(1f, 1f, 0f, 0.25f));
            foreach (var move in chess.GetLegalMoves(col, row))
                AddSelectionHighlight(move.col, move.row, new Color(1f, 1f, 1f, 0.1f));
        }
        else if (selectedCol != -1)
        {
            bool isLegal = false;
            var legalMoves = chess.GetLegalMoves(selectedCol, selectedRow);
            foreach (var m in legalMoves)
                if (m.col == col && m.row == row) { isLegal = true; break; }

            if (isLegal)
            {
                string uciMove = $"{(char)('a' + selectedCol)}{selectedRow + 1}{(char)('a' + col)}{row + 1}";
            
                ClearSelectionHighlights();
                puzzleManager.TryMove(uciMove);
            }
            else
            {
                ClearSelectionHighlights();
                Debug.Log("Mossa non permessa dalle regole.");
            }

            selectedCol = -1; selectedRow = -1;
        }
    }

    
    public void HighlightMove(string uci, Color color)
    {
        
        int fc = uci[0] - 'a';
        int fr = uci[1] - '1';
        int tc = uci[2] - 'a';
        int tr = uci[3] - '1';

        AddBotHighlight(fc, fr, color);
        AddBotHighlight(tc, tr, color);
    }

    void AddBotHighlight(int col, int row, Color c)
    {
        if (col < 0 || col > 7 || row < 0 || row > 7) return;

       
        int displayCol = blackAtBottom ? 7 - col : col;
        int displayRow = blackAtBottom ? 7 - row : row;

        GameObject hl = new GameObject("BotHighlight");
      
        hl.transform.position = squares[displayCol, displayRow].transform.position + new Vector3(0, 0, -0.1f);

        SpriteRenderer sr = hl.AddComponent<SpriteRenderer>();
        sr.sprite = lightSquareSprite; 
        sr.color = c;
        sr.sortingOrder = 1; 

        botHighlights.Add(hl);
    }

    void AddSelectionHighlight(int col, int row, Color c)
    {
        if (col < 0 || col > 7 || row < 0 || row > 7) return;

        int displayCol = blackAtBottom ? 7 - col : col;
        int displayRow = blackAtBottom ? 7 - row : row;

        GameObject hl = new GameObject("SelectionHighlight");
        hl.transform.position = squares[displayCol, displayRow].transform.position + new Vector3(0, 0, -0.1f);

        SpriteRenderer sr = hl.AddComponent<SpriteRenderer>();
        sr.sprite = lightSquareSprite;
        sr.color = c;
        sr.sortingOrder = 1;

        selectionHighlights.Add(hl);
    }

    
    void ClearSelectionHighlights()
    {
        foreach (var h in selectionHighlights) Destroy(h);
        selectionHighlights.Clear();
    }

    
    public void ClearHighlights()
    {
        foreach (var h in botHighlights) Destroy(h);
        botHighlights.Clear();
        ClearSelectionHighlights();
    }

    

    public void SpawnPieces()
    {
        foreach (var p in pieces) if (p != null) Destroy(p);

        for (int c = 0; c < 8; c++)
        {
            for (int r = 0; r < 8; r++)
            {
                ChessLogic.Piece p = chess.board[c, r];
                if (p.type == ChessLogic.PieceType.None) continue;

                GameObject pObj = new GameObject($"P_{c}_{r}");

               
                int displayCol = blackAtBottom ? 7 - c : c;
                int displayRow = blackAtBottom ? 7 - r : r;

                pObj.transform.position = squares[displayCol, displayRow].transform.position + new Vector3(0, 0, -0.2f);

                SpriteRenderer sr = pObj.AddComponent<SpriteRenderer>();
                sr.sprite = GetSprite(p);
                sr.sortingOrder = 2;
                pieces[c, r] = pObj;
            }
        }
    }

    public void LoadPosition(string fen)
    {
        chess.LoadFen(fen);
        SpawnPieces();
        CreateCoordinates();
        ClearHighlights();
    }

    public void ExecuteMove(string uci)
    {
        chess.MakeMove(uci);
        SpawnPieces();
    }

    Sprite GetSprite(ChessLogic.Piece p)
    {
        bool isW = p.color == ChessLogic.PieceColor.White;
        return p.type switch
        {
            ChessLogic.PieceType.King => isW ? wKing : bKing,
            ChessLogic.PieceType.Queen => isW ? wQueen : bQueen,
            ChessLogic.PieceType.Rook => isW ? wRook : bRook,
            ChessLogic.PieceType.Bishop => isW ? wBishop : bBishop,
            ChessLogic.PieceType.Knight => isW ? wKnight : bKnight,
            ChessLogic.PieceType.Pawn => isW ? wPawn : bPawn,
            _ => null
        };
    }
}