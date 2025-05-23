using System.Collections.Generic;
using UnityEngine;

public class SpawnModel : MonoBehaviour
{
    [SerializeField] private GameObject[] model;
    [SerializeField] private Transform[] spawnPoints;

    [SerializeField] private int value;

    
    
    private List<int> _availableIndices = new();

    public static SpawnModel Instance;
    
    private void Awake()
    {
        Instance = this;
        
        for (int i = 0; i < spawnPoints.Length; i++)
            _availableIndices.Add(i);

        Shuffle(_availableIndices); // Fisher-Yates 방식 추천
    }

    private void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    
    public void ChangeFace(Texture2D texture)
    {
        if (_availableIndices.Count == 0) return;

        int val = Random.Range(0, model.Length);
        val = 0;

        int pos = _availableIndices[^1];
        _availableIndices.RemoveAt(_availableIndices.Count - 1); // pop

        GameObject prefab = Instantiate(model[value], spawnPoints[pos].position, spawnPoints[pos].rotation);
        SetMaterials setMat = prefab.GetComponent<SetMaterials>();
        setMat.SetPosition(pos);
        setMat.ChangeFace(texture);
    }

    public void ChangeBody(Texture2D texture)
    {
        if (_availableIndices.Count == 0) return;

        int val = Random.Range(0, model.Length);
        val = 0;

        int pos = _availableIndices[^1];
        _availableIndices.RemoveAt(_availableIndices.Count - 1); // pop

        GameObject prefab = Instantiate(model[value], spawnPoints[pos].position, spawnPoints[pos].rotation);
        SetMaterials setMat = prefab.GetComponent<SetMaterials>();
        setMat.SetPosition(pos);
        setMat.ChangeBody(texture);
    }

    public void AddPosition(int index)
    {
        _availableIndices.Add(index);
    }
}