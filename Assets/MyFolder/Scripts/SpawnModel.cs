using UnityEngine;

public class SpawnModel : MonoBehaviour
{
    [SerializeField] private GameObject[] model;
    [SerializeField] private Transform[] spawnPoints;
    
    [SerializeField] private int value;

    public void ChangeFace(Texture2D texture)
    {
        int val = Random.Range(0, model.Length);
        
        int pos = Random.Range(0, spawnPoints.Length);
        
        val = 0;
        GameObject prefab = Instantiate(model[value], spawnPoints[pos].position, spawnPoints[pos].rotation);
        
        SetMaterials setMat = prefab.GetComponent<SetMaterials>();
        
        setMat.ChangeFace(texture);
    }

    public void ChangeBody(Texture2D texture)
    {
        int val = Random.Range(0, model.Length);
        val = 0;
        
        int pos = Random.Range(0, spawnPoints.Length);
        GameObject prefab = Instantiate(model[value], spawnPoints[pos].position, spawnPoints[pos].rotation);
        
        SetMaterials setMat = prefab.GetComponent<SetMaterials>();
        
        setMat.ChangeBody(texture);
    }
}
