using UnityEngine;

/// <summary>
/// In general feedback scritps can stay as regular MonoBehaviours because the feedback doesnt influences the outcome of the game.
/// In this case Server can trigger the feedback and each Player can play the sound on their own client without the need to synchronize it over the network.
/// We have to remember that exchanging data nedlessly over the network can cause performance issues and influence to cost of running a multiplayer server.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class ItemsAudio : MonoBehaviour
{
    private AudioSource m_audioSource;
    [SerializeField] private AudioClip m_audioClip;
    private void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_audioSource.playOnAwake = false;
        m_audioSource.volume = 0.2f;
        m_audioSource.clip = m_audioClip;
    }
    
    public void PlaySound()
    {
        m_audioSource.Play();
    }

    public void PlaySoundSeparate()
    {
        AudioSource.PlayClipAtPoint(m_audioClip,transform.position, 0.2f);
    }
}