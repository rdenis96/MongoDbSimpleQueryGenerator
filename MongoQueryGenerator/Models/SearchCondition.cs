namespace MongoQueryGenerator.Models
{
    public class SearchCondition
    {
        public string Field { get; set; }
        public string Value { get; set; }
        public string Comparator { get; set; }
        public string Operator { get; set; }

        public bool? Exists { get; set; }
    }
}