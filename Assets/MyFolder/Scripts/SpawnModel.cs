using UnityEngine;

public class SpawnModel : MonoBehaviour
{
    [SerializeField] private GameObject[] model;

    [SerializeField] private int value;

    public void ChangeFace(Texture2D texture)
    {
        int val = Random.Range(0, model.Length);
        val = 0;
        GameObject prefab = Instantiate(model[value]);
        
        SetMaterials setMat = prefab.GetComponent<SetMaterials>();
        
        setMat.ChangeFace(texture);
    }

    public void ChangeBody(Texture2D texture)
    {
        int val = Random.Range(0, model.Length);
        val = 0;
        GameObject prefab = Instantiate(model[value]);
        
        SetMaterials setMat = prefab.GetComponent<SetMaterials>();
        
        setMat.ChangeBody(texture);
    }
}
