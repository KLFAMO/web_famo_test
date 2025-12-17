namespace FamoNET.Model
{
    public class FileStatus
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ProgressStatus Status { get; set; }

        public FileStatus()
        {
            
        }

        public FileStatus(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}
