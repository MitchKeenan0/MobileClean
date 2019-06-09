using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameSystem : MonoBehaviour
{
	private int ScreenX;
	private int ScreenY;

	//private Text ScoreText;
	private Text TouchCountText;
	private Text PileScoreText;

	private SweepTouchControl Sweeper;
	private ScoreAnimation ScoreAnim;

    
    void Start()
    {
		ScreenX = Screen.width;
		ScreenY = Screen.height;

		//ScoreText = FindObjectOfType<Text>();
		Sweeper = FindObjectOfType<SweepTouchControl>();

		InitScoring();

		InitGame();
    }


	public void InitGame()
	{
		CameraController camControl = Camera.main.gameObject.GetComponent<CameraController>();
		camControl.ResetCamera();

		ResetScores();

		/// Set dress
		var FoundObjects = FindObjectsOfType<Dirt>();
		int numObjects = FoundObjects.Length;
		if (numObjects > 0)
		{

			for (int i = 0; i < numObjects; i++)
			{
				
				if (FoundObjects[i] != null)
				{
					Dirt ThisDirt = FoundObjects[i];

					/// Scatter each dirt onscreen
					int ScreensafeXLeft = Mathf.FloorToInt(ScreenX * 0.2f);
					int ScreensafeXRight = Mathf.FloorToInt(ScreenX * 0.8f);
					int ScreensafeYTop = Mathf.FloorToInt(ScreenY * 0.2f);
					int ScreensafeYBottom = Mathf.FloorToInt(ScreenY * 0.8f);
					int RandomX = Mathf.FloorToInt(Random.Range(ScreensafeXLeft, ScreensafeXRight));
					int RandomY = Mathf.FloorToInt(Random.Range(ScreensafeYTop, ScreensafeYBottom));
					Vector3 RandomOnScreen = new Vector3(RandomX, RandomY, 0.0f);
					RandomOnScreen = Camera.main.ScreenToWorldPoint(RandomOnScreen);
					RandomOnScreen.z = 0.0f;
					ThisDirt.transform.position = RandomOnScreen;

					/// Scale and colour variance
					float safeIndex = 0.1f * (i + 1);
					float dirtIdentity = Mathf.Sqrt(safeIndex * ThisDirt.MaxSize);
					float scaling = Mathf.Clamp(dirtIdentity, ThisDirt.MinSize, ThisDirt.MaxSize);
					ThisDirt.InitDirt(scaling);
				}
			}
		}
	}

	public void ExitGame()
	{
		Application.Quit();
	}


	public void UpdateScore(float score, float touchNumber)
	{
		/// Inform score's animation
		if (ScoreAnim != null)
		{
			ScoreAnim.NewScore(score);
		}

		/// Update touch touch
		if (TouchCountText != null)
		{
			TouchCountText.text = ("Touch: " + touchNumber);
		}
	}


	void InitScoring()
	{
		var textArray = FindObjectsOfType<Text>();
		int numTexts = textArray.Length;
		if (numTexts > 0)
		{

			/// Touch count
			for (int i = 0; i < numTexts; i++)
			{
				Text ThisText = textArray[i];
				if (ThisText.tag == "TouchCount")
				{
					TouchCountText = ThisText;
					TouchCountText.text = "Touch: 0";
				}
			}

			/// Pile score
			for (int i = 0; i < numTexts; i++)
			{
				Text ThisText = textArray[i];
				if (ThisText.tag == "Score")
				{
					PileScoreText = ThisText;
					PileScoreText.text = "0";
				}
			}

			/// Animation system
			if (PileScoreText != null)
			{
				ScoreAnim = PileScoreText.gameObject.GetComponent<ScoreAnimation>();
			}

		}
	}


	void ResetScores()
	{
		if (TouchCountText != null)
		{
			TouchCountText.text = ("Touch: 0");
		}

		if (PileScoreText != null)
		{
			PileScoreText.text = "0";
		}

		if (ScoreAnim != null)
		{
			ScoreAnim.NewScore(0.0f);
		}

		if (Sweeper != null)
		{
			Sweeper.ResetSweeps();
		}
	}



}
