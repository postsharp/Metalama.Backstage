// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;

namespace PostSharp.Cli.Tests.Commands.Licensing
{
    internal static class TestLicenses
    {
        public const string Key1 = "2-ZTQQQQQQQT2ZTQQQQQQQQQQQQQAEGDAFA6KQ7ADVA6JE7BDV62EX37EVVJ8KZSQSKCLTQFGSFVSM8Y4RHVASNSYYC2GXGBB82AEG5YD6HEH2Z5Y8QBFP2HZPXQGKLTET4QQWANS3P3";

        public const string Format1 = @"({0}) PostSharp Threading (License)
Licensee: SharpCrafters s.r.o.
Subscription End Date: Never (perpetual license)
Maintenance Expiration: Friday, 01 April 2050

";

        public const string Key2 = "2-ZTQQQQQQZE2EQCRCE4UW3UFEB4URXMHRB8KQBJJSB64LX7EAEG26YXHZG7MR59XBTN4M26LZTC6GQ5LUMAZQSHKJ2QYBWHN6AD6RCEAH6R4WCJP2GUZQQDTFJJPA";

        public const string Format2 = @"({0}) PostSharp MVVM (Per-Developer Subscription)
Licensee: SharpCrafters s.r.o.
Subscription End Date: Never (perpetual license)
Maintenance Expiration: Friday, 01 April 2050

";

        public const string Key3 = "2-ZTQQQQQQQE2ZTQQQQQQQQQQQQQAEGDAFA6KQ7ADVA6JE7BDV62EX37EVVJ8KZSQSHBZDWDEZX22E68YNRTE7USY94T2FGS353TZ9JGPA6ERTXYTE4JUK9UY8GH6WA6RA4QQWANS3P3";

        public const string Format3 = @"({0}) PostSharp MVVM (License)
Licensee: SharpCrafters s.r.o.
Subscription End Date: Never (perpetual license)
Maintenance Expiration: Friday, 01 April 2050

";

        public const string CommunityFormat = @"({0}) PostSharp Caravela (Community License)
Subscription End Date: Never (perpetual license)

";

        public const string EvaluationFormat = @"({0}) PostSharp Caravela (Evaluation License)
Subscription End Date: Saturday, 15 February 2020
Maintenance Expiration: Saturday, 15 February 2020

";

        public const string NextEvaluationFormat = @"({0}) PostSharp Caravela (Evaluation License)
Subscription End Date: Monday, 15 February 2021
Maintenance Expiration: Monday, 15 February 2021

";

        public static readonly DateTime EvaluationStart = new( 2020, 1, 1 );

        public static readonly DateTime InvalidNextEvaluationStart = new( 2020, 1, 14 );

        public static readonly DateTime ValidNextEvaluationStart = new( 2021, 1, 1 );
    }
}
