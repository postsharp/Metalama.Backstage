// Copyright (c) SharpCrafters s.r.o. This file is not open source. It is released under a commercial
// source-available license. Please see the LICENSE.md file in the repository root for details.

using PostSharp.Extensibility;

namespace PostSharp.Backstage.Licensing
{
    internal class LicensingMessageSource : MessageSource
    {
        public static readonly LicensingMessageSource Instance = new LicensingMessageSource();

        private LicensingMessageSource( ) : base( "PostSharp.Licensing", new LicensingMessageDispenser() )
        {
        }

        private class LicensingMessageDispenser : MessageDispenser
        {
            public LicensingMessageDispenser( ) : base( "PS" )
            {
            }

            protected override string GetMessage( int number )
            {
                switch ( number )
                {
                    case 0132:
                        return @"License error. The user license '{0}' is invalid or has expired.";

                    case 0133:
                        return @"License {0} found in assembly '{1}' has been revoked. Please contact the vendor of this component.";

                    case 128:
                        return @"You are using a test license key. It means that your copy of PostSharp is not properly licensed.";

                    case 146:
                        return @"License error. The license {0} in file '{1}' is invalid: {2}";

                    case 147:
                        return @"License error. The license {0} needs to be activated on the current machine (MachineHash={1:x}).";

                    case 148:
                        return @"Cannot get a lease from the license server: {0}.";

                    case 149:
                        return @"Cannot use the license {0} from a license server: invalid license type.";

                    case 260:
                        return @"License error. The license {0} is not allowed to be loaded from {1}.";


                    case 300:
                        return @"License error. The license {0} is for a different product than PostSharp 3";

                    case 301:
                        return @"Cannot parse license key string '{0}'.";
                    
                    case 302:
                        return @"Cannot use multiple per-usage licenses at the same time. License '{0}' not loaded.";
                    
                    case 303:
                        return @"Per-usage licenses can only be used in configuration files. License '{0}' not loaded.";

                    default:
                        return null;
                }
            }
        }
    }
}