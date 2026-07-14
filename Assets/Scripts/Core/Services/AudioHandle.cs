using System;
using UnityEngine;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.Services
{
    public class AudioHandle : IAudioHandle
    {
        private readonly AudioSource _source;
        private readonly AudioSourcePool _pool;
        private readonly Action<AudioHandle> _onStopped;

        private bool _stopped;

        public AudioHandle(AudioSource source, AudioSourcePool pool, Action<AudioHandle> onStopped)
        {
            _source = source;
            _pool = pool;
            _onStopped = onStopped;
        }

        public bool IsPlaying => !_stopped && _source != null && _source.isPlaying;

        public void SetPitch(float pitch)
        {
            if (_stopped) return;
            _source.pitch = pitch;
        }

        public void Stop()
        {
            if (_stopped) return;
            _stopped = true;

            _pool.Return(_source);
            _onStopped?.Invoke(this);
        }
    }
}
