using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HH_Parser
{
    public class CompanySearchItem
    {
        public string Name { get; set; }
        public string Website { get; set; }
        public bool CheckOnWebsite { get; set; }
        public int CheckOnWebsiteCount { get; set; }
        public List<string> AlternativeNames { get; set; } = new List<string>();
        public bool RestrictByDate { get; set; }
        public int RestrictYears { get; set; }
        public int RestrictMonthes { get; set; }
        public List<Place> Places { get; set; } = new List<Place>();
        [JsonIgnore]
        public List<Resume> CompanyResumes { get; set; }
        public bool ParseByMetro { get; set; }
        public int MinResumesCount { get; set; } = 5000;
        public string CompaniesArea
        {
            get
            {
                var _PUBCCompaniesAreaData = "";
                if (this.Places != null)
                {
                    foreach (var item in this.Places)
                    {
                        _PUBCCompaniesAreaData += $"area={item.ID}&";
                    }
                    if (!String.IsNullOrEmpty(_PUBCCompaniesAreaData))
                    {
                        _PUBCCompaniesAreaData = _PUBCCompaniesAreaData.Substring(0, _PUBCCompaniesAreaData.Length - 1);
                    }
                    _PUBCCompaniesAreaData = "&" + _PUBCCompaniesAreaData;
                }
                return _PUBCCompaniesAreaData;
            }
        }
        public int type { get; set; }
    }
}
