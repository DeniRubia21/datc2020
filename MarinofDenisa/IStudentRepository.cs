using System.Collections.Generic;
using System.Threading.Tasks;
using L04;
public interface IStudentRepository 
// interfata - defineste metodele
{
    Task<List<StudentEntity>> GetAllStudents();

    Task<string> CreateNewStudent(StudentEntity student);
    Task<string> DeleteStudent(StudentEntity student);
    Task<string> EditStudent(StudentEntity student);
}