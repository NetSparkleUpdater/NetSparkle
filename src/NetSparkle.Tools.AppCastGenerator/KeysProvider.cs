using System;
using System.Collections.Generic;
using System.Linq;

namespace NetSparkleUpdater.AppCastGenerator
{
    public class KeysProvider
    {
        private readonly IEnumerable<IKeysProvider> _keysProviders;

        public KeysProvider(IEnumerable<IKeysProvider> keysProviders)
        {
            _keysProviders = keysProviders;
        }

        public TProvider GetRequiredProvider<TProvider>() where TProvider : IKeysProvider
        {
            return _keysProviders.OfType<TProvider>().First();
        }

        public bool KeysExist() => GetPublicKey() != null && GetPrivateKey() != null;

        public byte[]? GetPrivateKey() => GetKey(provider => provider.GetPrivateKey());

        public byte[]? GetPublicKey() => GetKey(provider => provider.GetPublicKey());

        public byte[]? GetKey(Func<IKeysProvider, byte[]?> keyRetriever)
        {
            foreach (var provider in _keysProviders)
            {
                var key = keyRetriever(provider);

                if (key != null)
                {
                    return key;
                }
            }

            return null;
        }
    }
}
