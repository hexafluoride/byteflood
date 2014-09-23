using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PhotoLoader.ImageLoaders
{
    internal static class LoaderFactory
    {
        public static ILoader CreateLoader(SourceType sourceType)
        {
            switch (sourceType)
            {
                case SourceType.CachedExternalResoucre:
                    return new CachedExternalResoucre();
                case SourceType.ExternalResource:
                    return new ExternalLoader();
                case SourceType.LocalDisk:
                    return new LocalDiskLoader();
                default:
                    throw new ApplicationException("Unexpected exception");
            }
        }
    }
}
