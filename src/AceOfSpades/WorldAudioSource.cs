using Dash.Engine.Audio;
using System;
using System.Collections.Generic;

namespace AceOfSpades
{
    public class WorldAudioSource : IDisposable
    {
        readonly AudioSource audioSource;
        readonly List<int> auxSlots = new List<int>();
        readonly List<int> effects = new List<int>();
        
        public WorldAudioSource(AudioSource audioSource)
        {
            this.audioSource = audioSource;
        }

        public int AddAuxSlot()
        {
            // Gen aux slot
            int auxSlot = AL.Efx.GenAuxiliaryEffectSlot();
            AL.Utils.CheckError();

            // Attach aux slot to audio source
            AL.Source(audioSource.SourceId, ALSource3i.EfxAuxiliarySendFilter, auxSlot, auxSlots.Count, 0);

            auxSlots.Add(auxSlot);

            return auxSlot;
        }

        public int AddEffect(EfxEffectType type, int auxSlot)
        {
            // Create effect
            int effect = AL.Efx.GenEffect();
            AL.Utils.CheckError();

            effects.Add(effect);
            
            // Set effect type
            AL.Efx.Effect(effect, EfxEffecti.EffectType, (int)type);
            AL.Utils.CheckError();

            // Attach to aux slot
            AL.Efx.AuxiliaryEffectSlot(auxSlot, EfxAuxiliaryi.EffectslotEffect, effect);
            AL.Utils.CheckError();

            return effect;
        }

        public void Play()
        {
            AL.Utils.CheckError();

            audioSource.Play();
        }

        public bool IsDone()
        {
            return audioSource.State == ALSourceState.Stopped;
        }

        public void Dispose()
        {
            audioSource.Dispose();

            foreach (int effect in effects)
                AL.Efx.DeleteEffect(effect);

            foreach (int auxSlot in auxSlots)
                AL.Efx.DeleteAuxiliaryEffectSlot(auxSlot);

            AL.Utils.CheckError();
        }
    }
}
