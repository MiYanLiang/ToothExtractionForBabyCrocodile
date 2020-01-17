using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TicTacCanvasManager : MonoBehaviour {

	public static  TicTacCanvasManager instance;

	public Canvas  HUDLobby;
	
	public Canvas  gameCanvas;

	public Canvas alertgameDialog;

	public Text alertDialogText;

	public Text txtSearchServerStatus;
	
	public Text txtHeader;
	
	public Text txtFooter;

	public GameObject lobbyCamera;

	public int currentMenu;

	public Canvas loadingImg;

	public  AudioClip buttonSound;

	public AudioClip fallSound;

	public AudioClip victorySound;


	public float delay = 0f;

	
	// Use this for initialization
	void Start () {

		if (instance == null) {

			DontDestroyOnLoad (this.gameObject);

			instance = this;

			OpenScreen(0);

			CloseAlertDialog ();

			CloseLoadingImg();
		}
		else
		{
			Destroy(this.gameObject);
		}



	}

	void Update()
	{
		delay += Time.deltaTime;

		if (Input.GetKey ("escape") && delay > 1f) {

		  switch (currentMenu) {

			case 0:
			 Application.Quit ();
			break;

			case 1:
			Application.Quit ();
			 delay = 0f;
			break;
			
			case 2:
			Application.Quit ();
			 delay = 0f;
			break;
			
			case 3:
			 Application.Quit ();
			break;

		 }//END_SWITCH

	 }//END_IF
}
	/// <summary>
	/// Opens the screen.
	/// </summary>
	/// <param name="_current">Current.</param>
	public void  OpenScreen(int _current)
	{
		switch (_current)
		{
		    //lobby menu
		    case 0:
			currentMenu = _current;
			HUDLobby.enabled = true;
			gameCanvas.enabled = false;
			break;


		    case 1:
			currentMenu = _current;
			HUDLobby.enabled = false;
			gameCanvas.enabled = true;
		
			break;

	
		}

	}


	/// <summary>
	/// Shows the alert dialog.
	/// </summary>
	/// <param name="_message">Message.</param>
	public void ShowAlertDialog(string _message)
	{
		alertDialogText.text = _message;
		alertgameDialog.enabled = true;
	}

	public void ShowLoadingImg()
	{
		loadingImg.enabled = true;

	}

	public void CloseLoadingImg()
	{
		loadingImg.enabled = false;

	}
	/// <summary>
	/// Closes the alert dialog.
	/// </summary>
	public void CloseAlertDialog()
	{
		alertgameDialog.enabled = false;
	}


	public void PlayAudio(AudioClip _audioclip)
	{
		
	   GetComponent<AudioSource> ().PlayOneShot (_audioclip);

	}
}
