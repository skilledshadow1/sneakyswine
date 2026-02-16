using UnityEngine;

namespace _Scripts
{
    public class EnvironmentalSounds : MonoBehaviour
    {
        [SerializeField] private AudioClip natureSounds;
        [SerializeField] private AudioClip themeMusic;
        [SerializeField] private AudioClip franticMusic;
        
        [SerializeField] private AudioSource natureSource;
        [SerializeField] private AudioSource musicSource;
        
        void Start()
        {
            natureSource.clip = natureSounds;
            musicSource.clip = themeMusic;
            
            natureSource.Play();
            musicSource.Play();
        }
        
        public void SetLastHourMusic()
        {
            musicSource.Stop();
            musicSource.clip = franticMusic;
            musicSource.Play();
        }
    }
}
