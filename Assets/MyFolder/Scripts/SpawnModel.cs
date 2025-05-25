using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

public class SpawnModel : MonoBehaviour
{
    [SerializeField] private GameObject[] model;
    [SerializeField] private Transform[] spawnPoints;

    private readonly List<int> _availableIndices = new();

    public static SpawnModel Instance;

    private string _url;

    private string _recentPath;
    
    private readonly Queue<(int x, string url)> _pendingSpawns = new();
    
    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void Awake()
    {
        Instance = this;

        for (int i = 0; i < spawnPoints.Length; i++)
            _availableIndices.Add(i);

        Shuffle(_availableIndices); // Fisher-Yates 방식 추천
    }

    private async UniTaskVoid Start()
    {
        _url = JsonSaver.Instance.settings.url;

        await SetInitialData();

        CheckUrl().Forget();
        ProcessSpawnQueue().Forget();
    }
    
    private async UniTaskVoid ProcessSpawnQueue()
    {
        while (true)
        {
            if (_pendingSpawns.Count > 0 && _availableIndices.Count > 0)
            {
                var (x, url) = _pendingSpawns.Dequeue();
                Spawn(x, url).Forget();
            }

            await UniTask.Delay(100); // 너무 자주 실행되지 않도록 약간의 딜레이
        }
    }

    
    private async UniTask SetInitialData()
    {
        _recentPath = (await GetStringData(_url))[0];
    }
    
    private async UniTaskVoid CheckUrl()
    {
        while (true)
        {
            try
            {
                CompareAndSpawn(_url).Forget();
                await UniTask.Delay(1000);
            }
            catch (Exception e)
            {
                Debug.LogError($"[CompareAndSpawn Error] {e}");
            }
        }
    }
    
    private async UniTaskVoid CompareAndSpawn(string url)
    {
        if (_availableIndices.Count == 0) return;
        
        Shuffle(_availableIndices);
        
        string[] split = await GetStringData(url);

        int sameIndex = 0;
        
        for (int i = 0; i < split.Length; i++)
        {
            if (string.Equals(split[i], _recentPath))
            {
                sameIndex = i;
                break;
            }
        }

        if (sameIndex == 0) return;
        _recentPath = split[0];
        
        // sameIndex -1 까지 스폰시켜야함, 그 전에 번호 추출해야함
        
        for (int i = sameIndex-1; i >= 0; i--)
        {
            string str = split[i].Split('/')[^1];
            int val = str[0] - '0';
            _pendingSpawns.Enqueue((val, split[i]));
            //Spawn(val, split[i]).Forget();
        }
    }
    
    private async UniTask<string[]> GetStringData(string url)
    {
        int exitTimes = 100;
        int currentTimes = 0;

        while (true)
        {
            float t = Time.realtimeSinceStartup;

            using var request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerBuffer();
            await request.SendWebRequest();

            if (request.result is not UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Request Failed : " + request.error);

                t = Time.realtimeSinceStartup - t;
                int time = 2000 - Mathf.FloorToInt(t * 1000f);
                await UniTask.Delay(Mathf.Max(time, 0));
                ++currentTimes;

                if (currentTimes >= exitTimes)
                {
                    Debug.LogError("Request has Exceeded Max Time");
                    return null;
                }

                continue;
            }

            string fullText = request.downloadHandler.text;
            fullText = Regex.Replace(fullText, @"\s+", "");
            
            string[] split = fullText.Split(',');
            return split;
        }
    }

    public void ChangeFace(Texture2D texture)
    {
        if (_availableIndices.Count == 0) return;

        int val = Random.Range(0, model.Length);

        int pos = _availableIndices[^1];
        _availableIndices.RemoveAt(_availableIndices.Count - 1); // pop

        GameObject prefab = Instantiate(model[val], spawnPoints[pos].position, spawnPoints[pos].rotation);
        SetMaterials setMat = prefab.GetComponent<SetMaterials>();
        setMat.SetPosition(pos);
        setMat.ChangeFace(texture);
    }

    private void ChangeBody(int bodyValue, Texture2D texture)
    {
        if (_availableIndices.Count == 0) return;

        int pos = _availableIndices[^1];
        _availableIndices.RemoveAt(_availableIndices.Count - 1); // pop

        GameObject prefab = Instantiate(model[bodyValue], spawnPoints[pos].position, spawnPoints[pos].rotation);
        SetMaterials setMat = prefab.GetComponent<SetMaterials>();
        setMat.SetPosition(pos);
        setMat.ChangeBody(texture);
    }

    public void AddPosition(int index)
    {
        _availableIndices.Add(index);
    }


    private async UniTaskVoid Spawn(int x, string url)
    {
        switch (x)
        {
            case <= 4 :
                ChangeBody(x-1, await GetTexture(url));
                break;
            case 5:
                ChangeFace(await GetTexture(url));
                break;
        }
    }



    private async UniTask<Texture2D> GetTexture(string url)
    {
        int exitTimes = 100;
        int currentTimes = 0;
        
        Debug.Log("Url : " + url);

        while (true)
        {
            float t = Time.realtimeSinceStartup;

            using var request = UnityWebRequest.Get(url);
            request.downloadHandler = new DownloadHandlerTexture();
            await request.SendWebRequest();

            if (request.result is not UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("Request Failed : " + request.error);

                t = Time.realtimeSinceStartup - t;
                int time = 2000 - Mathf.FloorToInt(t * 1000f);
                await UniTask.Delay(Mathf.Max(time, 0));
                ++currentTimes;

                if (currentTimes >= exitTimes)
                {
                    Debug.LogError("Request has Exceeded Max Time");
                    return null;
                }

                continue;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            return texture;
        }
    }
}