// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

// ReSharper disable StringLiteralTypo

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Licenses;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Metalama.Backstage.Licensing.Tests
{
    internal static class TestLicenseKeys
    {
        public static string PostSharpUltimate
            => "1-ZEQQQQQQZTQEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEJZWKEM8SCXJK6KJLFD92CAJFQKCGC67A9NVYA2JGNEHLB8QQG4JAF94J58KUJQZW8ZQQDTFJJPA";

        public static string Logging
            => "1-ZEQQQQQQBQ2EQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEA26798K7BDCC6J4AVC4RA58S57EJRJP7XMW2ZFU8QN7HH5DLGT568B2WT7XMBFD5MZQQDTFJJPA";

        public static string Caching
            => "2-ZTQQQQQQZU2EQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEAKRW84MFYS4AFSXEQSKCUVQXSRASB83N63W6PP82747CG4QUEUS73VTFR7BP33M6LZQQDTFJJPA";

        public static string OpenSource
            => "1-ZEQQQQQQXTQEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7YQ2U6X7JDAEDSBJASRU274BE6P2PS8M9N3YVNXKRJX3MVVPQ78YGJZ4XRXLEJJYH8FEY7GRKUKSJZQQDTFJJPA";

        public static string MetalamaUltimate
            => "1-ZEQQQQQQZEAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEASD8CXFHZY99JSJCPGSS6F3Q258BHCEBQCCLP85GRPFUZWBPAKLCV8CDZQ3JUUZFPZQQDEZJGP4Q8USJG4X6P2";

        public static string PostSharpEssentials
            => "1-ZEQQQQQQATQEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEJFEB4V4U8DUPY3JP4Y9SXVNF9CSV3ADB53Z69RDR7PZMZGF7GRQPQQ5ZH3PQF7PHJZQTP2";

        public const string MetalamaUltimateEssentials
            = "2-ZTQQQQQQAEAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEJNYQZ645W4ZPHXRWCAW2WVVGKMS4FAAFH6W6FCBMXTNX5PHLC9X5HT92MNBVUKMQUZQQDEZJGP4Q8USJG4X6P2";

        public static string OpenSourceNamespace => "Oss";

        public static string CreateMetalamaEssentialsLicense( IServiceProvider services )
        {
            var licenseFactory = new UnsignedLicenseFactory( services );
            var licenseKey = licenseFactory.CreateEssentialsLicense().LicenseKey;
            return licenseKey;
        }
    }
}