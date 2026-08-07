using System.Collections;
using UnityEngine;

/// <summary>
/// Helper script to add some visual feedback when the player interacts with the tree. 
/// It scales up the tree a bit and then scales it back to the original size.
/// It is not important in terms of the Multiplayer setup beyond the fact that we dont have to synchronize things like visual feedback across the network.
/// It doenst matter if the feedback appears slightly different for you and for the other clients as it is only a visual effect.
/// </summary>
public class InteractAnimation : MonoBehaviour
{
    [SerializeField] private Transform m_resourceVisual;
    [SerializeField] private float m_shakeScale = 1.2f;
    [SerializeField] private float m_shakeDuration = 0.15f;
    [SerializeField] private float m_returnDuration = 0.3f;

    private Vector3 m_originScale;
    private Coroutine m_shakeCoroutine;

    private void Awake()
    {
        m_originScale = m_resourceVisual.localScale;
    }

    public void Shake()
    {
        if (m_shakeCoroutine != null)
            StopCoroutine(m_shakeCoroutine);
        m_shakeCoroutine = StartCoroutine(ShakeCoroutine());
    }

    private IEnumerator ShakeCoroutine()
    {
        Vector3 targetScale = m_originScale * m_shakeScale;
        float elapsed = 0f;

        // Scale up
        while (elapsed < m_shakeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / m_shakeDuration;
            m_resourceVisual.localScale = Vector3.Lerp(m_originScale, targetScale, t);
            yield return null;
        }

        m_resourceVisual.localScale = targetScale;
        elapsed = 0f;

        // Scale back
        while (elapsed < m_returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / m_returnDuration;
            // EaseOut: fast at start, slow at end
            t = 1f - Mathf.Pow(1f - t, 3f);
            m_resourceVisual.localScale = Vector3.Lerp(targetScale, m_originScale, t);
            yield return null;
        }

        m_resourceVisual.localScale = m_originScale;
        m_shakeCoroutine = null;
    }
}