using System;
using P24.WSDL;
using P24.REST;
using System.Reflection;
using System.CodeDom.Compiler;


namespace SampleWsdlP24
{
    public class Program
    {
        public static void Main (string[] args)
        {
//            // This is only a test field for P24 WLSD Library.
//            P24WsdlDynamicClass p24 = 
//                new P24WsdlDynamicClass (49518, WsdlMode.sandobx, WsdlType.encoded, WsdlVersion.Wsdl30);
//
//            //            if (p24.Errors.Length > 0)
//            //            {
//            //                Console.WriteLine ("Errors :");
//            //                foreach (CompilerError error in p24.Errors)
//            //                {
//            //                    Console.WriteLine ("\t{0}", error.ToString ());
//            //                }
//            //            }
//            //            if (p24.Warnings.Length > 0)
//            //            {
//            //                Console.WriteLine ("Warnings :");
//            //                foreach (ServiceDescriptionImportWarnings warn in p24.Warnings)
//            //                {
//            //                    Console.WriteLine (warn.ToString ());
//            //                }
//            //            }
//            //            if (p24.Exceptions.Length > 0)
//            //            {
//            //                Console.WriteLine ("Exceptions :");
//            //                foreach (Exception ex in p24.Exceptions)
//            //                {
//            //                    Console.WriteLine (ex.ToString ());
//            //                }
//            //            }
//
//            if (p24.HasErrors)
//                Console.WriteLine (p24.GetErrorsAsOneString ());
//
//            Console.WriteLine ("{0}\n\r", p24.WsdlAddress);
//
//            if (p24.P24Assembly != null)
//            {
//                foreach (Type t in p24.P24Assembly.CompiledAssembly.GetTypes ())
//                {
//                    Console.WriteLine (t.Name);
//                }
//            } else
//            {
//                Console.WriteLine ("NULL");
//            }

            API32 api32 = new API32 (49518, "d13be93f322c7ec3");
            api32.P24Mode = P24.P24Mode.sandbox;
            api32.Received += (object sender, MyEventArgs e) =>
            {
                Console.WriteLine ("Response: {0}", e.Response);
            };
            api32.TestConnection ();
            Console.ReadLine ();
        }
    }
}

