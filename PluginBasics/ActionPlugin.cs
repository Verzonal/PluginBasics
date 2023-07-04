using System;
using Microsoft.Xrm.Sdk;
using System.ServiceModel;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Json;
using PluginBasics;

namespace Plugins
{
    public class ActionPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = null;
            IOrganizationService service = null;
            try
            {
                IExecutionContext context = (IExecutionContext)serviceProvider.GetService(typeof(IExecutionContext));
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                service = serviceFactory.CreateOrganizationService(context.UserId);
                tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
                // TODO: Core Logic
                GetContactRecord(service).GetAwaiter().GetResult();
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred in FollowUpPlugin." + ex.Message, ex);
            }
            catch (Exception ex)
            {
                tracingService.Trace("FollowUpPlugin: {0}", ex.ToString());
                throw;
            }
        }

        public async Task GetContactRecord(IOrganizationService service)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://iqtr.crm.dynamics.com/api/data/v9.2/contacts(F5973462-768E-EB11-B1AC-000D3AE92B46)");
            request.Headers.Add("Authorization", "Bearer " + await GetCRMToken());
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string jsonData = await response.Content.ReadAsStringAsync();
            Entity caseRecord = new Entity("incident");
            caseRecord["title"] = "Case Created By Action Pluin";
            caseRecord["description"] = jsonData.Substring(0, 100);
            caseRecord["customerid"] = new EntityReference("contact", new Guid("F5973462-768E-EB11-B1AC-000D3AE92B46"));
            service.Create(caseRecord);
        }

        public async Task<string> GetCRMToken()
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://login.microsoftonline.com/5af1f8cd-bc52-4bf7-af52-6a75b233e91a/oauth2/token");
            request.Headers.Add("Cookie", "fpc=AksxMfvn63hGpr7Co2Z_Myq5VxC7AQAAAD3vNdwOAAAA; stsservicecookie=estsfd; x-ms-gateway-slice=estsfd");
            var content = new MultipartFormDataContent();
            content.Add(new StringContent("client_credentials"), "grant_type");
            content.Add(new StringContent("https://iqtr.crm.dynamics.com"), "resource");
            content.Add(new StringContent("83836b76-ff49-4515-bbae-a576af44fd99"), "client_id");
            content.Add(new StringContent("fpp8Q~aXoUH0xJVSlJa0oOEKldqIColzEUhiwbox"), "client_secret");
            request.Content = content;
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            string stringData = await response.Content.ReadAsStringAsync();
            CRMToken token = Helper.ConvertIntoObject<CRMToken>(stringData);
            return token.access_token;
        }
    }
}