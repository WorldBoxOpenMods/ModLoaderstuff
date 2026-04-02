using FMOD;
using UnityEngine;

namespace NeoModLoader.utils.Sounds;
using static FMODHelper;
/// <summary>
/// an Audio library. multiple can be used
/// </summary>
public class SoundLibrary : AssetLibrary<SoundAsset>
{
    /// <summary>
    /// Loads a custom sound from the wav library
    /// </summary>
    /// <param name="pX">the X position</param>
    /// <param name="pY">the Y position</param>
    /// <param name="ID">the ID of the wav file, aka its file name</param>
    /// <param name="AttachedTo">The transform to attach the sound to, if 3D</param>
    /// <returns>The Audio Channel</returns>
    public static AudioChannel LoadSound(SoundAsset Sound, float pX, float pY, Transform AttachedTo = null)
    {
        var player = Sound.GetRandom();
        AudioChannel channel = new AudioChannel(player.PlaySound(pX, pY), AttachedTo);
        Sound.Handler.Channels.Add(channel);
        return channel;
    }
    /// <summary>
    /// the main audio library. updates by itself
    /// </summary>
    public static SoundLibrary MainLibrary { get; internal set; }
    public void ClearSounds()
    {
        foreach (var sound in list)
        {
           sound.Handler.ClearChannels();
        }
    }
    public void Update()
    {
        foreach (var sound in list)
        {
            sound.Handler.Update();
        }
    }
    /// <summary>
    /// plays a sound at a location unless another sound with the same path is playing, then the other sound is set to that position 
    /// </summary>
    public AudioChannel LoadDrawingSound(string pSoundPath, float pX, float pY)
    {
        var Sound = dict[pSoundPath];
        if(Sound.Handler.DrawingSound is { Finushed: false })
        {
            SetChannelPosition(Sound.Handler.DrawingSound.Channel, pX, pY);
        }
        else
        {
            Sound.Handler.DrawingSound = LoadSound(Sound, pX, pY);
        }
        return Sound.Handler.DrawingSound;
    }
    public SoundAsset CheckID(string ID)
    {
        return dict.TryGetValue(ID, out var checkID) ? checkID : add(new SoundAsset(ID));
    }
}