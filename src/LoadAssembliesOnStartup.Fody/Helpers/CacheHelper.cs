// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CacheHelper.cs" company="Catel development team">
//   Copyright (c) 2008 - 2014 Catel development team. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace LoadAssembliesOnStartup.Fody
{
    using System.Collections;
    using System.Collections.Generic;

    public static class CacheHelper
    {
        private static readonly Dictionary<string, IDictionary> _cacheByName = new Dictionary<string, IDictionary>(); 

        public static T GetCache<T>(string name)
            where T : IDictionary, new()
        {
            if (!_cacheByName.ContainsKey(name))
            {
                _cacheByName[name] = new T();
            }

            return (T)_cacheByName[name];
        }

        public static void ClearAllCaches()
        {
            foreach (var cache in _cacheByName)
            {
                cache.Value.Clear();
            }
        }
    }
}