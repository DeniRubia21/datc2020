using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace L04
{
    public class StudentRepository : IStudentRepository 
    {
        private string _connectionString;
        private CloudTableClient _tableClient;
        private CloudTable _studentsTable;
        private List<StudentEntity> students;
        public StudentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetValue<string>("AzureStorageAccountConnectionString");
            
         
            Task.Run(async () => { await InitializeTable(); })
            .GetAwaiter()
            .GetResult(); 
            // se creaza tabelul de studenti (daca nu exista deja)
        }
        public async Task<string> CreateNewStudent(StudentEntity student)
        {
            object p = await GetAllStudents();
             // primeste lista actualizata
            int status = 0;
            
            foreach(var obiect in students) 
            // parcurege lista
            {
                if(obiect.PartitionKey.Equals(student.PartitionKey) && obiect.RowKey.Equals(student.RowKey))
                 // daca exista obiectul cautat
                {
                    status = 1;
                    //daca este eroare
                    break;
                }
                else 
                    status = 0;
                     // va adauga un obiect nou
            }
            
            if(status == 1)
                return "Eroare: studentul exista in lista!";
            else
            {
                var insert = TableOperation.Insert(student);
                 //se insereaza studentul
                await _studentsTable.ExecuteAsync(insert);

                return "Studentul a fost adaugat in lista !";
            }
        }

        public async Task<string> DeleteStudent(StudentEntity student)
        {
            await GetAllStudents();
            int status = 0;
            
            foreach(var obiect in students)
            {
                if(obiect.PartitionKey.Equals(student.PartitionKey) && obiect.RowKey.Equals(student.RowKey))
                 // cautare obiect
                {
                    status = 0;
                    var delete = TableOperation.Delete(new TableEntity(student.PartitionKey, student.RowKey) { ETag = "*" });
                     // stergere obiect dupa PartitionKey, RowKey si ETag
                    await _studentsTable.ExecuteAsync(delete);

                    students.Remove(obiect); 
                    break;
                }
                else 
                    status = 1;
            }
            
            if(status == 1)
                return "Eroare: studentul nu se afla in lista!";
            else
                return "Studentul a fost sters din lista !";
        }

        public async Task<string> EditStudent(StudentEntity student)
        {
            await GetAllStudents();
            int status = 0;

            foreach(var obiect in students)
            {
                if(obiect.PartitionKey.Equals(student.PartitionKey) && obiect.RowKey.Equals(student.RowKey))
                 // se cauta obiectul care urmeaza sa fie modificat
                {
                    status = 0;
                    var delete = TableOperation.Delete(new TableEntity(student.PartitionKey, student.RowKey) { ETag = "*" });
                     // se sterge obiectul care exista in lista
                    await _studentsTable.ExecuteAsync(delete);
                    students.Remove(obiect);

                    var insert = TableOperation.Insert(student);
                     // se adauga obiectul curent
                    await _studentsTable.ExecuteAsync(insert);
                    students.Add(obiect);
                    break;
                }
                else 
                    status = 1;
            }
            
            if(status == 1)
                return "Eroare: Studentul nu exista in lista!";
            else
                return "Studentul a fost modificat in lista !";
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public async Task<List<StudentEntity>> GetAllStudents()
        {
            students = new List<StudentEntity>();

            TableQuery<StudentEntity> query = new TableQuery<StudentEntity>();

            TableContinuationToken token = null; 
            do{
                TableQuerySegment<StudentEntity> resultSegment = await _studentsTable.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;
            
                students.AddRange(resultSegment.Results);
                 // se extrage sublista cu rezultate partiale si se adauga la lista principala

            }while(token != null);

            return students;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }

        private async Task InitializeTable()
        {
            var account = CloudStorageAccount.Parse(_connectionString);
            _tableClient = account.CreateCloudTableClient();

            _studentsTable = _tableClient.GetTableReference("studenti");

            await _studentsTable.CreateIfNotExistsAsync();

        }
    }
}