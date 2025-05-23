using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using DG.Tweening;

public class SetMaterials : MonoBehaviour
{
    [SerializeField] private GameObject face;
    [SerializeField] private SkinnedMeshRenderer[] meshRenderers;
    [SerializeField] private Material bodyMat;
    [SerializeField] private Material faceMat;
    
    private Material _newMaterial;

    private const int MaxClick = 10;

    private int _currentClick;
    private int CurrentClick
    {
        get => _currentClick;
        set
        {
            _currentClick =  value;
            if (_currentClick >= MaxClick)
            {
                _canClick = false;
            }
        }
    }
    
    private bool _canClick = true;
    private int _currentPosition;

    private bool CanClick
    {
        get => _canClick;
        set
        {
            if (true)
            {
                
            }
            _canClick = value;
        }
    }


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

    private void OnDestroy()
    {
        if (_newMaterial)
        {
            Destroy(_newMaterial);
        }
        
        SpawnModel.Instance.AddPosition(_currentPosition);
    }

    public void SetPosition(int position)
    {
        _currentPosition = position;
    }

    public void Clicked()
    {
        if (_canClick)
        {
            AudioManager.Instance.PlayOneShotAudio(AudioName.Touch);
            PlayScaleBounce().Forget();
            ++CurrentClick;
        }
    }
    
    private float scaleUpPercent = 1.5f;  // 150%
    private float scaleDownPercent = 1.1f;  // 105%
    private Vector3 scaleUpPercentVector = Vector3.one * 0.04f * 0.5f;  // 50%
    private Vector3 scaleDownPercentVector = Vector3.one * 0.04f * 0.1f;  // 10%
    private float scaleUpDuration = 0.3f;
    private float scaleDownDuration = 0.2f;
    private Ease scaleUpEase = Ease.OutQuad;
    private Ease scaleDownEase = Ease.OutBack;

    private Tween _currentTween;
    private Vector3 _initialScale;

    [SerializeField] private int val = 150;
    [SerializeField] private float upTime = 3;

    private async UniTaskVoid PlayScaleBounce()
    {
        _canClick = false;
        _currentTween?.Kill(); // 기존 Tween 제거

        await UniTask.Delay(val);
        
		_initialScale = transform.localScale;
        _currentTween = transform.DOScale(_initialScale * scaleUpPercent, scaleUpDuration)
            .SetEase(scaleUpEase)
            .OnComplete(() =>
            {
                transform.DOScale(_initialScale + scaleDownPercentVector, scaleDownDuration)
                    .SetEase(scaleDownEase).OnComplete(() =>
                    {
                        if (CurrentClick < MaxClick)
                        {
                          _canClick = true;
                        }
                        else
                        {
                          transform.DOMoveY(7,upTime).OnComplete(() => Destroy(gameObject));
                        }
                    });
            });
    }
}
