using System.Collections;
using System.Collections.Generic;

namespace MVS.Realtime
{
    public class DataDictionary : IEnumerable<KeyValuePair<byte, object>>, IEnumerable
    {
        
        public IEnumerator<KeyValuePair<byte, object>> GetEnumerator()
        {
            throw new System.NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}