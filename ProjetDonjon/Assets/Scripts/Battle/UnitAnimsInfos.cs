using UnityEngine;

public class UnitAnimsInfos : MonoBehaviour
{
    [Header("Public Infos")]
    public bool PlaySkillEffect;
    public bool PlayVFX;
    public bool PlayCritVFX;

    [Header("Private Infos")]
    private bool isPlayingVFX;
    private bool isPlayingCritVFX;

    [Header("References")]
    [SerializeField] private ParticleSystem _vfxToPlay;
    [SerializeField] private Animator _critVFXAnim;

    private void Update()
    {
        if(!isPlayingVFX && PlayVFX)
        {
            isPlayingVFX = true;
            _vfxToPlay.Play();
        }
        else if(isPlayingVFX && !PlayVFX)
        {
            _vfxToPlay.Stop();
            isPlayingVFX = false; 
        }

        if (PlayCritVFX && !isPlayingCritVFX)
        {
            isPlayingCritVFX = true;
            _critVFXAnim.SetTrigger("PlayVFX");
        }
        else if(!PlayCritVFX && isPlayingCritVFX)
        {
            isPlayingCritVFX = false;
        }
    }
}
