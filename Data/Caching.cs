using HeyDo.Controllers;
using HeyDo.Models;
using Microsoft.Extensions.Caching.Memory;
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
        public Caching(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }
        private async Task<List<JObject>> GetOrSetCachedData(Dictionary<string, string> auth, Enums.DataType dataType, string id = null)
        {
            var authed = auth["uid"] == await AuthController.Google(auth["token"]);
            if (!authed)
            {
                //Logout();
            }
            var data = new List<JObject>();
            var uData = new List<JObject>();

            var isIt = _cache.TryGetValue(auth["uid"] + dataType, out data);

            if (!isIt && authed)
            {
                // Key not in cache, so get data.
                data = await DataController.GetData(auth, dataType);
                if (data.Count > 0)
                {
                    // Save data in cache if no error
                    _cache.Set(auth["uid"] + dataType, data);
                }
            }

            if (id != null && data.Count > 0)
            {
                var task = data.Find(u => u["Id"].ToString() == id);
                uData.Add(task);
                return uData;
            }

            return data;
        }

        private async Task UpdateAndClearCache(Dictionary<string, string> auth, Enums.DataType dataType, Enums.UpdateType updateType, string jData = null)
        {
            _cache.Remove(auth["uid"] + dataType);

            switch (updateType)
            {
                case Enums.UpdateType.Add:
                    await DataController.AddData(auth, dataType, jData, false);
                    break;
                case Enums.UpdateType.Edit:
                    await DataController.AddData(auth, dataType, jData, true);
                    break;
                case Enums.UpdateType.Delete:
                    await DataController.DeleteData(auth, dataType, "/" + jData);
                    break;
                default:
                    break;
            }
        }
    }
}
