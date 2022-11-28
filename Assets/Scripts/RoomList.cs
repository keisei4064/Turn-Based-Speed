using System.Collections;
using System.Collections.Generic;
using Photon.Realtime;

public class RoomList : IEnumerable<RoomInfo>
{
    private Dictionary<string, RoomInfo> m_dictionary = new Dictionary<string, RoomInfo>();

    public void Update(List<RoomInfo> changedRoomlist)
    {
        foreach (var info in changedRoomlist)
        {
            if (!info.RemovedFromList)
            {
                m_dictionary[info.Name] = info;
            }
            else
            {
                m_dictionary.Remove(info.Name);
            }
        }
    }

    public void Clear()
    {
        m_dictionary.Clear();
    }

    public bool TryGetRoomInfo(string roomName, out RoomInfo roomInfo)
    {
        return m_dictionary.TryGetValue(roomName, out roomInfo);
    }

    public IEnumerator<RoomInfo> GetEnumerator()
    {
        foreach (var kvp in m_dictionary)
        {
            yield return kvp.Value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
