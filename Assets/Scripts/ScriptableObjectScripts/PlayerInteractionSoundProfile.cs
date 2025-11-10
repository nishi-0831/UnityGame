using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInteractionSoundProfile", menuName = "Scriptable Objects/PlayerInteractionSoundProfile")]
public class PlayerInteractionSoundProfile : ScriptableObject
{
    [SerializeField] protected AudioClip onStompedAudio;
    [SerializeField][Range(0.0f, 1.0f)] protected float onStompedAudioVolume = 0.5f;
    [SerializeField] protected AudioClip onSideHitAudio;
    [SerializeField][Range(0.0f, 1.0f)] protected float onSideHitAudioVolume = 0.5f;
}
