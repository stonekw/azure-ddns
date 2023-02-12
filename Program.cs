/*
    azure-ddns - set Azure Public DNS zone to current public IP of host machine
    Copyright (C) 2022  Kevin Stone

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
*/
using System;
using System.Net.Http;
using Azure.ResourceManager.Dns;
using Azure.ResourceManager.Dns.Models;
using Azure.Identity;

namespace Envixe.Server
{
    internal class DDNS
    {
        private static string ClientID = "5a646c92-2d85-47eb-884b-57c29815f5af";
        private static string TenantID = "e40a1a09-bb9c-46a5-b2b7-6c179e7417c0";
        private static string SubscriptionID = "9ca8af70-8d94-4297-a4e8-7c6ebfa5eb14";
        private static string ResourceGroupName = "envixe-misc";
        private static string DnsZoneName = "envixe.com";
        private static List<string> RecordSetNames = new List<string> {"valheim.redgods.envixe.com","ark.redgods.envixe.com"};
        static async Task Main(string[] args)
        {
            Console.WriteLine
            (
                @"azure-ddns  Copyright (C) 2022  Kevin Stone

                This program comes with ABSOLUTELY NO WARRANTY; for details type `show w'.
                This is free software, and you are welcome to redistribute it
                under certain conditions; type `show c' for details."
            );
            try 
            {
                await SetAzureDns();
            }
            catch (Exception e)
            {
                throw e;
            }
            
        }
        public static async Task SetAzureDns()
        {
            string publicIP = await GetPublicIPAddress();

            if (publicIP == null)
            {
                throw new Exception("Cannot run app. Public IP not found. api.ipify.org down?");
            }

            string appSecretFile = $"{AppContext.BaseDirectory}appsecret.txt";

            bool fileExists = File.Exists(appSecretFile);

            if (!fileExists)
            {
                throw new Exception("Cannot run app. app secret file not found in build directory");
            }
            string appSecret = await File.ReadAllTextAsync(appSecretFile);

            ClientSecretCredential credential = new ClientSecretCredential(TenantID,ClientID,appSecret);
            
            DnsManagementClient dnsClient = new DnsManagementClient (SubscriptionID,credential);

            Zone existingZone = await dnsClient.Zones.GetAsync(ResourceGroupName,DnsZoneName);

            if (existingZone == null)
            {
                throw new Exception($"Cannot run app. Dns Zone {DnsZoneName} does not exist in subscription {SubscriptionID} or principal {ClientID} does not have access");
            }
            
            foreach(string recordSetNameIteration in RecordSetNames)
            {
                //record set name must be relative record set name, not including Dns Zone Name - i.e. envixe.com
                string recordSetName = (recordSetNameIteration.EndsWith(DnsZoneName)) ?recordSetNameIteration.Replace($".{DnsZoneName}","") : recordSetNameIteration;

                RecordSet existingRecordSet = null;
                try
                {
                    existingRecordSet = await dnsClient.RecordSets.GetAsync(ResourceGroupName,DnsZoneName,recordSetName,RecordType.A);
                } 
                catch (Azure.RequestFailedException exception)
                {
                    if (exception.Status != 404)
                    {
                        throw exception;
                    }
                }

                //if no record set or IP has changed/doesnt exist
                if (existingRecordSet == null || !existingRecordSet.ARecords.Any(r => r.Ipv4Address == publicIP))
                {
                    //create or update record set
                    
                    ARecord ARecord = new ARecord();

                    ARecord.Ipv4Address = publicIP;

                    RecordSet recordSet = new RecordSet();

                    recordSet.ARecords.Add(ARecord);
                    
                    recordSet.TTL = 3600;

                    existingRecordSet = await dnsClient.RecordSets.CreateOrUpdateAsync(ResourceGroupName,DnsZoneName,recordSetName,RecordType.A,recordSet);
                }
            }
        }
        public static async Task<string> GetPublicIPAddress()
        {
            using (HttpClient client = new HttpClient())
            {
                string url = "https://api.ipify.org";

                HttpResponseMessage responseMessage = await client.GetAsync(url);

                string responseContent = await responseMessage.Content.ReadAsStringAsync();

                return responseContent;
            }
        }
    }
}