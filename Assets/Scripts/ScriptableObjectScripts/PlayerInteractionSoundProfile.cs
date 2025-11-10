using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInteractionSoundProfile", menuName = "Scriptable Objects/PlayerInteractionSoundProfile")]
public class PlayerInteractionSoundProfile : ScriptableObject
{
    [SerializeField] public AudioClip onStompedAudio;
    [SerializeField][Range(0.0f, 1.0f)] public float onStompedAudioVolume = 0.5f;
    [SerializeField] public AudioClip onSideHitAudio;
    [SerializeField][Range(0.0f, 1.0f)] public float onSideHitAudioVolume = 0.5f;
}
