using HeyDo.Controllers;
using HeyDo.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeyDo.Data
{
    public class Caching
    {
        //TODO use this somehow
        private IMemoryCache _cache;
        private ILogger _logger;
        private DataService _ds;
        public Caching(IMemoryCache memoryCache, ILogger<Caching> logger, DataService ds)
        {
            _cache = memoryCache;
            _logger = logger;
            _ds = ds;
        }
         public async Task<List<JObject>> GetOrSetCachedData(Dictionary<string, string> auth, Enums.DataType dataType, string id = null)
        {
            var authed = auth["uid"] == await AuthController.Google(auth["token"]);
            if (!authed)
            {
                return new List<JObject>() { new JObject("Error", "Error") };
            }
            var data = new List<JObject>();
            var uData = new List<JObject>();

            var isIt = _cache.TryGetValue(auth["uid"] + dataType, out data);

            if (!isIt && authed)
            {
                // Key not in cache, so get data.
                data = await _ds.GetData(auth, dataType);
                if (data.Count>0)
                { 
                    // Save data in cache if no error
                    _cache.Set(auth["uid"] + dataType, data);
                }
            }

            if (id != null && data.Count>0)
            {
                var task = data.Find(u => u["Id"].ToString() == id);
                uData.Add(task);
                return uData;
            }

            return data;
        }

        public async Task UpdateAndClearCache(Dictionary<string, string> auth, Enums.DataType dataType, Enums.UpdateType updateType, string jData=null)
        {
            _cache.Remove(auth["uid"] + dataType);

            switch (updateType)
            {
                case Enums.UpdateType.Add:
                    await _ds.AddData(auth, dataType, jData, false);                    
                    break;
                case Enums.UpdateType.Edit:
                    await _ds.AddData(auth, dataType, jData, true);
                    break;
                case Enums.UpdateType.Delete:
                    await _ds.DeleteData(auth, dataType, "/" + jData);
                    break;
                default:
                    break;
            }
        }
    }
}
