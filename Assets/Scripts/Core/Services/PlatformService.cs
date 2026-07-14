using System.Collections.Generic;
using SweetSweeps.Core.Contracts;

namespace SweetSweeps.Core.Services
{
    public class PlatformService : IPlatformService
    {
        private readonly List<IPlatformMover> _platforms = new();

        public void Register(IPlatformMover platform)
        {
            if (platform == null || _platforms.Contains(platform)) return;
            _platforms.Add(platform);
        }

        public void Clear() => _platforms.Clear();

        public void FreezeAll()
        {
            for (int i = 0; i < _platforms.Count; i++)
                _platforms[i].Freeze();
        }
    }
}
