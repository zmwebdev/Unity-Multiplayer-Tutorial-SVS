using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Helper script that allows animation events to be sent as C# events 
/// so that we can easily listend to them from the PlayerController script.
/// </summary>
public class AnimationEvents : MonoBehaviour
{
    public event Action OnChop, OnAnimationDone, OnInteract;
    public UnityEvent OnStep;

    public void ChopAction()
    {
        OnChop?.Invoke();
    }

    public void AnimationDone()
    {
        OnAnimationDone?.Invoke();
    }

    public void Interact()
    {
        OnInteract?.Invoke();
    }

    public void Step()
    {
        OnStep?.Invoke();
    }
}
