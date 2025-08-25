namespace BTL.Web.Models
{
    public class LayoutData
    {
        public int GridSize { get; set; }
        public List<TablePosition> Tables { get; set; } = new List<TablePosition>();
    }

    public class TablePosition
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int Type { get; set; }
        public Position Position { get; set; } = new Position();
    }

    public class Position
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
}
