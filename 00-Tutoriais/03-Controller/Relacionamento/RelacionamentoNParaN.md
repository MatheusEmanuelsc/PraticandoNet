# 📘 ASP.NET Core 8 - Relacionamento N:N (Muitos para Muitos)

## 📑 Índice

1. [Visão Geral](#visão-geral)
2. [Modelos (Entities)](#modelos-entities)
3. [Configuração do DbContext](#configura%C3%A7%C3%A3o-do-dbcontext)
4. [DTOs e AutoMapper](#dtos-e-automapper)
5. [Repository e Unit of Work](#repository-e-unit-of-work)
6. [Controllers](#controllers)
7. [Conclusão](#conclus%C3%A3o)

---

## ✨ Visão Geral

Este resumo mostra como implementar um relacionamento **N:N (muitos para muitos)** no ASP.NET Core 8, utilizando uma **tabela de junção** explícita e boas práticas com **DTOs**, **AutoMapper**, **Repository Pattern** e **Unit of Work**.

> Exemplo: Um `Student` pode se inscrever em vários `Courses`, e um `Course` pode ter vários `Students`.

---

## 🧱 Modelos (Entities)

### `Student.cs`
```csharp
public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}
```

### `Course.cs`
```csharp
public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;

    public ICollection<StudentCourse> StudentCourses { get; set; } = new List<StudentCourse>();
}
```

### `StudentCourse.cs` (Tabela de junção)
```csharp
public class StudentCourse
{
    public int StudentId { get; set; }
    public Student Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;
}
```

> ✅ A classe de junção é necessária para permitir configurações mais ricas (ex: colunas extras).

---

## 🛠️ Configuração do DbContext

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<StudentCourse> StudentCourses => Set<StudentCourse>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<StudentCourse>()
            .HasKey(sc => new { sc.StudentId, sc.CourseId });

        modelBuilder.Entity<StudentCourse>()
            .HasOne(sc => sc.Student)
            .WithMany(s => s.StudentCourses)
            .HasForeignKey(sc => sc.StudentId);

        modelBuilder.Entity<StudentCourse>()
            .HasOne(sc => sc.Course)
            .WithMany(c => c.StudentCourses)
            .HasForeignKey(sc => sc.CourseId);
    }
}
```

> ⚠️ Essa configuração **não é gerada automaticamente** pelo EF Core. É **obrigatória** quando usamos uma entidade de junção com propriedades próprias.

---

## 🧾 DTOs e AutoMapper

### `EnrollStudentDto.cs`
```csharp
public class EnrollStudentDto
{
    public int StudentId { get; set; }
}
```

### `StudentCourseDto.cs`
```csharp
public class StudentCourseDto
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
}
```

### `MappingProfile.cs`
```csharp
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<StudentCourse, StudentCourseDto>()
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student.Name));
    }
}
```

---

## 📦 Repository e Unit of Work

### `IStudentCourseRepository.cs`
```csharp
public interface IStudentCourseRepository
{
    Task<IEnumerable<StudentCourse>> GetStudentsByCourseAsync(int courseId);
    Task<StudentCourse?> GetEnrollmentAsync(int courseId, int studentId);
    Task AddAsync(StudentCourse enrollment);
    void Remove(StudentCourse enrollment);
}
```

> ✅ Repositório específico para o relacionamento N:N.

---

## 🎮 Controllers

### `CourseStudentsController.cs`

```csharp
[ApiController]
[Route("api/courses/{courseId:int}/students")]
public class CourseStudentsController : ControllerBase
{
    private readonly IStudentCourseRepository _repo;
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public CourseStudentsController(IStudentCourseRepository repo, IUnitOfWork uow, IMapper mapper)
    {
        _repo = repo;
        _uow = uow;
        _mapper = mapper;
    }

    // GET /api/courses/1/students
    [HttpGet]
    public async Task<IActionResult> GetAll(int courseId)
    {
        var students = await _repo.GetStudentsByCourseAsync(courseId);
        return Ok(_mapper.Map<IEnumerable<StudentCourseDto>>(students));
    }

    // POST /api/courses/1/students
    [HttpPost]
    public async Task<IActionResult> Enroll(int courseId, EnrollStudentDto dto)
    {
        var existing = await _repo.GetEnrollmentAsync(courseId, dto.StudentId);
        if (existing is not null) return Conflict("Student already enrolled.");

        var enrollment = new StudentCourse
        {
            CourseId = courseId,
            StudentId = dto.StudentId
        };

        await _repo.AddAsync(enrollment);
        await _uow.CommitAsync();

        return CreatedAtAction(nameof(GetAll), new { courseId }, null);
    }

    // DELETE /api/courses/1/students/2
    [HttpDelete("{studentId:int}")]
    public async Task<IActionResult> Remove(int courseId, int studentId)
    {
        var enrollment = await _repo.GetEnrollmentAsync(courseId, studentId);
        if (enrollment is null) return NotFound();

        _repo.Remove(enrollment);
        await _uow.CommitAsync();
        return NoContent();
    }
}
```

> 🔁 Todas as ações são feitas com base no `Course`, que é o recurso "pai" neste contexto.

---

## ✅ Conclusão

- Em relacionamentos N:N, é recomendado usar uma **entidade de junção explícita** (`StudentCourse`).
- A configuração via `OnModelCreating` é **obrigatória** neste caso.
- É uma boa prática criar **repositórios específicos** para trabalhar com a entidade de junção.
- O controlador pode expor o recurso dependente como `/api/courses/{courseId}/students`, facilitando o controle e evitando inconsistência de dados.


