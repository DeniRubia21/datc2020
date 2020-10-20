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
             // acces appsettings.json
            Task.Run(async () => { await InitializeTable(); })
            .GetAwaiter()
            .GetResult(); // va crea tabelul de studenti daca nu exista deja
        }
        public async Task<string> CreateNewStudent(StudentEntity student)
        {
            await GetAllStudents(); // primeste lista actualizata
            int status = 0;
            
            foreach(var obiect in students) // parcuregere lista
            {
                if(obiect.PartitionKey.Equals(student.PartitionKey) && obiect.RowKey.Equals(student.RowKey)) // daca exista obiectul cautat
                {
                    status = 1; // eroare
                    break;
                }
                else 
                    status = 0; // va adauga un obiect nou
            }
            
            if(status == 1)
                return "Eroare: studentul exista in lista!";
            else
            {
                var insert = TableOperation.Insert(student); // inserare student
                await _studentsTable.ExecuteAsync(insert);

                return "Studentul a fost adaugat in lista cu succes!";
            }
        }

        public async Task<string> DeleteStudent(StudentEntity student)
        {
            await GetAllStudents();
            int status = 0;
            
            foreach(var obiect in students)
            {
                if(obiect.PartitionKey.Equals(student.PartitionKey) && obiect.RowKey.Equals(student.RowKey)) // cautare obiect
                {
                    status = 0;
                    var delete = TableOperation.Delete(new TableEntity(student.PartitionKey, student.RowKey) { ETag = "*" }); // stergere obiect dupa PartitionKey, RowKey si ETag
                    await _studentsTable.ExecuteAsync(delete);

                    students.Remove(obiect); 
                    break;
                }
                else 
                    status = 1;
            }
            
            if(status == 1)
                return "Eroare: studentul nu exista in lista!";
            else
                return "Studentul a fost sters din lista cu succes!";
        }

        public async Task<string> EditStudent(StudentEntity student)
        {
            await GetAllStudents();
            int status = 0;

            foreach(var obiect in students)
            {
                if(obiect.PartitionKey.Equals(student.PartitionKey) && obiect.RowKey.Equals(student.RowKey)) // cautare obiectul care urmeaza sa fie modificat
                {
                    status = 0;
                    var delete = TableOperation.Delete(new TableEntity(student.PartitionKey, student.RowKey) { ETag = "*" }); // stergere obiectul existent
                    await _studentsTable.ExecuteAsync(delete);
                    students.Remove(obiect);

                    var insert = TableOperation.Insert(student); // adaugare obiectul curent
                    await _studentsTable.ExecuteAsync(insert);
                    students.Add(obiect);
                    break;
                }
                else 
                    status = 1;
            }
            
            if(status == 1)
                return "Eroare: studentul nu exista in lista!";
            else
                return "Studentul a fost modificat in lista cu succes!";
        }

        public async Task<List<StudentEntity>> GetAllStudents()
        {
            students = new List<StudentEntity>();

            TableQuery<StudentEntity> query = new TableQuery<StudentEntity>();

            TableContinuationToken token = null; 
            do{
                TableQuerySegment<StudentEntity> resultSegment = await _studentsTable.ExecuteQuerySegmentedAsync(query, token);
                token = resultSegment.ContinuationToken;
            
                students.AddRange(resultSegment.Results); // extragere sublista cu rezultate partiale si le adaugam la lista principala

            }while(token != null);

            return students;
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