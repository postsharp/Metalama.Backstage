// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

// ReSharper disable StringLiteralTypo

using Metalama.Backstage.Extensibility;
using Metalama.Backstage.Licensing.Consumption.Sources;
using Metalama.Backstage.Licensing.Licenses;
using Metalama.Backstage.Testing.Services;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Metalama.Backstage.Licensing.Tests
{
    internal static class TestLicenses
    {
        public const string PostSharpEssentials
            = "1-ZEQQQQQQATQEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEJFEB4V4U8DUPY3JP4Y9SXVNF9CSV3ADB53Z69RDR7PZMZGF7GRQPQQ5ZH3PQF7PHJZQTP2";

        public const string PostSharpFramework
            = "1-ZEQQQQQQVTQEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEDQ66TTJ4DS3VU8WVCU82LXX67S8SZ6X54UMRTATBV5CA7LDPHS2SQC85ZLBNMBFJKZQQDTFJJPA";

        public const string PostSharpUltimate
            = "1-ZEQQQQQQZTQEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEJZWKEM8SCXJK6KJLFD92CAJFQKCGC67A9NVYA2JGNEHLB8QQG4JAF94J58KUJQZW8ZQQDTFJJPA";

        public const string PostSharpEnterprise
            = "1-ZEQQQQQQ5TQEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEJX22BA3Y5MW5DV9YNZYXT2ZKHLK8KSFWEZQZ94MN5XRJKJL89XDF4ETFCGR5BN5KKZQQDTFJJPA";

        public const string Caching
            = "2-ZTQQQQQQZU2EQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEAKRW84MFYS4AFSXEQSKCUVQXSRASB83N63W6PP82747CG4QUEUS73VTFR7BP33M6LZQQDTFJJPA";

        public const string Logging
            = "2-ZTQQQQQQQQ2ZTQQQQQQQQQQQQQAEGDAFA6KQ7ADVA6JE7BDV62EX37EVVJ8KZSQSV6EZFNK97UUULVWZQPRPZRSPHNGYCBYA3J9DYC4HE4U77SEK7XJ6DKQ3DBK3DUS34QQWANS3P3";

        public const string Model
            = "2-ZTQQQQQQQE2ZTQQQQQQQQQQQQQAEGDAFA6KQ7ADVA6JE7BDV62EX37EVVJ8KZSQSHBZDWDEZX22E68YNRTE7USY94T2FGS353TZ9JGPA6ERTXYTE4JUK9UY8GH6WA6RA4QQWANS3P3";

        public const string Threading
            = "2-ZTQQQQQQQT2ZTQQQQQQQQQQQQQAEGDAFA6KQ7ADVA6JE7BDV62EX37EVVJ8KZSQSKCLTQFGSFVSM8Y4RHVASNSYYC2GXGBB82AEG5YD6HEH2Z5Y8QBFP2HZPXQGKLTET4QQWANS3P3";

        public const string NamespaceLimitedPostSharpUltimateOpenSourceNamespace = "Oss";

        public const string NamespaceLimitedPostSharpUltimateOpenSource
            = "1-ZEQQQQQQXTQEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7YQ2U6X7JDAEDSBJASRU274BE6P2PS8M9N3YVNXKRJX3MVVPQ78YGJZ4XRXLEJJYH8FEY7GRKUKSJZQQDTFJJPA";

        public const string MetalamaFreePersonal
            = "2-ZTQQQQQQ6QZEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEJ2D6DXU2WGSFJYN7NBESWRPV5AX9D5WWKRKRQQK4J3YELXMLRVDXRVBTZSKQADXRJZQQDEZJGP4Q8USJG4X6P2";

        public const string MetalamaFreeBusiness
            = "10-ZTZQQQQQZQZEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEGX2QXEM3NKE2BSNC8TTTSRTDALZM76GWYLWPL57HZ42QBQGKTWPSZY88MB4BZNL39ZQQDEZJGP4Q8USJG4X6P2";

        public const string MetalamaStarterPersonal
            = "9-ZEZQQQQQ6UAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEDW2LC3GUTP4FXFXU9RD6ZBVAJWMMR2348NXGW9FQM7A4MYTU322829RKJPZ6C3T9LZQQDEZJGP4Q8USJG4X6P2";

        public const string MetalamaStarterBusiness
            = "3-ZUQQQQQQZUAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAED3WYKF9V8KYEUMGXKBYPRQNNUVQDV3BHMGEHNJHLBKGSD57EEJQRD4F3SWWEA42CUZQQDEZJGP4Q8USJG4X6P2";

        public const string MetalamaProfessionalPersonal
            = "11-ZUZQQQQQ6TAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEGQM4FBBD3KBB3JBXWAS9UBFLGGN7WJWBF9TXKVTWH2PWUB5HE52LBPVQ2LJQNK5YKZQQDEZJGP4Q8USJG4X6P2";

        public const string MetalamaProfessionalBusiness
            = "4-ZQAQQQQQZTAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEGGU48KSPSAKJRSXFLZAF72CTB3QUMT9RR6WTPVYSNJT46PKU645EAPBXKGNFCJPVKZQQDEZJGP4Q8USJG4X6P2";

        public const string MetalamaUltimatePersonal
            = "12-ZQ2QQQQQ6EAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEG4V9LTN8CUJULDVFUEVQSC2VEDADP4C68LEKPUAX569HN3AQR8RD2R554RDLPM92LZQQDEZJGP4Q8USJG4X6P2";

        public const string MetalamaUltimateBusiness
            = "1-ZEQQQQQQZEAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEASD8CXFHZY99JSJCPGSS6F3Q258BHCEBQCCLP85GRPFUZWBPAKLCV8CDZQ3JUUZFPZQQDEZJGP4Q8USJG4X6P2";

        public const string MetalamaUltimateOpenSourceRedistribution
            = "5-ZEAQQQQQXEAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEAPQPUW2VKAJDJ45JUQENSNR7N3L5PGFRUVNNYP3CTNN4DE2U6PTPXWC9UA2GDLGPMZQQDEZJGP4Q8USJG4X6P2";

        public const string MetalamaUltimateCommercialRedistribution
            = "13-ZE2QQQQQREAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEGXV77EYWKMKUFW9XAL863M96TGYH6NHA6QB4R4TV99GJ79P3GFT5PFQQCD9EPTGJLZQQDEZJGP4Q8USJG4X6P2";

        public const string NamespaceLimitedMetalamaUltimateNamespace = "NonRedistributionTests.TargetNamespace";

        public const string NamespaceLimitedMetalamaUltimatePersonal
            = "14-ZT2QQQQQ6EAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7YQXG6LX7DCS2JBNJGVB4MWNMGVD88KXCHR629X37TCA6KVNBGVVCUMNBFVT4UXNBDAEJZ9DAFEFG9CS2SUYHNG5APFXXWWFKCKFE2A27A7VTJ8XMVV7MFD8XPN8X9QZLTC2KZQQDEZJGP4Q8USJG4X6P2";

        public const string NamespaceLimitedMetalamaUltimateBusiness
            = "6-ZTAQQQQQZEAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7YQXG6LX7DCS2JBNJGVB4MWNMGVD88KXCHR629X37TCA6KVNBGVVCUMNBFVT4UXNBDAEJVU2BU4NZCND5MQR3M9ZHSUEXFPJG72M4AZQUHMXKBYDQW8HW82EWDWJARNVBKDXUZQQDEZJGP4Q8USJG4X6P2";

        public const string NamespaceLimitedMetalamaFreeNamespace = "NonRedistributionTests.TargetNamespace";

        public const string NamespaceLimitedMetalamaFreePersonal
            = "7-ZUAQQQQQ6QZEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7YQXG6LX7DCS2JBNJGVB4MWNMGVD88KXCHR629X37TCA6KVNBGVVCUMNBFVT4UXNBDAEGPU3DDAH2CZCPE5KPFEEF6ERQX5ARC2LU3T3MSD3K564GWZBSAGTD5XBHGHH3WGTUZQQDEZJGP4Q8USJG4X6P2";

        public const string NamespaceLimitedMetalamaFreeBusiness
            = "15-ZU2QQQQQZQZEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7YQXG6LX7DCS2JBNJGVB4MWNMGVD88KXCHR629X37TCA6KVNBGVVCUMNBFVT4UXNBDAEGKW87D68UGA4ZG7QVFLVPNYBNB65WE3X68R3D32BSH6PYYXJX4QNMS63GLU2UFKWLZQQDEZJGP4Q8USJG4X6P2";

        public const string NamespaceLimitedMetalamaUltimateRedistributionNamespace = "RedistributionTests.TargetNamespace";

        public const string NamespaceLimitedMetalamaUltimateOpenSourceRedistribution
            = "8-ZQZQQQQQXEAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7YQ2GYCXBSF629W7YDRH29BN7JFYCJX3MFVVAHZXJ9RS29KYTHFS8KQ7TFRS6ZTBVWZLKJVF3HZZHWA4ZKSX3DXZYBKR4MWCZF4AW43L2DLEPB5T8HFVMFKBYLUG2X78SQQBTWB2P7QNG4B27RXP3";

        public const string NamespaceLimitedMetalamaUltimateCommercialRedistribution
            = "16-ZQWQQQQQREAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7YQ2GYCXBSF629W7YDRH29BN7JFYCJX3MFVVAHZXJ9RS29KYTHFS8KQ7TFRS6ZTZHUAE5D6EGGR7GKBRC4N4BYSAS4MX2JRFGTW3AELYYSMRX29B3S6XLBLCS4C5MG6NSQQBTWB2P7QNG4B27RXP3";

        public static readonly DateTime SubscriptionExpirationDate = new DateTime( 2050, 1, 1 );

        public static string CreateMetalamaFreeLicense( IServiceProvider services )
        {
            var licenseFactory = new UnsignedLicenseFactory( services );
            var licenseKey = licenseFactory.CreateFreeLicense().LicenseKey;
            return licenseKey;
        }

        public static string CreateMetalamaEvaluationLicense( IServiceProvider services )
        {
            var licenseFactory = new UnsignedLicenseFactory( services );
            var licenseKey = licenseFactory.CreateEvaluationLicense().LicenseKey;
            return licenseKey;
        }

        public static PreviewLicenseSource CreatePreviewLicenseSource( bool isPrerelease, int daysAfterBuild )
        {
            var services = new ServiceCollection();
            var timeProvider = new TestDateTimeProvider();
            services.AddSingleton<IDateTimeProvider>( timeProvider );

            services.AddSingleton<IApplicationInfoProvider>(
                new ApplicationInfoProvider( new TestApplicationInfo( "Test", isPrerelease, "<version>", timeProvider.Now.AddDays( -daysAfterBuild ) ) ) );

            var serviceProvider = services.BuildServiceProvider();

            return new PreviewLicenseSource( serviceProvider );
        }

        public static UnattendedLicenseSource CreateUnattendedLicenseSource()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IApplicationInfoProvider>(
                new ApplicationInfoProvider( new TestApplicationInfo( "Test", false, "<version>", DateTime.Now, true ) ) );

            var servicesProvider = services.BuildServiceProvider();

            return new UnattendedLicenseSource( servicesProvider );
        }
    }
}