using System.Collections.Generic;

public class ChessLogic
{
    public enum PieceType { None, Pawn, Knight, Bishop, Rook, Queen, King }
    public enum PieceColor { None, White, Black }

    public struct Piece
    {
        public PieceType type;
        public PieceColor color;
        public Piece(PieceType t, PieceColor c) { type = t; color = c; }
    }

    public Piece[,] board = new Piece[8, 8];
    public PieceColor currentTurn = PieceColor.White;

    public void LoadFen(string fen)
    {
        board = new Piece[8, 8];
        string[] parts = fen.Split(' ');
        string[] rows = parts[0].Split('/');

        for (int row = 0; row < 8; row++)
        {
            int col = 0;
            int targetRow = 7 - row; 

            foreach (char c in rows[row])
            {
                if (char.IsDigit(c)) col += (int)char.GetNumericValue(c);
                else
                {
                    board[col, targetRow] = CharToPiece(c);
                    col++;
                }
            }
        }
        currentTurn = (parts.Length > 1 && parts[1] == "b") ? PieceColor.Black : PieceColor.White;
    }

    private Piece CharToPiece(char c)
    {
        PieceColor color = char.IsUpper(c) ? PieceColor.White : PieceColor.Black;
        PieceType type = char.ToLower(c) switch
        {
            'p' => PieceType.Pawn,
            'n' => PieceType.Knight,
            'b' => PieceType.Bishop,
            'r' => PieceType.Rook,
            'q' => PieceType.Queen,
            'k' => PieceType.King,
            _ => PieceType.None
        };
        return new Piece(type, color);
    }

    public bool MakeMove(string uciMove)
    {
        if (uciMove.Length < 4) return false;
        int fc = uciMove[0] - 'a', fr = uciMove[1] - '1';
        int tc = uciMove[2] - 'a', tr = uciMove[3] - '1';

        Piece piece = board[fc, fr];
        board[tc, tr] = piece;
        board[fc, fr] = new Piece(PieceType.None, PieceColor.None);

        if (piece.type == PieceType.Pawn && (tr == 7 || tr == 0))
            board[tc, tr] = new Piece(PieceType.Queen, piece.color);

        currentTurn = (currentTurn == PieceColor.White) ? PieceColor.Black : PieceColor.White;
        return true;
    }

    public List<(int col, int row)> GetLegalMoves(int fromCol, int fromRow)
    {
        List<(int, int)> moves = new List<(int, int)>();
        Piece piece = board[fromCol, fromRow];
        if (piece.type == PieceType.None) return moves;

        switch (piece.type)
        {
            case PieceType.Pawn: AddPawnMoves(fromCol, fromRow, piece.color, moves); break;
            case PieceType.Knight: AddKnightMoves(fromCol, fromRow, piece.color, moves); break;
            case PieceType.Bishop: AddSlidingMoves(fromCol, fromRow, piece.color, moves, true, false); break;
            case PieceType.Rook: AddSlidingMoves(fromCol, fromRow, piece.color, moves, false, true); break;
            case PieceType.Queen: AddSlidingMoves(fromCol, fromRow, piece.color, moves, true, true); break;
            case PieceType.King: AddKingMoves(fromCol, fromRow, piece.color, moves); break;
        }
        return moves;
    }

   
    private void AddPawnMoves(int c, int r, PieceColor color, List<(int, int)> moves)
    {
        int dir = color == PieceColor.White ? 1 : -1;
        if (InBounds(c, r + dir) && board[c, r + dir].type == PieceType.None)
        {
            moves.Add((c, r + dir));
            int start = color == PieceColor.White ? 1 : 6;
            if (r == start && board[c, r + 2 * dir].type == PieceType.None) moves.Add((c, r + 2 * dir));
        }
        foreach (int dc in new[] { -1, 1 })
            if (InBounds(c + dc, r + dir) && board[c + dc, r + dir].type != PieceType.None && board[c + dc, r + dir].color != color)
                moves.Add((c + dc, r + dir));
    }

    private void AddKnightMoves(int c, int r, PieceColor color, List<(int, int)> moves)
    {
        int[] dx = { 2, 2, -2, -2, 1, 1, -1, -1 }, dy = { 1, -1, 1, -1, 2, -2, 2, -2 };
        for (int i = 0; i < 8; i++)
            if (InBounds(c + dx[i], r + dy[i]) && board[c + dx[i], r + dy[i]].color != color)
                moves.Add((c + dx[i], r + dy[i]));
    }

    private void AddSlidingMoves(int c, int r, PieceColor color, List<(int, int)> moves, bool diag, bool str)
    {
        List<int[]> dirs = new List<int[]>();
        if (str) { dirs.Add(new[] { 1, 0 }); dirs.Add(new[] { -1, 0 }); dirs.Add(new[] { 0, 1 }); dirs.Add(new[] { 0, -1 }); }
        if (diag) { dirs.Add(new[] { 1, 1 }); dirs.Add(new[] { 1, -1 }); dirs.Add(new[] { -1, 1 }); dirs.Add(new[] { -1, -1 }); }
        foreach (var d in dirs)
        {
            int nc = c + d[0], nr = r + d[1];
            while (InBounds(nc, nr))
            {
                if (board[nc, nr].type != PieceType.None)
                {
                    if (board[nc, nr].color != color) moves.Add((nc, nr));
                    break;
                }
                moves.Add((nc, nr));
                nc += d[0]; nr += d[1];
            }
        }
    }

    private void AddKingMoves(int c, int r, PieceColor color, List<(int, int)> moves)
    {
        for (int dc = -1; dc <= 1; dc++)
            for (int dr = -1; dr <= 1; dr++)
                if ((dc != 0 || dr != 0) && InBounds(c + dc, r + dr) && board[c + dc, r + dr].color != color)
                    moves.Add((c + dc, r + dr));
    }

    private bool InBounds(int c, int r) => c >= 0 && c < 8 && r >= 0 && r < 8;

    public bool IsKingInCheck(PieceColor color)
    {
       
        int kingCol = -1, kingRow = -1;
        for (int c = 0; c < 8; c++)
            for (int r = 0; r < 8; r++)
                if (board[c, r].type == PieceType.King && board[c, r].color == color)
                { kingCol = c; kingRow = r; }

        if (kingCol == -1) return false;

        
        PieceColor opponent = color == PieceColor.White ? PieceColor.Black : PieceColor.White;
        for (int c = 0; c < 8; c++)
            for (int r = 0; r < 8; r++)
                if (board[c, r].color == opponent)
                    foreach (var move in GetLegalMoves(c, r))
                        if (move.col == kingCol && move.row == kingRow)
                            return true;

        return false;
    }

    public List<(int col, int row)> GetLegalMovesFiltered(int fromCol, int fromRow)
    {
        List<(int, int)> legalMoves = new List<(int, int)>();
        Piece piece = board[fromCol, fromRow];
        if (piece.type == PieceType.None) return legalMoves;

        foreach (var move in GetLegalMoves(fromCol, fromRow))
        {
            
            Piece backup = board[move.col, move.row];
            board[move.col, move.row] = piece;
            board[fromCol, fromRow] = new Piece(PieceType.None, PieceColor.None);

            
            bool inCheck = IsKingInCheck(piece.color);

           
            board[fromCol, fromRow] = piece;
            board[move.col, move.row] = backup;

            if (!inCheck)
                legalMoves.Add(move);
        }

        return legalMoves;
    }
}