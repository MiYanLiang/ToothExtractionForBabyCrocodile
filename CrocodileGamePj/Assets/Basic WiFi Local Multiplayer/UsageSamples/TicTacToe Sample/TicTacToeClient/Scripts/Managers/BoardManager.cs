using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardManager : MonoBehaviour {

    public static BoardManager instance;

	// store the buttons present in PanelGame
	public GameObject[] tiles;

	// sprite representing empty cells
	public GameObject spriteEmptyTilePref;

	// sprites to draw X markings
	public GameObject spriteXTilePref;

	// sprite to draw O markings
	public GameObject spriteSquareTilePref;

	// this variable is set by the CustomButtonEvent script marking the line index corresponding to the button chosen by the user
	public int current_i;

	// this variable is set by the CustomButtonEvent script marking the column index corresponding to the button chosen by the user
	public int current_j;

	public int fullcells = 0;


	// matrix to store button tray cells
	private GameObject[,] board = new GameObject[3,3]{
		                                               {null,null,null},
		                                               {null,null,null},
		                                               {null,null,null},
		                                             };	
	
	
	// Use this for initialization
	void Start () {
	
	if (instance == null) {

			instance = this;		

		}
		
	}
	

	/// <summary>
	/// Loads the board.
	/// </summary>
	public void LoadBoard()
	{
	  
		board [0, 0] = tiles [0];
		board [0, 1] = tiles [1];
		board [0, 2] = tiles [2];
		board [1, 0] = tiles [3];
		board [1, 1] = tiles [4];
		board [1, 2] = tiles [5];
		board [2, 0] = tiles [6];
		board [2, 1] = tiles [7];
		board [2, 2] = tiles [8];

	
	}

	/// <summary>
	/// method called when the user presses a button on the board
	/// </summary>
	public void ProcessMove()
	{

		// check the user's current turn
		if (TicTacNetworkManager.instance.myTurn) {

			// trigger button audio
			TicTacCanvasManager.instance.PlayAudio (TicTacCanvasManager.instance.buttonSound);

			// check if the user is an X or O
			if (TicTacNetworkManager.instance.GetPlayerType ().Equals ("square")) {

				// change the sprite of the chosen button to O
				SpawnSquare ();
			
			} else {

				// change the sprite of the chosen button to X
				SpawnX ();
			}
		}
	}

	/// <summary>
	/// change the button sprite and transmit the new board update to TicTacToeServer.cs.
	/// </summary>
	public void SpawnSquare()
	{


		// check if the user chose an empty cell
		if (board [current_i, current_j].GetComponent<Tile> ().GetTileType ().Equals ("none"))
		{
			
			// change the cell sprite
			board [current_i, current_j].GetComponent<Image>().sprite = spriteSquareTilePref.GetComponent<SpriteRenderer> ().sprite;

			// update the Tile script with the new cell type, in this case an O
			board [current_i, current_j].GetComponent<Tile> ().SetTileType ("square");

			// check if it's the player's current turn
			if(TicTacNetworkManager.instance.myTurn)
			{
				//transmite ao tictacToeServer a nova atualização do tabuleiro
				TicTacNetworkManager.instance.EmitUpdateBoard(current_i, current_j);



				/*********** CHECK PLAYER WINNING CONDITIONS **********************/

				bool win = false;
				// O|*|*
				// O|*|*
				// O|*|*
				if(board[0,0].GetComponent<Tile>().GetTileType().Equals("square") 
					&& board[1,0].GetComponent<Tile>().GetTileType().Equals("square")
					&& board[2,0].GetComponent<Tile>().GetTileType().Equals("square"))
				{
					//player win
					win = true;
					ResetGame();
				}

				// O|*|*
				// *|O|*
				// *|*|O
				if(board[0,1].GetComponent<Tile>().GetTileType().Equals("square") 
					&& board[1,1].GetComponent<Tile>().GetTileType().Equals("square")
					&& board[2,1].GetComponent<Tile>().GetTileType().Equals("square"))
				{
					//player win
					win = true;
					ResetGame();
				}


				// *|*|O
				// *|*|O
				// *|*|O
				if(board[0,2].GetComponent<Tile>().GetTileType().Equals("square") 
					&& board[1,2].GetComponent<Tile>().GetTileType().Equals("square")
					&& board[2,2].GetComponent<Tile>().GetTileType().Equals("square"))
				{
					//player win
					win = true;
					ResetGame();
				}

				// O|O|O
				// *|*|*
				// *|*|*
				if(board[0,0].GetComponent<Tile>().GetTileType().Equals("square") 
					&& board[0,1].GetComponent<Tile>().GetTileType().Equals("square")
					&& board[0,2].GetComponent<Tile>().GetTileType().Equals("square"))
				{
					//player win
					win = true;
					ResetGame();
				}

				// *|*|*
				// O|O|O
				// *|*|*
				if(board[1,0].GetComponent<Tile>().GetTileType().Equals("square") 
					&& board[1,1].GetComponent<Tile>().GetTileType().Equals("square")
					&& board[1,2].GetComponent<Tile>().GetTileType().Equals("square"))
				{
					//player win
					win = true;
					ResetGame();
				}

				if(board[2,0].GetComponent<Tile>().GetTileType().Equals("square") 
					&& board[2,1].GetComponent<Tile>().GetTileType().Equals("square")
					&& board[2,2].GetComponent<Tile>().GetTileType().Equals("square"))
				{
					//player win
					win = true;
					ResetGame();
				}

				if(board[0,0].GetComponent<Tile>().GetTileType().Equals("square") 
					&& board[1,1].GetComponent<Tile>().GetTileType().Equals("square")
					&& board[2,2].GetComponent<Tile>().GetTileType().Equals("square"))
				{
					//player win
					win = true;
					ResetGame();
				}

				if(board[2,0].GetComponent<Tile>().GetTileType().Equals("square") 
					&& board[1,1].GetComponent<Tile>().GetTileType().Equals("square")
					&& board[0,2].GetComponent<Tile>().GetTileType().Equals("square"))
				{
					//player win
					win = true;
					ResetGame();
				}



				if(board[0,2].GetComponent<Tile>().GetTileType().Equals("square") 
					&& board[1,1].GetComponent<Tile>().GetTileType().Equals("square")
					&& board[2,0].GetComponent<Tile>().GetTileType().Equals("square"))
				{
					//player win
					win = true;
					ResetGame();
				}


				//empate
				if (!win && VerifyTie ())
				{
					ResetGame();
				}

			}


		}
	 
	
	}
	
	public void SpawnX()
	{
		// check if the user chose an empty cell
		if (board [current_i, current_j].GetComponent<Tile> ().GetTileType ().Equals ("none"))
		{
			// change the cell sprite
			board [current_i, current_j].GetComponent<Image>().sprite = spriteXTilePref.GetComponent<SpriteRenderer> ().sprite;

			// update the Tile script with the new cell type, in this case an X
			board [current_i, current_j].GetComponent<Tile> ().SetTileType ("x");

			// check if it's the player's current turn
			if(TicTacNetworkManager.instance.myTurn)
			{

				// send update to ticTacToeServer
				TicTacNetworkManager.instance.EmitUpdateBoard(current_i, current_j);


				/*********** CHECK PLAYER WINNING CONDITIONS **********************/

				bool win = false;
				// X|*|*
				// X|*|*
				// X|*|*
				if(board[0,0].GetComponent<Tile>().GetTileType().Equals("x") 
					&& board[1,0].GetComponent<Tile>().GetTileType().Equals("x")
					&& board[2,0].GetComponent<Tile>().GetTileType().Equals("x"))
				{
					//player win
					win = true;
					ResetGame();
				}

				if(board[0,1].GetComponent<Tile>().GetTileType().Equals("x") 
					&& board[1,1].GetComponent<Tile>().GetTileType().Equals("x")
					&& board[2,1].GetComponent<Tile>().GetTileType().Equals("x"))
				{
					//player win
					win = true;
					ResetGame();
				}
				if(board[0,2].GetComponent<Tile>().GetTileType().Equals("x") 
					&& board[1,2].GetComponent<Tile>().GetTileType().Equals("x")
					&& board[2,2].GetComponent<Tile>().GetTileType().Equals("x"))
				{
					//player win
					win = true;
					ResetGame();
				}

				if(board[0,0].GetComponent<Tile>().GetTileType().Equals("x") 
					&& board[0,1].GetComponent<Tile>().GetTileType().Equals("x")
					&& board[0,2].GetComponent<Tile>().GetTileType().Equals("x"))
				{
					//player win
					win = true;
					ResetGame();
				}

				if(board[1,0].GetComponent<Tile>().GetTileType().Equals("x") 
					&& board[1,1].GetComponent<Tile>().GetTileType().Equals("x")
					&& board[1,2].GetComponent<Tile>().GetTileType().Equals("x"))
				{
					//player win
					win = true;
					ResetGame();
				}

				if(board[2,0].GetComponent<Tile>().GetTileType().Equals("x") 
					&& board[2,1].GetComponent<Tile>().GetTileType().Equals("x")
					&& board[2,2].GetComponent<Tile>().GetTileType().Equals("x"))
				{
					//player win
					ResetGame();
				}

				if(board[0,0].GetComponent<Tile>().GetTileType().Equals("x") 
					&& board[1,1].GetComponent<Tile>().GetTileType().Equals("x")
					&& board[2,2].GetComponent<Tile>().GetTileType().Equals("x"))
				{
					//player win
					win = true;
					ResetGame();
				}

				if(board[2,0].GetComponent<Tile>().GetTileType().Equals("x") 
					&& board[1,1].GetComponent<Tile>().GetTileType().Equals("x")
					&& board[0,2].GetComponent<Tile>().GetTileType().Equals("x"))
				{
					//player win
					win = true;
					ResetGame();
				}



				if(board[0,2].GetComponent<Tile>().GetTileType().Equals("x") 
					&& board[1,1].GetComponent<Tile>().GetTileType().Equals("x")
					&& board[2,0].GetComponent<Tile>().GetTileType().Equals("x"))
				{
					//player win
					win = true;
					ResetGame();

				}


				//tie condition if all cells are full
				if (!win && VerifyTie ())
				{
					ResetGame();
				}


			}



		}
	  


	}


	/// <summary>
	/// Resets the game.
	/// </summary>
	void ResetGame()
	{
       
	   TicTacCanvasManager.instance.ShowAlertDialog("YOU WIN!!!");

	   TicTacCanvasManager.instance.PlayAudio (TicTacCanvasManager.instance.victorySound);

	   TicTacCanvasManager.instance.OpenScreen(0);

	   TicTacNetworkManager.instance.SetPlayerType("x");

		//notify TicTacToeServer of this player's victory
	   TicTacNetworkManager.instance.EmitGameOver();

	   ClearBoard();
	  
	}

	//reset the game for the losing player
	public void ResetGameForLoserPlayer()
	{
	   TicTacCanvasManager.instance.ShowAlertDialog("YOU LOSE!!!");
	   
	   TicTacCanvasManager.instance.PlayAudio (TicTacCanvasManager.instance.fallSound);

	   TicTacCanvasManager.instance.OpenScreen(0);

	   ClearBoard();

	   TicTacNetworkManager.instance.SetPlayerType("x");
	}

	/// <summary>
	/// Resets the game for remain player.
	/// </summary>
	public void ResetGameForWOPlayer()
	{
		TicTacCanvasManager.instance.ShowAlertDialog("ANOTHER PLAYER GET OUT!");
		TicTacCanvasManager.instance.OpenScreen(0);
		ClearBoard();
		TicTacNetworkManager.instance.SetPlayerType("x");
	}
	
	void ClearBoard()
	{
	  for(int i=0;i<3;i++)
	  {
	    for(int j =0;j<3;j++)
		{
				board [i, j].GetComponent<Image>().sprite = spriteEmptyTilePref.GetComponent<SpriteRenderer> ().sprite;

				board [i, j].GetComponent<Tile> ().SetTileType ("none");
		}
	  }
	}


	/// <summary>
	/// Verifies the tie.
	/// </summary>
	/// <returns><c>true</c>, if tie was verifyed, <c>false</c> otherwise.</returns>
	 bool VerifyTie()
	{
		
		for(int i=0;i<3;i++)
		{
			for(int j =0;j<3;j++)
			{
				

				if(!board [i, j].GetComponent<Tile> ().GetTileType().Equals("none"))
				{
					fullcells +=1;
				}
			}
		}

		if(fullcells == 9)
		{
			fullcells =0;
			return true;
		}
		fullcells =0;
		return false;
	}
	
}
