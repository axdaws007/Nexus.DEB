namespace Nexus.DEB.Application.Common.Models.Dms
{
    public class DmsDataTableParameters
    {
        public int Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; } = 10;
        public List<DmsDataTableOrder> Order { get; set; } = new();
    }
}
