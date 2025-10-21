using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioClip defaultClip; //テスト用(任意)

    void Awake()
    {
        // シングルトン化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); //シーンをまたいでも消えない
        }
        else
        {
            Destroy(gameObject);
        }
    }
    //void Start()
    //{
    //    PlayDefault(); //起動時にdefaultClipを再生
    //}
    ///<summary>
    ///指定したAudioClipを再生する
    ///</summary>
    public void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip == null)
        {
            Debug.LogWarning("再生するAudioClipが設定されていません。");
            return;
        }

        //一時的なオブジェクトを生成して音を鳴らす
        GameObject obj = new GameObject("Audio_" + clip.name);
        AudioSource source = obj.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.Play();

        //再生が終わったら自動で削除
        Destroy(obj, clip.length);
    }

    /// <summary>
    /// テスト用：defaultClipを鳴らす
    /// </summary>
    public void PlayDefault()
    {
        PlaySound(defaultClip);
    }
}
