using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SmoothTurnProvider : LocomotionProvider
{
    public float turnSegment = 45.0f;

    public float turnTime = 3.0f;

    public InputHelpers.Button rightTurnButton = InputHelpers.Button.PrimaryAxis2DRight;
    public InputHelpers.Button leftTurnButton = InputHelpers.Button.PrimaryAxis2DLeft;

    public List<XRController> controllers = new List<XRController>();

    private float targetTurnAmount = 0.0f;

    // Start is called before the first frame update
  
    // Update is called once per frame
    private void Update()
    {
        if (CanBeginLocomotion())
            CheckForInput();
    }

    private void CheckForInput()
    {
        foreach (XRController controller in controllers)
        {
            targetTurnAmount = CheckForTurn(controller);
            if (targetTurnAmount != 0.0f)
                TrySmoothTurn();
        }    
    }

    private float CheckForTurn(XRController controller)
    {
        if (controller.inputDevice.IsPressed(rightTurnButton,out bool rightPress))
        {
            if (rightPress)
                return turnSegment;
        }

        if (controller.inputDevice.IsPressed(leftTurnButton, out bool leftPress))
        {
            if (leftPress)
                return -turnSegment;
        }

        return 0.0f;
    }

    private void TrySmoothTurn()
    {
        StartCoroutine(TurnRoutine(targetTurnAmount));
        targetTurnAmount = 0.0f;
    }

    private IEnumerator TurnRoutine(float turnAmount)
    {
        float previousTurnChange = 0.0f;
        float elapsedTime = 0.0f;

        BeginLocomotion();

        while (elapsedTime <= turnTime)
        {
            float blend = elapsedTime / turnTime;
            float turnChange = Mathf.Lerp(0, turnAmount, blend);
            float turnDifference = turnChange - previousTurnChange;
            system.xrOrigin.RotateAroundCameraUsingOriginUp(turnDifference);
            previousTurnChange = turnChange;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        EndLocomotion();
    }
}
