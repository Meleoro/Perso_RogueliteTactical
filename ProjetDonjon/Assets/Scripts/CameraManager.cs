using UnityEngine;
using Utilities;

public class CameraManager : GenericSingletonClass<CameraManager>
{
    [Header("Parameters")]
    [SerializeField] private float followSpeed;
    [SerializeField] private Vector3 followOffset;

    [Header("Private Infos")]
    private Transform followedTransform;
    private bool isInitialised;

    [Header("Public Infos")]
    public Camera Camera { get { return _camera; } }

    [Header("References")]
    [SerializeField] private Camera _camera;


    public void Initialise(Transform followedTransform)
    {
        isInitialised = true;

        this.followedTransform = followedTransform;
    }


    private void Update()
    {
        if (!isInitialised) return;

        transform.position = Vector3.Lerp(transform.position, followedTransform.position + followOffset, followSpeed * Time.deltaTime);
    }


    public void EnterBattle()
    {

    }

    public void ExitBattle()
    {

    }


}
