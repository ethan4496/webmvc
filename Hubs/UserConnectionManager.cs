namespace WebMVC.Hubs
{
    public class UserConnectionManager
    {
        private readonly Dictionary<string, string> _userConnections = new Dictionary<string, string>();

        public void AddConnection(string userId, string connectionId)
        {
            lock (_userConnections)
            {
                _userConnections[userId] = connectionId;
            }
        }

        public void RemoveConnection(string userId)
        {
            lock (_userConnections)
            {
                _userConnections.Remove(userId);
            }
        }

        public bool IsUserConnected(string userId)
        {
            return _userConnections.ContainsKey(userId);
        }

        public string GetConnectionId(string userId)
        {
            return _userConnections.TryGetValue(userId, out var connectionId) ? connectionId : null;
        }
    }

}
