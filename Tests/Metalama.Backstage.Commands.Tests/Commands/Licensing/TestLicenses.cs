// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;

namespace Metalama.Tools.Config.Tests.Commands.Licensing
{
    internal static class TestLicenses
    {
        public const string MetalamaStarterBusinessKey =
            "3-ZUQQQQQQZUAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAED3WYKF9V8KYEUMGXKBYPRQNNUVQDV3BHMGEHNJHLBKGSD57EEJQRD4F3SWWEA42CUZQQDEZJGP4Q8USJG4X6P2";

        public const string MetalamaProfessionalPersonalKey =
            "11-ZUZQQQQQ6TAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEGQM4FBBD3KBB3JBXWAS9UBFLGGN7WJWBF9TXKVTWH2PWUB5HE52LBPVQ2LJQNK5YKZQQDEZJGP4Q8USJG4X6P2";

        public const string MetalamaUltimateOpenSourceRedistributionKey =
            "8-ZQZQQQQQXEAEQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7YQ2GYCXBSF629W7YDRH29BN7JFYCJX3MFVVAHZXJ9RS29KYTHFS8KQ7TFRS6ZTBVWZLKJVF3HZZHWA4ZKSX3DXZYBKR4MWCZF4AW43L2DLEPB5T8HFVMFKBYLUG2X78SQQBTWB2P7QNG4B27RXP3";

        public static readonly DateTime EvaluationStart = new( 2020, 1, 1 );

        public static readonly DateTime InvalidNextEvaluationStart = new( 2020, 1, 14 );

        public static readonly DateTime ValidNextEvaluationStart = new( 2021, 1, 1 );
    }
}