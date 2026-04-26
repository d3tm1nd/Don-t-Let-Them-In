using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// IntroVideoController
/// - เล่นวิดีโอ Intro แล้วเมื่อวิดีโอจบจะโหลดไป Scene แรกของเกม
/// - ผู้เล่นสามารถกด Spacebar เพื่อข้ามได้
///
/// วิธีใช้:
/// 1) สร้าง Scene ชื่อ IntroScene
/// 2) สร้าง GameObject ที่มี VideoPlayer (Render Mode: Camera Near Plane หรือ RenderTexture ก็ได้)
/// 3) ใส่สคริปต์นี้ไว้ที่ GameObject เดียวกับ VideoPlayer
/// 4) ตั้งค่า nextSceneName ให้ตรงกับชื่อ Scene แรกของเกม (ต้องอยู่ใน Build Settings)
/// </summary>
[DisallowMultipleComponent]
public class IntroVideoController : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Next Scene")]
    [Tooltip("ชื่อ Scene แรกของเกม (ต้อง Add ใน File > Build Settings)")]
    [SerializeField] private string nextSceneName = "DayScene";

    [Header("Skip")]
    [Tooltip("อนุญาตให้กด Space เพื่อข้ามวิดีโอ")]
    [SerializeField] private bool allowSkip = true;

    [Tooltip("ถ้า true: โหลดฉากทันทีเมื่อกดข้าม (ไม่รอหยุดวิดีโอ)")]
    [SerializeField] private bool loadImmediatelyOnSkip = true;

    private bool _loading;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (videoPlayer == null)
            Debug.LogError("IntroVideoController: VideoPlayer is missing on this GameObject.");
    }

    private void OnEnable()
    {
        if (videoPlayer == null) return;

        // เมื่อเล่นจบ
        videoPlayer.loopPointReached += HandleVideoFinished;
        // หากมี error
        videoPlayer.errorReceived += HandleVideoError;
    }

    private void OnDisable()
    {
        if (videoPlayer == null) return;

        videoPlayer.loopPointReached -= HandleVideoFinished;
        videoPlayer.errorReceived -= HandleVideoError;
    }

    private void Start()
    {
        // ให้แน่ใจว่าไม่ loop
        if (videoPlayer != null)
            videoPlayer.isLooping = false;
    }

    private void Update()
    {
        if (_loading || !allowSkip) return;

        if (IsSkipPressed())
        {
            if (loadImmediatelyOnSkip)
            {
                LoadNextScene();
            }
            else
            {
                // ถ้าไม่โหลดทันที ให้หยุดวิดีโอก่อนแล้วค่อยโหลด
                if (videoPlayer != null)
                    videoPlayer.Stop();
                LoadNextScene();
            }
        }
    }

    private bool IsSkipPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Space);
#endif
    }

    private void HandleVideoFinished(VideoPlayer vp)
    {
        LoadNextScene();
    }

    private void HandleVideoError(VideoPlayer vp, string message)
    {
        Debug.LogWarning($"IntroVideoController: Video error: {message}");
        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (_loading) return;
        _loading = true;

        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("IntroVideoController: nextSceneName is empty.");
            _loading = false;
            return;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
