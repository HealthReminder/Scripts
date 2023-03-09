using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class CameraOrbitOrtographicData : MonoBehaviour {
    [SerializeField]internal Camera Cam;
    [SerializeField]internal Transform FocusedTransform;
    [SerializeField] internal float NormalSpeed = 5f;
    [SerializeField] internal float FastSpeed = 50f;
    [SerializeField] internal Vector2 ortographicSizeRange;
    internal float scrollwheelBuff = 20.0f;
    internal Vector3 targetPosition;
    internal float _currentSpeed;
    public bool InstantHorizontalInput = false;
}
public abstract class CameraOrbitOrtographicController: CameraOrbitOrtographicData
{
    internal void ManualLookAt(Transform target, Transform spectator)
    {

        if (target.position != spectator.position)
        {
            Vector3 viewForward;
            Vector3 viewUp;
            Vector3 viewRight;

            // Create viewVector
            viewForward = target.position - spectator.position;

            // normalize viewVector
            viewForward.Normalize();

            // Now we get the perpendicular projection of the viewForward vector onto the world up vector
            // Uperp = U - ( U.V / V.V ) * V
            viewUp = Vector3.up - (Vector3.Project(viewForward, Vector3.up));
            viewUp.Normalize();

            // Alternatively for getting viewUp you could just use:
            // viewUp = thisTransform.TransformDirection(thisTransform.up);
            // viewUp.Normalize();

            // Calculate rightVector using Cross Product of viewOut and viewUp
            // this is order is because we use left-handed coordinates
            viewRight = Vector3.Cross(viewUp, viewForward);

            // set new vectors
            spectator.right = new Vector3(viewRight.x, viewRight.y, viewRight.z);
            spectator.up = new Vector3(viewUp.x, viewUp.y, viewUp.z);
            spectator.forward = new Vector3(viewForward.x, viewForward.y, viewForward.z);
        }

        else
        {
            print("position vectors are equal. No rotation needed");
        }

    }
    internal float _zoomMomentum = 0;
    internal void InputZoom()
    {
        float scrollwheelInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollwheelInput != 0f)
        {
            ///The zoom momentum dictates smoothness of zoom
            if (_zoomMomentum < 1.0f)
                _zoomMomentum += 0.1f;
            else
                _zoomMomentum = 1.0f;

            float newSize = Cam.orthographicSize + _currentSpeed * Time.deltaTime * scrollwheelInput * scrollwheelBuff * -1;
            if (newSize > ortographicSizeRange.x && newSize < ortographicSizeRange.y)
                Cam.orthographicSize = newSize;
        }
        else
        {
            if (_zoomMomentum > 0.0f)
                _zoomMomentum -= 0.025f;
            else
                _zoomMomentum = 0.0f;
        }
    }
    internal void InputMovement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        if (InstantHorizontalInput)
            horizontalInput = 10;
        if (horizontalInput != 0)
            transform.position = transform.position + (transform.right * _currentSpeed * Time.deltaTime * horizontalInput);

        float verticalInput = Input.GetAxis("Vertical");
        if (verticalInput != 0)
            if (Vector3.Dot(transform.up, FocusedTransform.up) > -0.85f)
                transform.position = transform.position + (transform.up * _currentSpeed * verticalInput * Time.deltaTime);
    }

}
public class CameraOrbitOrtographic : CameraOrbitOrtographicController
{
    public AnimationCurve MovementCurve;
    public void Awake()
    {
        targetPosition = FocusedTransform.position;
        Debug.LogWarning("The player camera should not have any input collection");
    }
    bool isFocusMoving = false;

    public void Update()
    {
        if (!FocusedTransform)
            return;

        bool fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        _currentSpeed = fastMode ? this.FastSpeed : this.NormalSpeed;

        //Depth
        InputZoom();

        //Left and Right
        InputMovement();

        ManualLookAt(FocusedTransform, transform);

        if (Input.GetKeyDown(KeyCode.Mouse2) && !isFocusMoving)
        {

            Ray ray = GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                StopCoroutine(MoveFocus(Vector3.zero));
                StartCoroutine(MoveFocus(hit.point));
            }
        }

        if (Vector3.Distance(FocusedTransform.position, targetPosition) > 0.1f)
            FocusedTransform.transform.position += (targetPosition - FocusedTransform.position).normalized*Time.deltaTime*(5 + (targetPosition - FocusedTransform.position).magnitude);
    }
    IEnumerator MoveFocus(Vector3 newPos)
    {
        Vector3 initialPos = FocusedTransform.position;
        float progress = 0;
        while(progress < 1)
        {
            FocusedTransform.position = Vector3.Lerp(initialPos, newPos, MovementCurve.Evaluate(progress));
            progress += Time.deltaTime;
            targetPosition = FocusedTransform.position;
            isFocusMoving = true;
            yield return null;
        }
        targetPosition = FocusedTransform.position;

        FocusedTransform.position = newPos;
        isFocusMoving = false;
        yield break;
    }

    public void ChangeZoom(float value)
    {
        value = (Mathf.Clamp(value, 0.0f, 1.0f) + _zoomMomentum * _zoomMomentum) /20;
        Cam.orthographicSize = Mathf.Lerp(ortographicSizeRange.x, ortographicSizeRange.y, value);
    }

}
