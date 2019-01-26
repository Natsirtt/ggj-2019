using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    private Vector2 AccumulatedInput;
    private Vector2 Velocity;
    public float MaxSpeed = 5.0f;

    public void AddMovementInput(Vector2 inputVector)
    {
        AccumulatedInput += inputVector;
    }

    void Update()
    {
        HandleInput();
        HandleTranslation();
    }

    void HandleInput()
    {
        if(AccumulatedInput.magnitude > 0.0f) // accelerate
        {
            Vector2 normalizedInput = AccumulatedInput.magnitude > 1.0f ? AccumulatedInput.normalized : AccumulatedInput;

            Velocity += normalizedInput * MaxSpeed;
            Velocity = Velocity.normalized * Mathf.Min(MaxSpeed, Velocity.magnitude);
        }
        else // break
        {
            Velocity = Vector2.zero;
        }

        AccumulatedInput = Vector2.zero;
    }
    
    void HandleTranslation()
    {
        transform.position += new Vector3(Velocity.x, Velocity.y, 0) * Time.deltaTime;
    }
}
