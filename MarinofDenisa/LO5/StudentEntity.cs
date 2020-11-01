using Microsoft.WindowsAzure.Storage.Table;

namespace L05
{
    public class StudentEntity : TableEntity
    {
        public StudentEntity(string university, string cnp)
        {
            this.PartitionKey = university;
            this.RowKey = cnp;
        }

        public StudentEntity()
        {

        }

        public string FirstName { get; set; }
        public string LastName{ get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Faculty { get; set; }
        public int Year { get; set; }
    }
}