using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.Route53;
using Amazon.Route53.Model;
using Amazon.Runtime;

namespace AutoUpdateRoute53
{
    class Program
    {
        static string getExternalIPAddress()
        {
            string externalip = string.Empty;

            try
            {
                externalip = new WebClient().DownloadString("http://icanhazip.com").Trim();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error while getting external IP address from http://icanhazip.com \n" + e.Message);
            }
            return externalip;
        }
        static void Main(string[] args)
        {
            //Load environment variabels 
            string awsRegion = Environment.GetEnvironmentVariable("AWS_REGION");
            string domainName = Environment.GetEnvironmentVariable("DOMAIN_NAME");
            string hostingZoneId = Environment.GetEnvironmentVariable("HOSTING_ZONE_ID");
            string accessKeyID = Environment.GetEnvironmentVariable("ACCESS_KEY_ID");
            string secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
            int syncEverySeconds = int.Parse(Environment.GetEnvironmentVariable("SYNC_EVERY_SECONDS").ToString())*1000; //15*1000;
            string recordIDStr = Environment.GetEnvironmentVariable("RECORD_IDS");
            string[] recordIDList = recordIDStr != null ? recordIDStr.Split(','): new string[] { }; //Ternary IF just because.


            //Print all regions 
            var tmp = RegionEndpoint.EnumerableAllRegions;
            foreach (var i in tmp)
            {
                Console.WriteLine(i.DisplayName + "\t\t" + i.SystemName);
            }
            Console.WriteLine("********************************************************************************");



            var credentials = new BasicAWSCredentials(accessKeyID, secretKey);
            //RegionEndpoint.GetBySystemName

            var route53Client = new AmazonRoute53Client(credentials, RegionEndpoint.GetBySystemName(awsRegion));
            //[2] Create a hosted zone
            var zoneRequest = new GetHostedZoneRequest()
            {
                Id = hostingZoneId
                //Name = domainName,
                //CallerReference = "my_change_request"                 

            };
            string external_IP_Address = String.Empty;            
            external_IP_Address = getExternalIPAddress(); // Initial Retrival of external IP address 

            var zoneResponse = route53Client.GetHostedZone(zoneRequest);
            var listRecordSetRequest = new ListResourceRecordSetsRequest() { HostedZoneId = hostingZoneId, StartRecordType = RRType.A, StartRecordName = domainName };
            var listRecordSet = route53Client.ListResourceRecordSets(listRecordSetRequest);
            var all_A_Records = listRecordSet.ResourceRecordSets.Where(a => a.Type == RRType.A); //get only A records.
            while (true)
            {
                external_IP_Address = getExternalIPAddress();
                if (all_A_Records.Count() == 1) // if there is only 1 A record then just updated it. 
                {
                    updateRecordSet(route53Client, all_A_Records.First(), external_IP_Address, zoneResponse);
                }
                else //then there is more than 1 A record and a more sophisticated sorting process needs to take place
                {
                    if (recordIDList.Length == 0) // if you reached here but the is no values in RECORD_IDS environment variable exit with message 
                    {
                        Console.WriteLine("There is more than one A record present.");
                        Console.WriteLine("You must enter atleast 1 value in RECORD_IDS Enviroment variable to identify which A record to update .");
                        Console.WriteLine("eg 1. RECORD_IDS = \"name1\"   eg 2. RECORD_IDS = \"name1,name2,name3\"");
                        System.Environment.Exit(0);
                    }
                    foreach (var rs in all_A_Records) //if there is more than one A record the Environment variable RECORD_IDS should be populated to identify which A records to update.
                    {                        
                        foreach (var ri in recordIDList)
                        {
                            if (rs.SetIdentifier == ri)
                            { // Only attempt change if the RecordID on Route53 Matches the provided list of name(s) in                             
                                updateRecordSet(route53Client, rs, external_IP_Address, zoneResponse);
                            }
                        }

                    }
                }
                Thread.Sleep(syncEverySeconds);
            }
        }
        static void updateRecordSet(AmazonRoute53Client route53Client, ResourceRecordSet recordSet, string externalip, GetHostedZoneResponse zoneResponse)
        {
            bool makeChangeFlag = false;

            Console.Write("" + recordSet.Failover.ToString().Substring(0, 4) + "" + "-(" + recordSet.Name + ")" + "-(" + recordSet.SetIdentifier.Substring(0, 7) + ")"); 

            if (recordSet.ResourceRecords == null) //In case resourceRocords IP address is empty
            {
                recordSet.ResourceRecords = new List<ResourceRecord>() { new ResourceRecord() { Value = externalip } };
            }
            
            foreach (var rr in recordSet.ResourceRecords) //don't really have to iterate over all but if you have more than one IP address it replaces with all with external IP address.(just leaving finger prints)
            {
                Console.Write("=" + rr.Value + " IP(" + externalip + ")");
                if (rr.Value != externalip) //
                {
                    rr.Value = externalip;
                    makeChangeFlag = true;
                }
                    
            }
            
            if (makeChangeFlag == false)
            {
                Console.WriteLine(" No change required.");
                return;
            }

            var change1 = new Change()
            {
                ResourceRecordSet = recordSet,
                Action = ChangeAction.UPSERT
            };

            var changeBatch = new ChangeBatch()
            {
                Changes = new List<Change> { change1 }
            };

            //[4] Update the zone's resource record sets
            var recordsetRequest = new ChangeResourceRecordSetsRequest()
            {
                HostedZoneId = zoneResponse.HostedZone.Id,
                ChangeBatch = changeBatch
            };

            Console.WriteLine("\n......Updating Ip address with(" + externalip + ")");
            var recordsetResponse = route53Client.ChangeResourceRecordSets(recordsetRequest);

            //[5] Monitor the change status
            var changeRequest = new GetChangeRequest()
            {
                Id = recordsetResponse.ChangeInfo.Id
            };

            //Wait for the change to take effect.
            while (ChangeStatus.PENDING == route53Client.GetChange(changeRequest).ChangeInfo.Status)
            {
                Console.WriteLine("Change is pending.");
                Thread.Sleep(15000);
            }

            Console.WriteLine("Change is complete.");
        }

        static void updateRecordSetWithDomainName(AmazonRoute53Client route53Client, string domainName, string externalip, GetHostedZoneResponse zoneResponse)
        {

            //[3] Create a resource record set change batch
            var recordSet = new ResourceRecordSet()
            {
                Name = domainName,                
                TTL = 60,
                Type = RRType.A,
                ResourceRecords = new List<ResourceRecord>
                {
                  new ResourceRecord { Value = externalip }
                }
            };

            updateRecordSet(route53Client, recordSet, externalip, zoneResponse);            
        }
    }

}


