using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace StudentsApp.Controllers
{
    [Route("[controller]")]
    public class StudentsController : Controller
    {
        [HttpGet]
        public IEnumerable<Student> Get()
        {
            return StudentsRepo.StudentsList;
        }

        [HttpPost]
        public string Post([FromBody] Student student)
        {
            StudentsRepo.StudentsList.Add(student);
            return "Adaugat!";
        }

        [HttpGet("{id}")]
        public Student Get(int id)
        {
            return StudentsRepo.StudentsList.Find(student => student.Id == id);
        }

        [HttpDelete("{id}")]
        public string Delete(int id)
        {
            Student s = StudentsRepo.StudentsList.Find(student => student.Id == id);
            StudentsRepo.StudentsList.Remove(s);
            return "Sters!";
        }

        [HttpPut("{id}")]
        public Student Update(int id, [FromBody]string name)
        {
            Student s = StudentsRepo.StudentsList.Find(student => student.Id == id);
            s.Name = name;
            return s;
        }
    }
}
