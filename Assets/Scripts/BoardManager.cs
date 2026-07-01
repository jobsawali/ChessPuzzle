using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Sprites - Pezzi Bianchi")]
    public Sprite wKing, wQueen, wRook, wBishop, wKnight, wPawn;
    [Header("Sprites - Pezzi Neri")]
    public Sprite bKing, bQueen, bRook, bBishop, bKnight, bPawn;
    [Header("Sprites - Scacchiera")]
    public Sprite lightSquareSprite, darkSquareSprite;

    [Header("Suoni")]
    public AudioClip moveSound;
    public AudioClip moveSoundOpponent;
    public AudioClip captureSound;
    public AudioClip illegalSound;

    private AudioSource audioSource;

    [Header("Touch Cursor")]
    public Sprite handGrabSprite;
    private GameObject handCursor;


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
    // Drag & Drop
    private bool isDragging = false;
    private int dragCol = -1, dragRow = -1;
    private GameObject dragGhost = null;
    private GameObject dragCursor = null;

    void Start() {

        audioSource = GetComponent<AudioSource>();
        CreateBoard();
    }

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
        bool down = false, move = false, up = false;

        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            var mouse = UnityEngine.InputSystem.Mouse.current;
            screenPos = mouse.position.ReadValue();
            down = mouse.leftButton.wasPressedThisFrame;
            move = mouse.leftButton.isPressed;
            up = mouse.leftButton.wasReleasedThisFrame;
        }

        if (UnityEngine.InputSystem.Touchscreen.current != null)
        {
            var touch = UnityEngine.InputSystem.Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame || touch.press.isPressed || touch.press.wasReleasedThisFrame)
            {
                screenPos = touch.position.ReadValue();
                down = touch.press.wasPressedThisFrame;
                move = touch.press.isPressed;
                up = touch.press.wasReleasedThisFrame;
            }
        }

        if (down) OnPointerDown(screenPos);
        else if (move && isDragging) OnPointerMove(screenPos);
        else if (up) OnPointerUp(screenPos);
    }

    void TryApplyMove(int toCol, int toRow)
    {
        bool isLegal = false;
        var legalMoves = chess.GetLegalMovesFiltered(selectedCol, selectedRow);
        foreach (var m in legalMoves)
            if (m.col == toCol && m.row == toRow) { isLegal = true; break; }

        if (isLegal)
        {
            string uciMove = $"{(char)('a' + selectedCol)}{selectedRow + 1}{(char)('a' + toCol)}{toRow + 1}";
            ChessLogic.Piece movingPiece = chess.board[selectedCol, selectedRow];
            if (movingPiece.type == ChessLogic.PieceType.Pawn && (toRow == 7 || toRow == 0))
                uciMove += "q";

            ClearSelectionHighlights();
            puzzleManager.TryMove(uciMove);
        }
        else
        {
            ClearSelectionHighlights();
            if (audioSource != null && illegalSound != null)
                if (PlayerPrefs.GetInt("SFX", 1) == 1)
                    audioSource.PlayOneShot(illegalSound);
        }

        selectedCol = -1;
        selectedRow = -1;
        isDragging = false;
    }

    void OnPointerDown(Vector2 screenPos)
    {
        var (col, row, hit) = GetSquareAt(screenPos);
        if (!hit) return;

        ChessLogic.Piece piece = chess.board[col, row];

        if (piece.type != ChessLogic.PieceType.None && piece.color == chess.currentTurn)
        {
            
            isDragging = true;
            dragCol = col;
            dragRow = row;
            selectedCol = col;
            selectedRow = row;

            ClearSelectionHighlights();
            AddSelectionHighlight(col, row, new Color(0.443f, 0.894f, 0.918f, 1f));
            foreach (var m in chess.GetLegalMovesFiltered(col, row))
                AddSelectionHighlight(m.col, m.row, new Color(1f, 1f, 1f, 0.1f));

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            CreateDragGhost(col, row, worldPos);
        }
        else if (selectedCol != -1 && !isDragging)
        {
            TryApplyMove(col, row);
        }
    }

    void OnPointerMove(Vector2 screenPos)
    {
        if (!isDragging || dragGhost == null) return;
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        dragGhost.transform.position = new Vector3(worldPos.x, worldPos.y, -0.5f);
        if (handCursor != null)
            handCursor.transform.position = new Vector3(worldPos.x + 0.4f, worldPos.y + 0.4f, -0.6f);
    }

    void OnPointerUp(Vector2 screenPos)
    {
        if (!isDragging) return;

        var (col, row, hit) = GetSquareAt(screenPos);

        
        int fromCol = dragCol;
        int fromRow = dragRow;

        DestroyDragGhost();

        if (hit && (col != fromCol || row != fromRow))
        {
            
            selectedCol = fromCol;
            selectedRow = fromRow;
            TryApplyMove(col, row);
        }
        else
        {
            
            selectedCol = fromCol;
            selectedRow = fromRow;
            isDragging = false;
        }
    }

    void CreateDragGhost(int col, int row, Vector2 worldPos)
    {
        if (pieces[col, row] != null)
            pieces[col, row].GetComponent<SpriteRenderer>().enabled = false;

        dragGhost = new GameObject("DragGhost");
        SpriteRenderer sr = dragGhost.AddComponent<SpriteRenderer>();
        sr.sprite = GetSprite(chess.board[col, row]);
        sr.sortingOrder = 10;
        sr.color = new Color(1f, 1f, 1f, 0.92f);
        dragGhost.transform.position = new Vector3(worldPos.x, worldPos.y, -0.5f);
        dragGhost.transform.localScale = Vector3.one * 1.1f;

        ChessLogic.Piece piece = chess.board[col, row];
        bool isWhitePiece = piece.color == ChessLogic.PieceColor.White;

        if (handGrabSprite != null)
        {
            handCursor = new GameObject("HandCursor");
            SpriteRenderer hsr = handCursor.AddComponent<SpriteRenderer>();
            hsr.sprite = handGrabSprite;
            hsr.sortingOrder = 11;
            handCursor.transform.localScale = Vector3.one * 1.5f;
            handCursor.transform.position = new Vector3(worldPos.x + 0.4f, worldPos.y + 0.4f, -0.6f);
        }


    }

    void DestroyDragGhost()
    {
        if (dragCol >= 0 && dragCol < 8 && dragRow >= 0 && dragRow < 8)
            if (pieces[dragCol, dragRow] != null)
            {
                var sr = pieces[dragCol, dragRow].GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = true;
            }

        if (dragGhost != null) { Destroy(dragGhost); dragGhost = null; }
        if (dragCursor != null) { Destroy(dragCursor); dragCursor = null; }

        isDragging = false;
        dragCol = -1;
        dragRow = -1;

        if (handCursor != null) { Destroy(handCursor); handCursor = null; }
    }

    (int col, int row, bool hit) GetSquareAt(Vector2 screenPos)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);
        if (hit.collider != null)
            for (int c = 0; c < 8; c++)
                for (int r = 0; r < 8; r++)
                    if (hit.collider.gameObject == squares[c, r])
                    {
                        int logicCol = blackAtBottom ? 7 - c : c;
                        int logicRow = blackAtBottom ? 7 - r : r;
                        return (logicCol, logicRow, true);
                    }
        return (0, 0, false);
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

    public void ExecuteMove(string uci, bool isOpponent = false)
    {
        int tc = uci[2] - 'a';
        int tr = uci[3] - '1';

        bool isCapture = chess.board[tc, tr].type != ChessLogic.PieceType.None;

        chess.MakeMove(uci);
        SpawnPieces();

        if (PlayerPrefs.GetInt("SFX", 1) == 1)
        {
            if (isCapture)
                audioSource.PlayOneShot(captureSound);
            else if (isOpponent)
                audioSource.PlayOneShot(moveSoundOpponent);
            else
                audioSource.PlayOneShot(moveSound);
        }
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