using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[System.Serializable]
public class PuzzleData
{
    public string id;
    public string fen;
    public string[] solution;
    public int difficulty;
}

[System.Serializable]
public class PuzzleList
{
    public PuzzleData[] puzzles;
}

public class PuzzleManager : MonoBehaviour
{
    [Header("Riferimenti")]
    public BoardManager boardManager;
    public UIManager uiManager;

    private List<PuzzleData> puzzles = new List<PuzzleData>();
    private PuzzleData currentPuzzle;
    private int currentMoveIndex = 0;
    private int currentPuzzleIndex = 0;
    private string pendingComputerMove;

    [Header("UI Contatore")]
    [SerializeField] private TextMeshProUGUI totalSolvedText;
    [SerializeField] private TextMeshProUGUI difficultyText;

    private int totalSolvedCount = 0;

    [Header("Move History Navigation")]
    private List<string> moveHistory = new List<string>();
    private int historyPointer = 0;
    public Button backButton;
    public Button forwardButton;

    void Start()
    {

        totalSolvedCount = 0;
        UpdateCounterUI();

        LoadPuzzles();
        Invoke(nameof(LoadNextPuzzle), 0.2f);

    }



    void LoadPuzzles()
    {
        TextAsset json = Resources.Load<TextAsset>("puzzles");
        if (json == null) return;

        string wrappedJson = "{\"puzzles\":" + json.text + "}";
        PuzzleList data = JsonUtility.FromJson<PuzzleList>(wrappedJson);

        puzzles.Clear();

        foreach (var puzzle in data.puzzles)
        {

            if (ValidatePuzzle(puzzle))
            {
                puzzles.Add(puzzle);
            }
        }


        puzzles.Sort((a, b) => a.difficulty.CompareTo(b.difficulty));

        Debug.Log($"[Database] Caricamento completato. Puzzle totali validi stoccati: {puzzles.Count}");
    }

    public void LoadNextPuzzle()
    {
        if (puzzles.Count == 0) return;

        int windowSize = 3;
        int maxIndex = Mathf.Min(currentPuzzleIndex + windowSize, puzzles.Count - 1);
        int randomIndex = Random.Range(currentPuzzleIndex, maxIndex + 1);

        currentPuzzle = puzzles[randomIndex];
        Debug.Log("ID: " + currentPuzzle.id + " FEN: " + currentPuzzle.fen);
        currentPuzzleIndex = randomIndex + 1;
        currentMoveIndex = 0;

        
        if (difficultyText != null)
        {
            difficultyText.text = $"Difficulty: {currentPuzzle.difficulty}";
            
        }

        bool computerIsWhite = currentPuzzle.fen.Contains(" w ");
        bool playerIsBlack = computerIsWhite;


        boardManager.blackAtBottom = playerIsBlack;
        boardManager.LoadPosition(currentPuzzle.fen);

        moveHistory.Clear();
        historyPointer = 0;
        UpdateNavigationButtons();


        if (uiManager != null)
            uiManager.UpdateWhoToMove(!playerIsBlack);


        if (currentPuzzle.solution.Length > 0)
            Invoke(nameof(ExecuteInitialMove), 0.5f);
    }

    void ExecuteInitialMove()
    {
        string m = currentPuzzle.solution[0];
        currentMoveIndex = 1;
        boardManager.ClearHighlights();
        boardManager.HighlightMove(m, new Color(1f, 0.9f, 0f, 0.6f));
        boardManager.ExecuteMove(m, true);
        
        moveHistory.Add(m);
        historyPointer = moveHistory.Count;
        UpdateNavigationButtons();
    }

    public void TryMove(string uci)
    {
        if (historyPointer < currentMoveIndex)
        {
            Debug.LogWarning("Input Bloccato: Non puoi fare mosse mentre navighi nella cronologia.");
            return;
        }
            
        if (currentPuzzle == null) return;

        if (uci == currentPuzzle.solution[currentMoveIndex])
        {
            boardManager.ClearHighlights();
            boardManager.HighlightMove(uci, new Color(0f, 1f, 0f, 0.6f));
            boardManager.ExecuteMove(uci);
            currentMoveIndex++;
            moveHistory.Add(uci);
            historyPointer = moveHistory.Count;
            UpdateNavigationButtons();

            if (currentMoveIndex < currentPuzzle.solution.Length)
            {
                pendingComputerMove = currentPuzzle.solution[currentMoveIndex];
                currentMoveIndex++;
                Invoke(nameof(ExecuteResponseMove), 0.6f);
            }
            else
            {
                OnPuzzleSolvedSuccess();

                if (uiManager != null)
                {
                    uiManager.PuzzleSolved();
                    uiManager.ShowPuzzleCorrect();
                    uiManager.AddHistoryItem(true, currentPuzzle.difficulty);
                }
                Invoke(nameof(LoadNextPuzzle), 1.2f);
            }
        }
        else
        {
            boardManager.ClearHighlights();
            boardManager.ExecuteMove(uci);
            boardManager.HighlightMove(uci, new Color(1f, 0f, 0f, 0.6f));

            if (uiManager != null)
            {
                uiManager.LoseLife();
                uiManager.ShowPuzzleIncorrect();
                uiManager.AddHistoryItem(false, currentPuzzle.difficulty);
            }

            if (uiManager != null && uiManager.IsGameOver())
                return;

            Invoke(nameof(LoadNextPuzzle), 1.2f);
        }
    }

    void ExecuteResponseMove()
    {
        boardManager.ClearHighlights();
        boardManager.HighlightMove(pendingComputerMove, new Color(1f, 0.9f, 0f, 0.6f));
        boardManager.ExecuteMove(pendingComputerMove, true);
        moveHistory.Add(pendingComputerMove);
        historyPointer = moveHistory.Count;
        UpdateNavigationButtons();

        if (currentMoveIndex >= currentPuzzle.solution.Length)
        {
            OnPuzzleSolvedSuccess();

            Invoke(nameof(LoadNextPuzzle), 1.2f);
        }
    }

    private bool ValidatePuzzle(PuzzleData puzzle)
    {

        ChessLogic testChess = new ChessLogic();

        try
        {

            testChess.LoadFen(puzzle.fen);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Validatore] Puzzle {puzzle.id} scartato: FEN invalida o corrotta. Errore: {e.Message}");
            return false;
        }


        foreach (string move in puzzle.solution)
        {

            if (string.IsNullOrEmpty(move) || move.Length < 4)
            {
                Debug.LogWarning($"[Validatore] Puzzle {puzzle.id} scartato: la mossa '{move}' ha una sintassi errata.");
                return false;
            }


            int fromCol = move[0] - 'a';
            int fromRow = move[1] - '1';
            int toCol = move[2] - 'a';
            int toRow = move[3] - '1';


            if (testChess.board[fromCol, fromRow].type == ChessLogic.PieceType.None)
            {
                Debug.LogWarning($"[Validatore] Puzzle {puzzle.id} scartato: la mossa '{move}' fallisce perché la casa di partenza è VUOTA!");
                return false;
            }


            var legalMoves = testChess.GetLegalMoves(fromCol, fromRow);
            bool isMoveLegal = false;

            foreach (var legalMove in legalMoves)
            {
                if (legalMove.col == toCol && legalMove.row == toRow)
                {
                    isMoveLegal = true;
                    break;
                }
            }


            if (!isMoveLegal)
            {
                Debug.LogWarning($"[Validatore] Puzzle {puzzle.id} scartato: la mossa '{move}' è ILLEGALE nella posizione attuale.");
                return false;
            }


            testChess.MakeMove(move);
        }


        return true;
    }

    public void OnPuzzleSolvedSuccess()
    {
        totalSolvedCount++;
        UpdateCounterUI();
    }

    private void UpdateCounterUI()
    {
        if (totalSolvedText != null)
            totalSolvedText.text = $"{totalSolvedCount}";
    }

    public void NavigateBack()
    {
        if (historyPointer > 0)
        {
            historyPointer--;
            RebuildBoardPosition();
        }
    }

    public void NavigateForward()
    {
        if (historyPointer < moveHistory.Count)
        {
            historyPointer++;
            RebuildBoardPosition();
        }
    }

    private void RebuildBoardPosition()
    {
        if (currentPuzzle == null || boardManager == null) return;

        
        boardManager.LoadPosition(currentPuzzle.fen);
        boardManager.ClearHighlights();


        for (int i = 0; i < historyPointer; i++)
        {
            string move = moveHistory[i];
            bool isComputerMove = (i % 2 == 0);

            if(i == historyPointer -1)
            {
                Color highlightColor = isComputerMove ? new Color(1f, 0.9f, 0f, 0.6f) : new Color(0f, 1f, 0f, 0.6f);
                boardManager.HighlightMove(move, highlightColor);
            }

            boardManager.ExecuteMove(move, true);
        }

        UpdateNavigationButtons();
    }

    private void UpdateNavigationButtons()
    {
        if (backButton != null)
            backButton.interactable = (historyPointer > 0);

        if (forwardButton != null)
            forwardButton.interactable = (historyPointer < moveHistory.Count);
    }



}
