﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Security.Permissions;
using System.Web.Services.Description;

namespace P24Wsdl
{
    public class P24WsdlDynamicClass
    {
        #region Fields

        private object p24Object;
        private ServiceDescription description;
        private CompilerResults p24Assembly;

        private int p24Id;
        private WsdlMode wsdlMode;
        private WsdlType wsdlType;
        private WsdlVersion wsdlVersion;

        private string wsdlAddress;

        private List<Exception> exceptions;
        private List<ServiceDescriptionImportWarnings> warnings;
        private List<CompilerError> compilerErrors;

        #endregion

        #region Properties

        /// <summary>
        /// Compiled Assembly for p24 WSDL
        /// </summary>
        /// <value>The p24 assembly.</value>
        public CompilerResults P24Assembly{ get { return this.p24Assembly; } }

        /// <summary>
        /// Array of exceptions thrown during creating assembly or objects
        /// </summary>
        /// <value>The exceptions.</value>
        public Exception[] Exceptions { get { return this.exceptions.ToArray (); } }
        /// <summary>
        /// Array of warnings from compiler
        /// </summary>
        /// <value>The warnings.</value>
        public ServiceDescriptionImportWarnings[] Warnings { get { return this.warnings.ToArray (); } }
        /// <summary>
        /// Array of Compilers Errors
        /// </summary>
        /// <value>The errors.</value>
        public CompilerError[] Errors { get { return this.compilerErrors.ToArray (); } }

        public string WsdlAddress { get { return this.wsdlAddress; } }

        #endregion


        #region C'tors
        /// <summary>
        /// Wsdls the p24 class.
        /// </summary>
        /// <returns>The p24 class.</returns>
        /// <param name="IdP24">Identifier p24.</param>
        [SecurityPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        public P24WsdlDynamicClass (int p24Id, WsdlMode wsdlMode, WsdlType wsdlType, WsdlVersion wsdlVersion)
        {
            // Maintandance fields
            this.exceptions = new List<Exception>();
            this.warnings = new List<ServiceDescriptionImportWarnings> ();
            this.compilerErrors = new List<CompilerError> ();

            // create fields
            this.p24Id = p24Id;
            this.wsdlMode = wsdlMode;
            this.wsdlType = wsdlType;
            this.wsdlVersion = wsdlVersion;

            try
            {
                this.p24Assembly = CreateP24Assemby ();
            }
            catch(Exception ex)
            {
                this.exceptions.Add (ex);
            }
        }

        #endregion

        /// <summary>
        /// Create Assembly from WSDL
        /// </summary>
        /// <returns>The p24 assemby.</returns>
        [SecurityPermissionAttribute(SecurityAction.Demand, Unrestricted = true)]
        private CompilerResults CreateP24Assemby()
        {
            // Create client
            System.Net.WebClient client = new System.Net.WebClient ();

            // Create wsdl address
            string shopId = this.p24Id.ToString ();
            string mode = "";
            string type = "";

            if (this.wsdlMode == WsdlMode.sandobx)
                mode = "sandbox";
            else
                mode = "secure";
            if (this.wsdlType == WsdlType.literal)
                type = "s";
            if (this.wsdlVersion == WsdlVersion.Wsdl30)
            {
                this.wsdlAddress = 
                    string.Format ("https://{0}.przelewy24.pl/external/{1}{2}.wsdl", mode, shopId, type);
            } else
            {
                this.wsdlAddress = 
                    string.Format ("https://{0}.przelewy24.pl/external/wsdl/service{1}.php?wsdl", mode, type);
            }

            // Connect to the P24 Web Service
            Stream stream = client.OpenRead(this.wsdlAddress);

            // Read descroiption
            this.description = ServiceDescription.Read(stream);

            ///////// LOAD THE DOM /////////

            // Initialize a service description importer.
            ServiceDescriptionImporter importer = new ServiceDescriptionImporter ();
            importer.ProtocolName = "Soap";
            importer.AddServiceDescription(this.description, null, null);

            // generate a proxy client
            importer.Style = ServiceDescriptionImportStyle.Client;

            // Generate properties to represent primitive values
            importer.CodeGenerationOptions = System.Xml.Serialization.CodeGenerationOptions.GenerateProperties;

            // Initialize a Code-DOM tree into which we will import service
            CodeNamespace nameSpace = new CodeNamespace ();
            CodeCompileUnit compileUnit = new CodeCompileUnit ();
            compileUnit.Namespaces.Add (nameSpace);

            // Import the service into the Code-DOM tree. This creates proxy code that uses service
            ServiceDescriptionImportWarnings warning = importer.Import (nameSpace, compileUnit);

            if (warning == 0)
            {
                try
                {
                    // Generate the proxy code
                    CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");

                    // Compile the assembly proxy with the appropriate references
                    string[] assemblyReferences = new string[5] 
                    { 
                        "System.dll", 
                        "System.Web.Services.dll", 
                        "System.Web.dll", 
                        "System.Xml.dll", 
                        "System.Data.dll" 
                    };

                    CompilerParameters parms = new CompilerParameters(assemblyReferences);
                    CompilerResults results = provider.CompileAssemblyFromDom(parms, compileUnit);

                    if(results.Errors.Count > 0)
                    {
                        foreach(CompilerError error in results.Errors)
                        {
                            this.compilerErrors.Add(error);
                        }
                        throw new Exception("Compile Error, Check Error List for details");
                    }
                    else
                    {
                        // return compiled assembly
                        return results;
                    }
                }
                catch(Exception ex)
                {
                    this.exceptions.Add (ex);
                }
                return null;
            } 
            else
            {
                this.warnings.Add (warning);
                return null;
            }
        }
    }
}

