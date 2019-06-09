using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public float moveSpeed = 1.0f;
	public float zoomSpeed = 1.0f;
	public float size = 5.0f;
	public float distance = 1.0f;

	private float targetSize = 0.0f;
	private Vector3 targetPosition;
	private Vector3 originalPosition;
	private Camera cam;
	
	// Start is called before the first frame update
    void Start()
    {
		targetSize = size;
		cam = GetComponent<Camera>();
		gameObject.isStatic = false;
		originalPosition = transform.position;
    }

	public void ResetCamera()
	{
		targetSize = size;
		cam.orthographicSize = size;
		transform.position = originalPosition;
		targetPosition = originalPosition;
	}

    // Update is called once per frame
    void Update()
    {
        if (!TargetMet())
		{
			UpdateCameraParameters();
		}
    }

	public void NewParameters(float sizeValue, Vector3 location)
	{
		float safeSize = sizeValue + 1.1f;
		targetSize = Mathf.Clamp((size * sizeValue * distance), 4.0f, 6.0f);
		targetPosition = location;
	}

	void UpdateCameraParameters()
	{
		float deltaTime = Time.deltaTime;

		/// Size, "scoping"
		float currentSize = cam.orthographicSize;
		float interpSize = Mathf.Lerp(currentSize, targetSize, deltaTime * zoomSpeed * currentSize);
		interpSize = Mathf.Clamp(interpSize, 3.0f, size);
		cam.orthographicSize = interpSize;

		/// Movement
		Vector3 intendedPosition = targetPosition;
		intendedPosition.z = -10.0f;
		Vector3 interpPosition = Vector3.Lerp(transform.position, intendedPosition, deltaTime * moveSpeed);
		transform.position = interpPosition;
	}

	/// Returns true if camera has reached its target
	bool TargetMet()
	{
		bool met = true;

		Vector3 comparativePosition = transform.position;
		comparativePosition.z = targetPosition.z;
		float dist = Vector3.Distance(comparativePosition, targetPosition);
		if (dist >= 0.1f)
		{
			met = false;
		}

		return met;
	}
}
