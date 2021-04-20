using RESTfulEngine.BusinessLogic;
using RESTfulEngine.DocumentRepository;
using RESTfulEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RESTfulEngine.DocumentRepository
{
    public class TestRepository : IRepository
    {
        public string TestVar1 { get; set; }
        public bool TestVar2 { get; set; }
        public int TestVar3 { get; set; }
        public TestRepository()
        {
            TestVar1 = "Testvar1 value";
            TestVar2 = true;
            TestVar3 = 52;
        }

        public string CreateRequest(Template template, RepositoryStatus.REQUEST_TYPE requestType)
        {
            throw new NotImplementedException();
        }

        public void DeleteReport(string guid)
        {
            throw new NotImplementedException();
        }

        public ServiceError GetError(string guid)
        {
            throw new NotImplementedException();
        }

        public Metrics GetMetrics(string guid)
        {
            throw new NotImplementedException();
        }

        public Document GetReport(string guid)
        {
            throw new NotImplementedException();
        }

        public RequestStatus GetReportStatus(string guid)
        {
            throw new NotImplementedException();
        }

        public TagTree GetTagTree(string guid)
        {
            throw new NotImplementedException();
        }

        public void SaveError(Template template, ServiceError error)
        {
            throw new NotImplementedException();
        }

        public void SaveMetrics(Template template, Metrics metrics)
        {
            throw new NotImplementedException();
        }

        public void SaveReport(Template template, Document document)
        {
            throw new NotImplementedException();
        }

        public void SaveTagTree(Template template, TagTree tree)
        {
            throw new NotImplementedException();
        }

        public void SetJobHandler(IJobHandler handler)
        {
            throw new NotImplementedException();
        }

        public void ShutDown()
        {
            throw new NotImplementedException();
        }

        public RepositoryRequest TakeRequest()
        {
            throw new NotImplementedException();
        }
    }
}
