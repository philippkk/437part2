using System.Collections.Generic;
using UnityEngine;

public class checkersController : MonoBehaviour
{
    public bool PlayerTurn = true;
    public GameObject checkersBoard;
    // stored from bottom left to top right. 
    public List<Transform> spots = new List<Transform>();
    public GameObject whitePiece;
    public GameObject whiteKing;
    public GameObject blackPiece;
    public GameObject blackKing;

    // Game state
    private GameObject[,] board = new GameObject[8, 8]; // 2D array to keep track of pieces
    private List<GameObject> whitePieces = new List<GameObject>();
    private List<GameObject> blackPieces = new List<GameObject>();
    private bool gameActive = false;
    
    // Piece selection and movement
    private GameObject selectedPiece = null;
    private Vector2Int selectedPiecePosition = new Vector2Int(-1, -1);
    private List<Vector2Int> validMoves = new List<Vector2Int>();
    private bool mustJump = false;
    
    // Highlight effects for valid move positions
    public Material highlightMaterial;
    private List<GameObject> highlightObjects = new List<GameObject>();
    
    void Start()
    {
        // Check if spots have colliders
        Debug.Log("Total spots: " + spots.Count);
        int spotsMissingColliders = 0;
        
        for (int i = 0; i < spots.Count; i++)
        {
            if (spots[i] != null && spots[i].GetComponent<Collider>() == null)
            {
                spotsMissingColliders++;
                Debug.LogWarning("Spot " + i + " is missing a collider!");
            }
        }
        
        if (spotsMissingColliders > 0)
        {
            Debug.LogError("Found " + spotsMissingColliders + " spots without colliders!");
        }
        
        // Check if the player camera is working correctly
        playerController player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<playerController>();
        if (player == null)
        {
            Debug.LogError("Player not found or missing playerController component!");
        }
        else if (player.playerCamera == null)
        {
            Debug.LogError("Player camera reference is missing!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameActive && Input.GetKeyDown(KeyCode.G))
        {
            // Allow player to exit the game
            EndCheckersGame();
        }
        
        if (gameActive && PlayerTurn)
        {
            HandlePlayerInput();
        }
    }

    void HandlePlayerInput()
    {            // Only process mouse input on left button click
        if (Input.GetMouseButtonDown(0))
        {
            playerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<playerController>();
            if (!player || !player.playerCamera) 
            {
                Debug.LogError("Player or player camera not found!");
                return;
            }
            
            // Cast ray from camera
            Ray ray = player.playerCamera.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2, 0));
            RaycastHit hit;
            Debug.DrawRay(ray.origin, ray.direction * 100, Color.red, 2f);
            Debug.Log("Casting ray from camera. Origin: " + ray.origin + ", Direction: " + ray.direction);
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Ray hit: " + hit.collider.gameObject.name + " with tag: " + hit.collider.gameObject.tag);
                // If we already selected a piece, try to move it
                if (selectedPiece != null)
                {
                    // Check if we hit a board position
                    int spotIndex = spots.FindIndex(spot => spot.gameObject == hit.collider.gameObject);
                    Debug.Log("Looking for spot. Hit object: " + hit.collider.gameObject.name + ", Found spot index: " + spotIndex);
                    if (spotIndex != -1)
                    {
                        // Convert spot index to board position
                        int row = spotIndex / 8;
                        int col = spotIndex % 8;
                        Vector2Int targetPos = new Vector2Int(row, col);
                        Debug.Log("Target position: " + targetPos + " (row: " + row + ", col: " + col + ")");
                        
                        // Check if the move is valid
                        Debug.Log("Checking if move is valid from " + selectedPiecePosition + " to " + targetPos);
                        if (IsValidMove(selectedPiecePosition, targetPos))
                        {
                            Debug.Log("Move is valid! Moving piece from " + selectedPiecePosition + " to " + targetPos);
                            // Move the piece
                            MovePiece(selectedPiecePosition, targetPos);
                            ClearHighlights();
                            
                            // Check if the piece needs to be kinged
                            if ((targetPos.x == 7 && board[targetPos.x, targetPos.y].CompareTag("White")) || 
                                (targetPos.x == 0 && board[targetPos.x, targetPos.y].CompareTag("Black")))
                            {
                                // Piece reached the opposite end of the board, make it a king
                                PromoteToKing(targetPos.x, targetPos.y);
                            }
                            
                            // End player turn
                            PlayerTurn = false;
                            
                            // Start AI turn (will be implemented with minimax)
                            // TODO: Add AI move logic here
                            // For now, just switch back to player's turn
                            PlayerTurn = true;
                        }
                        else
                        {
                            // Not a valid move position - deselect the current piece
                            DeselectPiece();
                        }
                    }
                    else
                    {
                        // Didn't hit a board spot, try to select a new piece instead
                        TrySelectPiece(hit.collider.gameObject);
                    }
                }
                else
                {
                    // No piece selected yet, try to select one
                    TrySelectPiece(hit.collider.gameObject);
                }
            }
        }
    }
    
    // Try to select a game piece
    void TrySelectPiece(GameObject hitObject)
    {
        Debug.Log("Attempting to select object: " + hitObject.name + " with tag: " + hitObject.tag);
        
        // Check if we're selecting a piece that belongs to the player
        if (hitObject.CompareTag("White") || hitObject.CompareTag("WhiteKing"))
        {
            Debug.Log("Selected a player piece: " + hitObject.name);
            
            // Find the piece position on the board
            bool foundPiece = false;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (board[row, col] == hitObject)
                    {
                        foundPiece = true;
                        Debug.Log("Found piece on board at position: row=" + row + ", col=" + col);
                        
                        // Deselect previous piece if any
                        if (selectedPiece != null)
                        {
                            Debug.Log("Deselecting previous piece");
                            DeselectPiece();
                        }
                        
                        // Select this piece
                        selectedPiece = hitObject;
                        selectedPiecePosition = new Vector2Int(row, col);
                        Debug.Log("Selected piece at position: " + selectedPiecePosition);

                        // Get valid moves for this piece
                        validMoves = GetValidMoves(selectedPiecePosition);
                        Debug.Log("Found " + validMoves.Count + " valid moves for selected piece");

                        // Highlight valid moves
                        HighlightValidMoves();

                        return;
                    }
                }
            }
            
            if (!foundPiece)
            {
                Debug.LogWarning("Selected piece not found in board array!");
            }
        }
        else
        {
            Debug.Log("Cannot select this object - not a player piece");
        }
    }
    
    // Deselect the currently selected piece
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
        
        Debug.Log("Getting valid moves for piece at position: " + position);
        
        // Validate the position and piece
        if (position.x < 0 || position.x >= 8 || position.y < 0 || position.y >= 8)
        {
            Debug.LogError("Invalid position: " + position);
            return moves;
        }
        
        GameObject piece = board[position.x, position.y];
        if (piece == null)
        {
            Debug.LogError("No piece at position: " + position);
            return moves;
        }
        
        Debug.Log("Piece at position: " + position + " is " + piece.tag);
        
        // First check for any mandatory jumps across the entire board
        bool jumpAvailable = IsAnyJumpAvailable();
        Debug.Log("Jump available anywhere on board: " + jumpAvailable);
        
        // If jump is available, only return jump moves
        if (jumpAvailable)
        {
            Debug.Log("Checking for jump moves for piece at " + position);
            // Check the four diagonal directions for jumps
            CheckJump(position, 1, 1, moves);  // Up-right
            CheckJump(position, 1, -1, moves); // Up-left
            CheckJump(position, -1, 1, moves); // Down-right
            CheckJump(position, -1, -1, moves); // Down-left
            
            mustJump = true;
            Debug.Log("Found " + moves.Count + " jump moves for piece at " + position);
        }
        else
        {
            Debug.Log("Checking for normal moves for piece at " + position);
            // No jumps available, check normal moves
            mustJump = false;
            
            // Determine if the piece is a king
            bool isKing = piece.CompareTag("WhiteKing");
            bool isWhitePiece = piece.CompareTag("White") || piece.CompareTag("WhiteKing");
            
            Debug.Log("Piece is: " + (isKing ? "King" : "Regular") + ", " + (isWhitePiece ? "White" : "Black"));
            
            // Regular white pieces should move up the board (positive row direction)
            if (isWhitePiece && !isKing)
            {
                // Check up-right (which is actually increasing row)
                if (IsValidNormalMove(position, 1, 1))
                {
                    moves.Add(new Vector2Int(position.x + 1, position.y + 1));
                    Debug.Log("Valid move found: up-right to " + new Vector2Int(position.x + 1, position.y + 1));
                }
                
                // Check up-left (which is actually increasing row)
                if (IsValidNormalMove(position, 1, -1))
                {
                    moves.Add(new Vector2Int(position.x + 1, position.y - 1));
                    Debug.Log("Valid move found: up-left to " + new Vector2Int(position.x + 1, position.y - 1));
                }
            }
            else if (!isWhitePiece && !isKing)
            {
                // Regular black pieces can only move down (positive row direction)
                // Check down-right
                if (IsValidNormalMove(position, 1, 1))
                {
                    moves.Add(new Vector2Int(position.x + 1, position.y + 1));
                    Debug.Log("Valid move found: down-right to " + new Vector2Int(position.x + 1, position.y + 1));
                }
                
                // Check down-left
                if (IsValidNormalMove(position, 1, -1))
                {
                    moves.Add(new Vector2Int(position.x + 1, position.y - 1));
                    Debug.Log("Valid move found: down-left to " + new Vector2Int(position.x + 1, position.y - 1));
                }
            }
            else
            {
                // Kings can move in all four diagonal directions
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
        
        // Check if the new position is on the board
        if (newRow < 0 || newRow >= 8 || newCol < 0 || newCol >= 8)
        {
            Debug.Log("Move from " + position + " in direction (" + rowDirection + "," + colDirection + ") is off the board");
            return false;
        }
        
        // Check if the new position is empty
        bool isEmpty = board[newRow, newCol] == null;
        if (!isEmpty)
        {
            Debug.Log("Move from " + position + " to (" + newRow + "," + newCol + ") is blocked by another piece");
        }
        return isEmpty;
    }
    
    // Check for a jump move in a specific direction
    void CheckJump(Vector2Int position, int rowDirection, int colDirection, List<Vector2Int> moves)
    {
        GameObject piece = board[position.x, position.y];
        if (piece == null) return;
        
        // Determine if the piece is a king
        bool isKing = piece.CompareTag("WhiteKing");
        bool isWhite = piece.CompareTag("White") || piece.CompareTag("WhiteKing");
        
        // Regular white pieces can only move up (positive row direction)
        // Regular black pieces can only move down (negative row direction)
        if (!isKing && ((isWhite && rowDirection < 0) || (!isWhite && rowDirection > 0)))
        {
            Debug.Log("Skipping jump check for piece at " + position + " in direction " + rowDirection + "," + colDirection + " because it's not valid for this piece type");
            return;
        }
        
        int jumpRow = position.x + rowDirection;
        int jumpCol = position.y + colDirection;
        
        // Check if there's a piece to jump over
        if (jumpRow >= 0 && jumpRow < 8 && jumpCol >= 0 && jumpCol < 8)
        {
            GameObject jumpPiece = board[jumpRow, jumpCol];
            
            // Check if there's an opponent's piece to jump over
            if (jumpPiece != null && (jumpPiece.CompareTag("Black") || jumpPiece.CompareTag("BlackKing")))
            {
                // Check if landing spot is on the board and empty
                int landRow = jumpRow + rowDirection;
                int landCol = jumpCol + colDirection;
                
                if (landRow >= 0 && landRow < 8 && landCol >= 0 && landCol < 8 && board[landRow, landCol] == null)
                {
                    // Valid jump
                    moves.Add(new Vector2Int(landRow, landCol));
                }
            }
        }
    }
    
    // Check if any jump is available on the board for white pieces (player's turn)
    bool IsAnyJumpAvailable()
    {
        Debug.Log("Checking for any available jumps on the board");
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
                    
                    // Check the four diagonal directions for jumps
                    bool isKing = piece.CompareTag("WhiteKing");
                    
                    // Up directions (valid for all white pieces) - increasing row value
                    CheckJump(pos, 1, 1, jumpMoves); // Up-right
                    CheckJump(pos, 1, -1, jumpMoves); // Up-left
                    
                    // Down directions (only valid for kings) - decreasing row value
                    if (isKing)
                    {
                        CheckJump(pos, -1, 1, jumpMoves);  // Down-right
                        CheckJump(pos, -1, -1, jumpMoves); // Down-left
                    }
                    
                    if (jumpMoves.Count > 0)
                    {
                        Debug.Log("Jump found for piece at " + pos + ": " + jumpMoves.Count + " possible jumps");
                        totalJumpsFound += jumpMoves.Count;
                    }
                }
            }
        }
        
        Debug.Log("Total jumps found across the board: " + totalJumpsFound);
        return totalJumpsFound > 0;
    }
    
    // Check if a specific move is in the valid moves list
    bool IsValidMove(Vector2Int from, Vector2Int to)
    {
        Debug.Log("Checking if move is valid. Valid moves count: " + validMoves.Count);
        foreach (Vector2Int move in validMoves)
        {
            Debug.Log("Comparing valid move " + move + " with target " + to);
            if (move.x == to.x && move.y == to.y)
            {
                Debug.Log("Found valid move!");
                return true;
            }
        }
        Debug.LogWarning("Move not found in valid moves list");
        return false;
    }
    
    // Move a piece from one position to another
    void MovePiece(Vector2Int from, Vector2Int to)
    {
        Debug.Log("Moving piece from " + from + " to " + to);
        
        // Get the piece and the target spot
        GameObject piece = board[from.x, from.y];
        if (piece == null)
        {
            Debug.LogError("No piece at the selected position " + from);
            return;
        }
        
        int targetSpotIndex = to.x * 8 + to.y;
        if (targetSpotIndex >= spots.Count || targetSpotIndex < 0)
        {
            Debug.LogError("Target spot index out of range: " + targetSpotIndex);
            return;
        }
        
        Transform targetSpot = spots[targetSpotIndex];
        Debug.Log("Moving " + piece.name + " to " + targetSpot.position);
        
        // Check if this is a jump move
        if (Mathf.Abs(from.x - to.x) == 2)
        {
            // Calculate the position of the jumped piece
            int jumpedRow = (from.x + to.x) / 2;
            int jumpedCol = (from.y + to.y) / 2;
            
            // Remove the jumped piece
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
        
        // Move the piece
        piece.transform.position = targetSpot.position;
        
        // Update the board
        board[from.x, from.y] = null;
        board[to.x, to.y] = piece;
        
        // Deselect the piece
        DeselectPiece();
    }
    
    // Promote a piece to king
    void PromoteToKing(int row, int col)
    {
        GameObject piece = board[row, col];
        if (piece == null) return;
        
        GameObject newPiece;
        if (piece.CompareTag("White"))
        {
            // Replace with white king
            newPiece = Instantiate(whiteKing, piece.transform.position, Quaternion.identity);
            whitePieces.Remove(piece);
            whitePieces.Add(newPiece);
        }
        else if (piece.CompareTag("Black"))
        {
            // Replace with black king
            newPiece = Instantiate(blackKing, piece.transform.position, Quaternion.identity);
            blackPieces.Remove(piece);
            blackPieces.Add(newPiece);
        }
        else
        {
            // Not a regular piece
            return;
        }
        
        // Replace on the board
        Destroy(piece);
        board[row, col] = newPiece;
    }
    
    // Create highlight objects for valid moves
    void HighlightValidMoves()
    {
        Debug.Log("Highlighting valid moves. Count: " + validMoves.Count);
        ClearHighlights();
        
        if (highlightMaterial == null)
        {
            Debug.LogError("Highlight material is not assigned!");
            return;
        }
        
        foreach (Vector2Int move in validMoves)
        {
            // Get the target spot position
            int spotIndex = move.x * 8 + move.y;
            Debug.Log("Looking for spot at index: " + spotIndex + " (row: " + move.x + ", col: " + move.y + ")");
            
            if (spotIndex >= spots.Count || spotIndex < 0)
            {
                Debug.LogError("Spot index out of range: " + spotIndex);
                continue;
            }
            
            Transform spot = spots[spotIndex];
            
            // Create a highlight object
            GameObject highlight = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            highlight.transform.position = spot.position;
            highlight.transform.localScale = new Vector3(0.8f, 0.02f, 0.8f)/5;
            highlight.GetComponent<Renderer>().material = highlightMaterial;
            highlight.GetComponent<Collider>().enabled = false;
            
            highlightObjects.Add(highlight);
            Debug.Log("Created highlight at position: " + highlight.transform.position);
        }
    }
    
    // Clear highlight objects
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

            if (!player.playingCheckers)
            {
                player.SetInteractionText("Press F to play");
                if (Input.GetKeyDown(KeyCode.F))
                {
                    player.SetInteractionText("");
                    player.playingCheckers = true;
                    SetupBoard(); // add checkers pieces to the board
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
        }
    }
    
    // Sets up the initial checkers board with pieces
    void SetupBoard()
    {
        Debug.Log("Setting up checkers board");
        // Clear any existing pieces
        ClearBoard();
        
        // Validate that we have all required pieces and spots
        if (whitePiece == null || blackPiece == null || whiteKing == null || blackKing == null)
        {
            Debug.LogError("Missing piece prefabs! White: " + (whitePiece != null) + 
                          ", Black: " + (blackPiece != null) + 
                          ", WhiteKing: " + (whiteKing != null) + 
                          ", BlackKing: " + (blackKing != null));
            return;
        }
        
        if (spots == null || spots.Count < 64)
        {
            Debug.LogError("Not enough spots on the board! Count: " + (spots != null ? spots.Count.ToString() : "null"));
            return;
        }
        
        // Place white pieces (player's pieces) on rows 0-2
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                // Checkers are only placed on dark squares, which are (row+col) % 2 == 1
                if ((row + col) % 2 == 1)
                {
                    // Create a white piece at this position
                    PlacePiece(row, col, true);
                }
            }
        }
        
        // Place black pieces (AI's pieces) on rows 5-7
        for (int row = 5; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                if ((row + col) % 2 == 1)
                {
                    // Create a black piece at this position
                    PlacePiece(row, col, false);
                }
            }
        }
        
        PlayerTurn = true; // Player goes first
    }
    
    // Place a piece on the board at the specified position
    void PlacePiece(int row, int col, bool isWhite)
    {
        // Get the transform position from the spots list
        int spotIndex = row * 8 + col;
        if (spotIndex < spots.Count)
        {
            Transform spotTransform = spots[spotIndex];
            
            // Create the appropriate game object
            GameObject pieceToSpawn = isWhite ? whitePiece : blackPiece;
            GameObject piece = Instantiate(pieceToSpawn, spotTransform.position, Quaternion.identity);
            
            // Ensure proper tagging for piece identification
            piece.tag = isWhite ? "White" : "Black";
            Debug.Log("Placed " + (isWhite ? "White" : "Black") + " piece at row: " + row + ", col: " + col + ", spot index: " + spotIndex);
            
            // Add to our tracking collections
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
    
    // Clear all pieces from the board
    void ClearBoard()
    {
        // Destroy all existing pieces
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
        
        // Clear our collections
        whitePieces.Clear();
        blackPieces.Clear();
        
        // Reset the board array
        for (int row = 0; row < 8; row++)
        {
            for (int col = 0; col < 8; col++)
            {
                board[row, col] = null;
            }
        }
        
        // Clear highlights
        ClearHighlights();
        
        // Reset selection
        selectedPiece = null;
        selectedPiecePosition = new Vector2Int(-1, -1);
        validMoves.Clear();
    }
    
    // End the checkers game and cleanup
    void EndCheckersGame()
    {
        playerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<playerController>();
        if (player != null)
        {
            player.playingCheckers = false;
            player.firstPersonMovement.enabled = true;
        }
        
        ClearBoard();
        gameActive = false;
    }
}
