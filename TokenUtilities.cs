/*
Copyright (c) Microsoft Corporation
All rights reserved.
Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the 
License at http://www.apache.org/licenses/LICENSE-2.0 
    
THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING 
WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR NON-INFRINGEMENT. 

See the Apache Version 2.0 License for specific language governing permissions and limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Data.EntityClient;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Threading.Tasks;


namespace mammerla.ServerIntegration
{

    public class Utilities
    {
        /// <summary>
        /// Retrieves and builds an entity connection string given one specified in web.config.
        /// </summary>
        /// <returns>Connection String for a store that is intended to host tokens.</returns>
        internal static string GetEntityConnectionString()
        {
            string cs = string.Empty;
            System.Configuration.Configuration rootWebConfig = System.Web.Configuration.WebConfigurationManager.OpenWebConfiguration("/MyWebSiteRoot");
            System.Configuration.ConnectionStringSettings connString;


            if (rootWebConfig.ConnectionStrings.ConnectionStrings.Count > 0)
            {
                connString = rootWebConfig.ConnectionStrings.ConnectionStrings["AppTokenStorage"];

                if (connString != null)
                {
                    cs = connString.ConnectionString;
                }
                else
                {
                    throw new Exception("No connection string named 'AppTokenStorage' was declared in web.config.");
                }
            }

            EntityConnectionStringBuilder ecsb = new EntityConnectionStringBuilder();

            ecsb.ProviderConnectionString = cs + ";MultipleActiveResultSets=True;Application Name=EntityFramework";
            ecsb.Provider = "System.Data.SqlClient";
            ecsb.Metadata = "res://*/TokenStore.csdl|res://*/TokenStore.ssdl|res://*/TokenStore.msl";

            return ecsb.ToString();
        }
    }
}
