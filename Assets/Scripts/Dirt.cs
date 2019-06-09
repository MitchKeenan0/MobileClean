using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dirt : MonoBehaviour
{
	public float Drag = 5.0f;
	public bool bExplodes = false;
	public float ExplodeStrength = 10.0f;
	public int TooManyNeighbors = 5;
	public float NeighborlyDistance = 1.0f;
	public ParticleSystem PuffOutEffect;

	public float MaxSize = 1.5f;
	public float MinSize = 0.3f;

	private SpriteRenderer sprite;
	private Rigidbody2D rb;


	void Start()
    {
		sprite = GetComponent<SpriteRenderer>();
		rb = GetComponent<Rigidbody2D>();
	}


	public void InitDirt(float scale)
	{
		/// Scale
		transform.localScale = Vector3.one * scale;


		/// Colour
		sprite = GetComponent<SpriteRenderer>();
		float safeColorScale = Mathf.Clamp(scale, 0.5f, 1.0f);
		Color spriteColor = Color.white * safeColorScale;
		spriteColor.a = 1.0f;
		sprite.color = spriteColor;

		/// Rotation
		Quaternion spawnRotation = transform.localRotation;
		spawnRotation.z = Random.Range(-0.15f, 0.15f);
		transform.localRotation = spawnRotation;
	}


	public void ProbeSurroundings()
	{
		var dirtsArray = FindObjectsOfType<Dirt>();
		int numDirts = dirtsArray.Length;
		int neighbors = 0;

		if (numDirts > 0)
		{
			/// Get average of distance for all dirts...
			for (int i = 0; i < numDirts; i++)
			{
				if ((dirtsArray[i] != null) && (dirtsArray[i] != gameObject))
				{
					Dirt ThisDirt = dirtsArray[i];

					float distToDirt = Vector2.Distance(transform.position, ThisDirt.transform.position);
					if (distToDirt <= NeighborlyDistance)
					{
						neighbors++;
					}
				}
			}

			/// Compaction detonation
			if (bExplodes && (neighbors >= TooManyNeighbors))
			{
				if (transform.localScale.magnitude >= 1.0f)
				{

					PuffOut();

					/// Forces
					for (int i = 0; i < numDirts; i++)
					{
						if ((dirtsArray[i] != null) && (dirtsArray[i] != gameObject))
						{
							Dirt ThisDirt = dirtsArray[i];

							Vector2 ExplodeVector = ThisDirt.transform.position - transform.position;
							float ExplodeForce = ExplodeStrength * Mathf.Clamp((1.0f / ExplodeVector.magnitude), 1.0f, 100.0f);

							Vector2 Force = ExplodeVector.normalized * ExplodeForce;
							ThisDirt.GetComponent<Rigidbody2D>().AddForce(Force);
						}
					}
				}
			}
		}
	}


	/// Blam!
	void PuffOut()
	{
		ParticleSystem particles = Instantiate(PuffOutEffect, transform.position, Quaternion.identity);
		Destroy(particles, 0.9f);

		sprite.enabled = false;

		Destroy(gameObject, 1.0f);
	}


}
