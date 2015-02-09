using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Threading;


namespace Kaltura
{
    class CodeExample
    {
        static void Main(string[] args)
        {
            int partnerId = 1883011;

            KalturaConfiguration config = new KalturaConfiguration(partnerId);
            config.ServiceUrl = "http://www.kaltura.com/";
            KalturaClient client = new KalturaClient(config);



            String secret = "b91add42c5587ec0e0dc6ac9a53eccef";


            String userId = null;


            KalturaSessionType type = KalturaSessionType.ADMIN;


            int expiry = 86400;


            String privileges = null;

            client.KS  = client.SessionService.Start(secret, userId, type, partnerId, expiry, privileges);

            KalturaMediaService kms = new KalturaMediaService(client);

           
           var mef = new KalturaMediaEntryFilter();
           mef.CreatorIdEqual = "igor.shevach@kaltura.com";
           mef.OrderBy = KalturaMediaEntryOrderBy.CREATED_AT_DESC;
            var kmsListResponce = kms.List(mef);

           KalturaMediaEntry lastCreatedEntry = kmsListResponce.Objects[0];

           foreach (KalturaMediaEntry e in kmsListResponce.Objects)
           {
               Console.WriteLine(e.Name);
           }

            // cleanup 
            List<KalturaThumbAsset> list = new List<KalturaThumbAsset>(client.ThumbAssetService.GetByEntryId(lastCreatedEntry.Id));

            list.ForEach((e)=>client.ThumbAssetService.Delete(e.Id));
    
            KalturaThumbParams [] thumbs = {
                new KalturaThumbParams(){Height = 500,Width = 500,VideoOffset= 2.0f,Name="A"},
                new KalturaThumbParams(){Height = 500,Width = 500,VideoOffset= 5.0f,Name="B"},
                new KalturaThumbParams(){Height = 500,Width = 500,VideoOffset= 7.0f,Name="C"}
                };
   
            List<KalturaThumbAsset> assets = new List<KalturaThumbAsset>();

            for(int i =0; i < thumbs.Length; i++)
            {
                thumbs[i] = client.ThumbParamsService.Add(thumbs[i]);
                KalturaThumbAsset asset = client.ThumbAssetService.GenerateByEntryId(lastCreatedEntry.Id, thumbs[i].Id);

                assets.Add(asset);
          
              }

     
            while (assets.Count > 0)
            {
                System.Threading.Thread.Sleep(10);

                for( int i = 0; i < assets.Count;i++)
                {
                    assets[i] = client.ThumbAssetService.Get(assets[i].Id);
                    switch(assets[i].Status )
                    {
                        case KalturaThumbAssetStatus.READY:
                            string uri = client.ThumbAssetService.GetUrl(assets[i].Id);

                            Console.WriteLine(uri);
                            var psi = new ProcessStartInfo() { UseShellExecute = true, Verb = "open", FileName = uri };
                            Process.Start(psi);
                            assets.RemoveAt(i--);
                            break;
                        case KalturaThumbAssetStatus.CAPTURING:
                            break;
                        default:
                            assets.RemoveAt(i--);
                            Console.WriteLine("Removing request {0}", assets[i].Status);
                            break;
                    }
                }
            }
          

            // var doc = new XmlDocument();
            

            ////doc.LoadXml(kmsListResponce.ToString);
            
            //using (FileStream fs = File.Create("c:\\Video Entries List.xml"))
            //{               
            //    doc.Save(fs);
            //}
        }
   
    }
}