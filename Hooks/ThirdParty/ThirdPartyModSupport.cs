// Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Reflection;

namespace Celeste.Mod.GravityHelper.Hooks.ThirdParty
{
    public abstract class ThirdPartyModSupport : IDisposable
    {
        private bool _loaded;

        public ThirdPartyModAttribute Attribute =>
            GetType().GetCustomAttribute<ThirdPartyModAttribute>(true);

        public EverestModule Module =>
            Everest.Modules.FirstOrDefault(m => m.Metadata.Name == Attribute.Name);

        public bool TryLoad()
        {
            var attr = Attribute;

            if (_loaded)
            {
                Logger.Log(nameof(GravityHelperModule), $"{attr.Name} already loaded, skipping.");
                return false;
            }

            var module = Module;

            if (module == null)
            {
                Logger.Log(nameof(GravityHelperModule), $"{attr.Name} not found, skipping.");
                return false;
            }

            if (Version.TryParse(attr.MinimumVersion ?? string.Empty, out var minVersion) && module.Metadata.Version < minVersion)
            {
                Logger.Log(nameof(GravityHelperModule), $"{module.Metadata.Name} ({module.Metadata.VersionString}) is less than minimum version {minVersion}, skipping.");
                return false;
            }

            if (Version.TryParse(attr.MaximumVersion ?? string.Empty, out var maxVersion) && module.Metadata.Version > maxVersion)
            {
                Logger.Log(nameof(GravityHelperModule), $"{module.Metadata.Name} ({module.Metadata.VersionString}) is greater than maximum version {minVersion}, skipping.");
                return false;
            }

            try
            {
                Logger.Log(nameof(GravityHelperModule), $"Loading mod support for {module.Metadata.Name} ({module.Metadata.Version})...");
                Load();
            }
            catch (Exception)
            {
                Logger.Log(LogLevel.Error, nameof(GravityHelperModule), $"Exception loading mod support for {module.Metadata.Name} ({module.Metadata.Version}).");
                throw;
            }

            _loaded = true;

            return true;
        }

        public bool TryUnload()
        {
            var attr = Attribute;

            if (!_loaded)
            {
                Logger.Log(nameof(GravityHelperModule), $"{attr.Name} not yet loaded, skipping.");
                return false;
            }

            var module = Module;

            try
            {
                Logger.Log(nameof(GravityHelperModule), $"Unloading mod support for {module.Metadata.Name} ({module.Metadata.Version})...");
                Unload();
            }
            catch (Exception)
            {
                Logger.Log(LogLevel.Error, nameof(GravityHelperModule), $"Exception unloading mod support for {module.Metadata.Name} ({module.Metadata.Version}).");
                throw;
            }

            _loaded = false;

            return true;
        }

        protected abstract void Load();
        protected abstract void Unload();

        protected virtual void Dispose(bool disposing)
        {
            if (_loaded)
            {
                TryUnload();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
