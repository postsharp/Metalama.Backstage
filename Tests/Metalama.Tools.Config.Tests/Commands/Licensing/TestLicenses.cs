// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this rep root for details.

using System;

namespace Metalama.Tools.Config.Tests.Commands.Licensing
{
    internal static class TestLicenses
    {
        public const string MetalamaStarterBusinessKey =
            "3-ZUQQQQQQZUAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAED3WYKF9V8KYEUMGXKBYPRQNNUVQDV3BHMGEHNJHLBKGSD57EEJQRD4F3SWWEA42CUZQQDEZJGP4Q8USJG4X6P2";

        public const string MetalamaStarterBusinessOutput = @"              License ID: 3
             License Key: 3-ZUQQQQQQZUAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAED3WYKF9V8KYEUMGXKBYPRQNNUVQDV3BHMGEHNJHLBKGSD57EEJQRD4F3SWWEA42CUZQQDEZJGP4Q8USJG4X6P2
             Description: Metalama Starter (Per-Developer Subscription)
                Licensee: SharpCrafters s.r.o.
      License Expiration: Never (perpetual license)
  Maintenance Expiration: Saturday, 01 January 2050

";

        public const string MetalamaProfessionalPersonalKey =
            "11-ZUZQQQQQ6TAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEGQM4FBBD3KBB3JBXWAS9UBFLGGN7WJWBF9TXKVTWH2PWUB5HE52LBPVQ2LJQNK5YKZQQDEZJGP4Q8USJG4X6P2";

        public const string MetalamaProfessionalPersonalOutput = @"              License ID: 11
             License Key: 11-ZUZQQQQQ6TAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEGQM4FBBD3KBB3JBXWAS9UBFLGGN7WJWBF9TXKVTWH2PWUB5HE52LBPVQ2LJQNK5YKZQQDEZJGP4Q8USJG4X6P2
             Description: Metalama Professional (Personal License)
                Licensee: SharpCrafters s.r.o.
      License Expiration: Never (perpetual license)
  Maintenance Expiration: Saturday, 01 January 2050

";

        public const string MetalamaUltimateOpenSourceRedistributionKey =
            "8-ZQZQQQQQXEAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7YQ2GYCXBSF629W7YDRH29BN7JFYCJX3MFVVAHZXJ9RS29KYTHFS8KQ7TFRS6ZTBVWZLKJVF3HZZHWA4ZKSX3DXZYBKR4MWCZF4AW43L2DLEPB5T8HFVMFKBYLUG2X78SQQBTWB2P7QNG4B27RXP3";

        public const string MetalamaUltimateOpenSourceRedistributionOutput = @"              License ID: 8
             License Key: 8-ZQZQQQQQXEAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7YQ2GYCXBSF629W7YDRH29BN7JFYCJX3MFVVAHZXJ9RS29KYTHFS8KQ7TFRS6ZTBVWZLKJVF3HZZHWA4ZKSX3DXZYBKR4MWCZF4AW43L2DLEPB5T8HFVMFKBYLUG2X78SQQBTWB2P7QNG4B27RXP3
             Description: Metalama Ultimate (Open-Source Redistribution License)
                Licensee: SharpCrafters s.r.o.
      License Expiration: Never (perpetual license)
  Maintenance Expiration: Saturday, 01 January 2050

";

        public const string FreeOutput = @"             Description: Metalama Free (Personal License)
      License Expiration: Never (perpetual license)

";

        public const string EvaluationOutput = @"             Description: Metalama Ultimate (Evaluation License)
      License Expiration: Saturday, 15 February 2020
  Maintenance Expiration: Saturday, 15 February 2020

";

        public const string NextEvaluationOutput = @"             Description: Metalama Ultimate (Evaluation License)
      License Expiration: Monday, 15 February 2021
  Maintenance Expiration: Monday, 15 February 2021

";

        public static readonly DateTime EvaluationStart = new( 2020, 1, 1 );

        public static readonly DateTime InvalidNextEvaluationStart = new( 2020, 1, 14 );

        public static readonly DateTime ValidNextEvaluationStart = new( 2021, 1, 1 );
    }
}