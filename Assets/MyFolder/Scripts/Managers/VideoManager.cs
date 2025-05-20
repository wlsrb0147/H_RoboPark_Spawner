using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;



[Serializable]
public class VideoPlayers
{
    public VideoPlayer[] mainPlayers;
    public bool[] isMainLoop;
    public bool[] hasLoop;
    public bool[] dontPlayNextVideo;
    public VideoPlayer[] loopPlayers;
    
    #region 여긴 안읽어도됨. 인스펙터 전용 함수

    private void CheckAndSet(ref bool[] array, int len)
    {
        if (array == null || array.Length != len)
        {
            bool[] newArray = new bool[len];
            if (array != null)
            {
                int copyLength = Mathf.Min(array.Length, len);
                Array.Copy(array, newArray, copyLength);
            }
            array = newArray;
        }
    }
    
    public void ValidateArrays()
    {
        if (mainPlayers == null)
            return;

        int targetLength = mainPlayers.Length;

        CheckAndSet(ref isMainLoop, targetLength);
        CheckAndSet(ref hasLoop, targetLength);
        CheckAndSet(ref dontPlayNextVideo, targetLength);
    }
    
    #endregion

}


public class VideoManager : MonoBehaviour
{
    #region 여기도 안읽어도 쓰는데 문제없는곳
    
    private bool _isUpdatingPage;
    private int[] _dictionaryIndex;

    
    private readonly HashSet<int> _notAllowedToChangeControl = new();
    
    private int CurrentPage
    {
        get => _currentPage;
        set
        {
            if (_currentPage == value) return;
        
            _currentPage = value;

            if (_isUpdatingPage) return;

            _isUpdatingPage = true;
            UpdateNextPage();
            _isUpdatingPage = false;
        }
    }
    
    private void UpdateNextPage()
    {
        if (_currentPage + 1 >= videoPlayers.Length)
            _nextPage = 0;
        else
            _nextPage = _currentPage + 1;
    }

    private int NextPage
    {
        get => _nextPage;
        set
        {
            if (_nextPage == value) return;

            if (_isUpdatingPage)
            {
                _nextPage = value;
                return;
            }
            else
            {
                _isUpdatingPage = true;
                UpdateCurrentPage();
                _isUpdatingPage = false;
                
                _nextPage = (value >= videoPlayers.Length) ? 0 : value;
                
            }
        }
    }

    private void UpdateCurrentPage()
    {
        _currentPage = _nextPage;
    }

    private void RegisterVideos()
    {
         if (videoPlayers == null || videoPlayers.Length == 0) return;
        
        _dictionaryIndex = new int[videoPlayers.Length];
        
        for (int i = 0; i < videoPlayers.Length; i++)
        {
            // 100의 자리는 현재 페이지 넘버
            _dictionaryIndex[i] = i * 100;
        }

        for (int i = 0; i < videoPlayers.Length; i++)
        {
            if (videoPlayers[i] == null || videoPlayers[i].mainPlayers == null) continue;

            for (int j = 0; j < videoPlayers[i].mainPlayers.Length; j++)
            {
                if (!videoPlayers[i].mainPlayers[j]) continue;
            
                int x = i * 100 + j;
                _videoArrayValue.Add(videoPlayers[i].mainPlayers[j], x);
            }
        }
        
        foreach (var t in videoPlayers)
        {
            if (t == null) continue;

            if (t.mainPlayers != null)
            {
                foreach (var v in t.mainPlayers)
                {
                    if (!v) continue;
                    v.isLooping = false;
                    v.skipOnDrop = false;
                    v.loopPointReached += AutoPlayNextVideo;
                    v.started += RegisterCurrentIndexOnControlManager;
                    v.errorReceived += OnVideoError;
                }
            }

            if (t.loopPlayers != null)
            {
                foreach (var v in t.loopPlayers)
                {
                    if (!v) continue;
                    v.isLooping = false;
                    v.loopPointReached += Loop;
                }
            }
        }

        for (int i = 0; i < videoPlayers.Length; i++)
        {
            if (videoPlayers[i] == null) continue;
        
            for (int j = 0; j < videoPlayers[i].mainPlayers.Length; j++)
            {
                if (videoPlayers[i].isMainLoop != null && videoPlayers[i].isMainLoop[j])
                {
                    IsMainLoopVideo(i, j);
                    continue;
                }

                if (videoPlayers[i].hasLoop != null && videoPlayers[i].hasLoop[j])
                {
                    InputLoopVideo(i, j);
                    continue;
                }

                if (videoPlayers[i].dontPlayNextVideo != null && videoPlayers[i].dontPlayNextVideo[j])
                {
                    DontPlayNextVideo(i, j);
                }
            }
        }
    }
    
    private void Loop(VideoPlayer source)
    {
        source.frame = 0;
        source.Play();
    }
    
    // 컨트롤 매니저에 인덱스 입력하는 코드
    private void RegisterCurrentIndexOnControlManager(VideoPlayer source)
    {
        if (!_notAllowedToChangeControl.Add(_videoArrayValue[source])) return;

        int page = _videoArrayValue[source] / 100;
        int index = _videoArrayValue[source] % 100;

        CurrentPage = page;
        _currentIndex = index;
        
        inputManager.SetCurrentIndex(page,index);
        DebugEx.Log("Current Video : " + page + "" + index);
    }
    
    // Loop영상 추가 메서드
    private void InputLoopVideo(int page, int index)
    {
        VideoPlayer player = videoPlayers[page].mainPlayers[index];
        // 100,101,102,200,201,202등으로 값 저장됨

        if (_loopAddedVideo.ContainsKey(player)) return;
        
        _loopAddedVideo.Add(player,_dictionaryIndex[page]);
        
        //첫 스트링이 page, 뒷자리 두개 스트링이 index
        player.loopPointReached -= AutoPlayNextVideo;
        player.loopPointReached += PlayLoopPlayer;
        ++_dictionaryIndex[page];
    }

    // 자동재생 
    private void AutoPlayNextVideo(VideoPlayer source)
    {
        DebugEx.Log($"AutoPlay By {source}");
        PlayNextMainVideo();
    }
    
    // 루프영상 재생
    private void PlayLoopPlayer(VideoPlayer source)
    {
        int page = _loopAddedVideo[source] / 100;
        int index = _loopAddedVideo[source] % 100;
        
        VideoPlayer player = videoPlayers[page].mainPlayers[index];
        player.Play();
        player.targetTexture = _currentRenderTexture;
        source.targetTexture = null;
        source.Stop();
    }
    
    private void PrepareNextVideo(VideoPlayer player)
    {
        _currentVideoPlayer = player;
        int page = _videoArrayValue[player] / 100; 
        int index = _videoArrayValue[player] % 100;
        
        bool removeAutoPlay = false;
        int count = 0;

        // 다음페이지 탐색
        while (true)
        {
            ++count;
            
            // 현재 페이지 길이 확인
            if (videoPlayers[page].mainPlayers.Length != 0)
            {
                ++index;
                
                // 인덱스 범위 밖이면 다음페이지로
                if (index >= videoPlayers[page].mainPlayers.Length)
                {
                    index = -1;
                    page = (page + 1) % videoPlayers.Length;
                    continue;
                }
                
                // 비디오 있으면 루프 중단
                if (videoPlayers[page].mainPlayers[index] != null)
                {
                    break;
                }
                
                // 유효하지 않을경우, 현재 비디오 자동재생 제거
                removeAutoPlay = true;

            }
            else
            {
                // 현재 페이지의 인덱스 길이가 0이면 다음페이지로
                removeAutoPlay = true;
                page = (page + 1) % videoPlayers.Length;
                index = -1;
            }
    
            // 무한 루프 방지를 위한 종료 조건
            if (count >= 100)
            {
                DebugEx.LogError("There's No Video In All Array");
                return;
            }
        }

        // 유효하지 않은 요소를 발견한 경우 자동재생 이벤트 취소
        if (removeAutoPlay)
        {
            player.loopPointReached -= AutoPlayNextVideo;
        }

        
        _nextVideoPlayer = videoPlayers[page].mainPlayers[index];
        _nextVideoPlayer.Prepare();
        
        if (_loopAddedVideo.ContainsKey(_nextVideoPlayer))
        {
            int page2 = _loopAddedVideo[_nextVideoPlayer] / 100;
            int index2 = _loopAddedVideo[_nextVideoPlayer] % 100;
        
            VideoPlayer loopVideo = videoPlayers[page2].loopPlayers[index2];
        
            loopVideo.Prepare();
        }
        
        DebugEx.Log("PreparedVideo is + " + page + "" + index);
    }
        
    private void StopVideoArray(VideoPlayer[] players)
    {
        if (players == null || players.Length == 0) return;

        foreach (var player in players)
        {
            if (player == null) continue;

            if (player.isPrepared)
            {
                player.Stop();
            }
            else if (player.isPlaying)
            {
                player.Stop();
                player.targetTexture = null;
            }
        }
    }
    
    private void IsMainLoopVideo(int page,int index)
    {
        VideoPlayer player = videoPlayers[page].mainPlayers[index];
        player.loopPointReached -= AutoPlayNextVideo;
        player.loopPointReached += Loop;
    }
    
    private void DontPlayNextVideo(int page,int index)
    {
        VideoPlayer player = videoPlayers[page].mainPlayers[index];
        player.loopPointReached -= AutoPlayNextVideo;
        player.loopPointReached -= Loop;
    }

    public void RemoveOrAddVideosOnHashSet(int changedPage)
    {
        if (videoPlayers == null) return;

        if (changedPage > _currentPage)
        {
            // currentPage부터 changedPage - 1까지 등록
            for (int page = _currentPage; page < changedPage; page++)
            {
                if (!IsValidPage(page))
                    continue;

                int startIndex = (page == _currentPage) ? _currentIndex : 0;
                int endIndex = videoPlayers[page].mainPlayers.Length - 1;

                for (int i = startIndex; i <= endIndex; i++)
                {
                    var video = videoPlayers[page].mainPlayers[i];
                    if (video)
                    {
                        _notAllowedToChangeControl.Add(_videoArrayValue[video]);
                    }
                }
            }
        }
        else if (changedPage < _currentPage)
        {
            // changedPage부터 currentPage까지 제거
            for (int page = changedPage; page <= _currentPage; page++)
            {
                if (!IsValidPage(page))
                    continue;

                int startIndex = 0;
                int endIndex = (page == _currentPage) ? _currentIndex : videoPlayers[page].mainPlayers.Length - 1;

                for (int i = startIndex; i <= endIndex; i++)
                {
                    var video = videoPlayers[page].mainPlayers[i];
                    if (video)
                    {
                        _notAllowedToChangeControl.Remove(_videoArrayValue[video]);
                    }
                }
            }
        }
    }

    private bool IsValidPage(int page)
    {
        return page >= 0
               && page < videoPlayers.Length
               && videoPlayers[page] != null
               && videoPlayers[page].mainPlayers != null;
    }
    
    #endregion
    
    public static VideoManager Instance;
    
    [Header("비디오 플레이어가 없는 페이지를 만났을 때, \n다음 비디오는 자동재생되지않음")]
    [Space]
    
    [SerializeField] private VideoPlayers[] videoPlayers;
    [SerializeField] private RenderTexture[] renderTexture;
    
    private VideoPlayer _currentVideoPlayer;
    private VideoPlayer _nextVideoPlayer;
    
    private RenderTexture _currentRenderTexture;
    private int _renderTextureIndex;
    
    private int _currentPage = -1;
    private int _nextPage = -1;
    private int _currentIndex = -1;
    
    // 영상 2차원 매핑 딕셔너리
    private readonly Dictionary<VideoPlayer, int> _videoArrayValue = new();
    
    // 루프영상이 추가된 영상 딕셔너리
    private readonly Dictionary<VideoPlayer, int> _loopAddedVideo = new();
    
    // 비디오 동시재생을 위한 PlayNextVideoOnOther실행시, 기존 비디어플레이어는 여기에 재생됨 
    private VideoPlayer _playerToStopLater; 
    
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PageController pageController;

    #region  생명주기

    private void Awake()
    {
        Instance = this;

        return;
        
        RegisterVideos();
        RegisterEvents();

    }

    private void Start()
    {
        return;
        StartVideo();
    }

    #endregion
    
    public void StartVideo()
    {
        videoPlayers[0].mainPlayers[0].prepareCompleted += InitializeVideo;
        videoPlayers[0].mainPlayers[0].Prepare();
    }
    
    private void InitializeVideo(VideoPlayer source)
    {
        DebugEx.Log("Initializing Video");
        source.prepareCompleted -= InitializeVideo;
        _notAllowedToChangeControl.Clear();
        source.Play();
        source.targetTexture = renderTexture[0];
        _currentRenderTexture = renderTexture[0];
        _renderTextureIndex = 0;
        _currentIndex = 0;
        CurrentPage = 0;
        PrepareNextVideo(source);
    }
    

    private void RegisterEvents()
    {
        foreach (var v in videoPlayers)
        {
            if (v?.mainPlayers == null || v.mainPlayers.Length == 0)
                continue;

            VideoPlayer lastPlayer = v.mainPlayers[^1];

            if (lastPlayer)
            {
                lastPlayer.loopPointReached += LoadNextPage;
            }
        }
        
        videoPlayers[^1].mainPlayers[^1].started += PrepareEnding;
        videoPlayers[^1].mainPlayers[^1].loopPointReached -= LoadNextPage;
        videoPlayers[^1].mainPlayers[^1].loopPointReached += GoTitle;
    }

    private void LoadNextPage(VideoPlayer source)
    {
        pageController.LoadNextPage();
    }

    private void PrepareEnding(VideoPlayer source)
    {
        ChangeNextVideo(0, 0);
    }
    
    // 모든 비디어 플레이어에 자동추가돼있음
    private void OnVideoError(VideoPlayer source, string message)
    {
        DebugEx.LogError($"Video Error! 메시지: {message}, 파일: {source}");
        
#if UNITY_EDITOR 
        // 에디터에서 실행 중이라면 Play 모드 종료
        UnityEditor.EditorApplication.isPlaying = false;
#else
    // 빌드된 실행 파일이라면 종료
    // Application.Quit();
#endif
    }
    
    private void GoTitle(VideoPlayer source) => GoTitleInternal();
    public void GoTitle() => GoTitleInternal();
    private void GoTitleInternal(VideoPlayer source = null)
    {
        DebugEx.Log(source ? $"GoTitle By {source}" : "GoTitle By Function");
        ChangeNextVideo(0, 0);
        CurrentPage = 0;
        _renderTextureIndex = 0;
        _currentRenderTexture = renderTexture[0];
        _notAllowedToChangeControl.Clear();
        PlayNextMainVideo();
        pageController.GoTitle();
    }
    
    // 다음 비디오 재생
    public void PlayNextMainVideo()
    {
        _nextVideoPlayer.Play();
        DebugEx.Log($"Play Video : {_nextVideoPlayer}");
        _nextVideoPlayer.targetTexture = _currentRenderTexture;
        
        if (_loopAddedVideo.ContainsKey(_currentVideoPlayer))
        {
            int page = _loopAddedVideo[_currentVideoPlayer] / 100;
            int index = _loopAddedVideo[_currentVideoPlayer] % 100;
            
            VideoPlayer player = videoPlayers[page].mainPlayers[index];

            if (player.targetTexture)
            {
                player.targetTexture = null;
            }
            
            player.Stop();
        }
        
        _currentVideoPlayer.targetTexture = null;
        _currentVideoPlayer.Stop();
        DebugEx.Log($"Stop Video : {_currentVideoPlayer}");
            
        _currentVideoPlayer = _nextVideoPlayer;
        
        PrepareNextVideo(_currentVideoPlayer);
    }
    
    public void PlayNextVideoOnOtherPage()
    {
        DebugEx.Log("MoviePlayer Page Index : " +  NextPage);
        
        pageController.LoadNextPage();
        ++_renderTextureIndex;
        _playerToStopLater = _currentVideoPlayer;
        _nextVideoPlayer.Play();
        _nextVideoPlayer.targetTexture = renderTexture[_renderTextureIndex];
        _currentRenderTexture = renderTexture[_renderTextureIndex];
        if (_loopAddedVideo.ContainsKey(_currentVideoPlayer))
        {
            int page = _loopAddedVideo[_currentVideoPlayer] / 100;
            int index = _loopAddedVideo[_currentVideoPlayer] % 100;
            
            VideoPlayer player = videoPlayers[page].mainPlayers[index];

            if (player.targetTexture)
            {
                player.targetTexture = null;
            }
            
            player.Stop();
        }
        
        _currentVideoPlayer = _nextVideoPlayer;
        
        ++NextPage;
        
        PrepareNextVideo(_currentVideoPlayer);
        
    }

    public void PlayMediumVideo(int page, int index)
    {
        StopAllVideo();
        RemoveOrAddVideosOnHashSet(page);
        
        videoPlayers[page].mainPlayers[index].Play();
        videoPlayers[page].mainPlayers[index].targetTexture = _currentRenderTexture;
        
        PrepareNextVideo(videoPlayers[page].mainPlayers[index]);
    }
    
    public void ChangeNextVideo(int page, int index)
    {
        if (videoPlayers[page].mainPlayers[index] == _nextVideoPlayer) return;
        if (_currentVideoPlayer != videoPlayers[page].mainPlayers[index])
        {
            _nextVideoPlayer.Stop();
        }
        
        RemoveOrAddVideosOnHashSet(page);
        
        _nextVideoPlayer = videoPlayers[page].mainPlayers[index];
        _nextVideoPlayer.Prepare();
    }
    
    public void ResetVideo()
    {
        StopAllVideo();

        _notAllowedToChangeControl.Clear();
        videoPlayers[0].mainPlayers[0].Play();
        videoPlayers[0].mainPlayers[0].targetTexture = renderTexture[0];
        
        _currentRenderTexture = renderTexture[0];
        _renderTextureIndex = 0;
        CurrentPage = 0;
        _currentIndex = 0;
        
        PrepareNextVideo(videoPlayers[0].mainPlayers[0]);
    }

    public void ChangeTextureToSingleColor(Color color)
    {
        _currentVideoPlayer.Stop();
        RenderTexture rt = RenderTexture.active;
        RenderTexture.active = _currentRenderTexture;
        GL.Clear(false,true, color);
        RenderTexture.active = rt;
    }
    
    private void StopAllVideo()
    {
        if (videoPlayers == null || videoPlayers.Length == 0)
        {
            DebugEx.LogWarning("videoPlayers가 비어 있습니다.");
            return;
        }

        foreach (var v in videoPlayers)
        {
            if (v == null) continue;

            StopVideoArray(v.mainPlayers);
            StopVideoArray(v.loopPlayers);
        }
    }
    
    #region 여기도 안읽어도됨. 인스펙터 전용 함수
    
    private void OnValidate()
    {
        if (videoPlayers == null)
            return;

        // videoPlayers 배열의 각 요소에 대해 ValidateArrays() 호출
        foreach (var v in videoPlayers)
        {
            v?.ValidateArrays();
        }
    }
    
    #endregion

}
