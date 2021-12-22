using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    AudioSource audioSource;

    public SoundManager(AudioSource audioSource) {
        this.audioSource = audioSource;
    }

    public void PlaySound(string reference, int channel) {
        audioSource.PlayOneShot(Loader.Instance.effects.Find(x => x.name== reference));
    }
    public void SetMusic(int musicId) {
        audioSource.clip = Loader.Instance.musics[musicId];
    }
    public void SetVolume(float volume) {
        audioSource.volume = volume;
    }
    public void Play() {
        audioSource.Play();
    }
    public void Stop() {
        audioSource.Stop();
    }
}
