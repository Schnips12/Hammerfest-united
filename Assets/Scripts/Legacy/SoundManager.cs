using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager
{
    AudioSource audioSource;
    List<AudioClip> musics;
    List<AudioClip> soundEffects;

    public SoundManager(AudioSource audioSource, List<AudioClip> musics, List<AudioClip> soundEffects) {
        this.audioSource = audioSource;
        this.musics = musics;
        this.soundEffects = soundEffects;
    }

    public void PlaySound(string reference, int channel) {
        audioSource.PlayOneShot(soundEffects.Find(x => x.name== reference));
    }
}
