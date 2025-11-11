using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInteractionSoundProfile", menuName = "Scriptable Objects/PlayerInteractionSoundProfile")]
public class PlayerInteractionSoundProfile : ScriptableObject
{
    [Header("“¥‚İ‚Â‚¯‚ç‚ê‚½Û‚ÉÄ¶‚³‚ê‚é‰¹º")]
    [SerializeField] public AudioClip onStompedAudio;
    [SerializeField][Range(0.0f, 1.0f)] public float onStompedAudioVolume = 0.5f;
    [Header("‰¡‚©‚çÕ“Ë‚µ‚½Û‚ÉÄ¶‚³‚ê‚é‰¹º")]
    [SerializeField] public AudioClip onSideHitAudio;
    [SerializeField][Range(0.0f, 1.0f)] public float onSideHitAudioVolume = 0.5f;
}
