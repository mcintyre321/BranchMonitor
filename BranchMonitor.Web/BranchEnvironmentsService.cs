using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using Microsoft.Web.Administration;

namespace BranchMonitor.Web
{
    public class BranchEnvironmentsService
    {
        private string _machineName = ConfigurationManager.AppSettings["testServerName"];
        private string _sqlDbNamePrefix = ConfigurationManager.AppSettings["sqlDbNamePrefix"];
        private string _serviceNamePrefix =  ConfigurationManager.AppSettings["serviceNamePrefix"];
        private string _sqlConnectionString = ConfigurationManager.AppSettings["sqlConnectionString"];
    

        public IEnumerable<string> ListEnvironments()
        {
            var serviceBranches = ServiceController.GetServices(_machineName)
                    .Where(sc => sc.DisplayName.Contains(_serviceNamePrefix))
                    .Select(sc => sc.DisplayName.Split(new[] { '.' }, 3)[1]);


            var siteBranches = ServerManager.OpenRemote(_machineName)
                .Sites.Select(s => s.Name.Split('.')[0]);


            List<string> databaseNames = new List<string>();
            using (var conn = new SqlConnection(_sqlConnectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"SELECT name FROM sys.databases WHERE Name LIKE '" + _sqlDbNamePrefix + "_%'", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.NextResult())
                    {
                        databaseNames.Add((string)reader[0]);
                    }
                }
            }
            var dbBranches = databaseNames.Select(n => n.Replace(_sqlDbNamePrefix + "_", ""));


            return serviceBranches.Concat(siteBranches).Concat(dbBranches).Distinct();

        }


        public void DeleteBranchEnvironment(string branchName)
        {

            branchName = branchName.Replace("feature_", "");

            //delete IIS sites
            {
                var sm = ServerManager.OpenRemote(_machineName);
                sm.Sites.Where(s => s.Name.StartsWith(branchName + ".")).ToList()
                    .ForEach(site =>
                    {
                        sm.Sites.Remove(site);
                    });
                sm.CommitChanges();
            }

            //delete services 
            {
                var services = ServiceController.GetServices(_machineName)
                       .Where(sc => sc.DisplayName.StartsWith(_serviceNamePrefix + branchName + ".")).ToList();
                services.ForEach(svc =>
                {
                    if (svc.Status == ServiceControllerStatus.Running)
                        svc.Stop();
                });
                services.ForEach(svc =>
                {
                    var psi = new ProcessStartInfo("sc.exe", "\\\\" + _machineName + " delete \"" + svc.ServiceName + "\"");
                    var p = Process.Start(psi);
                    p.WaitForExit();
                });
            }

            //delete database
            {
                var dbName = _sqlDbNamePrefix + "_" + branchName;
                using (var conn = new SqlConnection(_sqlConnectionString))
                {
                    conn.Open();
                    var cmd = new SqlCommand(string.Format(
        @"IF EXISTS (SELECT * FROM sys.databases WHERE Name = '{0}') BEGIN 
	alter database [{0}] set single_user with rollback immediate
	DROP DATABASE [{0}]
END", dbName), conn); ;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}