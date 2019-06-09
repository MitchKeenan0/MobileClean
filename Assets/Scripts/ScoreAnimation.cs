using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreAnimation : MonoBehaviour
{
	public float animSpeed = 15.0f;

	private float score = 0.0f;
	private float displayedScore = 0.0f;
	private float naturalFontSize = 0.0f;

	private Text scoreText;

	void Start()
	{
		scoreText = GetComponent<Text>();
		naturalFontSize = scoreText.fontSize;
	}

	void Update()
	{
		if (displayedScore != score)
		{
			UpdateScoreAnim();
		}
	}

	void UpdateScoreAnim()
	{
		/// Number accumulation
		float interpScore = Mathf.Lerp(displayedScore, score, Time.deltaTime * animSpeed);
		float scoreDifference = Mathf.Abs(score - displayedScore) / 20.0f;
		if (score <= displayedScore)
		{
			scoreDifference = 1.0f;
		}

		/// Sizing
		float safeSize = Mathf.Clamp(scoreDifference * naturalFontSize, naturalFontSize * 1.05f, naturalFontSize * 2.2f);
		int interpText = Mathf.FloorToInt(Mathf.Lerp(scoreText.fontSize, safeSize, Time.deltaTime * animSpeed));
		scoreText.fontSize = interpText;
		displayedScore = interpScore;

		/// Print
		int printedScore = Mathf.FloorToInt(interpScore);
		string scoringText = printedScore.ToString("F0");
		if (interpScore < 1.0f) {
			scoringText = "0";
		}
		scoreText.text = scoringText;

		/// Colour
		if (printedScore > 0)
		{
			Color scoreColor = new Color(1.0f, 1.0f, 1.0f);
			float colourShift = Mathf.Sqrt((1 / interpScore) * 1000.0f);
			scoreColor.b = scoreColor.g = colourShift;
			scoreText.color = scoreColor;
		}
	}

	public void NewScore(float value)
	{
		score = value;
	}
}
