using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class SetMaterials : MonoBehaviour
{
    [SerializeField] private GameObject face;
    [SerializeField] private SkinnedMeshRenderer[] meshRenderers;
    [SerializeField] private Material bodyMat;
    [SerializeField] private Material faceMat;

    private Material _newMaterial;

    private void OnEnable()
    {
        AudioManager.Instance.PlayOneShotAudio(AudioName.Down);
    }

    public void ChangeBody(Texture2D texture)
    {
         _newMaterial = new Material(bodyMat)
        {
            mainTexture = texture
        };

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            Material[] sharedMats = meshRenderers[i].sharedMaterials;
            bool changed = false;

            for (int j = 0; j < sharedMats.Length; j++)
            {
                if (sharedMats[j] == bodyMat)
                {
                    sharedMats[j] = _newMaterial;
                    changed = true;
                }
            }

            if (changed)
            {
                meshRenderers[i].sharedMaterials = sharedMats;
            }
        }
    }

    public void ChangeFace(Texture2D texture)
    {
        face.SetActive(false);
        
        _newMaterial = new Material(faceMat)
        {
            mainTexture = texture
        };

        Material[] sharedMats = meshRenderers[3].sharedMaterials;

        for (int j = 0; j < sharedMats.Length; j++)
        {
            if (sharedMats[j] == faceMat)
            {
                sharedMats[j] = _newMaterial;
            }
        }
        
        meshRenderers[3].sharedMaterials = sharedMats;
        
    }

    private void OnDisable()
    {
        if (_newMaterial)
        {
            Destroy(_newMaterial);
        }
    }

    public void Clicked()
    {
        AudioManager.Instance.PlayOneShotAudio(AudioName.Touch);
        
    }
}
