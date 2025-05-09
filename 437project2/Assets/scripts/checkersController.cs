using UnityEngine;
using System.Collections.Generic;

public class checkersController : MonoBehaviour
{
    public int aiDepth = 3;
    public bool PlayerTurn = true;
    private bool playerWon = false;
    public List<Transform> spots = new List<Transform>();
    public GameObject whitePiece;
    public GameObject whiteKing;
    public GameObject blackPiece;
    public GameObject blackKing;
    private GameObject[,] board = new GameObject[8, 8];
    private List<GameObject> whitePieces = new List<GameObject>();
    private List<GameObject> blackPieces = new List<GameObject>();
    private bool gameActive = false;
    
    private GameObject selectedPiece = null;
    private Vector2Int selectedPiecePosition = new Vector2Int(-1, -1);
    private List<Vector2Int> validMoves = new List<Vector2Int>();
    private bool mustJump = false;
    public Material highlightMaterial;
    private List<GameObject> highlightObjects = new List<GameObject>();
    
    void Start()
    {
        int spotsMissingColliders = 0;
        for (int i = 0; i < spots.Count; i++)
        {
            if (spots[i] != null && spots[i].GetComponent<Collider>() == null)
            {
                spotsMissingColliders++;
            }
        }
        playerController player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<playerController>();
        if (player == null)
        {
        }
        else if (player.playerCamera == null)
        {
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameActive && Input.GetKeyDown(KeyCode.G))
        {
            EndCheckersGame();
        }
        
        if (gameActive && PlayerTurn)
        {
            if (CheckForWinCondition())
            {
                return;
            }
            HandlePlayerInput();
        }
        else if (gameActive && !PlayerTurn)
        {
            if (CheckForWinCondition())
            {
                return;
            }
            MakeAIMove();
        }
    }

    void HandlePlayerInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            playerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<playerController>();
            if (!player || !player.playerCamera) 
            {
                return;
            }
            
            
            Ray ray = player.playerCamera.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0));
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2f);
            if (Physics.Raycast(ray, out hit))
            {
                if (selectedPiece != null)
                {
                    int spotIndex = spots.FindIndex(spot => spot.gameObject == hit.collider.gameObject);
                    if (spotIndex != -1)
                    {
                        int row = spotIndex / 8;
                        int col = spotIndex % 8;
                        Vector2Int targetPos = new Vector2Int(row, col);
                        if (IsValidMove(selectedPiecePosition, targetPos))
                        {
                            MovePiece(selectedPiecePosition, targetPos);
                            ClearHighlights();
                            if ((targetPos.x == 7 && board[targetPos.x, targetPos.y].CompareTag("White")) || 
                                (targetPos.x == 0 && board[targetPos.x, targetPos.y].CompareTag("Black")))
                            {
                                PromoteToKing(targetPos.x, targetPos.y);
                            }
                            if (!CheckForWinCondition())
                            {
                                PlayerTurn = false;
                            }
                        }
                        else
                        {
                            DeselectPiece();
                        }
                    }
                    else
                    {
                        TrySelectPiece(hit.collider.gameObject);
                    }
                }
                else
                {
                    TrySelectPiece(hit.collider.gameObject);
                }
            }
        }
    }
    
    // Try to select a game piece
    void TrySelectPiece(GameObject hitObject)
    {
        
        
        if (hitObject.CompareTag("White") || hitObject.CompareTag("WhiteKing"))
        {
            bool foundPiece = false;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (board[row, col] == hitObject)
                    {
                        foundPiece = true;
                        if (selectedPiece != null)
                        {
                            DeselectPiece();
                        }
                        selectedPiece = hitObject;
                        selectedPiecePosition = new Vector2Int(row, col);
                        validMoves = GetValidMoves(selectedPiecePosition);
                        HighlightValidMoves();
                        return;
                    }
                }
            }
        }
    }
    
    
    void DeselectPiece()
    {
        selectedPiece = null;
        selectedPiecePosition = new Vector2Int(-1, -1);
        validMoves.Clear();
        ClearHighlights();
    }
    
    // Get valid moves for a piece at the given position
    List<Vector2Int> GetValidMoves(Vector2Int position)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        
        
        if (position.x < 0 || position.x >= 8 || position.y < 0 || position.y >= 8)
        {
            return moves;
        }
        
        GameObject piece = board[position.x, position.y];
        if (piece == null)
        {
            return moves;
        }
        
        
        
        bool jumpAvailable = IsAnyJumpAvailable();
        
        
        if (jumpAvailable)
        {
            CheckJump(position, 1, 1, moves);  // Up-right
            CheckJump(position, 1, -1, moves); // Up-left
            CheckJump(position, -1, 1, moves); // Down-right
            CheckJump(position, -1, -1, moves); // Down-left
            mustJump = true;
        }
        else
        {
            mustJump = false;
            bool isKing = piece.CompareTag("WhiteKing");
            bool isWhitePiece = piece.CompareTag("White") || piece.CompareTag("WhiteKing");
            if (isWhitePiece && !isKing)
            {
                if (IsValidNormalMove(position, 1, 1))
                {
                    moves.Add(new Vector2Int(position.x + 1, position.y + 1));
                }
                if (IsValidNormalMove(position, 1, -1))
                {
                    moves.Add(new Vector2Int(position.x + 1, position.y - 1));
                }
            }
            else if (!isWhitePiece && !isKing)
            {
                if (IsValidNormalMove(position, 1, 1))
                {
                    moves.Add(new Vector2Int(position.x + 1, position.y + 1));
                }
                if (IsValidNormalMove(position, 1, -1))
                {
                    moves.Add(new Vector2Int(position.x + 1, position.y - 1));
                }
            }
            else
            {
                if (IsValidNormalMove(position, -1, 1)) // Up-right
                    moves.Add(new Vector2Int(position.x - 1, position.y + 1));
                if (IsValidNormalMove(position, -1, -1)) // Up-left
                    moves.Add(new Vector2Int(position.x - 1, position.y - 1));
                if (IsValidNormalMove(position, 1, 1)) // Down-right
                    moves.Add(new Vector2Int(position.x + 1, position.y + 1));
                if (IsValidNormalMove(position, 1, -1)) // Down-left
                    moves.Add(new Vector2Int(position.x + 1, position.y - 1));
            }
        }
        
        return moves;
    }
    
    // Check if a normal (non-jump) move is valid
    bool IsValidNormalMove(Vector2Int position, int rowDirection, int colDirection)
    {
        int newRow = position.x + rowDirection;
        int newCol = position.y + colDirection;
        
        
        if (newRow < 0 || newRow >= 8 || newCol < 0 || newCol >= 8)
        {
            return false;
        }
        
        
        bool isEmpty = board[newRow, newCol] == null;
        return isEmpty;
    }
    
    // Check for a jump move in a specific direction
    void CheckJump(Vector2Int position, int rowDirection, int colDirection, List<Vector2Int> moves)
    {
        GameObject piece = board[position.x, position.y];
        if (piece == null) return;
        
        
        bool isKing = piece.CompareTag("WhiteKing");
        bool isWhite = piece.CompareTag("White") || piece.CompareTag("WhiteKing");
        
        
        if (!isKing && ((isWhite && rowDirection < 0) || (!isWhite && rowDirection > 0)))
        {
            return;
        }
        
        int jumpRow = position.x + rowDirection;
        int jumpCol = position.y + colDirection;
        
        
        if (jumpRow >= 0 && jumpRow < 8 && jumpCol >= 0 && jumpCol < 8)
        {
            GameObject jumpPiece = board[jumpRow, jumpCol];
            
            
            if (jumpPiece != null && (jumpPiece.CompareTag("Black") || jumpPiece.CompareTag("BlackKing")))
            {
                
                int landRow = jumpRow + rowDirection;
                int landCol = jumpCol + colDirection;
                
                if (landRow >= 0 && landRow < 8 && landCol >= 0 && landCol < 8 && board[landRow, landCol] == null)
                {
                    
                    moves.Add(new Vector2Int(landRow, landCol));
                }
            }
        }
    }
    
    // Check if any jump is available on the board for white pieces (player's turn)
    bool IsAnyJumpAvailable()
    {
        int totalJumpsFound = 0;
        
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                GameObject piece = board[row, col];
                if (piece != null && (piece.CompareTag("White") || piece.CompareTag("WhiteKing")))
                {
                    Vector2Int pos = new Vector2Int(row, col);
                    List<Vector2Int> jumpMoves = new List<Vector2Int>();
                    
        
                    bool isKing = piece.CompareTag("WhiteKing");
                    
                    
                    CheckJump(pos, 1, 1, jumpMoves); // Up-right
                    CheckJump(pos, 1, -1, jumpMoves); // Up-left
                    
                    
                    if (isKing)
                    {
                        CheckJump(pos, -1, 1, jumpMoves);
                        CheckJump(pos, -1, -1, jumpMoves);
                    }
                    
                    if (jumpMoves.Count > 0)
                    {
                        totalJumpsFound += jumpMoves.Count;
                    }
                }
            }
        }
        
        return totalJumpsFound > 0;
    }
    
    
    bool IsValidMove(Vector2Int from, Vector2Int to)
    {
        foreach (Vector2Int move in validMoves)
        {
            if (move.x == to.x && move.y == to.y)
            {
                return true;
            }
        }
        return false;
    }
    
    
    void MovePiece(Vector2Int from, Vector2Int to)
    {
        
        
        GameObject piece = board[from.x, from.y];
        if (piece == null)
        {
            return;
        }
        
        int targetSpotIndex = to.x * 8 + to.y;
        if (targetSpotIndex >= spots.Count || targetSpotIndex < 0)
        {
            return;
        }
        
        Transform targetSpot = spots[targetSpotIndex];
        
        
        if (Mathf.Abs(from.x - to.x) == 2)
        {
            
            int jumpedRow = (from.x + to.x) / 2;
            int jumpedCol = (from.y + to.y) / 2;
            
            
            GameObject jumpedPiece = board[jumpedRow, jumpedCol];
            if (jumpedPiece != null)
            {
                if (jumpedPiece.CompareTag("Black") || jumpedPiece.CompareTag("BlackKing"))
                    blackPieces.Remove(jumpedPiece);
                else
                    whitePieces.Remove(jumpedPiece);
                
                Destroy(jumpedPiece);
                board[jumpedRow, jumpedCol] = null;
            }
        }
        
        
        piece.transform.position = targetSpot.position;
        
        
        board[from.x, from.y] = null;
        board[to.x, to.y] = piece;
        
        
        DeselectPiece();

        
        CheckForWinCondition();
    }
    
    
    void PromoteToKing(int row, int col)
    {
        GameObject piece = board[row, col];
        if (piece == null) return;
        
        GameObject newPiece;
        if (piece.CompareTag("White"))
        {
            
            newPiece = Instantiate(whiteKing, piece.transform.position, Quaternion.identity);
            whitePieces.Add(newPiece);
            whitePieces.Remove(piece);
        }
        else if (piece.CompareTag("Black"))
        {
            
            newPiece = Instantiate(blackKing, piece.transform.position, Quaternion.identity);
            blackPieces.Add(newPiece);
            blackPieces.Remove(piece);
        }
        else
        {
            
            return;
        }
        
        
        Destroy(piece);
        board[row, col] = newPiece;
    }
    
    
    void HighlightValidMoves()
    {
        ClearHighlights();
        
        if (highlightMaterial == null)
        {
            return;
        }
        
        foreach (Vector2Int move in validMoves)
        {
            int spotIndex = move.x * 8 + move.y;
            if (spotIndex >= spots.Count || spotIndex < 0)
            {
                continue;
            }
            Transform spot = spots[spotIndex];
            GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            highlight.transform.position = spot.position;
            highlight.transform.localScale = new Vector3(0.8f, 0.02f, 0.8f)/5;
            highlight.GetComponent<Renderer>().material = highlightMaterial;
            highlight.GetComponent<Collider>().enabled = false;
            highlightObjects.Add(highlight);
        }
    }
    
    
    void ClearHighlights()
    {
        foreach (GameObject highlight in highlightObjects)
        {
            if (highlight != null)
                Destroy(highlight);
        }
        
        highlightObjects.Clear();
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerController player = other.GetComponent<playerController>();

            if (!player.playingCheckers && !playerWon)
            {
                player.SetInteractionText("Press F to play");
                if (Input.GetKeyDown(KeyCode.F))
                {
                    player.SetInteractionText("");
                    player.playingCheckers = true;
                    SetupBoard(); 
                    gameActive = true;
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerController player = other.GetComponent<playerController>();
            if (!player.playingCheckers)
            {
                player.SetInteractionText("");
            }
            else if (!gameActive)
            {
                player.SetInteractionText("");
            }
        }
    }
    
    
    void SetupBoard()
    {
        
        ClearBoard();
        
        
        if (whitePiece == null || blackPiece == null || whiteKing == null || blackKing == null)
        {
            return;
        }
        
        if (spots == null || spots.Count < 64)
        {
            return;
        }
        
        
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                
                if ((row + col) % 2 == 1)
                {
                    
                    PlacePiece(row, col, true);
                }
            }
        }
        
        
        for (int row = 5; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if ((row + col) % 2 == 1)
                {
                    PlacePiece(row, col, false);
                }
            }
        }
        
        PlayerTurn = true;
    }
    
    
    void PlacePiece(int row, int col, bool isWhite)
    {
        
        int spotIndex = row * 8 + col;
        if (spotIndex < spots.Count)
        {
            Transform spotTransform = spots[spotIndex];
            
            
            GameObject pieceToSpawn = isWhite ? whitePiece : blackPiece;
            GameObject piece = Instantiate(pieceToSpawn, spotTransform.position, Quaternion.identity);
            
            
            piece.tag = isWhite ? "White" : "Black";
            
            
            board[row, col] = piece;
            if (isWhite)
            {
                whitePieces.Add(piece);
            }
            else
            {
                blackPieces.Add(piece);
            }
        }
    }
    
    
    void ClearBoard()
    {
        
        foreach (GameObject piece in whitePieces)
        {
            if (piece != null)
                Destroy(piece);
        }
        
        foreach (GameObject piece in blackPieces)
        {
            if (piece != null)
                Destroy(piece);
        }
        
        
        whitePieces.Clear();
        blackPieces.Clear();
        
        
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                board[row, col] = null;
            }
        }
        
        
        ClearHighlights();
        
        
        selectedPiece = null;
        selectedPiecePosition = new Vector2Int(-1, -1);
        validMoves.Clear();
    }
    
    
    void EndCheckersGame()
    {
        playerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<playerController>();
        if (player != null)
        {
            player.playingCheckers = false;
            player.firstPersonMovement.enabled = true;
            
            
            if (player.interactionText.text == "" || player.interactionText.text == "Press G to exit") 
            {
                player.SetInteractionText("Press G to exit");
            }
        }
        
        ClearBoard();
        gameActive = false;
    }

    
    void MakeAIMove()
    {
        
        int bestScore = int.MinValue;
        Vector2Int bestMoveFrom = new Vector2Int(-1, -1);
        Vector2Int bestMoveTo = new Vector2Int(-1, -1);
        
        
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                GameObject piece = board[row, col];
                if (piece != null && (piece.CompareTag("Black") || piece.CompareTag("BlackKing")))
                {
                    Vector2Int from = new Vector2Int(row, col);
                    List<Vector2Int> moves = GetValidMovesForBlack(from);
                    
                    foreach (Vector2Int to in moves)
                    {
                        
                        GameObject[,] boardCopy = CopyBoard();
                        SimulateMove(boardCopy, from, to);
                        
                        
                        int score = Minimax(boardCopy, aiDepth, false, int.MinValue, int.MaxValue); // depth = aiDepth, minimizing = false (AI is maximizing)
                        
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestMoveFrom = from;
                            bestMoveTo = to;
                        }
                    }
                }
            }
        }
        
        // Execute the best move
        if (bestMoveFrom.x != -1 && bestMoveTo.x != -1)
        {
            MovePiece(bestMoveFrom, bestMoveTo);
            if (bestMoveTo.x == 0 && board[bestMoveTo.x, bestMoveTo.y].CompareTag("Black"))
            {
                PromoteToKing(bestMoveTo.x, bestMoveTo.y);
            }
        }
        
        // Check for win conditions before ending AI's turn
        if (!CheckForWinCondition())
        {
                        
            PlayerTurn = true;
        }
    }
    
    // Minimax algorithm with alpha-beta pruning for checkers
    int Minimax(GameObject[,] board, int depth, bool isMinimizing, int alpha, int beta)
    {
        
        if (depth == 0)
        {
            return EvaluateBoard(board);
        }
        
        if (isMinimizing)
        {
            
            int bestScore = int.MaxValue;
            
            
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    GameObject piece = board[row, col];
                    if (piece != null && (piece.CompareTag("White") || piece.CompareTag("WhiteKing")))
                    {
                        Vector2Int from = new Vector2Int(row, col);
                        List<Vector2Int> moves = GetValidMovesForWhiteSimulation(board, from);
                        
                        foreach (Vector2Int to in moves)
                        {
                            
                            GameObject[,] newBoard = CopyBoard(board);
                            SimulateMove(newBoard, from, to);
                            
                            
                            int score = Minimax(newBoard, depth - 1, false, alpha, beta);
                            bestScore = Mathf.Min(score, bestScore);
                            
                            
                            beta = Mathf.Min(beta, bestScore);
                            if (beta <= alpha)
                            {
                                break;
                            }
                        }
                        
                        
                        if (beta <= alpha)
                        {
                            break;
                        }
                    }
                }
                
                
                if (beta <= alpha)
                {
                    break;
                }
            }
            
            return bestScore == int.MaxValue ? EvaluateBoard(board) : bestScore;
        }
        else
        {
            
            int bestScore = int.MinValue;
            
            
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    GameObject piece = board[row, col];
                    if (piece != null && (piece.CompareTag("Black") || piece.CompareTag("BlackKing")))
                    {
                        Vector2Int from = new Vector2Int(row, col);
                        List<Vector2Int> moves = GetValidMovesForBlackSimulation(board, from);
                        
                        foreach (Vector2Int to in moves)
                        {
                            
                            GameObject[,] newBoard = CopyBoard(board);
                            SimulateMove(newBoard, from, to);
                            
                            
                            int score = Minimax(newBoard, depth - 1, true, alpha, beta);
                            bestScore = Mathf.Max(score, bestScore);
                            
                            
                            alpha = Mathf.Max(alpha, bestScore);
                            if (beta <= alpha)
                            {
                                break;
                            }
                        }
                        
                        
                        if (beta <= alpha)
                        {
                            break;
                        }
                    }
                }
                
                
                if (beta <= alpha)
                {
                    break;
                }
            }
            
            return bestScore == int.MinValue ? EvaluateBoard(board) : bestScore;
        }
    }
    
    
    GameObject[,] CopyBoard()
    {
        GameObject[,] copy = new GameObject[8, 8];
        
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                copy[row, col] = board[row, col];
            }
        }
        
        return copy;
    }
    
    
    GameObject[,] CopyBoard(GameObject[,] sourceBoard)
    {
        GameObject[,] copy = new GameObject[8, 8];
        
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                copy[row, col] = sourceBoard[row, col];
            }
        }
        
        return copy;
    }
    
    
    void SimulateMove(GameObject[,] boardState, Vector2Int from, Vector2Int to)
    {
        
        GameObject piece = boardState[from.x, from.y];
        
        
        if (Mathf.Abs(from.x - to.x) == 2)
        {
            
            int jumpedRow = (from.x + to.x) / 2;
            int jumpedCol = (from.y + to.y) / 2;
            
            
            boardState[jumpedRow, jumpedCol] = null;
        }
        
        
        boardState[to.x, to.y] = piece;
        boardState[from.x, from.y] = null;
    }
    
    
    int EvaluateBoard(GameObject[,] boardState)
    {
        int score = 0;
        int blackPieceCount = 0;
        int whitePieceCount = 0;
        
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                GameObject piece = boardState[row, col];
                if (piece != null)
                {
                    if (piece.CompareTag("White"))
                    {
                        score -= 1;
                        whitePieceCount++;
                        
                        score -= row / 10;
                    }
                    else if (piece.CompareTag("WhiteKing"))
                    {
                        score -= 3;
                        whitePieceCount++;
                    }
                    else if (piece.CompareTag("Black"))
                    {
                        score += 1;
                        blackPieceCount++;
                        
                        score += (7 - row) / 10;
                    }
                    else if (piece.CompareTag("BlackKing"))
                    {
                        score += 3;
                        blackPieceCount++;
                    }
                }
            }
        }
        
        
        if (blackPieceCount > 0 && whitePieceCount == 0)
            score += 100;
        else if (whitePieceCount > 0 && blackPieceCount == 0)
            score -= 100;
        else
            score += (blackPieceCount - whitePieceCount) / 2;
            
        return score;
    }
    
    
    List<Vector2Int> GetValidMovesForBlack(Vector2Int position)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        GameObject piece = board[position.x, position.y];
        if (piece == null || !(piece.CompareTag("Black") || piece.CompareTag("BlackKing")))
            return moves;
            
        
        bool jumpAvailable = IsAnyJumpAvailableForBlack();
        
        if (jumpAvailable)
        {
            CheckJumpForBlack(position, 1, 1, moves);
            CheckJumpForBlack(position, 1, -1, moves);
            CheckJumpForBlack(position, -1, 1, moves);
            CheckJumpForBlack(position, -1, -1, moves);
        }
        else
        {
            
            bool isKing = piece.CompareTag("BlackKing");
            
            if (!isKing)
            {
                if (IsValidNormalMoveForBlack(position, -1, 1))
                    moves.Add(new Vector2Int(position.x - 1, position.y + 1));
                if (IsValidNormalMoveForBlack(position, -1, -1))
                    moves.Add(new Vector2Int(position.x - 1, position.y - 1));
            }
            else
            {
                
                if (IsValidNormalMoveForBlack(position, -1, 1))
                    moves.Add(new Vector2Int(position.x - 1, position.y + 1));
                if (IsValidNormalMoveForBlack(position, -1, -1))
                    moves.Add(new Vector2Int(position.x - 1, position.y - 1));
                if (IsValidNormalMoveForBlack(position, 1, 1))
                    moves.Add(new Vector2Int(position.x + 1, position.y + 1));
                if (IsValidNormalMoveForBlack(position, 1, -1))
                    moves.Add(new Vector2Int(position.x + 1, position.y - 1));
            }
        }
        
        return moves;
    }
    
    
    List<Vector2Int> GetValidMovesForBlackSimulation(GameObject[,] boardState, Vector2Int position)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        GameObject piece = boardState[position.x, position.y];
        if (piece == null || !(piece.CompareTag("Black") || piece.CompareTag("BlackKing")))
            return moves;
            
        
        bool isKing = piece.CompareTag("BlackKing");
        
        if (!isKing)
        {
            
            if (IsValidNormalMoveSimulation(boardState, position, -1, 1))
                moves.Add(new Vector2Int(position.x - 1, position.y + 1));
            if (IsValidNormalMoveSimulation(boardState, position, -1, -1))
                moves.Add(new Vector2Int(position.x - 1, position.y - 1));
        }
        else
        {
            
            if (IsValidNormalMoveSimulation(boardState, position, -1, 1))
                moves.Add(new Vector2Int(position.x - 1, position.y + 1));
            if (IsValidNormalMoveSimulation(boardState, position, -1, -1))
                moves.Add(new Vector2Int(position.x - 1, position.y - 1));
            if (IsValidNormalMoveSimulation(boardState, position, 1, 1))
                moves.Add(new Vector2Int(position.x + 1, position.y + 1));
            if (IsValidNormalMoveSimulation(boardState, position, 1, -1))
                moves.Add(new Vector2Int(position.x + 1, position.y - 1));
        }
        
        return moves;
    }
    
    
    List<Vector2Int> GetValidMovesForWhiteSimulation(GameObject[,] boardState, Vector2Int position)
    {
        List<Vector2Int> moves = new List<Vector2Int>();
        
        GameObject piece = boardState[position.x, position.y];
        if (piece == null || !(piece.CompareTag("White") || piece.CompareTag("WhiteKing")))
            return moves;
            
        
        bool isKing = piece.CompareTag("WhiteKing");
        
        if (!isKing)
        {
            
            if (IsValidNormalMoveSimulation(boardState, position, 1, 1))
                moves.Add(new Vector2Int(position.x + 1, position.y + 1));
            if (IsValidNormalMoveSimulation(boardState, position, 1, -1))
                moves.Add(new Vector2Int(position.x + 1, position.y - 1));
        }
        else
        {
            
            if (IsValidNormalMoveSimulation(boardState, position, 1, 1))
                moves.Add(new Vector2Int(position.x + 1, position.y + 1));
            if (IsValidNormalMoveSimulation(boardState, position, 1, -1))
                moves.Add(new Vector2Int(position.x + 1, position.y - 1));
            if (IsValidNormalMoveSimulation(boardState, position, -1, 1))
                moves.Add(new Vector2Int(position.x - 1, position.y + 1));
            if (IsValidNormalMoveSimulation(boardState, position, -1, -1))
                moves.Add(new Vector2Int(position.x - 1, position.y - 1));
        }
        
        return moves;
    }
    
    
    bool IsValidNormalMoveForBlack(Vector2Int position, int rowDirection, int colDirection)
    {
        int newRow = position.x + rowDirection;
        int newCol = position.y + colDirection;
        
        
        if (newRow < 0 || newRow >= 8 || newCol < 0 || newCol >= 8)
            return false;
        
        
        return board[newRow, newCol] == null;
    }
    
    
    bool IsValidNormalMoveSimulation(GameObject[,] boardState, Vector2Int position, int rowDirection, int colDirection)
    {
        int newRow = position.x + rowDirection;
        int newCol = position.y + colDirection;
        
        
        if (newRow < 0 || newRow >= 8 || newCol < 0 || newCol >= 8)
            return false;
        
        
        return boardState[newRow, newCol] == null;
    }
    
    
    void CheckJumpForBlack(Vector2Int position, int rowDirection, int colDirection, List<Vector2Int> moves)
    {
        GameObject piece = board[position.x, position.y];
        if (piece == null) return;
        
        bool isKing = piece.CompareTag("BlackKing");
        
        
        if (!isKing && rowDirection > 0)
            return;
        
        int jumpRow = position.x + rowDirection;
        int jumpCol = position.y + colDirection;
        
        
        if (jumpRow >= 0 && jumpRow < 8 && jumpCol >= 0 && jumpCol < 8)
        {
            GameObject jumpPiece = board[jumpRow, jumpCol];
            
            
            if (jumpPiece != null && (jumpPiece.CompareTag("White") || jumpPiece.CompareTag("WhiteKing")))
            {
                
                int landRow = jumpRow + rowDirection;
                int landCol = jumpCol + colDirection;
                
                if (landRow >= 0 && landRow < 8 && landCol >= 0 && landCol < 8 && board[landRow, landCol] == null)
                {
                    
                    moves.Add(new Vector2Int(landRow, landCol));
                }
            }
        }
    }
    
    
    bool IsAnyJumpAvailableForBlack()
    {
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                GameObject piece = board[row, col];
                if (piece != null && (piece.CompareTag("Black") || piece.CompareTag("BlackKing")))
                {
                    Vector2Int pos = new Vector2Int(row, col);
                    List<Vector2Int> jumpMoves = new List<Vector2Int>();
                    
                    bool isKing = piece.CompareTag("BlackKing");
                    
                    
                    CheckJumpForBlack(pos, -1, 1, jumpMoves);  // Down-right
                    CheckJumpForBlack(pos, -1, -1, jumpMoves); // Down-left
                    
                    
                    if (isKing)
                    {
                        CheckJumpForBlack(pos, 1, 1, jumpMoves);
                        CheckJumpForBlack(pos, 1, -1, jumpMoves);
                    }
                    
                    if (jumpMoves.Count > 0)
                        return true;
                }
            }
        }
        
        return false;
    }
    
    
    bool CheckForWinCondition()
    {
        
        if (whitePieces.Count == 0)
        {
            DisplayWinner("Black");
            return true;
        }
        
        
        if (blackPieces.Count == 0)
        {
            DisplayWinner("White");
            return true;
        }
        
        
        bool whiteHasValidMoves = false;
        for (int row = 0; row < 8 && !whiteHasValidMoves; row++)
        {
            for (int col = 0; col < 8 && !whiteHasValidMoves; col++)
            {
                GameObject piece = board[row, col];
                if (piece != null && (piece.CompareTag("White") || piece.CompareTag("WhiteKing")))
                {
                    List<Vector2Int> moves = GetValidMoves(new Vector2Int(row, col));
                    if (moves.Count > 0)
                    {
                        whiteHasValidMoves = true;
                        break;
                    }
                }
            }
        }
        
        if (!whiteHasValidMoves && whitePieces.Count > 0)
        {
            DisplayWinner("Black");
            return true;
        }
        
        
        bool blackHasValidMoves = false;
        for (int row = 0; row < 8 && !blackHasValidMoves; row++)
        {
            for (int col = 0; col < 8 && !blackHasValidMoves; col++)
            {
                GameObject piece = board[row, col];
                if (piece != null && (piece.CompareTag("Black") || piece.CompareTag("BlackKing")))
                {
                    List<Vector2Int> moves = GetValidMovesForBlack(new Vector2Int(row, col));
                    if (moves.Count > 0)
                    {
                        blackHasValidMoves = true;
                        break;
                    }
                }
            }
        }
        
        if (!blackHasValidMoves && blackPieces.Count > 0)
        {
            DisplayWinner("White");
            return true;
        }
        
        return false;
    }
    
    
    void DisplayWinner(string winner)
    {
        playerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<playerController>();
        if (player != null)
        {
            playerWon = true;
            player.SetInteractionText(winner + " wins! Press G to exit. (this is the end of the game :)");
        }
    }
}
