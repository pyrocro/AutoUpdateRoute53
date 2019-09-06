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
        static void Main(string[] args)
        {
            /*string domainName = "8codebubble.com";
            string hostingZoneId = "Z1WNQ8M9WYMDH6";
            string accessKeyID = "AKIAWZUNZPANJ2LASLTG";
            string secretKey = "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";
            int syncEverySeconds = 15*1000;*/

            
            string domainName = Environment.GetEnvironmentVariable("DOMAIN_NAME");
            string hostingZoneId = Environment.GetEnvironmentVariable("HOSTING_ZONE_ID");
            string accessKeyID = Environment.GetEnvironmentVariable("ACCESS_KEY_ID");
            string secretKey = Environment.GetEnvironmentVariable("SECRET_KEY");
            int syncEverySeconds = int.Parse(Environment.GetEnvironmentVariable("SYNC_EVERY_SECONDS").ToString())*1000; //15*1000;
            



            var credentials = new BasicAWSCredentials(accessKeyID, secretKey);
            //RegionEndpoint.GetBySystemName

            var route53Client = new AmazonRoute53Client(credentials, RegionEndpoint.USWest1);
            //[2] Create a hosted zone
            var zoneRequest = new GetHostedZoneRequest()
            {
                Id = hostingZoneId
                //Name = domainName,
                //CallerReference = "my_change_request"                 

            };

            string externalip = new WebClient().DownloadString("http://icanhazip.com").Trim(); ;
            

            var zoneResponse = route53Client.GetHostedZone(zoneRequest);
            var listRecordSetRequest = new ListResourceRecordSetsRequest() { HostedZoneId = hostingZoneId, StartRecordType = RRType.A, StartRecordName = domainName };
            var listRecordSet = route53Client.ListResourceRecordSets(listRecordSetRequest);
            while (true)
            {
                try
                { 
                    externalip = new WebClient().DownloadString("http://icanhazip.com").Trim();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error while getting external IP address from http://icanhazip.com \n"+e.Message);
                }


                foreach (var rs in listRecordSet.ResourceRecordSets)
                {
                    if (rs.Type == RRType.A)
                    {
                        Console.WriteLine("..." + rs.Name);
                        foreach (var rr in rs.ResourceRecords)
                        {
                            if (rr.Value.Trim() != externalip.Trim())
                            {
                                Console.WriteLine("\n*\nExternal IP Address: " + externalip + "\n*");
                                Console.WriteLine("Syncing A Records in hosting zone: " + zoneResponse.HostedZone.Name);
                                Console.Write("......Updating Ip address with(" + externalip + ")");
                                updateRecordSet(route53Client, domainName, externalip, zoneResponse);
                            }
                            else
                            {
                                //Console.Write("......No Change Value(" + rr.Value + ") external ip(" + externalip + ") ");
                            }
                        }
                    }
                }
                Thread.Sleep(syncEverySeconds);
            }
        }

        static void updateRecordSet(AmazonRoute53Client route53Client, string domainName, string externalip, GetHostedZoneResponse zoneResponse)
        {

            //Console.WriteLine("test");

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

            var recordsetResponse = route53Client.ChangeResourceRecordSets(recordsetRequest);

            //[5] Monitor the change status
            var changeRequest = new GetChangeRequest()
            {
                Id = recordsetResponse.ChangeInfo.Id
            };
            
            while (ChangeStatus.PENDING ==
              route53Client.GetChange(changeRequest).ChangeInfo.Status)
            {
                Console.WriteLine("Change is pending.");
                Thread.Sleep(15000);
            }

            Console.WriteLine("Change is complete.");
            //Console.ReadKey();
        }
    }

}


