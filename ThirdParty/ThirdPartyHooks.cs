// Copyright (c) Shane Woolcock. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable InconsistentNaming

namespace Celeste.Mod.GravityHelper.ThirdParty
{
    public static class ThirdPartyHooks
    {
        public static IEnumerable<Type> ThirdPartyModTypes =>
            ReflectionCache.LoadableTypes.Where(t =>
                t.GetCustomAttribute<ThirdPartyModAttribute>() != null);

        public static readonly Dictionary<string, ThirdPartyModSupport> LoadedMods = new();

        public static void Load()
        {
            Logger.Log(nameof(GravityHelperModule), "Loading third party hooks...");
            ReflectionCache.LoadThirdPartyTypes();

            foreach (var type in ThirdPartyModTypes)
            {
                if (Activator.CreateInstance(type) is ThirdPartyModSupport modSupport && modSupport.TryLoad())
                {
                    LoadedMods[modSupport.Attribute.Name] = modSupport;
                }
            }
        }

        // ReSharper disable once UnusedMember.Global
        public static void Unload()
        {
            Logger.Log(nameof(GravityHelperModule), "Unloading third party hooks...");
            var mods = LoadedMods.Values.ToArray();
            foreach (var mod in mods)
            {
                if (mod.TryUnload())
                {
                    LoadedMods.Remove(mod.Attribute.Name);
                }
            }
        }
    }
}
