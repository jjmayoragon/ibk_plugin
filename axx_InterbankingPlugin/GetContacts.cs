using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace axx_InterbankingPlugin
{
    public class GetContacts : IPlugin

    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracer = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = factory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));


            var id = string.Empty;
            try
            {
                if (context.InputParameters.Contains("IdPersona"))
                    id = context.InputParameters["IdPersona"].ToString();

                // Define Condition Values
                var query_axx_idpersona = "%" + id + "%";

                // Instantiate QueryExpression query
                var query = new QueryExpression("contact");
                query.TopCount = 100;

                // Add columns to query.ColumnSet
                query.ColumnSet.AddColumns("fullname", "axx_idpersona", "contactid");

                // Define filter query.Criteria
                query.Criteria.AddCondition("axx_idpersona", ConditionOperator.Like, query_axx_idpersona);

                EntityCollection contactFromEntity = service.RetrieveMultiple(query);


                var listOfContacts = new List<ContactObj>();

                
                foreach (var c in contactFromEntity.Entities)
                {
                    listOfContacts.Add(new ContactObj
                    {
                        fullName = c.Attributes["fullname"].ToString(),
                        idPersona = c.Attributes["axx_idpersona"].ToString(),
                        guid = c.Attributes["contactid"].ToString()
                    });
                }


                var json = string.Empty;

                using (var ms = new MemoryStream())
                {
                    // create Json seralizer
                    var serializedObj = new DataContractJsonSerializer(listOfContacts.GetType());
                    // serialize list
                    serializedObj.WriteObject(ms, listOfContacts);
                    // get the json string  from memory stream
                    json = Encoding.Default.GetString(ms.ToArray());

                }

                context.OutputParameters["JsonResponse"] = json;
                

            }
            catch (Exception ex)
            {

                throw new InvalidPluginExecutionException(ex.Message);

            }
        }
    }

    [DataContract]
    public class ContactObj
    {
        [DataMember]
        public string fullName { get; set; }

        [DataMember]
        public string idPersona { get; set; }

        [DataMember]
        public string guid { get; set; }
    }

}
