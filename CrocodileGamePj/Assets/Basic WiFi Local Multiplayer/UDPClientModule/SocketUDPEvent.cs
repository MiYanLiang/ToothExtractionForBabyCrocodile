using System.Text.RegularExpressions;
namespace UDPClientModule
{
    public class SocketUDPEvent
    {
        public string name { get; set; }
        public string[] pack { get; set; }
        static private readonly char[] Delimiter = new char[] { ':' };

        public SocketUDPEvent(string name) : this(name, null) { }

        public SocketUDPEvent(string name, string data)
        {
            this.name = name;
            this.pack = data.Split(Delimiter);
        }

        public override string ToString()
        {
            return string.Format("[SocketUDPEvent: name={0}, data={1}]", name, pack.ToString());
        }
    }
}