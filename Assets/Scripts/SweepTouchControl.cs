using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SweepTouchControl : MonoBehaviour
{
	// Variable controls
	public float SweepSpeed = 0.5f;
	public float TurningSpeed = 0.5f;
	public float BrushPower = 1.0f;
	public float FollowthroughFinishSpeed = 0.5f;
	public Vector3 StartPosition;
	public bool enter = true;
	public bool stay = false;
	public bool exit = true;


	// Component members
	private Rigidbody2D rb;
	private SpriteRenderer sprite;
	private GameSystem game;
	
	private Vector3 DeltaTouch;
	private Vector3 DeltaTouch3d;
	private Vector3 CurrentMotion3d;
	private Vector3 currentTouchPosition;
	private List<GameObject> SweepDirts;
	private int NumberOfTouches = 0;
	private float PileScore = 0;
	private float stayCount = 0.0f;
	private bool bTouching = false;

	private Vector2 touchStartPosition;
	private float touchMagnitude = 0.0f;
	

	void Start()
    {
		rb = GetComponent<Rigidbody2D>();
		sprite = GetComponent<SpriteRenderer>();
		SweepDirts = new List<GameObject>();
		game = FindObjectOfType<GameSystem>();

		transform.position = Camera.main.ScreenToWorldPoint(StartPosition);
    }

    
    void Update()
    {
		if (Input.touchCount > 0)
		{
			UpdateSweepTouch();
		}

		if (sprite.enabled)
		{
			EndOfSweep();
		}

		if (bTouching)
		{
			UpdateSweepForce();
		}
	}


	public void ResetSweeps()
	{
		NumberOfTouches = 0;

	}


	void UpdateSweepTouch()
	{
		Touch touch = Input.GetTouch(0);
		currentTouchPosition = Camera.main.ScreenToWorldPoint(touch.position);
		float TouchToSweepDistance = Vector3.Distance(currentTouchPosition, transform.position);

		switch (touch.phase)
		{
			/// Touch location start
			case TouchPhase.Began:

				bTouching = true;
				touchStartPosition = touch.position;
				DeltaTouch = currentTouchPosition - transform.position;

				currentTouchPosition.z = 0.0f;
				transform.position = currentTouchPosition;
				rb.velocity = Vector2.zero;

				Vector3 dir1 = (Camera.main.ScreenToWorldPoint(Vector2.zero) - Camera.main.ScreenToWorldPoint(transform.position)).normalized;
				dir1.z = 0.0f;
				float angle1 = Mathf.Atan2(dir1.y, dir1.x) * Mathf.Rad2Deg;
				transform.rotation = Quaternion.Euler(0f, 0f, angle1);

				sprite.enabled = true;

				break;

			
			/// Sweeping motion driver
			case TouchPhase.Moved:

				DeltaTouch = currentTouchPosition - transform.position;
				Vector3 IntendedPosition = DeltaTouch * Time.deltaTime * SweepSpeed;
				IntendedPosition = Vector3.ClampMagnitude(IntendedPosition, SweepSpeed);
				rb.velocity = IntendedPosition;

				Vector3 dir = DeltaTouch.normalized;
				float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
				Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
				transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * TurningSpeed);

				break;
			
			
			/// Touch release
			case TouchPhase.Ended:

				bTouching = false;
				touchMagnitude = (touchStartPosition - touch.position).magnitude;

				break;
		}
	}


	// Monitor for moment of rest to score
	void EndOfSweep()
	{
		if (!bTouching 
			&& (rb.velocity.magnitude <= FollowthroughFinishSpeed))
		{
			sprite.enabled = false;

			if (touchMagnitude > 20.0f)
			{
				ReadPile();
			}
		}
	}


	// Collisions
	void OnTriggerEnter2D(Collider2D other)
	{
		if (enter)
		{
			if (other.gameObject.GetComponent<Dirt>())
			{
				SweepDirts.Add(other.gameObject);
			}
		}
	}

	void OnTriggerStay2D(Collider2D other)
	{
		if (stay)
		{
			if (stayCount > 0.25f)
			{
				Debug.Log("staying");
				stayCount = stayCount - 0.25f;
			}
			else
			{
				stayCount = stayCount + Time.deltaTime;
			}
		}
	}

	void OnTriggerExit2D(Collider2D other)
	{
		if (exit)
		{
			SweepDirts.Remove(other.gameObject);
		}
	}


	void UpdateSweepForce()
	{
		int numDirts = SweepDirts.Count;
		if (numDirts > 0)
		{

			for (int i = 0; i < numDirts; i++)
			{

				if (SweepDirts[i] != null)
				{
					GameObject thisDirt = SweepDirts[i];

					Vector2 sweepVelocity = rb.velocity;
					Vector2 DirtVelocity = sweepVelocity * BrushPower;

					thisDirt.GetComponent<Rigidbody2D>().velocity = DirtVelocity;
				}
			}
		}
	}


	void ReadPile()
	{
		/// Finished the touch
		NumberOfTouches += 1;

		var dirtsArray = FindObjectsOfType<Dirt>();
		int numDirts = dirtsArray.Length;
		float superSum = 0.0f;
		float superAverage = 0.0f;

		/// For camera..
		float lowestAverage = 999999.9f;
		Vector3 coreLocation = Vector2.zero;

		if (numDirts > 0)
		{
			/// Get average of distance for all dirts...
			for (int i = 0; i < numDirts; i++)
			{
				if (dirtsArray[i] != null)
				{
					/// Each dirt...
					Dirt ThisDirt = dirtsArray[i];
					if (CheckBounds(ThisDirt.gameObject))
					{
						float personalSum = 0.0f;
						for (int j = 0; j < numDirts; j++)
						{
							if (dirtsArray[j] != null)
							{
								Dirt ThatDirt = dirtsArray[j];
								float distToDirt = Vector2.Distance(ThisDirt.transform.position, ThatDirt.transform.position);
								personalSum += distToDirt;
							}
						}

						/// Contribute to the average
						float personalAverage = personalSum / numDirts;
						superSum += personalAverage;

						/// Possibly nominate centerpoint for camera
						if (personalAverage < lowestAverage)
						{
							lowestAverage = personalAverage;
							coreLocation = ThisDirt.transform.position;
						}
					}

					/// Determines how many neighbors this dirt has
					ThisDirt.ProbeSurroundings();
				}
			}

			/// Inform camera for tigher angle
			//Debug.Log("lowest average: " + lowestAverage);
			CameraController CamControl = Camera.main.GetComponent<CameraController>();
			if (CamControl != null)
			{
				CamControl.NewParameters(lowestAverage, coreLocation);
			}


			/// Inverse of the combined average is our pile score
			superAverage = superSum / numDirts;
			PileScore = Mathf.Pow((1.0f / superAverage) * 10.0f, 3.0f);

			if (game != null)
			{
				game.UpdateScore(PileScore, NumberOfTouches);
			}
		}
	}


	bool CheckBounds(GameObject obj)
	{
		Vector3 ScreenPosition = Camera.main.WorldToScreenPoint(obj.transform.position);
		float MyX = ScreenPosition.x;
		float MyY = ScreenPosition.y;
		float ScreenX = Screen.width;
		float ScreenY = Screen.height;
		bool bOnscreen = true;

		if ((MyX <= 0.0f) || (MyY <= 0.0f) || (MyX >= ScreenX) || (MyY >= ScreenY))
		{
			bOnscreen = false;
			Rigidbody2D dirtRb = obj.GetComponent<Rigidbody2D>();
			if (dirtRb != null)
			{
				Vector2 reflection = dirtRb.velocity * -1.0f;
				dirtRb.velocity = reflection;
			}
		}

		return bOnscreen;
	}


}
